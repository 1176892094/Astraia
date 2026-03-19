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
using System.Collections.Generic;
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
    internal class Weaver
    {
        public const int SYNC_LIMIT = 64;
        public const string CTOR = ".ctor";
        public const string GEN_TYPE = "Astraia.Net";
        public const string GEN_SKIP = "ILPP_IGNORE";
        public const string GEN_FUNC = "NetworkProcessor";
        public const string INV_METHOD = "_0";
        public const string RPC_METHOD = "_1";
        public const string SER_METHOD = "SerializeSyncVars";
        public const string DES_METHOD = "DeserializeSyncVars";
        public const Method GEN_RPC = Method.HideBySig | Method.Family | Method.Static;
        public const Method GEN_RAW = Method.HideBySig | Method.Public | Method.Static;
        public const Method GEN_VAR = Method.HideBySig | Method.Public | Method.Virtual;
        public const Method GEN_SYNC = Method.HideBySig | Method.Public | Method.SpecialName;
        public const Method GEN_CTOR = Method.HideBySig | Method.Static | Method.SpecialName | Method.Private | Method.RTSpecialName;
        private const Member GEN_ATTR = Member.AutoClass | Member.Public | Member.Class | Member.AnsiClass | Member.Abstract | Member.Sealed | Member.BeforeFieldInit;

        private bool failed;
        private Module module;
        private Writer writer;
        private Reader reader;
        private SyncVarAccess access;
        private TypeDefinition expand;
        private AssemblyDefinition assembly;
        private readonly ILogPostProcessor debugger;

        public Weaver(ILogPostProcessor debugger)
        {
            this.debugger = debugger;
        }

        public bool Weave(AssemblyDefinition assembly, IAssemblyResolver resolver, out bool modified)
        {
            failed = false;
            modified = false;
            try
            {
                this.assembly = assembly;
                var watch = Stopwatch.StartNew();
                if (assembly.MainModule.GetTypes().Any(type => type.Namespace == GEN_TYPE && type.Name == nameof(NetworkProcessor)))
                {
                    return true;
                }

                access = new SyncVarAccess();
                module = new Module(assembly, debugger, ref failed);

                expand = new TypeDefinition(GEN_TYPE, nameof(NetworkProcessor), GEN_ATTR, module.Import<object>());
                writer = new Writer(assembly, module, expand, debugger);
                reader = new Reader(assembly, module, expand, debugger);
                modified = NetworkRuntime.Process(assembly, resolver, debugger, writer, reader, ref failed);

                var mainModule = assembly.MainModule;
                foreach (var td in mainModule.Types)
                {
                    if (td.IsClass && td.BaseType.CanResolve())
                    {
                        modified |= ProcessModule(td, ref failed);
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

                watch.Stop();
                debugger.Warn("Module: {0:F2} 秒  ".Color("G").Format(watch.ElapsedMilliseconds / 1000F) + assembly.Name.Name);
                return true;
            }
            catch (Exception e)
            {
                failed = true;
                debugger.Error(e.ToString());
                return false;
            }
        }

        private bool ProcessModule(TypeDefinition td, ref bool failed)
        {
            if (!td.IsClass)
            {
                return false;
            }

            if (!td.IsDerivedFrom<NetworkModule>())
            {
                return false;
            }

            var modules = new List<TypeDefinition>();
            var parent = td;
            while (parent != null)
            {
                if (parent.Is<NetworkModule>())
                {
                    break;
                }

                try
                {
                    modules.Insert(0, parent);
                    parent = parent.BaseType.Resolve();
                }
                catch (AssemblyResolutionException)
                {
                    break;
                }
            }

            var changed = false;
            foreach (var m in modules)
            {
                changed |= new NetworkMember(assembly, access, module, writer, reader, debugger, m).Process(ref failed);
            }

            return changed;
        }


        public static string GetMethodName(MethodDefinition md, string prefix)
        {
            return md.Name + prefix;
        }
    }

    internal class Module
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
            LogError = Resolve.GetMethod(Import<Debug>(), assembly, OnLogError, debugger, ref failed);
            SyncVarHook = Resolve.GetMethod(Import(typeof(Action<,>)), assembly, Weaver.CTOR, debugger, ref failed);
            InvokeDelegate = Resolve.GetMethod(Import<InvokeDelegate>(), assembly, Weaver.CTOR, debugger, ref failed);
            AddArraySegment = Resolve.GetMethod(Import(typeof(ArraySegment<>)), assembly, Weaver.CTOR, debugger, ref failed);
            GetTypeFromHandle = Resolve.GetMethod(Import<Type>(), assembly, "GetTypeFromHandle", debugger, ref failed);
            ReadNetworkModule = Resolve.GetMethod(Import(typeof(Net.Extensions)), assembly, ReadModule, debugger, ref failed);

            WriterDequeue = Resolve.GetMethod(Import<MemoryWriter>(), assembly, "Pop", debugger, ref failed);
            WriterEnqueue = Resolve.GetMethod(Import<MemoryWriter>(), assembly, "Push", debugger, ref failed);
            GetClientActive = Resolve.GetMethod(Import<NetworkManager>(), assembly, "get_isClient", debugger, ref failed);
            GetServerActive = Resolve.GetMethod(Import<NetworkManager>(), assembly, "get_isServer", debugger, ref failed);
            RegisterServerRpc = Resolve.GetMethod(Import(typeof(NetworkAttribute)), assembly, nameof(RegisterServerRpc), debugger, ref failed);
            RegisterClientRpc = Resolve.GetMethod(Import(typeof(NetworkAttribute)), assembly, nameof(RegisterClientRpc), debugger, ref failed);

            var NetworkModuleType = Import<NetworkModule>();
            SyncVarDirty = Resolve.GetProperty(NetworkModuleType, assembly, "syncVarDirty");
            SyncVarGetterGeneral = Resolve.GetMethod(NetworkModuleType, assembly, nameof(SyncVarGetterGeneral), debugger, ref failed);
            SyncVarGetterGameObject = Resolve.GetMethod(NetworkModuleType, assembly, nameof(SyncVarGetterGameObject), debugger, ref failed);
            SyncVarGetterNetworkEntity = Resolve.GetMethod(NetworkModuleType, assembly, nameof(SyncVarGetterNetworkEntity), debugger, ref failed);
            SyncVarGetterNetworkModule = Resolve.GetMethod(NetworkModuleType, assembly, nameof(SyncVarGetterNetworkModule), debugger, ref failed);

            SyncVarSetterGeneral = Resolve.GetMethod(NetworkModuleType, assembly, nameof(SyncVarSetterGeneral), debugger, ref failed);
            SyncVarSetterGameObject = Resolve.GetMethod(NetworkModuleType, assembly, nameof(SyncVarSetterGameObject), debugger, ref failed);
            SyncVarSetterNetworkEntity = Resolve.GetMethod(NetworkModuleType, assembly, nameof(SyncVarSetterNetworkEntity), debugger, ref failed);
            SyncVarSetterNetworkModule = Resolve.GetMethod(NetworkModuleType, assembly, nameof(SyncVarSetterNetworkModule), debugger, ref failed);

            GetSyncVarGameObject = Resolve.GetMethod(NetworkModuleType, assembly, nameof(GetSyncVarGameObject), debugger, ref failed);
            GetSyncVarNetworkEntity = Resolve.GetMethod(NetworkModuleType, assembly, nameof(GetSyncVarNetworkEntity), debugger, ref failed);
            GetSyncVarNetworkModule = Resolve.GetMethod(NetworkModuleType, assembly, nameof(GetSyncVarNetworkModule), debugger, ref failed);

            SendServerRpcInternal = Resolve.GetMethod(NetworkModuleType, assembly, nameof(SendServerRpcInternal), debugger, ref failed);
            SendClientRpcInternal = Resolve.GetMethod(NetworkModuleType, assembly, nameof(SendClientRpcInternal), debugger, ref failed);
            SendTargetRpcInternal = Resolve.GetMethod(NetworkModuleType, assembly, nameof(SendTargetRpcInternal), debugger, ref failed);
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