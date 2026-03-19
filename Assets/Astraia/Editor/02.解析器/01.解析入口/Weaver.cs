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
using System.Diagnostics;
using System.Linq;
using Astraia.Net;
using Mono.Cecil;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Member = Mono.Cecil.TypeAttributes;
using Method = Mono.Cecil.MethodAttributes;

namespace Astraia.Editor
{
    [Serializable]
    internal sealed class Weaver
    {
        public const int BIT_COUNT = 64;
        public const string GEN_TYPE = "Astraia.Net";
        public const string GEN_CTOR = ".ctor";
        public const string GEN_CCTOR = ".cctor";
        public const string MED_RPC = "V1";
        public const string MED_INV = "V2";
        public const string MED_SER = "SerializeSyncVars";
        public const string MED_DES = "DeserializeSyncVars";
        public const string GEN_FUN = nameof(NetworkProcessor);
        public const Method GEN_RPC = Method.HideBySig | Method.Family | Method.Static;
        public const Method GEN_RAW = Method.HideBySig | Method.Public | Method.Static;
        public const Method GEN_VAR = Method.HideBySig | Method.Public | Method.Virtual;
        public const Method GEN_SYNC = Method.HideBySig | Method.Public | Method.SpecialName;
        public const Method GEN_DATA = Method.HideBySig | Method.Static | Method.SpecialName | Method.Private | Method.RTSpecialName;
        public const Member GEN_ATTR = Member.AutoClass | Member.Public | Member.Class | Member.AnsiClass | Member.Abstract | Member.Sealed | Member.BeforeFieldInit;

        public bool Weave(AssemblyDefinition assembly, ILogPostProcessor debugger, IAssemblyResolver resolver, out bool modified)
        {
            modified = false;
            try
            {
                var failed = false;
                if (assembly.MainModule.GetTypes().Any(td => td.Namespace == GEN_TYPE && td.Name == GEN_FUN))
                {
                    return true;
                }

                var elapse = Stopwatch.StartNew();
                var access = new SyncVarAccess();
                var module = new Module(assembly, debugger, ref failed);
                var expand = new TypeDefinition(GEN_TYPE, GEN_FUN, GEN_ATTR, module.Import<object>());
                var writer = new Writer(assembly, module, expand, debugger);
                var reader = new Reader(assembly, module, expand, debugger);
                modified = NetworkRuntime.Process(assembly, resolver, debugger, writer, reader, ref failed);

                var mainModule = assembly.MainModule;
                foreach (var td in mainModule.Types.Where(td => td.IsSubclassOf<NetworkModule>()))
                {
                    var parent = td;
                    while (parent != null)
                    {
                        if (parent.Is<NetworkModule>())
                        {
                            break;
                        }

                        modified |= new NetworkMember(assembly, access, module, writer, reader, debugger, parent).Process(ref failed);
                        parent = parent.GetBaseType();
                    }
                }

                if (failed)
                {
                    return false;
                }

                if (modified)
                {
                    SyncVarReplace.Process(mainModule, access);
                    mainModule.Types.Add(expand);
                    NetworkRuntime.Processed(assembly, module, writer, reader, expand);
                }

                elapse.Stop();
                //  debugger.Warn("{0:F2}ms ".Color("G").Format(elapse.ElapsedMilliseconds / 1000F) + assembly.Name.Name);
                return true;
            }
            catch (Exception e)
            {
                debugger.Error(e.ToString());
                return false;
            }
        }
    }

    internal sealed class Module
    {
        private readonly AssemblyDefinition assembly;
        public readonly TypeDefinition Initialized;

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

        public Module(AssemblyDefinition assembly, ILogPostProcessor debugger, ref bool failed)
        {
            this.assembly = assembly;
            Initialized = Import<RuntimeInitializeOnLoadMethodAttribute>().Resolve();
            LogError = Common.GetMethod(Import<Debug>(), assembly, OnLogError, debugger, ref failed);
            SyncVarHook = Common.GetMethod(Import(typeof(Action<,>)), assembly, Weaver.GEN_CTOR, debugger, ref failed);
            InvokeDelegate = Common.GetMethod(Import<InvokeDelegate>(), assembly, Weaver.GEN_CTOR, debugger, ref failed);
            AddArraySegment = Common.GetMethod(Import(typeof(ArraySegment<>)), assembly, Weaver.GEN_CTOR, debugger, ref failed);
            GetTypeFromHandle = Common.GetMethod(Import<Type>(), assembly, "GetTypeFromHandle", debugger, ref failed);
            ReadNetworkModule = Common.GetMethod(Import(typeof(Net.Extensions)), assembly, ReadModule, debugger, ref failed);

            WriterDequeue = Common.GetMethod(Import<MemoryWriter>(), assembly, "Pop", debugger, ref failed);
            WriterEnqueue = Common.GetMethod(Import<MemoryWriter>(), assembly, "Push", debugger, ref failed);
            GetClientActive = Common.GetMethod(Import<NetworkManager>(), assembly, "get_isClient", debugger, ref failed);
            GetServerActive = Common.GetMethod(Import<NetworkManager>(), assembly, "get_isServer", debugger, ref failed);
            RegisterServerRpc = Common.GetMethod(Import(typeof(NetworkAttribute)), assembly, nameof(RegisterServerRpc), debugger, ref failed);
            RegisterClientRpc = Common.GetMethod(Import(typeof(NetworkAttribute)), assembly, nameof(RegisterClientRpc), debugger, ref failed);

            var NetworkModuleType = Import<NetworkModule>();
            SyncVarDirty = Common.GetProperty(NetworkModuleType, assembly, "syncVarDirty");
            SyncVarGetterGeneral = Common.GetMethod(NetworkModuleType, assembly, nameof(SyncVarGetterGeneral), debugger, ref failed);
            SyncVarGetterGameObject = Common.GetMethod(NetworkModuleType, assembly, nameof(SyncVarGetterGameObject), debugger, ref failed);
            SyncVarGetterNetworkEntity = Common.GetMethod(NetworkModuleType, assembly, nameof(SyncVarGetterNetworkEntity), debugger, ref failed);
            SyncVarGetterNetworkModule = Common.GetMethod(NetworkModuleType, assembly, nameof(SyncVarGetterNetworkModule), debugger, ref failed);

            SyncVarSetterGeneral = Common.GetMethod(NetworkModuleType, assembly, nameof(SyncVarSetterGeneral), debugger, ref failed);
            SyncVarSetterGameObject = Common.GetMethod(NetworkModuleType, assembly, nameof(SyncVarSetterGameObject), debugger, ref failed);
            SyncVarSetterNetworkEntity = Common.GetMethod(NetworkModuleType, assembly, nameof(SyncVarSetterNetworkEntity), debugger, ref failed);
            SyncVarSetterNetworkModule = Common.GetMethod(NetworkModuleType, assembly, nameof(SyncVarSetterNetworkModule), debugger, ref failed);

            GetSyncVarGameObject = Common.GetMethod(NetworkModuleType, assembly, nameof(GetSyncVarGameObject), debugger, ref failed);
            GetSyncVarNetworkEntity = Common.GetMethod(NetworkModuleType, assembly, nameof(GetSyncVarNetworkEntity), debugger, ref failed);
            GetSyncVarNetworkModule = Common.GetMethod(NetworkModuleType, assembly, nameof(GetSyncVarNetworkModule), debugger, ref failed);

            SendServerRpcInternal = Common.GetMethod(NetworkModuleType, assembly, nameof(SendServerRpcInternal), debugger, ref failed);
            SendClientRpcInternal = Common.GetMethod(NetworkModuleType, assembly, nameof(SendClientRpcInternal), debugger, ref failed);
            SendTargetRpcInternal = Common.GetMethod(NetworkModuleType, assembly, nameof(SendTargetRpcInternal), debugger, ref failed);
        }

        public TypeReference Import(Type t)
        {
            return assembly.MainModule.ImportReference(t);
        }

        public TypeReference Import<T>()
        {
            return Import(typeof(T));
        }

        private static bool OnLogError(MethodDefinition method)
        {
            return method.Name == "LogError" && method.Parameters.Count == 1 && method.Parameters[0].ParameterType.FullName == typeof(object).FullName;
        }

        private static bool ReadModule(MethodDefinition method)
        {
            return method.Name == nameof(Net.Extensions.ReadNetworkModule) && method.HasGenericParameters;
        }
    }
}