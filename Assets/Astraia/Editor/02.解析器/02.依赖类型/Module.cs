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
        /// 读取泛型的 NetworkModule
        /// </summary>
        public readonly MethodReference ReadNetworkModule;

        /// <summary>
        /// NetworkModule.SendServerRpcInternal
        /// </summary>
        public readonly MethodReference SendServerRpcInternal;

        /// <summary>
        /// NetworkModule.SendTargetRpcInternal
        /// </summary>
        public readonly MethodReference SendTargetRpcInternal;

        /// <summary>
        /// NetworkModule.SendClientRpcInternal
        /// </summary>
        public readonly MethodReference SendClientRpcInternal;

        /// <summary>
        /// NetworkModule.SyncVarSetterGeneral
        /// </summary>
        public readonly MethodReference SyncVarSetterGeneral;

        /// <summary>
        /// NetworkModule.SyncVarSetterGameObject
        /// </summary>
        public readonly MethodReference SyncVarSetterGameObject;

        /// <summary>
        /// NetworkModule.SyncVarSetterNetworkEntity
        /// </summary>
        public readonly MethodReference SyncVarSetterNetworkEntity;

        /// <summary>
        /// NetworkModule.SyncVarSetterNetworkModule
        /// </summary>
        public readonly MethodReference SyncVarSetterNetworkModule;

        /// <summary>
        /// NetworkModule.SyncVarGetterGeneral
        /// </summary>
        public readonly MethodReference SyncVarGetterGeneral;

        /// <summary>
        /// NetworkModule.SyncVarGetterGameObject
        /// </summary>
        public readonly MethodReference SyncVarGetterGameObject;

        /// <summary>
        /// NetworkModule.SyncVarGetterNetworkEntity
        /// </summary>
        public readonly MethodReference SyncVarGetterNetworkEntity;

        /// <summary>
        /// NetworkModule.SyncVarGetterNetworkModule
        /// </summary>
        public readonly MethodReference SyncVarGetterNetworkModule;

        /// <summary>
        /// NetworkModule.GetSyncVarGameObject
        /// </summary>
        public readonly MethodReference GetSyncVarGameObject;

        /// <summary>
        /// NetworkModule.GetSyncVarNetworkEntity
        /// </summary>
        public readonly MethodReference GetSyncVarNetworkEntity;

        /// <summary>
        /// NetworkModule.GetSyncVarNetworkModule
        /// </summary>
        public readonly MethodReference GetSyncVarNetworkModule;

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
        /// <param name="debugger"></param>
        /// <param name="failed"></param>
        public Module(AssemblyDefinition assembly, ILogPostProcessor debugger, ref bool failed)
        {
            this.assembly = assembly;
            LogError = Resolve.GetMethod(Import<Debug>(), assembly, OnLogError, debugger, ref failed);
            SyncVarHook = Resolve.GetMethod(Import(typeof(Action<,>)), assembly, Weaver.CTOR, debugger, ref failed);
            InvokeDelegate = Resolve.GetMethod(Import<InvokeDelegate>(), assembly, Weaver.CTOR, debugger, ref failed);
            AddArraySegment = Resolve.GetMethod(Import(typeof(ArraySegment<>)), assembly, Weaver.CTOR, debugger, ref failed);
            GetTypeFromHandle = Resolve.GetMethod(Import<Type>(), assembly, "GetTypeFromHandle", debugger, ref failed);
            ReadNetworkModule = Resolve.GetMethod(Import(typeof(Net.Extensions)), assembly, NetworkModule, debugger, ref failed);

            WriterDequeue = Resolve.GetMethod(Import<MemoryWriter>(), assembly, "Pop", debugger, ref failed);
            WriterEnqueue = Resolve.GetMethod(Import<MemoryWriter>(), assembly, "Push", debugger, ref failed);
            GetClientActive = Resolve.GetMethod(Import<NetworkManager>(), assembly, "get_isClient", debugger, ref failed);
            GetServerActive = Resolve.GetMethod(Import<NetworkManager>(), assembly, "get_isServer", debugger, ref failed);

            var NetworkModuleType = Import<NetworkModule>();
            SyncVarDirty = Resolve.GetProperty(NetworkModuleType, assembly, "syncVarDirty");
            SyncVarSetterGeneral = Resolve.GetMethod(NetworkModuleType, assembly, "SyncVarSetterGeneral", debugger, ref failed);
            SyncVarSetterGameObject = Resolve.GetMethod(NetworkModuleType, assembly, "SyncVarSetterGameObject", debugger, ref failed);
            SyncVarSetterNetworkEntity = Resolve.GetMethod(NetworkModuleType, assembly, "SyncVarSetterNetworkEntity", debugger, ref failed);
            SyncVarSetterNetworkModule = Resolve.GetMethod(NetworkModuleType, assembly, "SyncVarSetterNetworkModule", debugger, ref failed);

            SyncVarGetterGeneral = Resolve.GetMethod(NetworkModuleType, assembly, "SyncVarGetterGeneral", debugger, ref failed);
            SyncVarGetterGameObject = Resolve.GetMethod(NetworkModuleType, assembly, "SyncVarGetterGameObject", debugger, ref failed);
            SyncVarGetterNetworkEntity = Resolve.GetMethod(NetworkModuleType, assembly, "SyncVarGetterNetworkEntity", debugger, ref failed);
            SyncVarGetterNetworkModule = Resolve.GetMethod(NetworkModuleType, assembly, "SyncVarGetterNetworkModule", debugger, ref failed);

            GetSyncVarGameObject = Resolve.GetMethod(NetworkModuleType, assembly, "GetSyncVarGameObject", debugger, ref failed);
            GetSyncVarNetworkEntity = Resolve.GetMethod(NetworkModuleType, assembly, "GetSyncVarNetworkEntity", debugger, ref failed);
            GetSyncVarNetworkModule = Resolve.GetMethod(NetworkModuleType, assembly, "GetSyncVarNetworkModule", debugger, ref failed);

            SendServerRpcInternal = Resolve.GetMethod(NetworkModuleType, assembly, "SendServerRpcInternal", debugger, ref failed);
            SendClientRpcInternal = Resolve.GetMethod(NetworkModuleType, assembly, "SendClientRpcInternal", debugger, ref failed);
            SendTargetRpcInternal = Resolve.GetMethod(NetworkModuleType, assembly, "SendTargetRpcInternal", debugger, ref failed);

            RegisterServerRpc = Resolve.GetMethod(Import(typeof(NetworkAttribute)), assembly, "RegisterServerRpc", debugger, ref failed);
            RegisterClientRpc = Resolve.GetMethod(Import(typeof(NetworkAttribute)), assembly, "RegisterClientRpc", debugger, ref failed);

            if (Resolve.IsEditor(assembly))
            {
                InitializeOnLoadMethodAttribute = Import<InitializeOnLoadMethodAttribute>().Resolve();
            }

            RuntimeInitializeOnLoadMethodAttribute = Import<RuntimeInitializeOnLoadMethodAttribute>().Resolve();
        }

        private static bool NetworkModule(MethodDefinition method)
        {
            return method.Name == nameof(Net.Extensions.ReadNetworkModule) && method.HasGenericParameters;
        }

        private static bool OnLogError(MethodDefinition method)
        {
            return method.Name == "LogError" && method.Parameters.Count == 1 && method.Parameters[0].ParameterType.FullName == typeof(object).FullName;
        }
    }
}