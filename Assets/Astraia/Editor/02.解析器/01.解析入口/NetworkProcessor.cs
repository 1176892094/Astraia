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

using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Unity.CompilationPipeline.Common.ILPostProcessing;

namespace Astraia.Editor
{
    internal class NetworkProcessor : ILPostProcessor
    {
        public override ILPostProcessor GetInstance() => this;

        public override bool WillProcess(ICompiledAssembly assembly)
        {
            return assembly.Name == Const.ASSEMBLY || FindReference(assembly);
        }

        private static bool FindReference(ICompiledAssembly assembly)
        {
            return assembly.References.Any(reference => Path.GetFileNameWithoutExtension(reference) == Const.ASSEMBLY);
        }

        public override ILPostProcessResult Process(ICompiledAssembly assembly)
        {
            var log = new LogPostProcessor();
            using var ar = new AssemblyResolver(assembly, log);
            using var ss = new MemoryStream(assembly.InMemoryAssembly.PdbData);
            var rp = new ReaderParameters
            {
                SymbolStream = ss,
                SymbolReaderProvider = new PortablePdbReaderProvider(),
                AssemblyResolver = ar,
                ReflectionImporterProvider = new ReflectionProvider(),
                ReadingMode = ReadingMode.Immediate
            };
            var pd = assembly.InMemoryAssembly.PeData;
            using var ms = new MemoryStream(pd);
            using var ad = AssemblyDefinition.ReadAssembly(ms, rp);
            ar.SetAssemblyDefinitionForCompiledAssembly(ad);
            var process = new Process(log);
            if (!process.Execute(ad, ar, out var change) || !change)
            {
                return new ILPostProcessResult(assembly.InMemoryAssembly, log.logs);
            }

            var mm = ad.MainModule;
            foreach (var nr in mm.AssemblyReferences)
            {
                if (nr.Name == ad.Name.Name)
                {
                    AssemblyNameReference an = null;
                    foreach (var reference in mm.AssemblyReferences)
                    {
                        if (reference.Name == ad.Name.Name)
                        {
                            an = reference;
                            break;
                        }
                    }

                    mm.AssemblyReferences.Remove(an);
                    break;
                }
            }

            using var pe = new MemoryStream();
            using var pdb = new MemoryStream();

            var wp = new WriterParameters
            {
                SymbolWriterProvider = new PortablePdbWriterProvider(),
                SymbolStream = pdb,
                WriteSymbols = true
            };


            ad.Write(pe, wp);
            return new ILPostProcessResult(new InMemoryAssembly(pe.ToArray(), pdb.ToArray()), log.logs);
        }
    }
}