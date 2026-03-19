// *********************************************************************************
// # Project: Forest
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2025-01-12 15:01:52
// # Recently: 2025-01-12 15:01:52
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System.Linq;
using System.Runtime.CompilerServices;
using Astraia.Core;
using Mono.Cecil;
using Mono.Cecil.Cil;
using UnityEngine;

namespace Astraia.Editor
{
    internal static class NetworkRuntime
    {
        public static bool Process(AssemblyDefinition assembly, IAssemblyResolver resolver, ILogPostProcessor debugger, Writer writer, Reader reader, ref bool failed)
        {
            ProcessAssembly(assembly, resolver, debugger, writer, reader, ref failed);
            return ProcessProperty(assembly, assembly, writer, reader, ref failed);
        }

        public static void Processed(AssemblyDefinition assembly, Module module, Writer writer, Reader reader, TypeDefinition expand)
        {
            var method = new MethodDefinition(nameof(Processed), MethodAttributes.Public | MethodAttributes.Static, module.Import(typeof(void)));

            var construct = module.Initialized.GetConstructors().Last();
            var attribute = new CustomAttribute(assembly.MainModule.ImportReference(construct));
            attribute.ConstructorArguments.Add(new CustomAttributeArgument(module.Import<RuntimeInitializeLoadType>(), RuntimeInitializeLoadType.BeforeSceneLoad));
            method.CustomAttributes.Add(attribute);

            var worker = method.Body.GetILProcessor();
            writer.InitializeSetters(worker);
            reader.InitializeGetters(worker);
            worker.Emit(OpCodes.Ret);
            expand.Methods.Add(method);
        }

        private static void ProcessAssembly(AssemblyDefinition assembly, IAssemblyResolver resolver, ILogPostProcessor debugger, Writer writer, Reader reader, ref bool failed)
        {
            var ar = assembly.MainModule.AssemblyReferences.FirstOrDefault(reference => reference.Name == Weaver.GEN_TYPE);
            if (ar != null)
            {
                var ad = resolver.Resolve(ar);
                if (ad != null)
                {
                    ProcessProperty(assembly, ad, writer, reader, ref failed);
                }
                else
                {
                    debugger.Error("自动生成网络代码失败: {0}".Format(ar));
                }
            }
            else
            {
                debugger.Error("注册程序集 Astraia.Net.dll 失败");
            }
        }

        private static bool ProcessProperty(AssemblyDefinition assembly, AssemblyDefinition ad, Writer writer, Reader reader, ref bool failed)
        {
            var modified = false;
            foreach (var definition in ad.MainModule.Types.Where(td => td.IsAbstract && td.IsSealed))
            {
                modified |= ProcessSetter(assembly, definition, writer);
                modified |= ProcessGetter(assembly, definition, reader);
            }

            foreach (var definition in ad.MainModule.Types)
            {
                modified |= ProcessMessage(assembly.MainModule, writer, reader, definition, ref failed);
            }

            return modified;
        }

        private static bool ProcessSetter(AssemblyDefinition assembly, TypeDefinition definition, Writer writer)
        {
            var modified = false;
            foreach (var md in definition.Methods)
            {
                if (md.Parameters.Count != 2)
                    continue;

                if (!md.Parameters[0].ParameterType.Is<MemoryWriter>())
                    continue;

                if (!md.ReturnType.Is(typeof(void)))
                    continue;

                if (!md.HasAttribute<ExtensionAttribute>())
                    continue;

                if (md.HasGenericParameters)
                    continue;

                writer.Register(md.Parameters[1].ParameterType, assembly.MainModule.ImportReference(md));
                modified = true;
            }

            return modified;
        }

        private static bool ProcessGetter(AssemblyDefinition assembly, TypeDefinition definition, Reader reader)
        {
            var modified = false;
            foreach (var md in definition.Methods)
            {
                if (md.Parameters.Count != 1)
                    continue;

                if (!md.Parameters[0].ParameterType.Is<MemoryReader>())
                    continue;

                if (md.ReturnType.Is(typeof(void)))
                    continue;

                if (!md.HasAttribute<ExtensionAttribute>())
                    continue;

                if (md.HasGenericParameters)
                    continue;

                reader.Register(md.ReturnType, assembly.MainModule.ImportReference(md));
                modified = true;
            }

            return modified;
        }

        private static bool ProcessMessage(ModuleDefinition module, Writer writer, Reader reader, TypeDefinition definition, ref bool failed)
        {
            var modified = false;
            if (!definition.IsAbstract && !definition.IsInterface && definition.HasInterface(typeof(IMessage)))
            {
                reader.GetFunction(module.ImportReference(definition), ref failed);
                writer.GetFunction(module.ImportReference(definition), ref failed);
                modified = true;
            }

            foreach (var nested in definition.NestedTypes)
            {
                modified |= ProcessMessage(module, writer, reader, nested, ref failed);
            }

            return modified;
        }
    }
}