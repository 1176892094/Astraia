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

using System.Collections;
using System.Collections.Generic;
using Astraia.Net;
using Mono.Cecil;
using Mono.Cecil.Cil;
using UnityEngine;

namespace Astraia.Editor
{
    internal partial class NetworkMember
    {
        private Dictionary<FieldDefinition, FieldDefinition> syncVarIds = new Dictionary<FieldDefinition, FieldDefinition>();
        private List<FieldDefinition> syncVars = new List<FieldDefinition>();
        private readonly Module module;
        private readonly Writer writer;
        private readonly Reader reader;
        private readonly SyncVarAccess access;
        private readonly TypeDefinition expand;
        private readonly SyncVarProcess process;
        private readonly ILogPostProcessor debugger;
        private readonly AssemblyDefinition assembly;
        private readonly List<(MethodDefinition, int)> serverRpcList = new List<(MethodDefinition, int)>();
        private readonly List<MethodDefinition> serverRpcFuncList = new List<MethodDefinition>();
        private readonly List<(MethodDefinition, int)> clientRpcList = new List<(MethodDefinition, int)>();
        private readonly List<MethodDefinition> clientRpcFuncList = new List<MethodDefinition>();
        private readonly List<(MethodDefinition, int)> targetRpcList = new List<(MethodDefinition, int)>();
        private readonly List<MethodDefinition> targetRpcFuncList = new List<MethodDefinition>();

        public NetworkMember(AssemblyDefinition assembly, SyncVarAccess access, Module module, Writer writer, Reader reader, ILogPostProcessor debugger, TypeDefinition expand)
        {
            this.expand = expand;
            this.module = module;
            this.access = access;
            this.writer = writer;
            this.reader = reader;
            this.debugger = debugger;
            this.assembly = assembly;
            process = new SyncVarProcess(assembly, access, module, debugger);
        }

        public bool Process(ref bool failed)
        {
            if (expand.GetMethod(Weaver.GEN_FUN) != null)
            {
                return false;
            }

            var method = new MethodDefinition(Weaver.GEN_FUN, MethodAttributes.Private, module.Import(typeof(void)));
            var worker = method.Body.GetILProcessor();
            worker.Emit(OpCodes.Ret);
            expand.Methods.Add(method);

            (syncVars, syncVarIds) = process.ProcessSyncVars(expand, ref failed);

            if (!failed)
            {
                ProcessRpc(ref failed);
            }

            if (!failed)
            {
                ProcessCctor(ref failed);
            }

            if (!failed)
            {
                SerializeSyncVars(ref failed);
            }

            if (!failed)
            {
                DeserializeSyncVars(ref failed);
            }

            debugger.Warn("{0} {1} {2}".Format("Y".Color("S"), assembly.MainModule, expand.Name.Color("Y")));
            return true;
        }

        public static void WriterDequeue(ILProcessor worker, Module module)
        {
            worker.Body.InitLocals = true;
            worker.Body.Variables.Add(new VariableDefinition(module.Import<MemoryWriter>()));
            worker.Emit(OpCodes.Call, module.WriterDequeue);
            worker.Emit(OpCodes.Stloc_0);
        }

        public static void WriterEnqueue(ILProcessor worker, Module module)
        {
            worker.Emit(OpCodes.Ldloc_0);
            worker.Emit(OpCodes.Call, module.WriterEnqueue);
        }

        public static void AddParameters(Module module, ICollection<ParameterDefinition> collection)
        {
            collection.Add(new ParameterDefinition("obj", ParameterAttributes.None, module.Import<NetworkModule>()));
            collection.Add(new ParameterDefinition("reader", ParameterAttributes.None, module.Import<MemoryReader>()));
            collection.Add(new ParameterDefinition("target", ParameterAttributes.None, module.Import<NetworkClient>()));
        }
    }

    internal partial class NetworkMember
    {
        private void ProcessRpc(ref bool failed)
        {
            var names = new HashSet<string>();
            var methods = new List<MethodDefinition>(expand.Methods);

            foreach (var method in methods)
            {
                foreach (var attribute in method.CustomAttributes)
                {
                    if (attribute.AttributeType.Is<ServerRpcAttribute>())
                    {
                        ProcessRpc(names, method, attribute, InvokeMode.ServerRpc, ref failed);
                        break;
                    }

                    if (attribute.AttributeType.Is<TargetRpcAttribute>())
                    {
                        ProcessRpc(names, method, attribute, InvokeMode.TargetRpc, ref failed);
                        break;
                    }

                    if (attribute.AttributeType.Is<ClientRpcAttribute>())
                    {
                        ProcessRpc(names, method, attribute, InvokeMode.ClientRpc, ref failed);
                        break;
                    }
                }
            }
        }

        private void ProcessRpc(HashSet<string> names, MethodDefinition md, CustomAttribute source, InvokeMode mode, ref bool failed)
        {
            if (md.IsAbstract)
            {
                debugger.Error("{0} 方法不能是抽象的。".Format(md.Name), md);
                failed = true;
                return;
            }

            if (md.IsStatic)
            {
                debugger.Error("{0} 方法不能是静态的。".Format(md.Name), md);
                failed = true;
                return;
            }

            if (md.HasGenericParameters)
            {
                debugger.Error("{0} 方法不能有泛型参数。".Format(md.Name), md);
                failed = true;
                return;
            }

            if (md.ReturnType.Is<IEnumerator>())
            {
                debugger.Error("{0} 方法不能被迭代。".Format(md.Name), md);
                failed = true;
                return;
            }

            if (!md.ReturnType.Is(typeof(void)))
            {
                debugger.Error("{0} 方法不能有返回值。".Format(md.Name), md);
                failed = true;
                return;
            }

            for (int i = 0; i < md.Parameters.Count; ++i)
            {
                var pd = md.Parameters[i];
                if (pd.ParameterType.IsGenericParameter)
                {
                    debugger.Error("{0} 方法不能有泛型参数。".Format(md.Name), md);
                    failed = true;
                    continue;
                }

                if (pd.IsOut)
                {
                    debugger.Error("{0} 方法不能携带 out 关键字。".Format(md.Name), md);
                    failed = true;
                    continue;
                }

                if (pd.IsOptional)
                {
                    debugger.Error("{0} 方法不能有可选参数。".Format(md.Name), md);
                    failed = true;
                    continue;
                }

                if (pd.ParameterType.Is<NetworkClient>() && (mode != InvokeMode.TargetRpc || i != 0))
                {
                    debugger.Error("{0} 方法不能传递 {1}。".Format(md.Name, nameof(NetworkClient)), md);
                    failed = true;
                }
            }

            names.Add(md.Name);
            if (mode == InvokeMode.ServerRpc)
            {
                serverRpcList.Add((md, (int)source.GetArgument()));
                var funcV1 = NetworkMethod.ServerRpcV1(module, writer, debugger, expand, md, source, ref failed);
                var funcV2 = NetworkMethod.ServerRpcV2(module, reader, debugger, expand, md, funcV1, ref failed);
                if (funcV2 != null) serverRpcFuncList.Add(funcV2);
            }
            else if (mode == InvokeMode.ClientRpc)
            {
                clientRpcList.Add((md, (int)source.GetArgument()));
                var funcV1 = NetworkMethod.ClientRpcV1(module, writer, debugger, expand, md, source, ref failed);
                var funcV2 = NetworkMethod.ClientRpcV2(module, reader, debugger, expand, md, funcV1, ref failed);
                if (funcV2 != null) clientRpcFuncList.Add(funcV2);
            }
            else if (mode == InvokeMode.TargetRpc)
            {
                targetRpcList.Add((md, (int)source.GetArgument()));
                var funcV1 = NetworkMethod.TargetRpcV1(module, writer, debugger, expand, md, source, ref failed);
                var funcV2 = NetworkMethod.TargetRpcV2(module, reader, debugger, expand, md, funcV1, ref failed);
                if (funcV2 != null) targetRpcFuncList.Add(funcV2);
            }
        }
    }

    internal partial class NetworkMember
    {
        /// <summary>
        /// 注入静态构造函数
        /// </summary>
        private void ProcessCctor(ref bool failed)
        {
            if (serverRpcList.Count == 0 && clientRpcList.Count == 0 && targetRpcList.Count == 0)
            {
                return;
            }

            var cctor = expand.GetMethod(Weaver.GEN_CCTOR);
            var empty = cctor == null;
            if (empty)
            {
                cctor = new MethodDefinition(Weaver.GEN_CCTOR, Weaver.GEN_DATA, module.Import(typeof(void)));
            }
            else
            {
                if (!EndInstruction(cctor))
                {
                    debugger.Error("{0} 无效的静态构造函数。".Format(expand.Name), cctor);
                    failed = true;
                    return;
                }
            }

            var worker = cctor.Body.GetILProcessor();
            for (int i = 0; i < serverRpcList.Count; ++i)
            {
                GenerateDelegate(worker, module.RegisterServerRpc, serverRpcFuncList[i], serverRpcList[i]);
            }

            for (int i = 0; i < clientRpcList.Count; ++i)
            {
                GenerateDelegate(worker, module.RegisterClientRpc, clientRpcFuncList[i], clientRpcList[i]);
            }

            for (int i = 0; i < targetRpcList.Count; ++i)
            {
                GenerateDelegate(worker, module.RegisterClientRpc, targetRpcFuncList[i], targetRpcList[i]);
            }

            worker.Append(worker.Create(OpCodes.Ret));
            if (empty)
            {
                expand.Methods.Add(cctor);
            }

            expand.Attributes &= ~TypeAttributes.BeforeFieldInit;
        }

        /// <summary>
        /// 获取最后一条指令
        /// </summary>
        private static bool EndInstruction(MethodDefinition md)
        {
            if (md.Body.Instructions.Count == 0)
            {
                return true;
            }

            if (md.Body.Instructions[^1].OpCode != OpCodes.Ret)
            {
                return false;
            }

            md.Body.Instructions.RemoveAt(md.Body.Instructions.Count - 1);
            return true;
        }

        /// <summary>
        /// 在静态构造函数中注入ClientRpc委托
        /// </summary>
        private void GenerateDelegate(ILProcessor worker, MethodReference mr, MethodDefinition md, (MethodDefinition, int) pair)
        {
            worker.Emit(OpCodes.Ldtoken, expand);
            worker.Emit(OpCodes.Call, module.GetTypeFromHandle);
            worker.Emit(OpCodes.Ldc_I4, pair.Item2);
            worker.Emit(OpCodes.Ldstr, pair.Item1.FullName);
            worker.Emit(OpCodes.Ldnull);
            worker.Emit(OpCodes.Ldftn, md);
            worker.Emit(OpCodes.Newobj, module.InvokeDelegate);
            worker.Emit(OpCodes.Call, mr);
        }
    }

    internal partial class NetworkMember
    {
        private void SerializeSyncVars(ref bool failed)
        {
            if (expand.GetMethod(Weaver.MED_SER) != null) return;
            if (syncVars.Count == 0) return;

            var method = new MethodDefinition(Weaver.MED_SER, Weaver.GEN_VAR, module.Import(typeof(void)));
            method.Parameters.Add(new ParameterDefinition("writer", ParameterAttributes.None, module.Import<MemoryWriter>()));
            method.Parameters.Add(new ParameterDefinition("initialize", ParameterAttributes.None, module.Import<bool>()));
            var worker = method.Body.GetILProcessor();

            method.Body.InitLocals = true;
            var reason = Common.GetMethod(expand.BaseType, assembly, Weaver.MED_SER);
            if (reason != null)
            {
                worker.Emit(OpCodes.Ldarg_0);
                worker.Emit(OpCodes.Ldarg_1);
                worker.Emit(OpCodes.Ldarg_2);
                worker.Emit(OpCodes.Call, reason);
            }

            var instruction = worker.Create(OpCodes.Nop);
            worker.Emit(OpCodes.Ldarg_2);
            worker.Emit(OpCodes.Brfalse, instruction);
            foreach (var value in syncVars)
            {
                FieldReference syncVar = value;
                if (expand.HasGenericParameters)
                {
                    syncVar = value.MakeGeneric();
                }

                worker.Emit(OpCodes.Ldarg_1);
                worker.Emit(OpCodes.Ldarg_0);
                worker.Emit(OpCodes.Ldfld, syncVar);
                var func = writer.GetFunction(syncVar.FieldType.IsSubclassOf<NetworkModule>() ? module.Import<NetworkModule>() : syncVar.FieldType, ref failed);
                if (func == null)
                {
                    debugger.Error("不支持 {0} 的类型".Format(syncVar.Name), syncVar);
                    failed = true;
                    continue;
                }

                worker.Emit(OpCodes.Call, func);
            }

            worker.Emit(OpCodes.Ret);
            worker.Append(instruction);
            worker.Emit(OpCodes.Ldarg_1);
            worker.Emit(OpCodes.Ldarg_0);
            worker.Emit(OpCodes.Call, module.SyncVarDirty);
            worker.Emit(OpCodes.Call, writer.GetFunction(module.Import<ulong>(), ref failed));
            var mask = access.GetSyncVar(expand.BaseType.FullName);
            foreach (var value in syncVars)
            {
                FieldReference syncVar = value;
                if (expand.HasGenericParameters)
                {
                    syncVar = value.MakeGeneric();
                }

                var varLabel = worker.Create(OpCodes.Nop);
                worker.Emit(OpCodes.Ldarg_0);
                worker.Emit(OpCodes.Call, module.SyncVarDirty);
                worker.Emit(OpCodes.Ldc_I8, 1L << mask);
                worker.Emit(OpCodes.And);
                worker.Emit(OpCodes.Brfalse, varLabel);
                worker.Emit(OpCodes.Ldarg_1);
                worker.Emit(OpCodes.Ldarg_0);
                worker.Emit(OpCodes.Ldfld, syncVar);

                var func = writer.GetFunction(syncVar.FieldType.IsSubclassOf<NetworkModule>() ? module.Import<NetworkModule>() : syncVar.FieldType, ref failed);
                if (func == null)
                {
                    debugger.Error("不支持 {0} 的类型".Format(syncVar.Name), syncVar);
                    failed = true;
                    continue;
                }

                worker.Emit(OpCodes.Call, func);
                worker.Append(varLabel);
                mask += 1;
            }

            worker.Emit(OpCodes.Ret);
            expand.Methods.Add(method);
        }

        private void DeserializeSyncVars(ref bool failed)
        {
            if (expand.GetMethod(Weaver.MED_DES) != null || syncVars.Count == 0)
            {
                return;
            }

            var method = new MethodDefinition(Weaver.MED_DES, Weaver.GEN_VAR, module.Import(typeof(void)));
            method.Parameters.Add(new ParameterDefinition("reader", ParameterAttributes.None, module.Import<MemoryReader>()));
            method.Parameters.Add(new ParameterDefinition("initialize", ParameterAttributes.None, module.Import<bool>()));
            var worker = method.Body.GetILProcessor();

            method.Body.InitLocals = true;
            method.Body.Variables.Add(new VariableDefinition(module.Import<long>()));
            var reason = Common.GetMethod(expand.BaseType, assembly, Weaver.MED_DES);
            if (reason != null)
            {
                worker.Append(worker.Create(OpCodes.Ldarg_0));
                worker.Append(worker.Create(OpCodes.Ldarg_1));
                worker.Append(worker.Create(OpCodes.Ldarg_2));
                worker.Append(worker.Create(OpCodes.Call, reason));
            }

            var instruction = worker.Create(OpCodes.Nop);
            worker.Append(worker.Create(OpCodes.Ldarg_2));
            worker.Append(worker.Create(OpCodes.Brfalse, instruction));
            foreach (var syncVar in syncVars)
            {
                DeserializeSyncVar(syncVar, worker, ref failed);
            }

            worker.Append(worker.Create(OpCodes.Ret));
            worker.Append(instruction);
            worker.Append(worker.Create(OpCodes.Ldarg_1));
            worker.Append(worker.Create(OpCodes.Call, reader.GetFunction(module.Import<ulong>(), ref failed)));
            worker.Append(worker.Create(OpCodes.Stloc_0));

            var mask = access.GetSyncVar(expand.BaseType.FullName);
            foreach (var syncVar in syncVars)
            {
                var nop = worker.Create(OpCodes.Nop);
                worker.Append(worker.Create(OpCodes.Ldloc_0));
                worker.Append(worker.Create(OpCodes.Ldc_I8, 1L << mask));
                worker.Append(worker.Create(OpCodes.And));
                worker.Append(worker.Create(OpCodes.Brfalse, nop));
                DeserializeSyncVar(syncVar, worker, ref failed);
                worker.Append(nop);
                mask += 1;
            }

            worker.Append(worker.Create(OpCodes.Ret));
            expand.Methods.Add(method);
        }

        private void DeserializeSyncVar(FieldDefinition syncVar, ILProcessor worker, ref bool failed)
        {
            worker.Append(worker.Create(OpCodes.Ldarg_0));
            worker.Emit(OpCodes.Ldarg_0);
            worker.Emit(OpCodes.Ldflda, expand.HasGenericParameters ? syncVar.MakeGeneric() : syncVar);

            var method = process.GetHookMethod(expand, syncVar, ref failed);
            if (method != null)
            {
                process.GenerateNewActionFromHookMethod(syncVar, worker, method);
            }
            else
            {
                worker.Emit(OpCodes.Ldnull);
            }

            if (syncVar.FieldType.Is<GameObject>())
            {
                var objectId = syncVarIds[syncVar];
                worker.Emit(OpCodes.Ldarg_1);
                worker.Emit(OpCodes.Ldarg_0);
                worker.Emit(OpCodes.Ldflda, objectId);
                worker.Emit(OpCodes.Call, module.SyncVarGetterGameObject);
            }
            else if (syncVar.FieldType.Is<NetworkEntity>())
            {
                var objectId = syncVarIds[syncVar];
                worker.Emit(OpCodes.Ldarg_1);
                worker.Emit(OpCodes.Ldarg_0);
                worker.Emit(OpCodes.Ldflda, objectId);
                worker.Emit(OpCodes.Call, module.SyncVarGetterNetworkEntity);
            }
            else if (syncVar.FieldType.IsSubclassOf<NetworkModule>() || syncVar.FieldType.Is<NetworkModule>())
            {
                var objectId = syncVarIds[syncVar];
                worker.Emit(OpCodes.Ldarg_1);
                worker.Emit(OpCodes.Ldarg_0);
                worker.Emit(OpCodes.Ldflda, objectId);
                var getFunc = module.SyncVarGetterNetworkModule.GenericInstance(assembly.MainModule, syncVar.FieldType);
                worker.Emit(OpCodes.Call, getFunc);
            }
            else
            {
                var func = reader.GetFunction(syncVar.FieldType, ref failed);
                if (func == null)
                {
                    debugger.Error("不支持 {0} 的类型。".Format(syncVar.Name), syncVar);
                    failed = true;
                    return;
                }

                worker.Emit(OpCodes.Ldarg_1);
                worker.Emit(OpCodes.Call, func);
                worker.Emit(OpCodes.Call, module.SyncVarGetterGeneral.GenericInstance(assembly.MainModule, syncVar.FieldType));
            }
        }
    }
}