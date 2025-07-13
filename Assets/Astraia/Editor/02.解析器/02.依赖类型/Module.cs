// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2024-12-19 03:12:36
// # Recently: 2024-12-22 20:12:33
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using Astraia.Net;
using Mono.Cecil;
using UnityEditor;
using UnityEngine;

namespace Astraia.Editor
{
    internal class Module
    {
        /// <summary>
        /// 注入的指定程序集
        /// </summary>
        private readonly AssemblyDefinition assembly;

        /// <summary>
        /// 当网络变量值改变时调用的方法
        /// </summary>
        public readonly MethodReference HookMethodRef;

        /// <summary>
        /// 网络行为被标记改变
        /// </summary>
        public readonly MethodReference NetworkSourceDirtyRef;

        /// <summary>
        /// Rpc委托的构造函数
        /// </summary>
        public readonly MethodReference RpcDelegateRef;

        /// <summary>
        /// 日志出现错误
        /// </summary>
        public readonly MethodReference logErrorRef;

        /// <summary>
        /// 获取NetworkClient.isActive
        /// </summary>
        public readonly MethodReference NetworkClientActiveRef;

        /// <summary>
        /// 获取NetworkServer.isActive
        /// </summary>
        public readonly MethodReference NetworkServerActiveRef;

        /// <summary>
        /// 对ArraySegment的构造函数的注入
        /// </summary>
        public readonly MethodReference ArraySegmentRef;

        /// <summary>
        /// 创建SO方法
        /// </summary>
        public readonly MethodReference CreateInstanceMethodRef;

        /// <summary>
        /// 读取泛型的 NetworkSource
        /// </summary>
        public readonly MethodReference ReadNetworkSourceGeneric;

        /// <summary>
        /// NetworkSource.SendServerRpcInternal
        /// </summary>
        public readonly MethodReference sendServerRpcInternal;

        /// <summary>
        /// NetworkSource.SendTargetRpcInternal
        /// </summary>
        public readonly MethodReference sendTargetRpcInternal;

        /// <summary>
        /// NetworkSource.SendClientRpcInternal
        /// </summary>
        public readonly MethodReference sendClientRpcInternal;

        /// <summary>
        /// NetworkSource.SyncVarSetterGeneral
        /// </summary>
        public readonly MethodReference syncVarSetterGeneral;

        /// <summary>
        /// NetworkSource.SyncVarSetterGameObject
        /// </summary>
        public readonly MethodReference syncVarSetterGameObject;

        /// <summary>
        /// NetworkSource.SyncVarSetterNetworkObject
        /// </summary>
        public readonly MethodReference syncVarSetterNetworkObject;

        /// <summary>
        /// NetworkSource.SyncVarSetterNetworkSource
        /// </summary>
        public readonly MethodReference syncVarSetterNetworkSource;

        /// <summary>
        /// NetworkSource.SyncVarGetterGeneral
        /// </summary>
        public readonly MethodReference syncVarGetterGeneral;

        /// <summary>
        /// NetworkSource.SyncVarGetterGameObject
        /// </summary>
        public readonly MethodReference syncVarGetterGameObject;

        /// <summary>
        /// NetworkSource.SyncVarGetterNetworkObject
        /// </summary>
        public readonly MethodReference syncVarGetterNetworkObject;

        /// <summary>
        /// NetworkSource.SyncVarGetterNetworkSource
        /// </summary>
        public readonly MethodReference syncVarGetterNetworkSource;

        /// <summary>
        /// NetworkSource.GetSyncVarGameObject
        /// </summary>
        public readonly MethodReference getSyncVarGameObject;

        /// <summary>
        /// NetworkSource.GetSyncVarNetworkObject
        /// </summary>
        public readonly MethodReference getSyncVarNetworkObject;

        /// <summary>
        /// NetworkSource.GetSyncVarNetworkSource
        /// </summary>
        public readonly MethodReference getSyncVarNetworkSource;

        /// <summary>
        /// NetworkUtilsRpc.RegisterServerRpc
        /// </summary>
        public readonly MethodReference registerServerRpcRef;

        /// <summary>
        /// NetworkUtilsRpc.RegisterClientRpc
        /// </summary>
        public readonly MethodReference registerClientRpcRef;

        /// <summary>
        /// NetworkWriter.Pop
        /// </summary>
        public readonly MethodReference PopSetterRef;

        /// <summary>
        /// NetworkWriter.Push
        /// </summary>
        public readonly MethodReference PushSetterRef;

        /// <summary>
        /// Type.GetTypeFromHandle
        /// </summary>
        public readonly MethodReference getTypeFromHandleRef;

        /// <summary>
        /// InitializeOnLoadMethodAttribute
        /// </summary>
        public readonly TypeDefinition InitializeOnLoadMethodAttribute;

        /// <summary>
        /// RuntimeInitializeOnLoadMethodAttribute
        /// </summary>
        public readonly TypeDefinition RuntimeInitializeOnLoadMethodAttribute;

        /// <summary>
        /// 导入类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public TypeReference Import<T>() => Import(typeof(T));

        /// <summary>
        /// 导入类型反射器
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public TypeReference Import(Type t) => assembly.MainModule.ImportReference(t);

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="logger"></param>
        /// <param name="failed"></param>
        public Module(AssemblyDefinition assembly, Logger logger, ref bool failed)
        {
            this.assembly = assembly;

            var HookMethodType = Import(typeof(Action<,>));
            HookMethodRef = Resolve.GetMethod(HookMethodType, assembly, logger, Const.CTOR, ref failed);

            var ArraySegmentType = Import(typeof(ArraySegment<>));
            ArraySegmentRef = Resolve.GetMethod(ArraySegmentType, assembly, logger, Const.CTOR, ref failed);
            
            var NetworkClientType = Import(typeof(NetworkManager.Client));
            NetworkClientActiveRef = Resolve.GetMethod(NetworkClientType, assembly, logger, "get_isActive", ref failed);
            var NetworkServerType = Import(typeof(NetworkManager.Server));
            NetworkServerActiveRef = Resolve.GetMethod(NetworkServerType, assembly, logger, "get_isActive", ref failed);

            var StreamExtensionType = Import(typeof(Net.Extensions));
            ReadNetworkSourceGeneric = Resolve.GetMethod(StreamExtensionType, assembly, logger, method => method.Name == nameof(Net.Extensions.GetNetworkSource) && method.HasGenericParameters, ref failed);

            var NetworkSourceType = Import<NetworkSource>();
            NetworkSourceDirtyRef = Resolve.GetProperty(NetworkSourceType, assembly, "syncVarDirty");

            syncVarSetterGeneral = Resolve.GetMethod(NetworkSourceType, assembly, logger, "SyncVarSetterGeneral", ref failed);
            syncVarSetterGameObject = Resolve.GetMethod(NetworkSourceType, assembly, logger, "SyncVarSetterGameObject", ref failed);
            syncVarSetterNetworkObject = Resolve.GetMethod(NetworkSourceType, assembly, logger, "SyncVarSetterNetworkObject", ref failed);
            syncVarSetterNetworkSource = Resolve.GetMethod(NetworkSourceType, assembly, logger, "SyncVarSetterNetworkSource", ref failed);

            syncVarGetterGeneral = Resolve.GetMethod(NetworkSourceType, assembly, logger, "SyncVarGetterGeneral", ref failed);
            syncVarGetterGameObject = Resolve.GetMethod(NetworkSourceType, assembly, logger, "SyncVarGetterGameObject", ref failed);
            syncVarGetterNetworkObject = Resolve.GetMethod(NetworkSourceType, assembly, logger, "SyncVarGetterNetworkObject", ref failed);
            syncVarGetterNetworkSource = Resolve.GetMethod(NetworkSourceType, assembly, logger, "SyncVarGetterNetworkSource", ref failed);

            getSyncVarGameObject = Resolve.GetMethod(NetworkSourceType, assembly, logger, "GetSyncVarGameObject", ref failed);
            getSyncVarNetworkObject = Resolve.GetMethod(NetworkSourceType, assembly, logger, "GetSyncVarNetworkObject", ref failed);
            getSyncVarNetworkSource = Resolve.GetMethod(NetworkSourceType, assembly, logger, "GetSyncVarNetworkSource", ref failed);

            sendServerRpcInternal = Resolve.GetMethod(NetworkSourceType, assembly, logger, "SendServerRpcInternal", ref failed);
            sendClientRpcInternal = Resolve.GetMethod(NetworkSourceType, assembly, logger, "SendClientRpcInternal", ref failed);
            sendTargetRpcInternal = Resolve.GetMethod(NetworkSourceType, assembly, logger, "SendTargetRpcInternal", ref failed);

            var InvokeType = Import(typeof(NetworkAttribute));
            registerServerRpcRef = Resolve.GetMethod(InvokeType, assembly, logger, "RegisterServerRpc", ref failed);
            registerClientRpcRef = Resolve.GetMethod(InvokeType, assembly, logger, "RegisterClientRpc", ref failed);

            var RpcDelegateType = Import<InvokeDelegate>();
            RpcDelegateRef = Resolve.GetMethod(RpcDelegateType, assembly, logger, Const.CTOR, ref failed);

            var ScriptableObjectType = Import<ScriptableObject>();
            CreateInstanceMethodRef = Resolve.GetMethod(ScriptableObjectType, assembly, logger, method => method.Name == "CreateInstance" && method.HasGenericParameters, ref failed);

            var DebugType = Import(typeof(Debug));
            logErrorRef = Resolve.GetMethod(DebugType, assembly, logger, method => method.Name == "LogError" && method.Parameters.Count == 1 && method.Parameters[0].ParameterType.FullName == typeof(object).FullName, ref failed);

            var Type = Import(typeof(Type));
            getTypeFromHandleRef = Resolve.GetMethod(Type, assembly, logger, "GetTypeFromHandle", ref failed);

            var NetworkWriterType = Import(typeof(MemoryWriter));
            PopSetterRef = Resolve.GetMethod(NetworkWriterType, assembly, logger, "Pop", ref failed);
            PushSetterRef = Resolve.GetMethod(NetworkWriterType, assembly, logger, "Push", ref failed);

            if (Resolve.IsEditor(assembly))
            {
                var InitializeOnLoadMethodAttributeType = Import(typeof(InitializeOnLoadMethodAttribute));
                InitializeOnLoadMethodAttribute = InitializeOnLoadMethodAttributeType.Resolve();
            }

            var RuntimeInitializeOnLoadMethodAttributeType = Import(typeof(RuntimeInitializeOnLoadMethodAttribute));
            RuntimeInitializeOnLoadMethodAttribute = RuntimeInitializeOnLoadMethodAttributeType.Resolve();
        }
    }
}