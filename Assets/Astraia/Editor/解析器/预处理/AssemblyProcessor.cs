// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-07-06 15:07:22
// # Recently: 2026-09-06 15:32:39
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Unity.CompilationPipeline.Common.ILPostProcessing;

namespace Astraia
{
    internal sealed class AssemblyProcessor : ILPostProcessor
    {
        public override ILPostProcessor GetInstance() => this;

        public override bool WillProcess(ICompiledAssembly compiledAssembly)
        {
            if (compiledAssembly.Name.StartsWith("Unity"))
            {
                return false;
            }

            if (compiledAssembly.Name.EndsWith("Editor"))
            {
                return false;
            }

            if (compiledAssembly.Name == "Astraia.Run")
            {
                return false;
            }

            if (compiledAssembly.Name == "Astraia.Fog")
            {
                return false;
            }

            return compiledAssembly.References.Any(r => Path.GetFileNameWithoutExtension(r) == "Astraia");
        }

        public override ILPostProcessResult Process(ICompiledAssembly compiledAssembly)
        {
            var debugger = new AssemblyDebugger();

            using var resolver = new AssemblyResolver(compiledAssembly, debugger);
            using var peData = new MemoryStream(compiledAssembly.InMemoryAssembly.PeData);
            using var pdbData = new MemoryStream(compiledAssembly.InMemoryAssembly.PdbData);

            var readParams = new ReaderParameters();
            readParams.SymbolStream = pdbData;
            readParams.SymbolReaderProvider = new PortablePdbReaderProvider();
            readParams.ReadSymbols = true;
            readParams.AssemblyResolver = resolver;
            readParams.ReflectionImporterProvider = new ReflectionProvider();
            readParams.ReadingMode = ReadingMode.Immediate;
            using var assembly = AssemblyDefinition.ReadAssembly(peData, readParams);
            resolver.SetAssemblyDefinitionForCompiledAssembly(assembly);

            var opcode = 0;
            foreach (var r in assembly.MainModule.AssemblyReferences)
            {
                switch (r.Name)
                {
                    case "Astraia":
                        opcode |= 1 << 0;
                        break;
                    case "Astraia.Net":
                        opcode |= 1 << 1;
                        break;
                }
            }

            if (opcode == 0)
            {
                return new ILPostProcessResult(compiledAssembly.InMemoryAssembly, debugger);
            }

            var result = compiledAssembly.Name == "Astraia.Net" || (opcode & 2) != 0;
            if (new Weaver().Weave(assembly, debugger, resolver, result, out var modified) && modified)
            {
                var module = assembly.MainModule;
                if (module.AssemblyReferences.Any(r => r.Name == assembly.Name.Name))
                {
                    var name = module.AssemblyReferences.First(r => r.Name == assembly.Name.Name);
                    module.AssemblyReferences.Remove(name);
                }

                using var peStream = new MemoryStream();
                using var pdbStream = new MemoryStream();

                var writeParams = new WriterParameters();
                writeParams.SymbolStream = pdbStream;
                writeParams.SymbolWriterProvider = new PortablePdbWriterProvider();
                writeParams.WriteSymbols = true;
                assembly.Write(peStream, writeParams);

                return new ILPostProcessResult(new InMemoryAssembly(peStream.ToArray(), pdbStream.ToArray()), debugger);
            }

            return new ILPostProcessResult(compiledAssembly.InMemoryAssembly, debugger);
        }
    }
}