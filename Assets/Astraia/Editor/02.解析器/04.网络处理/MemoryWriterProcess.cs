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
using Astraia.Net;
using Mono.Cecil;
using Mono.Cecil.Cil;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Astraia.Editor
{
    internal class Writer
    {
        private readonly Dictionary<TypeReference, MethodReference> methods = new Dictionary<TypeReference, MethodReference>(new Comparer());
        private readonly Module module;
        private readonly Logger logger;
        private readonly TypeDefinition generate;
        private readonly AssemblyDefinition assembly;

        public Writer(AssemblyDefinition assembly, Module module, TypeDefinition generate, Logger logger)
        {
            this.logger = logger;
            this.module = module;
            this.assembly = assembly;
            this.generate = generate;
        }

        public void Register(TypeReference tr, MethodReference md)
        {
            var imported = assembly.MainModule.ImportReference(tr);
            methods[imported] = md;
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
                if (tr is ArrayType { Rank: > 1 })
                {
                    logger.Error($"无法为多维数组 {tr.Name} 生成写入器", tr);
                    return null;
                }

                return AddCollection(tr, tr.GetElementType(), nameof(Net.Extensions.SetArray), ref failed);
            }

            var td = tr.Resolve();
            if (td == null)
            {
                logger.Error($"无法为空类型 {tr.Name} 生成写入器", tr);
                return null;
            }

            if (tr.IsByReference)
            {
                logger.Error($"无法为反射 {tr.Name} 生成写入器", tr);
                return null;
            }

            if (td.IsEnum)
            {
                return AddEnum(tr, ref failed);
            }

            if (tr.Is(typeof(ArraySegment<>)))
            {
                return AddArraySegment(tr, ref failed);
            }

            if (tr.Is(typeof(List<>)))
            {
                var genericInstance = (GenericInstanceType)tr;
                var elementType = genericInstance.GenericArguments[0];
                return AddCollection(tr, elementType, nameof(Net.Extensions.SetList), ref failed);
            }

            if (tr.IsDerivedFrom<NetworkAgent>() || tr.Is<NetworkAgent>())
            {
                return AddNetworkAgent(tr);
            }

            if (td.IsDerivedFrom<Component>())
            {
                logger.Error($"无法为组件 {tr.Name} 生成写入器", tr);
                return null;
            }

            if (tr.Is<Object>())
            {
                logger.Error($"无法为对象 {tr.Name} 生成写入器", tr);
                return null;
            }

            if (tr.Is<ScriptableObject>())
            {
                logger.Error($"无法为可视化脚本 {tr.Name} 生成写入器", tr);
                return null;
            }

            if (td.HasGenericParameters)
            {
                logger.Error($"无法为泛型参数 {tr.Name} 生成写入器", tr);
                return null;
            }

            if (td.IsInterface)
            {
                logger.Error($"无法为接口 {tr.Name} 生成写入器", tr);
                return null;
            }

            if (td.IsAbstract)
            {
                logger.Error($"无法为抽象或泛型 {tr.Name} 生成写入器", tr);
                return null;
            }

            return AddActivator(tr, ref failed);
        }

        private MethodDefinition AddEnum(TypeReference tr, ref bool failed)
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
        
        private MethodDefinition AddArraySegment(TypeReference tr, ref bool failed)
        {
            var genericInstance = (GenericInstanceType)tr;
            var elementType = genericInstance.GenericArguments[0];
            return AddCollection(tr, elementType, nameof(Net.Extensions.SetArraySegment), ref failed);
        }

        private MethodDefinition AddCollection(TypeReference tr, TypeReference element, string name, ref bool failed)
        {
            var md = AddMethod(tr);
            var func = GetFunction(element, ref failed);

            if (func == null)
            {
                logger.Error($"无法为 {tr} 生成写入器", tr);
                failed = true;
                return md;
            }
            
            var extensions = assembly.MainModule.ImportReference(typeof(Net.Extensions));
            var mr = Resolve.GetMethod(extensions, assembly, logger, name, ref failed);

            var method = new GenericInstanceMethod(mr);
            method.GenericArguments.Add(element);
            var worker = md.Body.GetILProcessor();
            worker.Emit(OpCodes.Ldarg_0);
            worker.Emit(OpCodes.Ldarg_1);
            worker.Emit(OpCodes.Call, method);
            worker.Emit(OpCodes.Ret);
            return md;
        }

        private MethodReference AddNetworkAgent(TypeReference tr)
        {
            if (!methods.TryGetValue(module.Import<NetworkAgent>(), out var mr))
            {
                throw new MissingMethodException("获取 NetworkAgent 方法丢失");
            }

            Register(tr, mr);
            return mr;
        }

        private MethodDefinition AddActivator(TypeReference tr, ref bool failed)
        {
            var md = AddMethod(tr);
            var worker = md.Body.GetILProcessor();
            var td = tr.Resolve();

            if (!td.IsValueType)
            {
                AddNullCheck(worker, ref failed);
            }

            if (!AddFields(tr, worker, ref failed))
            {
                return null;
            }

            worker.Emit(OpCodes.Ret);
            return md;
        }

        private MethodDefinition AddMethod(TypeReference tr)
        {
            var md = new MethodDefinition($"Write{NetworkMessage.Id(tr.FullName)}", Const.RAW_ATTRS, module.Import(typeof(void)));
            md.Parameters.Add(new ParameterDefinition("writer", ParameterAttributes.None, module.Import<MemoryWriter>()));
            md.Parameters.Add(new ParameterDefinition("value", ParameterAttributes.None, tr));
            md.Body.InitLocals = true;
            Register(tr, md);
            generate.Methods.Add(md);
            return md;
        }

        private void AddNullCheck(ILProcessor worker, ref bool failed)
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

        private bool AddFields(TypeReference tr, ILProcessor worker, ref bool failed)
        {
            foreach (var field in tr.Resolve().FindPublicFields())
            {
                var mr = GetFunction(field.FieldType, ref failed);
                if (mr == null)
                {
                    return false;
                }

                worker.Emit(OpCodes.Ldarg_0);
                worker.Emit(OpCodes.Ldarg_1);
                worker.Emit(OpCodes.Ldfld, assembly.MainModule.ImportReference(field));
                worker.Emit(OpCodes.Call, mr);
            }

            return true;
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
}