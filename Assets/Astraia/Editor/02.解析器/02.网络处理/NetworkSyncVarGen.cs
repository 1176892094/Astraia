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
using System.Linq;
using Astraia.Net;
using Mono.Cecil;
using Mono.Cecil.Cil;
using UnityEngine;

namespace Astraia.Editor
{
    internal class NetworkSyncVarGen
    {
        private readonly Module module;
        private readonly SyncVarAccess access;
        private readonly ILogPostProcessor debugger;
        private readonly AssemblyDefinition assembly;

        public NetworkSyncVarGen(AssemblyDefinition assembly, SyncVarAccess access, Module module, ILogPostProcessor debugger)
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

                if (fd.FieldType.IsArray)
                {
                    debugger.Error("{0} 不能使用数组。".Format(fd.Name), fd);
                    failed = true;
                    continue;
                }

                syncVars.Add(fd);

                ProcessSyncVar(td, fd, syncVars, 1L << dirtyBits, ref failed);
                dirtyBits += 1;

                if (dirtyBits > Weaver.BIT_COUNT)
                {
                    debugger.Error("{0} 网络变量数量大于 {1}。".Format(fd.Name, Weaver.BIT_COUNT), td);
                    failed = true;
                }
            }

            foreach (var fd in syncVars.Values)
            {
                td.Fields.Add(fd);
            }

            int parentSyncVarCount = access.GetSyncVar(td.BaseType.FullName);
            access.SetSyncVar(td.FullName, parentSyncVarCount + syncVars.Count);
        }

        private void ProcessSyncVar(TypeDefinition td, FieldDefinition fd, SyncVarList<FieldDefinition> syncVarIds, long dirtyBits, ref bool failed)
        {
            FieldDefinition objectId = null;
            if (fd.FieldType.Is<GameObject>() || fd.FieldType.Is<NetworkEntity>())
            {
                objectId = new FieldDefinition("{0}ID".Format(fd.Name), FieldAttributes.Family, module.Import<uint>());
                syncVarIds[fd] = objectId;
                objectId.DeclaringType = td;
            }
            else if (fd.FieldType.Is<NetworkModule>() || fd.FieldType.IsSubclassOf<NetworkModule>())
            {
                objectId = new FieldDefinition("{0}ID".Format(fd.Name), FieldAttributes.Family, module.Import<NetworkVariable>());
                syncVarIds[fd] = objectId;
                objectId.DeclaringType = td;
            }

            var getter = GenerateSyncVarGetter(fd, fd.Name, objectId);
            var setter = GenerateSyncVarSetter(fd, fd.Name, objectId, td, dirtyBits, ref failed);

            var property = new PropertyDefinition("{0}Var".Format(fd.Name), PropertyAttributes.None, fd.FieldType)
            {
                GetMethod = getter,
                SetMethod = setter
            };

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

        private MethodDefinition GenerateSyncVarSetter(FieldDefinition fd, string name, FieldDefinition fieldId, TypeDefinition td, long dirtyBit, ref bool failed)
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



    internal static class SyncVarReplace
    {
        /// <summary>
        /// 修正SyncVar
        /// </summary>
        public static void Process(ModuleDefinition md, SyncVarAccess access)
        {
            foreach (var td in md.Types.Where(td => td.IsClass))
            {
                ProcessClass(td, access);
            }
        }

        /// <summary>
        /// 处理类
        /// </summary>
        private static void ProcessClass(TypeDefinition td, SyncVarAccess access)
        {
            foreach (var md in td.Methods)
            {
                ProcessMethod(md, access);
            }

            foreach (var nested in td.NestedTypes)
            {
                ProcessClass(nested, access);
            }
        }

        /// <summary>
        /// 处理方法
        /// </summary>
        private static void ProcessMethod(MethodDefinition md, SyncVarAccess access)
        {
            if (md.Name == ".cctor" || md.Name == Weaver.GEN_FUN || md.Name.StartsWith(Weaver.MED_INV))
            {
                return;
            }

            if (md.IsAbstract)
            {
                return;
            }

            if (md.Body?.Instructions != null)
            {
                for (var i = 0; i < md.Body.Instructions.Count;)
                {
                    var instr = md.Body.Instructions[i];
                    i += ProcessInstruction(md, instr, i, access);
                }
            }
        }

        /// <summary>
        /// 处理指令
        /// </summary>
        private static int ProcessInstruction(MethodDefinition md, Instruction instr, int index, SyncVarAccess access)
        {
            if (instr.OpCode == OpCodes.Stfld && instr.Operand is FieldDefinition OpStfLd)
            {
                ProcessSetter(md, instr, OpStfLd, access);
            }

            if (instr.OpCode == OpCodes.Ldfld && instr.Operand is FieldDefinition OpLdfLd)
            {
                ProcessGetter(md, instr, OpLdfLd, access);
            }

            if (instr.OpCode == OpCodes.Ldflda && instr.Operand is FieldDefinition OpLdfLda)
            {
                return ProcessAddress(md, instr, OpLdfLda, access, index);
            }

            return 1;
        }

        /// <summary>
        /// 设置指令
        /// </summary>
        private static void ProcessSetter(MethodDefinition md, Instruction i, FieldDefinition opField, SyncVarAccess access)
        {
            if (md.Name == Weaver.GEN_CTOR)
            {
                return;
            }

            if (access.setter.TryGetValue(opField, out var method))
            {
                i.OpCode = OpCodes.Call;
                i.Operand = method;
            }
        }

        /// <summary>
        /// 获取指令
        /// </summary>
        private static void ProcessGetter(MethodDefinition md, Instruction i, FieldDefinition opField, SyncVarAccess access)
        {
            if (md.Name == Weaver.GEN_CTOR)
            {
                return;
            }

            if (access.getter.TryGetValue(opField, out var method))
            {
                i.OpCode = OpCodes.Call;
                i.Operand = method;
            }
        }

        /// <summary>
        /// 处理加载地址指令
        /// </summary>
        private static int ProcessAddress(MethodDefinition md, Instruction instr, FieldDefinition opField, SyncVarAccess access, int index)
        {
            if (md.Name == Weaver.GEN_CTOR)
            {
                return 1;
            }

            if (access.setter.TryGetValue(opField, out var method))
            {
                var next = md.Body.Instructions[index + 1];

                if (next.OpCode == OpCodes.Initobj)
                {
                    var worker = md.Body.GetILProcessor();
                    var variable = new VariableDefinition(opField.FieldType);
                    md.Body.Variables.Add(variable);

                    worker.InsertBefore(instr, worker.Create(OpCodes.Ldloca, variable));
                    worker.InsertBefore(instr, worker.Create(OpCodes.Initobj, opField.FieldType));
                    worker.InsertBefore(instr, worker.Create(OpCodes.Ldloc, variable));
                    worker.InsertBefore(instr, worker.Create(OpCodes.Call, method));

                    worker.Remove(instr);
                    worker.Remove(next);
                    return 4;
                }
            }

            return 1;
        }
    }
}