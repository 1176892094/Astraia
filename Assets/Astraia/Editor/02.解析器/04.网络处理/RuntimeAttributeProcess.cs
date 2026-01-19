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
    internal static class RuntimeAttribute
    {
        /// <summary>
        /// 初始化读写流
        /// </summary>
        public static void RuntimeInitializeOnLoad(AssemblyDefinition assembly, Module module, Writer writer, Reader reader, TypeDefinition td)
        {
            var method = new MethodDefinition(nameof(RuntimeInitializeOnLoad), MethodAttributes.Public | MethodAttributes.Static, module.Import(typeof(void)));

            AddRuntimeInitializeOnLoadAttribute(assembly, module, method);

            if (Resolve.IsEditor(assembly))
            {
                AddInitializeOnLoadAttribute(assembly, module, method);
            }

            var worker = method.Body.GetILProcessor();
            writer.InitializeSetters(worker);
            reader.InitializeGetters(worker);
            worker.Emit(OpCodes.Ret);
            td.Methods.Add(method);
        }

        /// <summary>
        /// [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        /// </summary>
        private static void AddRuntimeInitializeOnLoadAttribute(AssemblyDefinition assembly, Module module, MethodDefinition md)
        {
            var ctor = module.RuntimeInitializeOnLoadMethodAttribute.GetConstructors().Last();
            var attribute = new CustomAttribute(assembly.MainModule.ImportReference(ctor));
            attribute.ConstructorArguments.Add(new CustomAttributeArgument(module.Import<RuntimeInitializeLoadType>(), RuntimeInitializeLoadType.BeforeSceneLoad));
            md.CustomAttributes.Add(attribute);
        }

        /// <summary>
        /// [InitializeOnLoadMethod]
        /// </summary>
        private static void AddInitializeOnLoadAttribute(AssemblyDefinition assembly, Module module, MethodDefinition md)
        {
            var ctor = module.InitializeOnLoadMethodAttribute.GetConstructors().First();
            var attribute = new CustomAttribute(assembly.MainModule.ImportReference(ctor));
            md.CustomAttributes.Add(attribute);
        }

        /// <summary>
        /// 在Process中调用
        /// </summary>
        public static bool Process(AssemblyDefinition assembly, IAssemblyResolver resolver, ILog log, Writer writer, Reader reader, ref bool failed)
        {
            ProcessAssembly(assembly, resolver, log, writer, reader, ref failed);
            return ProcessCustomCode(assembly, assembly, writer, reader, ref failed);
        }

        /// <summary>
        /// 处理网络代码
        /// </summary>
        private static void ProcessAssembly(AssemblyDefinition assembly, IAssemblyResolver resolver, ILog log, Writer writer, Reader reader, ref bool failed)
        {
            AssemblyNameReference assemblyRef = null;
            foreach (var reference in assembly.MainModule.AssemblyReferences)
            {
                if (reference.Name == Const.ASSEMBLY)
                {
                    assemblyRef = reference;
                    break;
                }
            }

            if (assemblyRef != null)
            {
                var assemblyDef = resolver.Resolve(assemblyRef);
                if (assemblyDef != null)
                {
                    ProcessCustomCode(assembly, assemblyDef, writer, reader, ref failed);
                }
                else
                {
                    log.Error("自动生成网络代码失败: {0}".Format(assemblyRef));
                }
            }
            else
            {
                log.Error("注册程序集 Astraia.Net.dll 失败");
            }
        }

        /// <summary>
        /// 处理本地代码
        /// </summary>
        private static bool ProcessCustomCode(AssemblyDefinition assembly, AssemblyDefinition assemblyDef, Writer writer, Reader reader, ref bool failed)
        {
            var changed = false;
            foreach (var td in assemblyDef.MainModule.Types)
            {
                if (td.IsAbstract && td.IsSealed)
                {
                    changed |= ProcessSetter(assembly, td, writer);
                    changed |= ProcessGetter(assembly, td, reader);
                }
            }

            foreach (var td in assemblyDef.MainModule.Types)
            {
                changed |= ProcessMessage(assembly.MainModule, writer, reader, td, ref failed);
            }

            return changed;
        }

        /// <summary>
        /// 加载声明的写入器
        /// </summary>
        private static bool ProcessSetter(AssemblyDefinition assembly, TypeDefinition td, Writer writer)
        {
            var change = false;
            foreach (var md in td.Methods)
            {
                if (md.Parameters.Count != 2)
                    continue;

                if (!md.Parameters[0].ParameterType.Is<MemoryWriter>())
                    continue;

                if (!md.ReturnType.Is(typeof(void)))
                    continue;

                if (!md.HasCustomAttribute<ExtensionAttribute>())
                    continue;

                if (md.HasGenericParameters)
                    continue;

                writer.Register(md.Parameters[1].ParameterType, assembly.MainModule.ImportReference(md));
                change = true;
            }

            return change;
        }

        /// <summary>
        /// 加载声明的读取器
        /// </summary>
        private static bool ProcessGetter(AssemblyDefinition assembly, TypeDefinition td, Reader reader)
        {
            var change = false;
            foreach (var md in td.Methods)
            {
                if (md.Parameters.Count != 1)
                    continue;

                if (!md.Parameters[0].ParameterType.Is<MemoryReader>())
                    continue;

                if (md.ReturnType.Is(typeof(void)))
                    continue;

                if (!md.HasCustomAttribute<ExtensionAttribute>())
                    continue;

                if (md.HasGenericParameters)
                    continue;

                reader.Register(md.ReturnType, assembly.MainModule.ImportReference(md));
                change = true;
            }

            return change;
        }

        /// <summary>
        /// 加载读写流的信息
        /// </summary>
        private static bool ProcessMessage(ModuleDefinition module, Writer writer, Reader reader, TypeDefinition td, ref bool failed)
        {
            var change = false;
            if (!td.IsAbstract && !td.IsInterface && td.IsImplement<IMessage>())
            {
                reader.GetFunction(module.ImportReference(td), ref failed);
                writer.GetFunction(module.ImportReference(td), ref failed);
                change = true;
            }

            foreach (var nested in td.NestedTypes) // 嵌套类型
            {
                change |= ProcessMessage(module, writer, reader, nested, ref failed);
            }

            return change;
        }
    }
}