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
        public readonly MethodReference SyncVarHook;

        /// <summary>
        /// 网络行为被标记改变
        /// </summary>
        public readonly MethodReference SyncVarDirty;

        /// <summary>
        /// Rpc委托的构造函数
        /// </summary>
        public readonly MethodReference InvokeDelegate;

        /// <summary>
        /// 日志出现错误
        /// </summary>
        public readonly MethodReference LogError;

        /// <summary>
        /// 获取NetworkClient.isActive
        /// </summary>
        public readonly MethodReference GetClientActive;

        /// <summary>
        /// 获取NetworkServer.isActive
        /// </summary>
        public readonly MethodReference GetServerActive;

        /// <summary>
        /// 对ArraySegment的构造函数的注入
        /// </summary>
        public readonly MethodReference AddArraySegment;

        /// <summary>
        /// 创建SO方法
        /// </summary>
        public readonly MethodReference CreateInstance;

        /// <summary>
        /// 读取泛型的 NetworkAgent
        /// </summary>
        public readonly MethodReference ReadNetworkAgent;

        /// <summary>
        /// NetworkAgent.SendServerRpcInternal
        /// </summary>
        public readonly MethodReference SendServerRpcInternal;

        /// <summary>
        /// NetworkAgent.SendTargetRpcInternal
        /// </summary>
        public readonly MethodReference SendTargetRpcInternal;

        /// <summary>
        /// NetworkAgent.SendClientRpcInternal
        /// </summary>
        public readonly MethodReference SendClientRpcInternal;

        /// <summary>
        /// NetworkAgent.SyncVarSetterGeneral
        /// </summary>
        public readonly MethodReference SyncVarSetterGeneral;

        /// <summary>
        /// NetworkAgent.SyncVarSetterGameObject
        /// </summary>
        public readonly MethodReference SyncVarSetterGameObject;

        /// <summary>
        /// NetworkAgent.SyncVarSetterNetworkEntity
        /// </summary>
        public readonly MethodReference SyncVarSetterNetworkEntity;

        /// <summary>
        /// NetworkAgent.SyncVarSetterNetworkAgent
        /// </summary>
        public readonly MethodReference SyncVarSetterNetworkAgent;

        /// <summary>
        /// NetworkAgent.SyncVarGetterGeneral
        /// </summary>
        public readonly MethodReference SyncVarGetterGeneral;

        /// <summary>
        /// NetworkAgent.SyncVarGetterGameObject
        /// </summary>
        public readonly MethodReference SyncVarGetterGameObject;

        /// <summary>
        /// NetworkAgent.SyncVarGetterNetworkEntity
        /// </summary>
        public readonly MethodReference SyncVarGetterNetworkEntity;

        /// <summary>
        /// NetworkAgent.SyncVarGetterNetworkAgent
        /// </summary>
        public readonly MethodReference SyncVarGetterNetworkAgent;

        /// <summary>
        /// NetworkAgent.GetSyncVarGameObject
        /// </summary>
        public readonly MethodReference GetSyncVarGameObject;

        /// <summary>
        /// NetworkAgent.GetSyncVarNetworkEntity
        /// </summary>
        public readonly MethodReference GetSyncVarNetworkEntity;

        /// <summary>
        /// NetworkAgent.GetSyncVarNetworkAgent
        /// </summary>
        public readonly MethodReference GetSyncVarNetworkAgent;

        /// <summary>
        /// NetworkUtilsRpc.RegisterServerRpc
        /// </summary>
        public readonly MethodReference RegisterServerRpc;

        /// <summary>
        /// NetworkUtilsRpc.RegisterClientRpc
        /// </summary>
        public readonly MethodReference RegisterClientRpc;

        /// <summary>
        /// NetworkWriter.Pop
        /// </summary>
        public readonly MethodReference WriterDequeue;

        /// <summary>
        /// NetworkWriter.Push
        /// </summary>
        public readonly MethodReference WriterEnqueue;

        /// <summary>
        /// Type.GetTypeFromHandle
        /// </summary>
        public readonly MethodReference GetTypeFromHandle;

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

            SyncVarHook = Resolve.GetMethod(Import(typeof(Action<,>)), assembly, logger, Const.CTOR, ref failed);
            AddArraySegment = Resolve.GetMethod(Import(typeof(ArraySegment<>)), assembly, logger, Const.CTOR, ref failed);
            GetClientActive = Resolve.GetMethod(Import(typeof(NetworkManager.Client)), assembly, logger, "get_isActive", ref failed);
            GetServerActive = Resolve.GetMethod(Import(typeof(NetworkManager.Server)), assembly, logger, "get_isActive", ref failed);

            ReadNetworkAgent = Resolve.GetMethod(Import(typeof(Net.Extensions)), assembly, logger, method => method.Name == nameof(Net.Extensions.ReadNetworkAgent) && method.HasGenericParameters, ref failed);

            var NetworkAgentType = Import<NetworkAgent>();
            SyncVarDirty = Resolve.GetProperty(NetworkAgentType, assembly, "syncVarDirty");
            SyncVarSetterGeneral = Resolve.GetMethod(NetworkAgentType, assembly, logger, "SyncVarSetterGeneral", ref failed);
            SyncVarSetterGameObject = Resolve.GetMethod(NetworkAgentType, assembly, logger, "SyncVarSetterGameObject", ref failed);
            SyncVarSetterNetworkEntity = Resolve.GetMethod(NetworkAgentType, assembly, logger, "SyncVarSetterNetworkEntity", ref failed);
            SyncVarSetterNetworkAgent = Resolve.GetMethod(NetworkAgentType, assembly, logger, "SyncVarSetterNetworkAgent", ref failed);
            
            SyncVarGetterGeneral = Resolve.GetMethod(NetworkAgentType, assembly, logger, "SyncVarGetterGeneral", ref failed);
            SyncVarGetterGameObject = Resolve.GetMethod(NetworkAgentType, assembly, logger, "SyncVarGetterGameObject", ref failed);
            SyncVarGetterNetworkEntity = Resolve.GetMethod(NetworkAgentType, assembly, logger, "SyncVarGetterNetworkEntity", ref failed);
            SyncVarGetterNetworkAgent = Resolve.GetMethod(NetworkAgentType, assembly, logger, "SyncVarGetterNetworkAgent", ref failed);
            
            GetSyncVarGameObject = Resolve.GetMethod(NetworkAgentType, assembly, logger, "GetSyncVarGameObject", ref failed);
            GetSyncVarNetworkEntity = Resolve.GetMethod(NetworkAgentType, assembly, logger, "GetSyncVarNetworkEntity", ref failed);
            GetSyncVarNetworkAgent = Resolve.GetMethod(NetworkAgentType, assembly, logger, "GetSyncVarNetworkAgent", ref failed);
            
            SendServerRpcInternal = Resolve.GetMethod(NetworkAgentType, assembly, logger, "SendServerRpcInternal", ref failed);
            SendClientRpcInternal = Resolve.GetMethod(NetworkAgentType, assembly, logger, "SendClientRpcInternal", ref failed);
            SendTargetRpcInternal = Resolve.GetMethod(NetworkAgentType, assembly, logger, "SendTargetRpcInternal", ref failed);
            
            RegisterServerRpc = Resolve.GetMethod(Import(typeof(NetworkAttribute)), assembly, logger, "RegisterServerRpc", ref failed);
            RegisterClientRpc = Resolve.GetMethod(Import(typeof(NetworkAttribute)), assembly, logger, "RegisterClientRpc", ref failed);

            InvokeDelegate = Resolve.GetMethod(Import<InvokeDelegate>(), assembly, logger, Const.CTOR, ref failed);

            CreateInstance = Resolve.GetMethod(Import<ScriptableObject>(), assembly, logger, method => method.Name == "CreateInstance" && method.HasGenericParameters, ref failed);

            LogError = Resolve.GetMethod(Import<Debug>(), assembly, logger, method => method.Name == "LogError" && method.Parameters.Count == 1 && method.Parameters[0].ParameterType.FullName == typeof(object).FullName, ref failed);

            GetTypeFromHandle = Resolve.GetMethod(Import<Type>(), assembly, logger, "GetTypeFromHandle", ref failed);

            WriterDequeue = Resolve.GetMethod(Import<MemoryWriter>(), assembly, logger, "Pop", ref failed);
            WriterEnqueue = Resolve.GetMethod(Import<MemoryWriter>(), assembly, logger, "Push", ref failed);

            if (Resolve.IsEditor(assembly))
            {
                InitializeOnLoadMethodAttribute = Import<InitializeOnLoadMethodAttribute>().Resolve();
            }

            RuntimeInitializeOnLoadMethodAttribute = Import<RuntimeInitializeOnLoadMethodAttribute>().Resolve();
        }
    }
}