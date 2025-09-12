// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2024-12-19 03:12:36
// # Recently: 2024-12-22 20:12:33
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Mono.Cecil;
using Unity.CompilationPipeline.Common.ILPostProcessing;

namespace Astraia.Editor
{
    internal sealed class AssemblyResolver : IAssemblyResolver
    {
        private readonly Dictionary<string, AssemblyDefinition> definitions = new Dictionary<string, AssemblyDefinition>();
        private readonly Dictionary<string, string> assemblies = new Dictionary<string, string>();
        private readonly ICompiledAssembly assembly;
        private readonly ILog log;
        private AssemblyDefinition definition;

        public AssemblyResolver(ICompiledAssembly assembly, ILog log)
        {
            this.log = log;
            this.assembly = assembly;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public AssemblyDefinition Resolve(AssemblyNameReference assembly)
        {
            return Resolve(assembly, new ReaderParameters(ReadingMode.Deferred));
        }

        public AssemblyDefinition Resolve(AssemblyNameReference assembly, ReaderParameters parameters)
        {
            if (this.assembly.Name == assembly.Name)
            {
                return definition;
            }

            if (!assemblies.TryGetValue(assembly.Name, out var reference))
            {
                reference = LoadData(assembly.Name);
                assemblies.TryAdd(assembly.Name, reference);
            }

            if (reference == null)
            {
                log.Warn("无法找到文件:" + assembly);
                return null;
            }

            var writeTime = reference + File.GetLastWriteTime(reference);
            if (!definitions.TryGetValue(writeTime, out var result))
            {
                parameters.AssemblyResolver = this;
                var stream = Restart(reference, TimeSpan.FromSeconds(1));

                var fileName = reference + ".pdb";
                if (File.Exists(fileName))
                {
                    parameters.SymbolStream = Restart(fileName, TimeSpan.FromSeconds(1));
                }

                var assemblyDefinition = AssemblyDefinition.ReadAssembly(stream, parameters);
                definitions.TryAdd(writeTime, assemblyDefinition);
                return assemblyDefinition;
            }

            return result;
        }

        private string LoadData(string name)
        {
            foreach (var reference in assembly.References)
            {
                if (Path.GetFileNameWithoutExtension(reference) == name)
                {
                    return reference;
                }
            }

            var caches = new HashSet<string>();
            foreach (var reference in assembly.References)
            {
                var filePath = Path.GetDirectoryName(reference);
                if (filePath != null && caches.Add(filePath))
                {
                    var fileName = Path.Combine(filePath, name + ".dll");
                    if (File.Exists(fileName))
                    {
                        return fileName;
                    }
                }
            }

            return null;
        }

        private static MemoryStream Restart(string fileName, TimeSpan waitTime, int retryCount = 10)
        {
            try
            {
                byte[] bytes;
                using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    bytes = new byte[stream.Length];
                    var count = stream.Read(bytes, 0, (int)stream.Length);
                    if (count != stream.Length)
                    {
                        throw new InvalidOperationException("文件读取长度不完整。");
                    }
                }

                return new MemoryStream(bytes);
            }
            catch (IOException e)
            {
                if (retryCount == 0)
                {
                    throw new Exception(e.ToString());
                }

                Thread.Sleep(waitTime);
                return Restart(fileName, waitTime, --retryCount);
            }
        }

        public void SetAssemblyDefinitionForCompiledAssembly(AssemblyDefinition definition)
        {
            this.definition = definition;
        }
    }
}