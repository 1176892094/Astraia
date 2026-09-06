// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-09-06 23:09:10
// # Recently: 2026-09-06 23:12:10
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using Mono.Cecil;
using Unity.CompilationPipeline.Common.ILPostProcessing;

namespace Astraia
{
    internal sealed class AssemblyResolver : IAssemblyResolver
    {
        private readonly ConcurrentDictionary<string, AssemblyDefinition> Definitions = new ConcurrentDictionary<string, AssemblyDefinition>();
        private readonly ConcurrentDictionary<string, string> Assemblies = new ConcurrentDictionary<string, string>();
        private readonly ICompiledAssembly Assembly;
        private readonly AssemblyDebugger Debugger;
        private AssemblyDefinition Definition;

        public AssemblyResolver(ICompiledAssembly assembly, AssemblyDebugger debugger)
        {
            Debugger = debugger;
            Assembly = assembly;
        }

        public void Dispose()
        {
            foreach (var definition in Definitions.Values)
            {
                definition.Dispose();
            }

            GC.SuppressFinalize(this);
        }

        ~AssemblyResolver()
        {
            foreach (var definition in Definitions.Values)
            {
                definition.Dispose();
            }
        }

        public AssemblyDefinition Resolve(AssemblyNameReference assembly)
        {
            return Resolve(assembly, new ReaderParameters(ReadingMode.Deferred));
        }

        public AssemblyDefinition Resolve(AssemblyNameReference assembly, ReaderParameters parameters)
        {
            if (Assembly.Name == assembly.Name)
            {
                return Definition;
            }

            if (!Assemblies.TryGetValue(assembly.Name, out var reference))
            {
                reference = LoadData(assembly.Name);
                Assemblies.TryAdd(assembly.Name, reference);
            }

            if (reference == null)
            {
                Debugger.Warn("无法找到文件:" + assembly);
                return null;
            }

            var writeTime = reference + File.GetLastWriteTime(reference);
            if (!Definitions.TryGetValue(writeTime, out var result))
            {
                parameters.AssemblyResolver = this;
                var stream = Reload(reference, TimeSpan.FromSeconds(1));

                var fileName = reference + ".pdb";
                if (File.Exists(fileName))
                {
                    parameters.SymbolStream = Reload(fileName, TimeSpan.FromSeconds(1));
                }

                var definition = AssemblyDefinition.ReadAssembly(stream, parameters);
                Definitions.TryAdd(writeTime, definition);
                return definition;
            }

            return result;
        }

        private string LoadData(string name)
        {
            foreach (var reference in Assembly.References)
            {
                if (Path.GetFileNameWithoutExtension(reference) == name)
                {
                    return reference;
                }
            }

            var caches = new HashSet<string>();
            foreach (var reference in Assembly.References)
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

        private static MemoryStream Reload(string fileName, TimeSpan waitTime, int retryCount = 10)
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
                return Reload(fileName, waitTime, --retryCount);
            }
        }

        public void SetAssemblyDefinitionForCompiledAssembly(AssemblyDefinition definition)
        {
            Definition = definition;
        }
    }

    internal class ReflectionProvider : IReflectionImporterProvider
    {
        public IReflectionImporter GetReflectionImporter(ModuleDefinition definition)
        {
            return new ReflectionImporter(definition);
        }
    }

    internal class ReflectionImporter : DefaultReflectionImporter
    {
        private readonly AssemblyNameReference assemblyName;

        public ReflectionImporter(ModuleDefinition module) : base(module)
        {
            AssemblyNameReference assemblyData = null;
            foreach (var assembly in module.AssemblyReferences)
            {
                if (assembly.Name is "mscorlib" or "netstandard" or "System.Private.CoreLib")
                {
                    assemblyData = assembly;
                    break;
                }
            }

            assemblyName = assemblyData;
        }

        public override AssemblyNameReference ImportReference(AssemblyName name)
        {
            if (assemblyName != null && name.Name == "System.Private.CoreLib")
            {
                return assemblyName;
            }

            return base.ImportReference(name);
        }
    }
}