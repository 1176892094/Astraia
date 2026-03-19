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
using System.Linq;
using Astraia.Net;
using Mono.Cecil;
using Mono.Cecil.Cil;
using UnityEngine;

namespace Astraia.Editor
{
    using FieldDefinitionPair = KeyValuePair<List<FieldDefinition>, Dictionary<FieldDefinition, FieldDefinition>>;

    internal class SyncVarProcess
    {
        private readonly Module module;
        private readonly SyncVarAccess access;
        private readonly ILogPostProcessor debugger;
        private readonly AssemblyDefinition assembly;

        public SyncVarProcess(AssemblyDefinition assembly, SyncVarAccess access, Module module, ILogPostProcessor debugger)
        {
            this.access = access;
            this.module = module;
            this.debugger = debugger;
            this.assembly = assembly;
        }

        public void AddHook(FieldDefinition syncVar, ILProcessor worker, MethodDefinition func)
        {
            worker.Emit(func.IsStatic ? OpCodes.Ldnull : OpCodes.Ldarg_0);
            MethodReference method;
            if (func.DeclaringType.HasGenericParameters)
            {
                var param = new TypeReference[func.DeclaringType.GenericParameters.Count];
                for (int i = 0; i < param.Length; i++)
                {
                    param[i] = func.DeclaringType.GenericParameters[i];
                }

                method = func.GenericInstance(func.Module, func.DeclaringType.MakeGeneric(param));
            }
            else
            {
                method = func;
            }

            if (func.IsVirtual)
            {
                worker.Emit(OpCodes.Dup);
                worker.Emit(OpCodes.Ldvirtftn, method);
            }
            else
            {
                worker.Emit(OpCodes.Ldftn, method);
            }

            var main = assembly.MainModule;
            var instance = main.ImportReference(typeof(Action<,>)).MakeGeneric(syncVar.FieldType, syncVar.FieldType);
            worker.Emit(OpCodes.Newobj, module.SyncVarHook.GenericInstance(main, instance));
        }

        public MethodDefinition GetHook(TypeDefinition td, FieldDefinition syncVar, ref bool failed)
        {
            var name = (string)syncVar.GetAttribute<SyncVarAttribute>().GetArgument();
            if (name != null)
            {
                var methods = td.GetMethods(name);
                var results = methods.Where(method => method.Parameters.Count == 2).ToList();
                if (results.Count == 0)
                {
                    debugger.Error("{0} 缺少参数 void {1}({2} oldValue, {2} newValue)".Format(syncVar.Name, name, syncVar.FieldType), syncVar);
                    failed = true;
                    return null;
                }

                foreach (var result in results)
                {
                    var oldValue = result.Parameters[0].ParameterType.FullName == syncVar.FieldType.FullName;
                    var newValue = result.Parameters[1].ParameterType.FullName == syncVar.FieldType.FullName;
                    if (oldValue && newValue)
                    {
                        return result;
                    }
                }

                debugger.Error("{0} 参数错误 void {1}({2} oldValue, {2} newValue)".Format(syncVar.Name, name, syncVar.FieldType), syncVar);
                failed = true;
            }

            return null;
        }

        public FieldDefinitionPair ProcessSyncVars(TypeDefinition td, ref bool failed)
        {
            var syncVars = new List<FieldDefinition>();
            var syncVarIds = new Dictionary<FieldDefinition, FieldDefinition>();
            int dirtyBits = access.GetSyncVar(td.BaseType.FullName);

            foreach (var fd in td.Fields)
            {
                if (!fd.HasAttribute<SyncVarAttribute>())
                {
                    continue;
                }

                if ((fd.Attributes & FieldAttributes.Static) != 0)
                {
                    debugger.Error("{0} 不能是静态字段。".Format(fd.Name), fd);
                    failed = true;
                    continue;
                }

                if (fd.FieldType.IsGenericParameter)
                {
                    debugger.Error("{0} 不能用泛型参数。".Format(fd.Name), fd);
                    failed = true;
                    continue;
                }

                if (fd.FieldType.IsArray)
                {
                    debugger.Error("{0} 不能使用数组。".Format(fd.Name), fd);
                    failed = true;
                    continue;
                }

                syncVars.Add(fd);

                ProcessSyncVar(td, fd, syncVarIds, 1L << dirtyBits, ref failed);
                dirtyBits += 1;

                if (dirtyBits > Weaver.BIT_COUNT)
                {
                    debugger.Error("{0} 网络变量数量大于 {1}。".Format(fd.Name, Weaver.BIT_COUNT), td);
                    failed = true;
                }
            }

            foreach (var fd in syncVarIds.Values)
            {
                td.Fields.Add(fd);
            }

            int parentSyncVarCount = access.GetSyncVar(td.BaseType.FullName);
            access.SetSyncVar(td.FullName, parentSyncVarCount + syncVars.Count);
            return new FieldDefinitionPair(syncVars, syncVarIds);
        }

        private void ProcessSyncVar(TypeDefinition td, FieldDefinition fd, Dictionary<FieldDefinition, FieldDefinition> syncVarIds, long dirtyBits, ref bool failed)
        {
            FieldDefinition objectId = null;
            if (fd.FieldType.Is<NetworkModule>() || fd.FieldType.IsSubclassOf<NetworkModule>())
            {
                objectId = new FieldDefinition("{0}ID".Format(fd.Name), FieldAttributes.Family, module.Import<NetworkVariable>());
                syncVarIds[fd] = objectId;
                objectId.DeclaringType = td;
            }
            else if (fd.FieldType.Is<GameObject>() || fd.FieldType.Is<NetworkEntity>())
            {
                objectId = new FieldDefinition("{0}ID".Format(fd.Name), FieldAttributes.Family, module.Import<uint>());
                syncVarIds[fd] = objectId;
                objectId.DeclaringType = td;
            }

            var getter = GenerateSyncVarGetter(fd, fd.Name, objectId);
            var setter = GenerateSyncVarSetter(td, fd, fd.Name, dirtyBits, objectId, ref failed);

            var property = new PropertyDefinition("{0}Var".Format(fd.Name), PropertyAttributes.None, fd.FieldType)
            {
                GetMethod = getter,
                SetMethod = setter
            };

            td.Methods.Add(getter);
            td.Methods.Add(setter);
            td.Properties.Add(property);

            access.setter[fd] = setter;

            if (fd.FieldType.Support())
            {
                access.getter[fd] = getter;
            }
        }

        /// <summary>
        /// 生成SyncVer的Getter
        /// </summary>
        /// <param name="fd"></param>
        /// <param name="name"></param>
        /// <param name="fieldId"></param>
        /// <returns></returns>
        private MethodDefinition GenerateSyncVarGetter(FieldDefinition fd, string name, FieldDefinition fieldId)
        {
            var get = new MethodDefinition("get_{0}Var".Format(name), Weaver.GEN_SYNC, fd.FieldType);

            var worker = get.Body.GetILProcessor();

            var fr = fd.DeclaringType.HasGenericParameters ? fd.MakeGeneric() : fd;

            FieldReference fieldReference = null;
            if (fieldId != null)
            {
                fieldReference = fieldId.DeclaringType.HasGenericParameters ? fieldId.MakeGeneric() : fieldId;
            }

            if (fd.FieldType.Is<GameObject>())
            {
                worker.Emit(OpCodes.Ldarg_0);
                worker.Emit(OpCodes.Ldarg_0);
                worker.Emit(OpCodes.Ldfld, fieldReference);
                worker.Emit(OpCodes.Ldarg_0);
                worker.Emit(OpCodes.Ldflda, fr);
                worker.Emit(OpCodes.Call, module.GetSyncVarGameObject);
                worker.Emit(OpCodes.Ret);
            }
            else if (fd.FieldType.Is<NetworkEntity>())
            {
                worker.Emit(OpCodes.Ldarg_0);
                worker.Emit(OpCodes.Ldarg_0);
                worker.Emit(OpCodes.Ldfld, fieldReference);
                worker.Emit(OpCodes.Ldarg_0);
                worker.Emit(OpCodes.Ldflda, fr);
                worker.Emit(OpCodes.Call, module.GetSyncVarNetworkEntity);
                worker.Emit(OpCodes.Ret);
            }
            else if (fd.FieldType.IsSubclassOf<NetworkModule>() || fd.FieldType.Is<NetworkModule>())
            {
                worker.Emit(OpCodes.Ldarg_0);
                worker.Emit(OpCodes.Ldarg_0);
                worker.Emit(OpCodes.Ldfld, fieldReference);
                worker.Emit(OpCodes.Ldarg_0);
                worker.Emit(OpCodes.Ldflda, fr);
                worker.Emit(OpCodes.Call, module.GetSyncVarNetworkModule.GenericInstance(assembly.MainModule, fd.FieldType));
                worker.Emit(OpCodes.Ret);
            }
            else
            {
                worker.Emit(OpCodes.Ldarg_0);
                worker.Emit(OpCodes.Ldfld, fr);
                worker.Emit(OpCodes.Ret);
            }

            get.Body.Variables.Add(new VariableDefinition(fd.FieldType));
            get.Body.InitLocals = true;
            get.SemanticsAttributes = MethodSemanticsAttributes.Getter;
            return get;
        }

        /// <summary>
        /// 生成SyncVar的Setter
        /// </summary>
        /// <param name="td"></param>
        /// <param name="fd"></param>
        /// <param name="name"></param>
        /// <param name="dirtyBit"></param>
        /// <param name="fieldId"></param>
        /// <param name="failed"></param>
        /// <returns></returns>
        private MethodDefinition GenerateSyncVarSetter(TypeDefinition td, FieldDefinition fd, string name, long dirtyBit, FieldDefinition fieldId, ref bool failed)
        {
            var set = new MethodDefinition("set_{0}Var".Format(name), Weaver.GEN_SYNC, module.Import(typeof(void)));

            var worker = set.Body.GetILProcessor();
            var fr = fd.DeclaringType.HasGenericParameters ? fd.MakeGeneric() : fd;

            FieldReference fieldReference = null;
            if (fieldId != null)
            {
                fieldReference = fieldId.DeclaringType.HasGenericParameters ? fieldId.MakeGeneric() : fieldId;
            }

            var endOfMethod = worker.Create(OpCodes.Nop);

            worker.Emit(OpCodes.Ldarg_0);
            worker.Emit(OpCodes.Ldarg_1);
            worker.Emit(OpCodes.Ldarg_0);
            worker.Emit(OpCodes.Ldflda, fr);
            worker.Emit(OpCodes.Ldc_I8, dirtyBit);

            var hookMethod = GetHook(td, fd, ref failed);
            if (hookMethod != null)
            {
                AddHook(fd, worker, hookMethod);
            }
            else
            {
                worker.Emit(OpCodes.Ldnull);
            }

            if (fd.FieldType.Is<GameObject>())
            {
                worker.Emit(OpCodes.Ldarg_0);
                worker.Emit(OpCodes.Ldflda, fieldReference);
                worker.Emit(OpCodes.Call, module.SyncVarSetterGameObject);
            }
            else if (fd.FieldType.Is<NetworkEntity>())
            {
                worker.Emit(OpCodes.Ldarg_0);
                worker.Emit(OpCodes.Ldflda, fieldReference);
                worker.Emit(OpCodes.Call, module.SyncVarSetterNetworkEntity);
            }
            else if (fd.FieldType.IsSubclassOf<NetworkModule>() || fd.FieldType.Is<NetworkModule>())
            {
                worker.Emit(OpCodes.Ldarg_0);
                worker.Emit(OpCodes.Ldflda, fieldReference);
                worker.Emit(OpCodes.Call, module.SyncVarSetterNetworkModule.GenericInstance(assembly.MainModule, fd.FieldType));
            }
            else
            {
                worker.Emit(OpCodes.Call, module.SyncVarSetterGeneral.GenericInstance(assembly.MainModule, fd.FieldType));
            }

            worker.Append(endOfMethod);

            worker.Emit(OpCodes.Ret);

            set.Parameters.Add(new ParameterDefinition("value", ParameterAttributes.In, fd.FieldType));
            set.SemanticsAttributes = MethodSemanticsAttributes.Setter;
            return set;
        }
    }
}