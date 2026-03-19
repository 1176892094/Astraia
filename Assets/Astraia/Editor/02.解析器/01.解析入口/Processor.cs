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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Unity.CompilationPipeline.Common.Diagnostics;
using Unity.CompilationPipeline.Common.ILPostProcessing;

namespace Astraia.Editor
{
    internal sealed class NetworkProcessor : ILPostProcessor
    {
        public override ILPostProcessor GetInstance() => this;

        public override bool WillProcess(ICompiledAssembly compiledAssembly)
        {
            return compiledAssembly.Name == Weaver.GEN_TYPE || FindReference(compiledAssembly);
        }

        private static bool FindReference(ICompiledAssembly compiledAssembly)
        {
            var result = compiledAssembly.References.Any(reference => Path.GetFileNameWithoutExtension(reference) == Weaver.GEN_TYPE);
            return result && !compiledAssembly.Defines.Contains(Weaver.GEN_SKIP);
        }

        public override ILPostProcessResult Process(ICompiledAssembly compiledAssembly)
        {
            var debugger = new LogPostProcessor();
            using var resolver = new AssemblyResolver(compiledAssembly, debugger);

            using var peData = new MemoryStream(compiledAssembly.InMemoryAssembly.PeData);
            using var pdbData = new MemoryStream(compiledAssembly.InMemoryAssembly.PdbData);
            using var assembly = AssemblyDefinition.ReadAssembly(peData, new ReaderParameters
            {
                SymbolStream = pdbData,
                SymbolReaderProvider = new PortablePdbReaderProvider(),
                ReadSymbols = true,
                AssemblyResolver = resolver,
                ReflectionImporterProvider = new ReflectionProvider(),
                ReadingMode = ReadingMode.Immediate
            });
            resolver.SetAssemblyDefinitionForCompiledAssembly(assembly);
            if (new Weaver(debugger).Weave(assembly, resolver, out var modified) && modified)
            {
                var module = assembly.MainModule;
                if (module.AssemblyReferences.Any(reference => reference.Name == assembly.Name.Name))
                {
                    var name = module.AssemblyReferences.First(reference => reference.Name == assembly.Name.Name);
                    module.AssemblyReferences.Remove(name);
                }

                using var peStream = new MemoryStream();
                using var pdbStream = new MemoryStream();
                assembly.Write(peStream, new WriterParameters
                {
                    SymbolStream = pdbStream,
                    SymbolWriterProvider = new PortablePdbWriterProvider(),
                    WriteSymbols = true
                });
                return new ILPostProcessResult(new InMemoryAssembly(peStream.ToArray(), pdbStream.ToArray()), debugger.messages);
            }

            return new ILPostProcessResult(compiledAssembly.InMemoryAssembly, debugger.messages);
        }
    }

    internal interface ILogPostProcessor
    {
        void Warn(object message, MemberReference member = null);
        void Error(object message, MemberReference member = null);
    }

    public class LogPostProcessor : ILogPostProcessor
    {
        public readonly List<DiagnosticMessage> messages = new List<DiagnosticMessage>();

        public void Warn(object message, MemberReference member = null)
        {
            Add(message, member, DiagnosticType.Warning);
        }

        public void Error(object message, MemberReference member = null)
        {
            Add(message, member, DiagnosticType.Error);
        }

        private void Add(object message, MemberReference member, DiagnosticType mode)
        {
            var reason = message.ToString();
            if (member != null)
            {
                reason = "{0} with [{1}]".Format(reason, member.ToString().Color("G"));
            }

            var source = reason.Split('\n');
            foreach (var result in source)
            {
                messages.Add(new DiagnosticMessage
                {
                    DiagnosticType = mode,
                    MessageData = result,
                    File = string.Empty,
                });
            }
        }
    }

    internal sealed class AssemblyResolver : IAssemblyResolver
    {
        private readonly ConcurrentDictionary<string, AssemblyDefinition> Definitions = new ConcurrentDictionary<string, AssemblyDefinition>();
        private readonly ConcurrentDictionary<string, string> Assemblies = new ConcurrentDictionary<string, string>();
        private readonly ICompiledAssembly Assembly;
        private readonly ILogPostProcessor Debugger;
        private AssemblyDefinition Definition;

        public AssemblyResolver(ICompiledAssembly assembly, ILogPostProcessor debugger)
        {
            Debugger = debugger;
            Assembly = assembly;
        }

        public void Dispose()
        {
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