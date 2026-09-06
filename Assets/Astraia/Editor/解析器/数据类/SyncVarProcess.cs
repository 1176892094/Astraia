// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-06-08 15:06:06
// # Recently: 2026-09-06 15:32:39
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Linq;
using Astraia.Net;
using Mono.Cecil;
using Mono.Cecil.Cil;
using UnityEngine;

namespace Astraia
{
    internal class SyncVarProcess
    {
        private readonly Module module;
        private readonly SyncVarAccess access;
        private readonly AssemblyDebugger debugger;
        private readonly AssemblyDefinition assembly;

        public SyncVarProcess(AssemblyDefinition assembly, SyncVarAccess access, Module module, AssemblyDebugger debugger)
        {
            this.access = access;
            this.module = module;
            this.debugger = debugger;
            this.assembly = assembly;
        }

        public void AddFunc(FieldDefinition syncVar, ILProcessor worker, MethodDefinition func)
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

        public MethodDefinition GetFunc(TypeDefinition td, FieldDefinition syncVar, ref bool failed)
        {
            var name = syncVar.GetAttribute<SyncVarAttribute>().GetArgument<string>();
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

        public void Process(SyncVarList<FieldDefinition> syncVars, TypeDefinition td, ref bool failed)
        {
            var dirtyBits = access.GetSyncVar(td.BaseType.FullName);
            foreach (var fd in td.Fields.Where(fd => fd.HasAttribute<SyncVarAttribute>()))
            {
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

                syncVars.Add(fd);

                ProcessSyncVar(syncVars, td, fd, 1L << dirtyBits, ref failed);
                dirtyBits += 1;

                if (dirtyBits > 64)
                {
                    debugger.Error("{0} 网络变量数量大于 64".Format(fd.Name), td);
                    failed = true;
                }
            }

            foreach (var fd in syncVars.Values)
            {
                td.Fields.Add(fd);
            }

            int count = access.GetSyncVar(td.BaseType.FullName);
            access.SetSyncVar(td.FullName, count + syncVars.Count);
        }

        private void ProcessSyncVar(SyncVarList<FieldDefinition> syncVars, TypeDefinition td, FieldDefinition fd, long dirtyBits, ref bool failed)
        {
            FieldReference fr = null;
            if (fd.FieldType.Is<GameObject>() || fd.FieldType.Is<NetworkEntity>())
            {
                var id = new FieldDefinition("{0}ID".Format(fd.Name), FieldAttributes.Family, module.Import<uint>());
                syncVars[fd] = id;
                id.DeclaringType = td;
                fr = td.HasGenericParameters ? id.MakeGeneric() : id;
            }
            else if (fd.FieldType.Is<NetworkModule>() || fd.FieldType.IsSubclassOf<NetworkModule>())
            {
                var id = new FieldDefinition("{0}ID".Format(fd.Name), FieldAttributes.Family, module.Import<NetworkVariable>());
                syncVars[fd] = id;
                id.DeclaringType = td;
                fr = td.HasGenericParameters ? id.MakeGeneric() : id;
            }

            var getter = GenerateSyncVarGetter(fd, fd.Name, fr);
            var setter = GenerateSyncVarSetter(fd, fd.Name, fr, td, dirtyBits, ref failed);

            var property = new PropertyDefinition("{0}Var".Format(fd.Name), PropertyAttributes.None, fd.FieldType) { GetMethod = getter, SetMethod = setter };

            td.Methods.Add(getter);
            td.Methods.Add(setter);
            td.Properties.Add(property);

            access.setter[fd] = setter;

            if (Supported(fd.FieldType))
            {
                access.getter[fd] = getter;
            }
        }

        private static bool Supported(TypeReference self)
        {
            return self.Is<GameObject>() || self.Is<NetworkEntity>() || self.Is<NetworkModule>() || self.IsSubclassOf<NetworkModule>();
        }

        private MethodDefinition GenerateSyncVarGetter(FieldDefinition fd, string name, FieldReference obj)
        {
            var getter = new MethodDefinition("get_{0}Var".Format(name), Weaver.GEN_S3, fd.FieldType);
            var worker = getter.Body.GetILProcessor();
            var fr = fd.DeclaringType.HasGenericParameters ? fd.MakeGeneric() : fd;

            if (fd.FieldType.Is<GameObject>())
            {
                worker.Emit(OpCodes.Ldarg_0);
                worker.Emit(OpCodes.Ldarg_0);
                worker.Emit(OpCodes.Ldfld, obj);
                worker.Emit(OpCodes.Ldarg_0);
                worker.Emit(OpCodes.Ldflda, fr);
                worker.Emit(OpCodes.Call, module.GetSyncVarGameObject);
                worker.Emit(OpCodes.Ret);
            }
            else if (fd.FieldType.Is<NetworkEntity>())
            {
                worker.Emit(OpCodes.Ldarg_0);
                worker.Emit(OpCodes.Ldarg_0);
                worker.Emit(OpCodes.Ldfld, obj);
                worker.Emit(OpCodes.Ldarg_0);
                worker.Emit(OpCodes.Ldflda, fr);
                worker.Emit(OpCodes.Call, module.GetSyncVarNetworkEntity);
                worker.Emit(OpCodes.Ret);
            }
            else if (fd.FieldType.IsSubclassOf<NetworkModule>() || fd.FieldType.Is<NetworkModule>())
            {
                worker.Emit(OpCodes.Ldarg_0);
                worker.Emit(OpCodes.Ldarg_0);
                worker.Emit(OpCodes.Ldfld, obj);
                worker.Emit(OpCodes.Ldarg_0);
                worker.Emit(OpCodes.Ldflda, fr);
                worker.Emit(OpCodes.Call, module.GetSyncVarNetworkModule.MakeGeneric(assembly.MainModule, fd.FieldType));
                worker.Emit(OpCodes.Ret);
            }
            else
            {
                worker.Emit(OpCodes.Ldarg_0);
                worker.Emit(OpCodes.Ldfld, fr);
                worker.Emit(OpCodes.Ret);
            }

            getter.Body.Variables.Add(new VariableDefinition(fd.FieldType));
            getter.Body.InitLocals = true;
            getter.SemanticsAttributes = MethodSemanticsAttributes.Getter;
            return getter;
        }

        private MethodDefinition GenerateSyncVarSetter(FieldDefinition fd, string name, FieldReference obj, TypeDefinition td, long dirtyBit, ref bool failed)
        {
            var setter = new MethodDefinition("set_{0}Var".Format(name), Weaver.GEN_S3, module.Import(typeof(void)));
            var worker = setter.Body.GetILProcessor();
            var fr = fd.DeclaringType.HasGenericParameters ? fd.MakeGeneric() : fd;

            var nop = worker.Create(OpCodes.Nop);

            worker.Emit(OpCodes.Ldarg_0);
            worker.Emit(OpCodes.Ldarg_1);
            worker.Emit(OpCodes.Ldarg_0);
            worker.Emit(OpCodes.Ldflda, fr);
            worker.Emit(OpCodes.Ldc_I8, dirtyBit);

            var func = GetFunc(td, fd, ref failed);
            if (func != null)
            {
                AddFunc(fd, worker, func);
            }
            else
            {
                worker.Emit(OpCodes.Ldnull);
            }

            if (fd.FieldType.Is<GameObject>())
            {
                worker.Emit(OpCodes.Ldarg_0);
                worker.Emit(OpCodes.Ldflda, obj);
                worker.Emit(OpCodes.Call, module.SyncVarSetterGameObject);
            }
            else if (fd.FieldType.Is<NetworkEntity>())
            {
                worker.Emit(OpCodes.Ldarg_0);
                worker.Emit(OpCodes.Ldflda, obj);
                worker.Emit(OpCodes.Call, module.SyncVarSetterNetworkEntity);
            }
            else if (fd.FieldType.IsSubclassOf<NetworkModule>() || fd.FieldType.Is<NetworkModule>())
            {
                worker.Emit(OpCodes.Ldarg_0);
                worker.Emit(OpCodes.Ldflda, obj);
                worker.Emit(OpCodes.Call, module.SyncVarSetterNetworkModule.MakeGeneric(assembly.MainModule, fd.FieldType));
            }
            else
            {
                worker.Emit(OpCodes.Call, module.SyncVarSetterGeneral.MakeGeneric(assembly.MainModule, fd.FieldType));
            }

            worker.Append(nop);
            worker.Emit(OpCodes.Ret);

            setter.Parameters.Add(new ParameterDefinition("value", ParameterAttributes.In, fd.FieldType));
            setter.SemanticsAttributes = MethodSemanticsAttributes.Setter;
            return setter;
        }
    }
}