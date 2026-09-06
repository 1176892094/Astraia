// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-09-06 23:09:31
// # Recently: 2026-09-06 23:15:31
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using Astraia.Net;
using Mono.Cecil;
using UnityEngine;

namespace Astraia
{
    internal sealed class Module
    {
        private readonly AssemblyDefinition assembly;
        public readonly TypeDefinition Initialized;

        public readonly MemberReference ModuleGeneric;

        public readonly MethodReference Listen;
        public readonly MethodReference Remove;
        public readonly MethodReference Export;

        public readonly MethodReference LogError;
        public readonly MethodReference SyncVarHook;
        public readonly MethodReference InvokeDelegate;
        public readonly MethodReference AddArraySegment;
        public readonly MethodReference GetTypeFromHandle;
        public readonly MethodReference ReadNetworkModule;

        public readonly MethodReference WriterDequeue;
        public readonly MethodReference WriterEnqueue;
        public readonly MethodReference GetClientActive;
        public readonly MethodReference GetServerActive;
        public readonly MethodReference RegisterServerRpc;
        public readonly MethodReference RegisterClientRpc;

        public readonly MethodReference SyncVarDirty;
        public readonly MethodReference SyncVarGetterGeneral;
        public readonly MethodReference SyncVarGetterGameObject;
        public readonly MethodReference SyncVarGetterNetworkEntity;
        public readonly MethodReference SyncVarGetterNetworkModule;

        public readonly MethodReference SyncVarSetterGeneral;
        public readonly MethodReference SyncVarSetterGameObject;
        public readonly MethodReference SyncVarSetterNetworkEntity;
        public readonly MethodReference SyncVarSetterNetworkModule;

        public readonly MethodReference GetSyncVarGameObject;
        public readonly MethodReference GetSyncVarNetworkEntity;
        public readonly MethodReference GetSyncVarNetworkModule;

        public readonly MethodReference SendServerRpcInternal;
        public readonly MethodReference SendTargetRpcInternal;
        public readonly MethodReference SendClientRpcInternal;

        public Module(AssemblyDefinition assembly, AssemblyDebugger Log, ref bool failed)
        {
            this.assembly = assembly;
            Initialized = Import<RuntimeInitializeOnLoadMethodAttribute>().Resolve();

            LogError = Import<Debug>().GetMethod(assembly, OnLogError, Log, ref failed);
            SyncVarHook = Import(typeof(Action<,>)).GetMethod(assembly, Weaver.MED_C1, Log, ref failed);
            InvokeDelegate = Import<SyncFunc>().GetMethod(assembly, Weaver.MED_C1, Log, ref failed);
            AddArraySegment = Import(typeof(ArraySegment<>)).GetMethod(assembly, Weaver.MED_C1, Log, ref failed);
            GetTypeFromHandle = Import<Type>().GetMethod(assembly, "GetTypeFromHandle", Log, ref failed);
            ReadNetworkModule = Import(typeof(ReaderExtensions)).GetMethod(assembly, ReadModule, Log, ref failed);

            Listen = Import(typeof(EventManager)).GetMethod(assembly, nameof(Listen), Log, ref failed);
            Remove = Import(typeof(EventManager)).GetMethod(assembly, nameof(Remove), Log, ref failed);
            Export = Import(typeof(ExportManager)).GetMethod(assembly, nameof(Export), Log, ref failed);

            WriterDequeue = Import<MemoryWriter>().GetMethod(assembly, "Pop", Log, ref failed);
            WriterEnqueue = Import<MemoryWriter>().GetMethod(assembly, "Push", Log, ref failed);
            GetClientActive = Import<NetworkManager>().GetMethod(assembly, "get_isClient", Log, ref failed);
            GetServerActive = Import<NetworkManager>().GetMethod(assembly, "get_isServer", Log, ref failed);
            RegisterServerRpc = Import(typeof(NetworkAttribute)).GetMethod(assembly, nameof(RegisterServerRpc), Log, ref failed);
            RegisterClientRpc = Import(typeof(NetworkAttribute)).GetMethod(assembly, nameof(RegisterClientRpc), Log, ref failed);

            var module = Import<NetworkModule>();
            SyncVarDirty = module.GetProperty(assembly, "syncVarDirty");
            SyncVarGetterGeneral = module.GetMethod(assembly, nameof(SyncVarGetterGeneral), Log, ref failed);
            SyncVarGetterGameObject = module.GetMethod(assembly, nameof(SyncVarGetterGameObject), Log, ref failed);
            SyncVarGetterNetworkEntity = module.GetMethod(assembly, nameof(SyncVarGetterNetworkEntity), Log, ref failed);
            SyncVarGetterNetworkModule = module.GetMethod(assembly, nameof(SyncVarGetterNetworkModule), Log, ref failed);

            SyncVarSetterGeneral = module.GetMethod(assembly, nameof(SyncVarSetterGeneral), Log, ref failed);
            SyncVarSetterGameObject = module.GetMethod(assembly, nameof(SyncVarSetterGameObject), Log, ref failed);
            SyncVarSetterNetworkEntity = module.GetMethod(assembly, nameof(SyncVarSetterNetworkEntity), Log, ref failed);
            SyncVarSetterNetworkModule = module.GetMethod(assembly, nameof(SyncVarSetterNetworkModule), Log, ref failed);

            GetSyncVarGameObject = module.GetMethod(assembly, nameof(GetSyncVarGameObject), Log, ref failed);
            GetSyncVarNetworkEntity = module.GetMethod(assembly, nameof(GetSyncVarNetworkEntity), Log, ref failed);
            GetSyncVarNetworkModule = module.GetMethod(assembly, nameof(GetSyncVarNetworkModule), Log, ref failed);

            SendServerRpcInternal = module.GetMethod(assembly, nameof(SendServerRpcInternal), Log, ref failed);
            SendClientRpcInternal = module.GetMethod(assembly, nameof(SendClientRpcInternal), Log, ref failed);
            SendTargetRpcInternal = module.GetMethod(assembly, nameof(SendTargetRpcInternal), Log, ref failed);
        }

        public TypeReference Import(Type t)
        {
            return assembly.MainModule.ImportReference(t);
        }

        public TypeReference Import<T>()
        {
            return Import(typeof(T));
        }

        private static bool OnLogError(MethodDefinition md)
        {
            return md.Name == "LogError" && md.Parameters.Count == 1 && md.Parameters[0].ParameterType.FullName == typeof(object).FullName;
        }

        private static bool ReadModule(MethodDefinition md)
        {
            return md.Name == nameof(ReaderExtensions.ReadNetworkModule) && md.HasGenericParameters;
        }
    }
}