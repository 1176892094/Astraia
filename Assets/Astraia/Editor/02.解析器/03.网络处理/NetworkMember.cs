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
        private readonly TypeDefinition cached;
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
            cached = expand;
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
            if (cached.GetMethod(Weaver.GEN_FUN) != null)
            {
                return false;
            }

            MarkAsProcessed(cached);

            (syncVars, syncVarIds) = process.ProcessSyncVars(cached, ref failed);

            ProcessRpcMethods(ref failed);

            if (failed)
            {
                return true;
            }

            InjectStaticConstructor(ref failed);

            GenerateSerialize(ref failed);

            if (failed)
            {
                return true;
            }

            GenerateDeserialize(ref failed);
            return true;
        }

        private void MarkAsProcessed(TypeDefinition td)
        {
            var versionMethod = new MethodDefinition(Weaver.GEN_FUN, MethodAttributes.Private, module.Import(typeof(void)));
            var worker = versionMethod.Body.GetILProcessor();
            worker.Emit(OpCodes.Ret);
            td.Methods.Add(versionMethod);
        }

        public static void WriteInitLocals(ILProcessor worker, Module module)
        {
            worker.Body.InitLocals = true;
            worker.Body.Variables.Add(new VariableDefinition(module.Import<MemoryWriter>()));
        }

        public static void WritePopSetter(ILProcessor worker, Module module)
        {
            worker.Emit(OpCodes.Call, module.WriterDequeue);
            worker.Emit(OpCodes.Stloc_0);
        }

        public static void WritePushSetter(ILProcessor worker, Module module)
        {
            worker.Emit(OpCodes.Ldloc_0);
            worker.Emit(OpCodes.Call, module.WriterEnqueue);
        }

        public static void AddInvokeParameters(Module module, ICollection<ParameterDefinition> collection)
        {
            collection.Add(new ParameterDefinition("obj", ParameterAttributes.None, module.Import<NetworkModule>()));
            collection.Add(new ParameterDefinition("reader", ParameterAttributes.None, module.Import<MemoryReader>()));
            collection.Add(new ParameterDefinition("target", ParameterAttributes.None, module.Import<NetworkClient>()));
        }
    }

    internal partial class NetworkMember
    {
        /// <summary>
        /// 处理Rpc方法
        /// </summary>
        private void ProcessRpcMethods(ref bool failed)
        {
            var names = new HashSet<string>();
            var methods = new List<MethodDefinition>(expand.Methods);

            foreach (var md in methods)
            {
                foreach (var ca in md.CustomAttributes)
                {
                    if (ca.AttributeType.Is<ServerRpcAttribute>())
                    {
                        ProcessDelegate(names, md, ca, InvokeMode.ServerRpc, ref failed);
                        break;
                    }

                    if (ca.AttributeType.Is<TargetRpcAttribute>())
                    {
                        ProcessDelegate(names, md, ca, InvokeMode.TargetRpc, ref failed);
                        break;
                    }

                    if (ca.AttributeType.Is<ClientRpcAttribute>())
                    {
                        ProcessDelegate(names, md, ca, InvokeMode.ClientRpc, ref failed);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 处理ClientRpc
        /// </summary>
        private void ProcessDelegate(HashSet<string> names, MethodDefinition md, CustomAttribute rpc, InvokeMode mode, ref bool failed)
        {
            if (md.IsAbstract)
            {
                debugger.Error("{0}不能作用在抽象方法中。".Format(mode), md);
                failed = true;
                return;
            }

            if (!IsValidMethod(md, mode, ref failed))
            {
                return;
            }

            names.Add(md.Name);
            MethodDefinition func;
            MethodDefinition rpcFunc;
            switch (mode)
            {
                case InvokeMode.ServerRpc:
                    serverRpcList.Add((md, (int)rpc.GetArgument()));
                    func = NetworkMethod.ServerRpcInvoke(module, writer, debugger, expand, md, rpc, ref failed);
                    rpcFunc = NetworkMethod.ServerRpc(module, reader, debugger, expand, md, func, ref failed);
                    if (rpcFunc != null)
                    {
                        serverRpcFuncList.Add(rpcFunc);
                    }

                    break;
                case InvokeMode.ClientRpc:
                    clientRpcList.Add((md, (int)rpc.GetArgument()));
                    func = NetworkMethod.ClientRpcInvoke(module, writer, debugger, expand, md, rpc, ref failed);
                    rpcFunc = NetworkMethod.ClientRpc(module, reader, debugger, expand, md, func, ref failed);
                    if (rpcFunc != null)
                    {
                        clientRpcFuncList.Add(rpcFunc);
                    }

                    break;
                case InvokeMode.TargetRpc:
                    targetRpcList.Add((md, (int)rpc.GetArgument()));
                    func = NetworkMethod.TargetRpcInvoke(module, writer, debugger, expand, md, rpc, ref failed);
                    rpcFunc = NetworkMethod.TargetRpc(module, reader, debugger, expand, md, func, ref failed);
                    if (rpcFunc != null)
                    {
                        targetRpcFuncList.Add(rpcFunc);
                    }

                    break;
            }
        }

        /// <summary>
        /// 判断是否为非静态方法
        /// </summary>
        private bool IsValidMethod(MethodDefinition md, InvokeMode rpcType, ref bool failed)
        {
            if (md.IsStatic)
            {
                debugger.Error("{0} 方法不能是静态的。".Format(md.Name), md);
                failed = true;
                return false;
            }

            return IsValidFunc(md, ref failed) && IsValidParams(md, rpcType, ref failed);
        }

        /// <summary>
        /// 判断是否为有效Rpc
        /// </summary>
        private bool IsValidFunc(MethodReference mr, ref bool failed)
        {
            if (mr.ReturnType.Is<IEnumerator>())
            {
                debugger.Error("{0} 方法不能被迭代。".Format(mr.Name), mr);
                failed = true;
                return false;
            }

            if (!mr.ReturnType.Is(typeof(void)))
            {
                debugger.Error("{0} 方法不能有返回值。".Format(mr.Name), mr);
                failed = true;
                return false;
            }

            if (mr.HasGenericParameters)
            {
                debugger.Error("{0} 方法不能有泛型参数。".Format(mr.Name), mr);
                failed = true;
                return false;
            }

            return true;
        }

        /// <summary>
        /// 判断Rpc携带的参数
        /// </summary>
        private bool IsValidParams(MethodReference mr, InvokeMode rpcType, ref bool failed)
        {
            for (int i = 0; i < mr.Parameters.Count; ++i)
            {
                ParameterDefinition param = mr.Parameters[i];
                if (!IsValidParam(mr, param, rpcType, i == 0, ref failed))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 判断Rpc是否为有效参数
        /// </summary>
        private bool IsValidParam(MethodReference method, ParameterDefinition param, InvokeMode rpcType, bool firstParam, ref bool failed)
        {
            if (param.ParameterType.IsGenericParameter)
            {
                debugger.Error("{0} 方法不能有泛型参数。".Format(method.Name), method);
                failed = true;
                return false;
            }

            bool connection = param.ParameterType.Is<NetworkClient>();
            bool sendTarget = NetworkMethod.IsNetworkClient(param, rpcType);

            if (param.IsOut)
            {
                debugger.Error("{0} 方法不能携带 out 关键字。".Format(method.Name), method);
                failed = true;
                return false;
            }

            if (!sendTarget && connection && !(rpcType == InvokeMode.TargetRpc && firstParam))
            {
                debugger.Error("{0} 方法无效的参数 {1}，不能传递网络连接。".Format(method.Name, param), method);
                failed = true;
                return false;
            }

            if (param.IsOptional && !sendTarget)
            {
                debugger.Error("{0} 方法不能有可选参数。".Format(method.Name), method);
                failed = true;
                return false;
            }

            return true;
        }
    }

    internal partial class NetworkMember
    {
        /// <summary>
        /// 注入静态构造函数
        /// </summary>
        private void InjectStaticConstructor(ref bool failed)
        {
            if (serverRpcList.Count == 0 && clientRpcList.Count == 0 && targetRpcList.Count == 0) return;
            MethodDefinition cctor = expand.GetMethod(Weaver.GEN_CCTOR);
            bool cctorFound = cctor != null;
            if (cctor != null)
            {
                if (!RemoveFinalRetInstruction(cctor))
                {
                    debugger.Error("{0} 无效的静态构造函数。".Format(expand.Name), cctor);
                    failed = true;
                    return;
                }
            }
            else
            {
                cctor = new MethodDefinition(Weaver.GEN_CCTOR, Weaver.GEN_DATA, module.Import(typeof(void)));
            }

            ILProcessor worker = cctor.Body.GetILProcessor();
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
            if (!cctorFound)
            {
                expand.Methods.Add(cctor);
            }

            expand.Attributes &= ~TypeAttributes.BeforeFieldInit;
        }

        /// <summary>
        /// 判断自身静态构造函数是否被创建
        /// </summary>
        private static bool RemoveFinalRetInstruction(MethodDefinition md)
        {
            if (md.Body.Instructions.Count != 0)
            {
                Instruction retInstr = md.Body.Instructions[^1];
                if (retInstr.OpCode == OpCodes.Ret)
                {
                    md.Body.Instructions.RemoveAt(md.Body.Instructions.Count - 1);
                    return true;
                }

                return false;
            }

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
        /// <summary>
        /// 生成SyncVar的序列化方法
        /// </summary>
        private void GenerateSerialize(ref bool failed)
        {
            if (expand.GetMethod(Weaver.MED_SER) != null) return;
            if (syncVars.Count == 0) return;
            var serialize = new MethodDefinition(Weaver.MED_SER, Weaver.GEN_VAR, module.Import(typeof(void)));
            serialize.Parameters.Add(new ParameterDefinition("writer", ParameterAttributes.None, module.Import<MemoryWriter>()));
            serialize.Parameters.Add(new ParameterDefinition("initialize", ParameterAttributes.None, module.Import<bool>()));
            var worker = serialize.Body.GetILProcessor();

            serialize.Body.InitLocals = true;
            var baseSerialize = Resolve.GetMethod(expand.BaseType, assembly, Weaver.MED_SER);
            if (baseSerialize != null)
            {
                worker.Emit(OpCodes.Ldarg_0);
                worker.Emit(OpCodes.Ldarg_1);
                worker.Emit(OpCodes.Ldarg_2);
                worker.Emit(OpCodes.Call, baseSerialize);
            }

            Instruction instruction = worker.Create(OpCodes.Nop);
            worker.Emit(OpCodes.Ldarg_2);
            worker.Emit(OpCodes.Brfalse, instruction);
            foreach (var syncVarDef in syncVars)
            {
                FieldReference syncVar = syncVarDef;
                if (expand.HasGenericParameters)
                {
                    syncVar = syncVarDef.MakeHostInstanceGeneric();
                }

                worker.Emit(OpCodes.Ldarg_1);
                worker.Emit(OpCodes.Ldarg_0);
                worker.Emit(OpCodes.Ldfld, syncVar);
                var writeFunc = writer.GetFunction(syncVar.FieldType.IsSubclassOf<NetworkModule>() ? module.Import<NetworkModule>() : syncVar.FieldType, ref failed);

                if (writeFunc != null)
                {
                    worker.Emit(OpCodes.Call, writeFunc);
                }
                else
                {
                    debugger.Error("不支持 {0} 的类型".Format(syncVar.Name), syncVar);
                    failed = true;
                    return;
                }
            }

            worker.Emit(OpCodes.Ret);
            worker.Append(instruction);
            worker.Emit(OpCodes.Ldarg_1);
            worker.Emit(OpCodes.Ldarg_0);
            worker.Emit(OpCodes.Call, module.SyncVarDirty);
            var writeUint64Func = writer.GetFunction(module.Import<ulong>(), ref failed);
            worker.Emit(OpCodes.Call, writeUint64Func);
            int dirty = access.GetSyncVar(expand.BaseType.FullName);
            foreach (var syncVarDef in syncVars)
            {
                FieldReference syncVar = syncVarDef;
                if (expand.HasGenericParameters)
                {
                    syncVar = syncVarDef.MakeHostInstanceGeneric();
                }

                var varLabel = worker.Create(OpCodes.Nop);
                worker.Emit(OpCodes.Ldarg_0);
                worker.Emit(OpCodes.Call, module.SyncVarDirty);
                worker.Emit(OpCodes.Ldc_I8, 1L << dirty);
                worker.Emit(OpCodes.And);
                worker.Emit(OpCodes.Brfalse, varLabel);
                worker.Emit(OpCodes.Ldarg_1);
                worker.Emit(OpCodes.Ldarg_0);
                worker.Emit(OpCodes.Ldfld, syncVar);

                var writeFunc = writer.GetFunction(
                    syncVar.FieldType.IsSubclassOf<NetworkModule>() ? module.Import<NetworkModule>() : syncVar.FieldType,
                    ref failed);

                if (writeFunc != null)
                {
                    worker.Emit(OpCodes.Call, writeFunc);
                }
                else
                {
                    debugger.Error("不支持 {0} 的类型".Format(syncVar.Name), syncVar);
                    failed = true;
                    return;
                }

                worker.Append(varLabel);
                dirty += 1;
            }

            worker.Emit(OpCodes.Ret);
            expand.Methods.Add(serialize);
        }

        /// <summary>
        /// 生成SyncVar的反序列化方法
        /// </summary>
        private void GenerateDeserialize(ref bool failed)
        {
            if (expand.GetMethod(Weaver.MED_DES) != null) return;
            if (syncVars.Count == 0) return;
            var serialize = new MethodDefinition(Weaver.MED_DES, Weaver.GEN_VAR, module.Import(typeof(void)));
            serialize.Parameters.Add(new ParameterDefinition("reader", ParameterAttributes.None, module.Import<MemoryReader>()));
            serialize.Parameters.Add(new ParameterDefinition("initialize", ParameterAttributes.None, module.Import<bool>()));
            var worker = serialize.Body.GetILProcessor();

            serialize.Body.InitLocals = true;
            var dirtyBitsLocal = new VariableDefinition(module.Import<long>());
            serialize.Body.Variables.Add(dirtyBitsLocal);

            var baseDeserialize = Resolve.GetMethod(expand.BaseType, assembly, Weaver.MED_DES);
            if (baseDeserialize != null)
            {
                worker.Append(worker.Create(OpCodes.Ldarg_0));
                worker.Append(worker.Create(OpCodes.Ldarg_1));
                worker.Append(worker.Create(OpCodes.Ldarg_2));
                worker.Append(worker.Create(OpCodes.Call, baseDeserialize));
            }

            var instruction = worker.Create(OpCodes.Nop);

            worker.Append(worker.Create(OpCodes.Ldarg_2));
            worker.Append(worker.Create(OpCodes.Brfalse, instruction));

            foreach (var syncVar in syncVars)
            {
                DeserializeField(syncVar, worker, ref failed);
            }

            worker.Append(worker.Create(OpCodes.Ret));
            worker.Append(instruction);
            worker.Append(worker.Create(OpCodes.Ldarg_1));
            worker.Append(worker.Create(OpCodes.Call, reader.GetFunction(module.Import<ulong>(), ref failed)));
            worker.Append(worker.Create(OpCodes.Stloc_0));

            int dirtyBits = access.GetSyncVar(expand.BaseType.FullName);
            foreach (var syncVar in syncVars)
            {
                var varLabel = worker.Create(OpCodes.Nop);
                worker.Append(worker.Create(OpCodes.Ldloc_0));
                worker.Append(worker.Create(OpCodes.Ldc_I8, 1L << dirtyBits));
                worker.Append(worker.Create(OpCodes.And));
                worker.Append(worker.Create(OpCodes.Brfalse, varLabel));

                DeserializeField(syncVar, worker, ref failed);

                worker.Append(varLabel);
                dirtyBits += 1;
            }

            worker.Append(worker.Create(OpCodes.Ret));
            expand.Methods.Add(serialize);
        }

        /// <summary>
        /// 反序列化字段
        /// </summary>
        private void DeserializeField(FieldDefinition syncVar, ILProcessor worker, ref bool failed)
        {
            worker.Append(worker.Create(OpCodes.Ldarg_0));
            worker.Emit(OpCodes.Ldarg_0);
            worker.Emit(OpCodes.Ldflda, expand.HasGenericParameters ? syncVar.MakeHostInstanceGeneric() : syncVar);

            var hookMethod = process.GetHookMethod(expand, syncVar, ref failed);
            if (hookMethod != null)
            {
                process.GenerateNewActionFromHookMethod(syncVar, worker, hookMethod);
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
                var getFunc = module.SyncVarGetterNetworkModule.MakeGenericInstanceType(assembly.MainModule, syncVar.FieldType);
                worker.Emit(OpCodes.Call, getFunc);
            }
            else
            {
                var readFunc = reader.GetFunction(syncVar.FieldType, ref failed);
                if (readFunc == null)
                {
                    debugger.Error("不支持 {0} 的类型。".Format(syncVar.Name), syncVar);
                    failed = true;
                    return;
                }

                worker.Emit(OpCodes.Ldarg_1);
                worker.Emit(OpCodes.Call, readFunc);
                MethodReference generic = module.SyncVarGetterGeneral.MakeGenericInstanceType(assembly.MainModule, syncVar.FieldType);
                worker.Emit(OpCodes.Call, generic);
            }
        }
    }
}