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

using System;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;
using UnityEngine;
using Object = UnityEngine.Object;
using Astraia.Net;

namespace Astraia.Editor
{
    internal abstract class Stream
    {
        protected readonly Dictionary<TypeReference, MethodReference> methods = new Dictionary<TypeReference, MethodReference>(new Comparer());
        protected readonly Module module;
        protected readonly TypeDefinition expand;
        protected readonly ILogPostProcessor debugger;
        protected readonly AssemblyDefinition assembly;

        protected Stream(AssemblyDefinition assembly, Module module, TypeDefinition expand, ILogPostProcessor debugger)
        {
            this.module = module;
            this.expand = expand;
            this.debugger = debugger;
            this.assembly = assembly;
        }

        public void Register(TypeReference tr, MethodReference mr)
        {
            if (!methods.TryGetValue(tr, out var method) || method.FullName == mr.FullName)
            {
                var imported = assembly.MainModule.ImportReference(tr);
                methods[imported] = mr;
            }
        }

        public MethodReference GetFunction(TypeReference tr, ref bool failed)
        {
            if (methods.TryGetValue(tr, out var mr))
            {
                return mr;
            }

            var reference = assembly.MainModule.ImportReference(tr);
            return Process(reference, ref failed);
        }

        private MethodReference Process(TypeReference tr, ref bool failed)
        {
            if (tr.IsArray)
            {
                if (tr is ArrayType array && array.Rank > 1)
                {
                    debugger.Error("无法为多维数组 {0} 生成代码".Format(tr.Name), tr);
                    failed = true;
                    return null;
                }

                return AddCollection(tr, tr.GetElementType(), nameof(Net.Extensions.ReadArray), ref failed);
            }

            var td = tr.Resolve();
            if (td == null)
            {
                debugger.Error("无法为空类型 {0} 生成代码".Format(tr.Name), tr);
                failed = true;
                return null;
            }

            if (tr.IsByReference) // in ref out
            {
                debugger.Error("无法为反射 {0} 生成代码".Format(tr.Name), tr);
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
                return AddCollection(tr, ((GenericInstanceType)tr).GenericArguments[0], nameof(Net.Extensions.ReadList), ref failed);
            }

            if (td.Is(typeof(HashSet<>)))
            {
                return AddCollection(tr, ((GenericInstanceType)tr).GenericArguments[0], nameof(Net.Extensions.ReadHashSet), ref failed);
            }

            if (tr.IsDerivedFrom<NetworkModule>() || tr.Is<NetworkModule>())
            {
                return AddNetworkModule(tr);
            }

            if (td.IsDerivedFrom<Component>())
            {
                debugger.Error("无法为组件 {0} 生成代码".Format(tr.Name), tr);
                failed = true;
                return null;
            }

            if (tr.Is<Object>())
            {
                debugger.Error("无法为对象 {0} 生成代码".Format(tr.Name), tr);
                failed = true;
                return null;
            }

            if (td.HasGenericParameters)
            {
                debugger.Error("无法为泛型参数 {0} 生成代码".Format(tr.Name), tr);
                failed = true;
                return null;
            }

            if (td.IsInterface)
            {
                debugger.Error("无法为接口 {0} 生成代码".Format(tr.Name), tr);
                failed = true;
                return null;
            }

            if (td.IsAbstract)
            {
                debugger.Error("无法为抽象或泛型 {0} 生成代码".Format(tr.Name), tr);
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
    }

    internal class Writer : Stream
    {
        public Writer(AssemblyDefinition assembly, Module module, TypeDefinition expand, ILogPostProcessor debugger) : base(assembly, module, expand, debugger)
        {
        }

        protected override MethodDefinition AddEnum(TypeReference tr, ref bool failed)
        {
            var md = AddMethod(tr);
            var worker = md.Body.GetILProcessor();
            var mr = GetFunction(tr.Resolve().GetEnumUnderlyingType(), ref failed);
            worker.Emit(OpCodes.Ldarg_0);
            worker.Emit(OpCodes.Ldarg_1);
            worker.Emit(OpCodes.Call, mr);
            worker.Emit(OpCodes.Ret);
            return md;
        }

        protected override MethodDefinition AddSegment(TypeReference tr, ref bool failed)
        {
            return AddCollection(tr, ((GenericInstanceType)tr).GenericArguments[0], nameof(Net.Extensions.WriteArraySegment), ref failed);
        }

        protected override MethodDefinition AddCollection(TypeReference tr, TypeReference element, string name, ref bool failed)
        {
            var md = AddMethod(tr);
            var func = GetFunction(element, ref failed);

            if (func == null)
            {
                debugger.Error("无法为 {0} 生成代码".Format(tr.Name), tr);
                failed = true;
                return md;
            }

            var extensions = assembly.MainModule.ImportReference(typeof(Net.Extensions));
            var mr = Resolve.GetMethod(extensions, assembly, name, debugger, ref failed);

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

            foreach (var field in tr.Resolve().GetFields())
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
            var md = new MethodDefinition("Write{0}".Format(NetworkMessage.Id(tr.FullName)), Weaver.GEN_RAW, module.Import(typeof(void)));
            md.Parameters.Add(new ParameterDefinition("writer", ParameterAttributes.None, module.Import<MemoryWriter>()));
            md.Parameters.Add(new ParameterDefinition("value", ParameterAttributes.None, tr));
            md.Body.InitLocals = true;
            Register(tr, md);
            expand.Methods.Add(md);
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
                var instance = func.MakeGenericInstanceType(tr, type);
                worker.Emit(OpCodes.Newobj, mr.MakeHostInstanceGeneric(assembly.MainModule, instance));
                instance = writer.MakeGenericInstanceType(type);
                worker.Emit(OpCodes.Stsfld, fr.SpecializeField(assembly.MainModule, instance));
            }
        }
    }

    internal class Reader : Stream
    {
        public Reader(AssemblyDefinition assembly, Module module, TypeDefinition expand, ILogPostProcessor debugger) : base(assembly, module, expand, debugger)
        {
        }

        protected override MethodDefinition AddEnum(TypeReference tr, ref bool failed)
        {
            var md = AddMethod(tr);
            var worker = md.Body.GetILProcessor();
            var mr = GetFunction(tr.Resolve().GetEnumUnderlyingType(), ref failed);
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
            worker.Emit(OpCodes.Newobj, module.AddArraySegment.MakeHostInstanceGeneric(assembly.MainModule, generic));
            worker.Emit(OpCodes.Ret);
            return md;
        }

        protected override MethodDefinition AddCollection(TypeReference tr, TypeReference element, string name, ref bool failed)
        {
            var md = AddMethod(tr);
            var func = GetFunction(element, ref failed);

            if (func == null)
            {
                debugger.Error("无法为 {0} 生成代码".Format(tr.Name), tr);
                failed = true;
                return md;
            }

            var extensions = assembly.MainModule.ImportReference(typeof(Net.Extensions));
            var mr = Resolve.GetMethod(extensions, assembly, name, debugger, ref failed);

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
            var mr = module.ReadNetworkModule.MakeGenericInstanceType(assembly.MainModule, tr);
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

                var ctor = Resolve.GetConstructor(tr);
                if (ctor == null)
                {
                    debugger.Error("{0} 不能被反序列化，因为它没有默认的构造函数".Format(tr.Name), tr);
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

            foreach (var field in tr.Resolve().GetFields())
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
                    debugger.Error("{0} 有不受支持的类型".Format(field.Name), field);
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
            var md = new MethodDefinition("Read{0}".Format(NetworkMessage.Id(tr.FullName)), Weaver.GEN_RAW, tr);
            md.Parameters.Add(new ParameterDefinition("reader", ParameterAttributes.None, module.Import<MemoryReader>()));
            md.Body.InitLocals = true;
            Register(tr, md);
            expand.Methods.Add(md);
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
                var instance = func.MakeGenericInstanceType(tr, type);
                worker.Emit(OpCodes.Newobj, mr.MakeHostInstanceGeneric(assembly.MainModule, instance));
                instance = reader.MakeGenericInstanceType(type);
                worker.Emit(OpCodes.Stsfld, fr.SpecializeField(assembly.MainModule, instance));
            }
        }
    }
}