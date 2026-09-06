// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-09-02 16:09:48
// # Recently: 2026-09-06 15:32:39
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Mono.Cecil;
using Mono.Cecil.Cil;
using UnityEngine;
using Object = UnityEngine.Object;
using Astraia.Net;

namespace Astraia
{
    internal static class StreamProcess
    {
        public static bool Process(AssemblyDefinition assembly, IAssemblyResolver resolver, AssemblyDebugger Log, Writer writer, Reader reader, ref bool failed)
        {
            ProcessAssembly(assembly, resolver, Log, writer, reader, ref failed);
            return ProcessProperty(assembly, assembly, Log, writer, reader, ref failed);
        }

        public static void Processed(AssemblyDefinition assembly, Module module, Writer writer, Reader reader, TypeDefinition create)
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
            create.Methods.Add(method);
        }

        private static void ProcessAssembly(AssemblyDefinition assembly, IAssemblyResolver resolver, AssemblyDebugger Log, Writer writer, Reader reader, ref bool failed)
        {
            var ar = assembly.MainModule.AssemblyReferences.FirstOrDefault(r => r.Name == Weaver.WEAVER);
            AssemblyDefinition network = null;
            if (ar == null)
            {
                Log.Error("没有找到 Astraia.Net 程序集");
            }
            else
            {
                network = resolver.Resolve(ar);
                if (network == null)
                {
                    Log.Error("网络程序集解析失败: {0}".Format(ar));
                }
                else
                {
                    ProcessExtensions(assembly.MainModule, network, writer, reader);
                }
            }

            // Astraia.dll 插件里的 L.读写类 提供基础序列化方法，
            // 需要单独扫描，避免只处理 Astraia.Net 程序集里的 Unity 专用方法。
            var core = ResolveAssembly(assembly, resolver, "Astraia");
            core ??= ReadPlugin(resolver);
            if (core != null)
            {
                ProcessExtensions(assembly.MainModule, core, writer, reader);
            }

            if (network != null)
            {
                ProcessMessages(assembly.MainModule, network, writer, reader, ref failed);
            }
        }

        private static AssemblyDefinition ResolveAssembly(AssemblyDefinition assembly, IAssemblyResolver resolver, string name)
        {
            var reference = assembly.MainModule.AssemblyReferences.FirstOrDefault(r => r.Name == name);
            if (reference != null)
            {
                return resolver.Resolve(reference);
            }

            try
            {
                return resolver.Resolve(new AssemblyNameReference(name, new Version(0, 0, 0, 0)));
            }
            catch
            {
                return null;
            }
        }

        private static AssemblyDefinition ReadPlugin(IAssemblyResolver resolver)
        {
#if UNITY_6000_4_OR_NEWER
            var location = typeof(MemoryReader).Assembly.GetLoadedAssemblyPath();
#else
            var location = typeof(MemoryReader).Assembly.Location;
#endif

            if (string.IsNullOrEmpty(location) || !File.Exists(location))
            {
                return null;
            }

            using var stream = new FileStream(location, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            return AssemblyDefinition.ReadAssembly(stream, new ReaderParameters { AssemblyResolver = resolver, ReadingMode = ReadingMode.Immediate });
        }

        private static bool ProcessProperty(AssemblyDefinition assembly, AssemblyDefinition network, AssemblyDebugger Log, Writer writer, Reader reader, ref bool failed)
        {
            var modified = ProcessExtensions(assembly.MainModule, network, writer, reader);
            modified |= ProcessMessages(assembly.MainModule, network, writer, reader, ref failed);
            return modified;
        }

        private static bool ProcessExtensions(ModuleDefinition module, AssemblyDefinition source, Writer writer, Reader reader)
        {
            var modified = false;
            foreach (var td in source.MainModule.Types.Where(td => td.IsAbstract && td.IsSealed))
            {
                modified |= ProcessWriter(module, td, writer);
                modified |= ProcessReader(module, td, reader);
            }

            return modified;
        }

        private static bool ProcessMessages(ModuleDefinition module, AssemblyDefinition source, Writer writer, Reader reader, ref bool failed)
        {
            var modified = false;
            foreach (var td in source.MainModule.Types)
            {
                modified |= ProcessMessage(module, td, writer, reader, ref failed);
            }

            return modified;
        }

        private static bool ProcessWriter(ModuleDefinition module, TypeDefinition td, Writer writer)
        {
            var modified = false;
            foreach (var md in td.Methods)
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
                {
                    writer.RegisterGeneric(md.Name, module.ImportReference(md));
                    modified = true;
                    continue;
                }

                writer.Register(md.Parameters[1].ParameterType, module.ImportReference(md));
                modified = true;
            }

            return modified;
        }

        private static bool ProcessReader(ModuleDefinition module, TypeDefinition td, Reader reader)
        {
            var modified = false;
            foreach (var md in td.Methods)
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
                {
                    reader.RegisterGeneric(md.Name, module.ImportReference(md));
                    modified = true;
                    continue;
                }

                reader.Register(md.ReturnType, module.ImportReference(md));
                modified = true;
            }

            return modified;
        }

        private static bool ProcessMessage(ModuleDefinition module, TypeDefinition td, Writer writer, Reader reader, ref bool failed)
        {
            var modified = false;
            if (!td.IsAbstract && !td.IsInterface && td.HasInterface(typeof(IMessage)))
            {
                reader.GetFunction(module.ImportReference(td), ref failed);
                writer.GetFunction(module.ImportReference(td), ref failed);
                modified = true;
            }

            foreach (var nested in td.NestedTypes) //嵌套类型
            {
                modified |= ProcessMessage(module, nested, writer, reader, ref failed);
            }

            return modified;
        }
    }

    internal abstract class Stream
    {
        protected readonly Dictionary<TypeReference, MethodReference> methods = new Dictionary<TypeReference, MethodReference>(new Comparer());
        private readonly Dictionary<string, MethodReference> genericMethods = new Dictionary<string, MethodReference>();
        protected readonly Module module;
        protected readonly TypeDefinition create;
        protected readonly AssemblyDebugger Log;
        protected readonly AssemblyDefinition assembly;

        protected Stream(AssemblyDefinition assembly, Module module, TypeDefinition create, AssemblyDebugger Log)
        {
            this.Log = Log;
            this.module = module;
            this.create = create;
            this.assembly = assembly;
        }

        public void Register(TypeReference tr, MethodReference mr)
        {
            if (!methods.TryGetValue(tr, out var method) || method.FullName == mr.FullName)
            {
                methods[assembly.MainModule.ImportReference(tr)] = mr;
            }
        }

        public void RegisterGeneric(string name, MethodReference mr)
        {
            genericMethods[name] = mr;
        }

        public MethodReference GetGenericMethod(string name)
        {
            return genericMethods.TryGetValue(name, out var method) ? method : null;
        }

        public MethodReference GetFunction(TypeReference tr, ref bool failed)
        {
            return methods.TryGetValue(tr, out var mr) ? mr : Process(assembly.MainModule.ImportReference(tr), ref failed);
        }

        private MethodReference Process(TypeReference tr, ref bool failed)
        {
            if (tr.IsArray)
            {
                if (tr is ArrayType array && array.Rank > 1)
                {
                    Log.Error("无法为多维数组 {0} 生成代码".Format(tr.Name), tr);
                    failed = true;
                    return null;
                }

                return AddCollection(tr, tr.GetElementType(), "Array", ref failed);
            }

            var td = tr.Resolve();
            if (td == null)
            {
                Log.Error("无法为空类型 {0} 生成代码".Format(tr.Name), tr);
                failed = true;
                return null;
            }

            if (tr.IsByReference) // ref and out
            {
                Log.Error("无法为引用 {0} 生成代码".Format(tr.Name), tr);
                failed = true;
                return null;
            }

            if (td.IsEnum)
            {
                return AddEnum(tr, ref failed);
            }

            if (td.Is(typeof(ArraySegment<>)))
            {
                return AddSegment(tr, ref failed);
            }

            if (td.Is(typeof(List<>)))
            {
                return AddCollection(tr, ((GenericInstanceType)tr).GenericArguments[0], "List", ref failed);
            }

            if (td.Is(typeof(HashSet<>)))
            {
                return AddCollection(tr, ((GenericInstanceType)tr).GenericArguments[0], "HashSet", ref failed);
            }

            if (tr.IsSubclassOf<NetworkModule>() || tr.Is<NetworkModule>())
            {
                return AddNetworkModule(tr);
            }

            if (td.IsSubclassOf<Component>())
            {
                Log.Error("无法为组件 {0} 生成代码".Format(tr.Name), tr);
                failed = true;
                return null;
            }

            if (tr.Is<Object>())
            {
                Log.Error("无法为对象 {0} 生成代码".Format(tr.Name), tr);
                failed = true;
                return null;
            }

            if (td.HasGenericParameters)
            {
                Log.Error("无法为泛型参数 {0} 生成代码".Format(tr.Name), tr);
                failed = true;
                return null;
            }

            if (td.IsInterface)
            {
                Log.Error("无法为接口 {0} 生成代码".Format(tr.Name), tr);
                failed = true;
                return null;
            }

            if (td.IsAbstract)
            {
                Log.Error("无法为抽象或泛型 {0} 生成代码".Format(tr.Name), tr);
                failed = true;
                return null;
            }

            return AddActivator(tr, ref failed);
        }

        protected abstract MethodReference AddNetworkModule(TypeReference tr);
        protected abstract MethodDefinition AddCollection(TypeReference tr, TypeReference element, string name, ref bool failed);
        protected abstract MethodDefinition AddEnum(TypeReference tr, ref bool failed);
        protected abstract MethodDefinition AddSegment(TypeReference tr, ref bool failed);
        protected abstract MethodDefinition AddActivator(TypeReference tr, ref bool failed);

        private class Comparer : IEqualityComparer<TypeReference>
        {
            public bool Equals(TypeReference x, TypeReference y)
            {
                return x?.FullName == y?.FullName;
            }

            public int GetHashCode(TypeReference obj)
            {
                return obj.FullName.GetHashCode();
            }
        }
    }

    internal class Writer : Stream
    {
        public Writer(AssemblyDefinition assembly, Module module, TypeDefinition create, AssemblyDebugger debugger) : base(assembly, module, create, debugger) { }

        protected override MethodDefinition AddEnum(TypeReference tr, ref bool failed)
        {
            var md = AddMethod(tr);
            var worker = md.Body.GetILProcessor();
            var mr = GetFunction(tr.Resolve().GetField().FieldType, ref failed);
            worker.Emit(OpCodes.Ldarg_0);
            worker.Emit(OpCodes.Ldarg_1);
            worker.Emit(OpCodes.Call, mr);
            worker.Emit(OpCodes.Ret);
            return md;
        }

        protected override MethodDefinition AddSegment(TypeReference tr, ref bool failed)
        {
            return AddCollection(tr, ((GenericInstanceType)tr).GenericArguments[0], "ArraySegment", ref failed);
        }

        protected override MethodDefinition AddCollection(TypeReference tr, TypeReference element, string name, ref bool failed)
        {
            var md = AddMethod(tr);
            var func = GetFunction(element, ref failed);

            if (func == null)
            {
                Log.Error("无法为 {0} 生成代码".Format(tr.Name), tr);
                failed = true;
                return md;
            }

            var mr = GetGenericMethod("Write" + name);
            if (mr == null)
            {
                Log.Error("没有找到 {0} 的拓展方法".Format("Write" + name));
                failed = true;
                return md;
            }

            var method = new GenericInstanceMethod(mr);
            method.GenericArguments.Add(element);
            var worker = md.Body.GetILProcessor();
            worker.Emit(OpCodes.Ldarg_0);
            worker.Emit(OpCodes.Ldarg_1);
            worker.Emit(OpCodes.Call, method);
            worker.Emit(OpCodes.Ret);
            return md;
        }

        protected override MethodReference AddNetworkModule(TypeReference tr)
        {
            if (methods.TryGetValue(module.Import<NetworkModule>(), out var mr))
            {
                Register(tr, mr);
                return mr;
            }

            throw new MissingMethodException("获取 NetworkModule 方法丢失");
        }

        protected override MethodDefinition AddActivator(TypeReference tr, ref bool failed)
        {
            var md = AddMethod(tr);
            var worker = md.Body.GetILProcessor();
            var td = tr.Resolve();

            if (!td.IsValueType)
            {
                var nop = worker.Create(OpCodes.Nop);
                worker.Emit(OpCodes.Ldarg_1);
                worker.Emit(OpCodes.Brtrue, nop);
                worker.Emit(OpCodes.Ldarg_0);
                worker.Emit(OpCodes.Ldc_I4_0);
                worker.Emit(OpCodes.Call, GetFunction(module.Import<bool>(), ref failed));
                worker.Emit(OpCodes.Ret);
                worker.Append(nop);

                worker.Emit(OpCodes.Ldarg_0);
                worker.Emit(OpCodes.Ldc_I4_1);
                worker.Emit(OpCodes.Call, GetFunction(module.Import<bool>(), ref failed));
            }

            foreach (var field in tr.GetFields())
            {
                var mr = GetFunction(field.FieldType, ref failed);
                if (mr == null)
                {
                    return null;
                }

                worker.Emit(OpCodes.Ldarg_0);
                worker.Emit(OpCodes.Ldarg_1);
                worker.Emit(OpCodes.Ldfld, assembly.MainModule.ImportReference(field));
                worker.Emit(OpCodes.Call, mr);
            }

            worker.Emit(OpCodes.Ret);
            return md;
        }

        private MethodDefinition AddMethod(TypeReference tr)
        {
            var md = new MethodDefinition("Write{0}".Format(NetworkMessage.Id(tr.FullName)), Weaver.GEN_V2, module.Import(typeof(void)));
            md.Parameters.Add(new ParameterDefinition("writer", ParameterAttributes.None, module.Import<MemoryWriter>()));
            md.Parameters.Add(new ParameterDefinition("value", ParameterAttributes.None, tr));
            md.Body.InitLocals = true;
            Register(tr, md);
            create.Methods.Add(md);
            return md;
        }

        internal void InitializeSetters(ILProcessor worker)
        {
            var main = assembly.MainModule;
            var writer = main.ImportReference(typeof(Writer<>));
            var func = main.ImportReference(typeof(Action<,>));
            var tr = main.ImportReference(typeof(MemoryWriter));
            var fr = main.ImportReference(typeof(Writer<>).GetField(nameof(Writer<object>.writer)));
            var mr = main.ImportReference(typeof(Action<,>).GetConstructors()[0]);
            foreach (var (type, method) in methods)
            {
                worker.Emit(OpCodes.Ldnull);
                worker.Emit(OpCodes.Ldftn, method);
                worker.Emit(OpCodes.Newobj, mr.GenericInstance(main, func.MakeGeneric(tr, type)));
                worker.Emit(OpCodes.Stsfld, fr.GenericField(main, writer.MakeGeneric(type)));
            }
        }
    }

    internal class Reader : Stream
    {
        public Reader(AssemblyDefinition assembly, Module module, TypeDefinition create, AssemblyDebugger debugger) : base(assembly, module, create, debugger) { }

        protected override MethodDefinition AddEnum(TypeReference tr, ref bool failed)
        {
            var md = AddMethod(tr);
            var worker = md.Body.GetILProcessor();
            var mr = GetFunction(tr.Resolve().GetField().FieldType, ref failed);
            worker.Emit(OpCodes.Ldarg_0);
            worker.Emit(OpCodes.Call, mr);
            worker.Emit(OpCodes.Ret);
            return md;
        }

        protected override MethodDefinition AddSegment(TypeReference tr, ref bool failed)
        {
            var generic = (GenericInstanceType)tr;
            var element = generic.GenericArguments[0];
            var md = AddMethod(tr);
            var worker = md.Body.GetILProcessor();
            worker.Emit(OpCodes.Ldarg_0);
            worker.Emit(OpCodes.Call, GetFunction(new ArrayType(element), ref failed));
            worker.Emit(OpCodes.Newobj, module.AddArraySegment.GenericInstance(assembly.MainModule, generic));
            worker.Emit(OpCodes.Ret);
            return md;
        }

        protected override MethodDefinition AddCollection(TypeReference tr, TypeReference element, string name, ref bool failed)
        {
            var md = AddMethod(tr);
            var func = GetFunction(element, ref failed);

            if (func == null)
            {
                Log.Error("无法为 {0} 生成代码".Format(tr.Name), tr);
                failed = true;
                return md;
            }

            var mr = GetGenericMethod("Read" + name);
            if (mr == null)
            {
                Log.Error("没有找到 {0} 的拓展方法".Format("Read" + name));
                failed = true;
                return md;
            }

            var method = new GenericInstanceMethod(mr);
            method.GenericArguments.Add(element);
            var worker = md.Body.GetILProcessor();
            worker.Emit(OpCodes.Ldarg_0);
            worker.Emit(OpCodes.Call, method);
            worker.Emit(OpCodes.Ret);
            return md;
        }

        protected override MethodReference AddNetworkModule(TypeReference tr)
        {
            var mr = module.ReadNetworkModule.MakeGeneric(assembly.MainModule, tr);
            Register(tr, mr);
            return mr;
        }

        protected override MethodDefinition AddActivator(TypeReference tr, ref bool failed)
        {
            var md = AddMethod(tr);
            md.Body.Variables.Add(new VariableDefinition(tr));
            var worker = md.Body.GetILProcessor();
            var td = tr.Resolve();

            if (!td.IsValueType)
            {
                var nop = worker.Create(OpCodes.Nop);
                worker.Emit(OpCodes.Ldarg_0);
                worker.Emit(OpCodes.Call, GetFunction(module.Import<bool>(), ref failed));
                worker.Emit(OpCodes.Brtrue, nop);
                worker.Emit(OpCodes.Ldnull);
                worker.Emit(OpCodes.Ret);
                worker.Append(nop);

                var ctor = tr.GetConstructor();
                if (ctor == null)
                {
                    Log.Error("{0} 不能被反序列化，因为它没有默认的构造函数".Format(tr.Name), tr);
                    failed = true;
                }
                else
                {
                    worker.Emit(OpCodes.Newobj, assembly.MainModule.ImportReference(ctor));
                    worker.Emit(OpCodes.Stloc_0);
                }
            }
            else
            {
                worker.Emit(OpCodes.Ldloca, 0);
                worker.Emit(OpCodes.Initobj, tr);
            }

            foreach (var field in tr.GetFields())
            {
                worker.Emit(tr.IsValueType ? OpCodes.Ldloca : OpCodes.Ldloc, 0);
                var mr = GetFunction(field.FieldType, ref failed);
                if (mr != null)
                {
                    worker.Emit(OpCodes.Ldarg_0);
                    worker.Emit(OpCodes.Call, mr);
                }
                else
                {
                    Log.Error("{0} 有不受支持的类型".Format(field.Name), field);
                    failed = true;
                }

                worker.Emit(OpCodes.Stfld, assembly.MainModule.ImportReference(field));
            }

            worker.Emit(OpCodes.Ldloc_0);
            worker.Emit(OpCodes.Ret);
            return md;
        }

        private MethodDefinition AddMethod(TypeReference tr)
        {
            var md = new MethodDefinition("Read{0}".Format(NetworkMessage.Id(tr.FullName)), Weaver.GEN_V2, tr);
            md.Parameters.Add(new ParameterDefinition("reader", ParameterAttributes.None, module.Import<MemoryReader>()));
            md.Body.InitLocals = true;
            Register(tr, md);
            create.Methods.Add(md);
            return md;
        }

        internal void InitializeGetters(ILProcessor worker)
        {
            var main = assembly.MainModule;
            var reader = main.ImportReference(typeof(Reader<>));
            var func = main.ImportReference(typeof(Func<,>));
            var tr = main.ImportReference(typeof(MemoryReader));
            var fr = main.ImportReference(typeof(Reader<>).GetField(nameof(Reader<object>.reader)));
            var mr = main.ImportReference(typeof(Func<,>).GetConstructors()[0]);
            foreach (var (type, method) in methods)
            {
                worker.Emit(OpCodes.Ldnull);
                worker.Emit(OpCodes.Ldftn, method);
                worker.Emit(OpCodes.Newobj, mr.GenericInstance(main, func.MakeGeneric(tr, type)));
                worker.Emit(OpCodes.Stsfld, fr.GenericField(main, reader.MakeGeneric(type)));
            }
        }
    }
}