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
using System.Linq;
using System.Collections.Generic;
using Mono.Cecil;
using Astraia.Net;
using UnityEngine;
using InjectManager = Astraia.Core.Extensions;

namespace Astraia.Editor
{
    using TA = TypeAttributes;
    using MA = MethodAttributes;

    [Serializable]
    internal sealed class Weaver
    {
        public const string WEAVER = "Astraia.Net";
        public const string MED_V1 = "V1";
        public const string MED_V2 = "V2";
        public const string MED_C1 = ".ctor";
        public const string MED_C2 = ".cctor";
        public const string MED_S1 = "SerializeSyncVars";
        public const string MED_S2 = "DeserializeSyncVars";
        public const string MED_T1 = nameof(NetworkProcessor);
        public const MA GEN_V1 = MA.HideBySig | MA.Family | MA.Static;
        public const MA GEN_V2 = MA.HideBySig | MA.Public | MA.Static;
        public const MA GEN_S1 = MA.HideBySig | MA.Public | MA.Virtual;
        public const MA GEN_S2 = MA.HideBySig | MA.Public | MA.SpecialName;
        public const MA GEN_C2 = MA.HideBySig | MA.Static | MA.SpecialName | MA.Private | MA.RTSpecialName;
        public const TA GEN_T1 = TA.AutoClass | TA.Public | TA.Class | TA.AnsiClass | TA.Abstract | TA.Sealed | TA.BeforeFieldInit;

        public bool Weave(AssemblyDefinition assembly, ILogPostProcessor Log, IAssemblyResolver resolver, bool success, out bool modified)
        {
            modified = false;
            try
            {
                var change = false;
                var failed = false;
                var module = new Module(assembly, Log, ref failed);
                var writer = (Writer)null;
                var reader = (Reader)null;
                var access = (SyncVarAccess)null;
                var create = (TypeDefinition)null;

                if (success)
                {
                    if (assembly.MainModule.Types.Any(td => td.Namespace == WEAVER && td.Name == MED_T1))
                    {
                        success = false;
                    }
                    else
                    {
                        access = new SyncVarAccess();
                        create = new TypeDefinition(WEAVER, MED_T1, GEN_T1, module.Import<object>());
                        writer = new Writer(assembly, module, create, Log);
                        reader = new Reader(assembly, module, create, Log);
                        change = NetworkMemberGen.Process(assembly, resolver, Log, writer, reader, ref failed);
                    }
                }

                var mainModule = assembly.MainModule;
                foreach (var td in mainModule.Types)
                {
                    if (success)
                    {
                        if (td.IsSubclassOf<NetworkModule>())
                        {
                            var parent = td;
                            while (parent != null)
                            {
                                if (parent.Is<NetworkModule>())
                                {
                                    break;
                                }

                                change |= new NetworkModuleGen(assembly, access, module, writer, reader, Log, parent).Process(ref failed);
                                parent = parent.GetBaseType();
                            }
                        }
                    }

                    if (td.IsSubclassOf(typeof(Module<>)))
                    {
                        modified |= CustomGenerator.Processed(assembly, td, module, Log);
                    }
                }

                if (failed)
                {
                    return false;
                }

                if (success && change)
                {
                    SyncVarReplace.Process(mainModule, access);
                    mainModule.Types.Add(create);
                    NetworkMemberGen.Processed(assembly, module, writer, reader, create);
                }

                modified |= change;
                return true;
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
                return false;
            }
        }
    }

    internal sealed class Module
    {
        private readonly AssemblyDefinition assembly;
        public readonly TypeDefinition Initialized;

        public readonly MethodReference OnShow;
        public readonly MethodReference OnHide;
        public readonly MethodReference Dequeue;
        public readonly MethodReference Enqueue;
        public readonly MemberReference ModuleGeneric;

        public readonly MethodReference Listen;
        public readonly MethodReference Remove;
        public readonly MethodReference Inject;

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

        public Module(AssemblyDefinition assembly, ILogPostProcessor Log, ref bool failed)
        {
            this.assembly = assembly;
            Initialized = Import<RuntimeInitializeOnLoadMethodAttribute>().Resolve();
            LogError = Import<Debug>().GetMethod(assembly, OnLogError, Log, ref failed);
            SyncVarHook = Import(typeof(Action<,>)).GetMethod(assembly, Weaver.MED_C1, Log, ref failed);
            InvokeDelegate = Import<InvokeDelegate>().GetMethod(assembly, Weaver.MED_C1, Log, ref failed);
            AddArraySegment = Import(typeof(ArraySegment<>)).GetMethod(assembly, Weaver.MED_C1, Log, ref failed);
            GetTypeFromHandle = Import<Type>().GetMethod(assembly, "GetTypeFromHandle", Log, ref failed);
            ReadNetworkModule = Import(typeof(Net.Extensions)).GetMethod(assembly, ReadModule, Log, ref failed);

            OnShow = Import(typeof(Astraia.Module)).GetMethod(assembly, nameof(OnShow), Log, ref failed);
            OnHide = Import(typeof(Astraia.Module)).GetMethod(assembly, nameof(OnHide), Log, ref failed);
            Dequeue = Import(typeof(Astraia.Module)).GetMethod(assembly, nameof(Dequeue), Log, ref failed);
            Enqueue = Import(typeof(Astraia.Module)).GetMethod(assembly, nameof(Enqueue), Log, ref failed);

            Listen = Import(typeof(EventManager)).GetMethod(assembly, nameof(Listen), Log, ref failed);
            Remove = Import(typeof(EventManager)).GetMethod(assembly, nameof(Remove), Log, ref failed);
            Inject = Import(typeof(InjectManager)).GetMethod(assembly, nameof(Inject), Log, ref failed);
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

        private static bool ReadModule(MethodDefinition method)
        {
            return method.Name == nameof(Net.Extensions.ReadNetworkModule) && method.HasGenericParameters;
        }
    }

    internal static class Common
    {
        public static MethodReference GetProperty(this TypeReference tr, AssemblyDefinition ad, string name)
        {
            return tr.Resolve().Properties.Where(pd => pd.Name == name).Select(pd => ad.MainModule.ImportReference(pd.GetMethod)).FirstOrDefault();
        }

        public static MethodDefinition GetConstructor(TypeReference tr)
        {
            return tr.Resolve().Methods.FirstOrDefault(md => md.Name == Weaver.MED_C1 && md.Resolve().IsPublic && md.Parameters.Count == 0);
        }

        private static MethodReference GetMethod(this TypeReference tr, AssemblyDefinition ad, Predicate<MethodDefinition> match)
        {
            return tr.Resolve().Methods.Where(match.Invoke).Select(md => ad.MainModule.ImportReference(md)).FirstOrDefault();
        }

        public static MethodReference GetMethod(this TypeReference tr, AssemblyDefinition ad, Predicate<MethodDefinition> match, ILogPostProcessor Log, ref bool failed)
        {
            var mr = tr.GetMethod(ad, match);
            if (mr == null)
            {
                Log.Error("在类型 {0} 中没有找到方法".Format(tr), tr);
                failed = true;
            }

            return mr;
        }

        public static MethodReference GetMethod(this TypeReference tr, AssemblyDefinition ad, string name, ILogPostProcessor Log, ref bool failed)
        {
            var mr = tr.GetMethod(ad, method => method.Name == name);
            if (mr == null)
            {
                Log.Error("在类型 {0} 中没有找到名称 {1} 的方法".Format(tr, name), tr);
                failed = true;
            }

            return mr;
        }

        public static MethodReference GetMethod(TypeReference tr, AssemblyDefinition ad, string name)
        {
            while (tr != null)
            {
                var td = tr.Resolve();
                foreach (var md in td.Methods.Where(md => md.Name == name))
                {
                    MethodReference mr = md;
                    if (tr.IsGenericInstance)
                    {
                        mr = mr.GenericInstance(tr.Module, (GenericInstanceType)tr);
                    }

                    return ad.MainModule.ImportReference(mr);
                }

                tr = ApplyGenericParameters(tr);
            }

            return null;
        }

        private static TypeReference ApplyGenericParameters(TypeReference self)
        {
            var parent = self.Resolve().BaseType;
            if (parent.IsGenericInstance)
            {
                var args = (GenericInstanceType)parent;
                var it = new GenericInstanceType(parent.Resolve());
                foreach (var tr in args.GenericArguments)
                {
                    it.GenericArguments.Add(tr);
                }

                for (var i = 0; i < it.GenericArguments.Count; i++)
                {
                    if (it.GenericArguments[i].IsGenericParameter)
                    {
                        var tr = GetGenericArgument(self, it.GenericArguments[i].Name);
                        it.GenericArguments[i] = parent.Module.ImportReference(tr);
                    }
                }

                return it;
            }

            return parent;
        }

        private static TypeReference GetGenericArgument(TypeReference self, string name)
        {
            var td = self.Resolve();
            if (td.HasGenericParameters)
            {
                for (var i = 0; i < td.GenericParameters.Count; i++)
                {
                    if (td.GenericParameters[i].Name == name)
                    {
                        return ((GenericInstanceType)self).GenericArguments[i];
                    }
                }
            }

            throw new InvalidOperationException("方法带有泛型参数，但是参数不匹配。");
        }
    }

    internal static class Extensions
    {
        public static object GetArgument(this ICustomAttribute self)
        {
            return self.ConstructorArguments[0].Value;
        }

        public static string GetName(this MethodDefinition self, string name)
        {
            return self.Name + name;
        }

        public static bool Is(this TypeReference self, Type t)
        {
            return t.IsGenericType ? self.GetElementType().FullName == t.FullName : self.FullName == t.FullName;
        }

        public static bool Is<T>(this TypeReference self)
        {
            return self.Is(typeof(T));
        }

        public static bool IsSubclassOf(this TypeReference self, Type t)
        {
            var td = self.Resolve();
            if (!td.IsClass)
            {
                return false;
            }

            var tr = td.BaseType;
            if (tr == null)
            {
                return false;
            }

            if (tr.Is(t))
            {
                return true;
            }

            return tr.CanResolve() && tr.Resolve().IsSubclassOf(t);
        }

        public static bool IsSubclassOf<T>(this TypeReference self)
        {
            return self.IsSubclassOf(typeof(T));
        }

        private static bool CanResolve(this TypeReference self)
        {
            while (self != null)
            {
                if (self.Scope.Name == "Windows")
                {
                    return false;
                }

                if (self.Scope.Name == "mscorlib")
                {
                    return self.Resolve() != null;
                }

                try
                {
                    self = self.Resolve().BaseType;
                }
                catch
                {
                    return false;
                }
            }

            return true;
        }

        public static bool HasInterface(this TypeDefinition self, Type t)
        {
            while (self != null)
            {
                if (self.Interfaces.Any(ii => ii.InterfaceType.Is(t)))
                {
                    return true;
                }

                self = self.GetBaseType();
            }

            return false;
        }

        public static bool HasAttribute<T>(this ICustomAttributeProvider self)
        {
            return self.CustomAttributes.Any(custom => custom.AttributeType.Is<T>());
        }

        public static CustomAttribute GetAttribute<T>(this ICustomAttributeProvider self)
        {
            return self.CustomAttributes.FirstOrDefault(custom => custom.AttributeType.Is<T>());
        }

        public static FieldDefinition GetField(this TypeDefinition self)
        {
            return self.Fields.FirstOrDefault(fd => !fd.IsStatic);
        }

        public static MethodDefinition GetMethod(this TypeDefinition self, string name)
        {
            return self.Methods.FirstOrDefault(md => md.Name == name);
        }

        public static IEnumerable<MethodDefinition> GetMethods(this TypeDefinition self, string name)
        {
            return self.Methods.Where(md => md.Name == name);
        }

        public static IEnumerable<MethodDefinition> GetConstructors(this TypeDefinition self)
        {
            return self.Methods.Where(method => method.IsConstructor);
        }

        public static IEnumerable<FieldDefinition> GetFields(this TypeDefinition self)
        {
            while (self != null)
            {
                foreach (var fd in self.Fields.Where(field => field.IsPublic && !field.IsStatic))
                {
                    yield return fd;
                }

                self = self.GetBaseType();
            }
        }

        public static MethodDefinition GetBaseMethod(this TypeDefinition self, string name)
        {
            while (self != null)
            {
                foreach (var md in self.Methods.Where(md => md.Name == name))
                {
                    return md;
                }

                self = self.GetBaseType();
            }

            return null;
        }

        public static TypeDefinition GetBaseType(this TypeDefinition self)
        {
            try
            {
                return self.BaseType?.Resolve();
            }
            catch (AssemblyResolutionException)
            {
                return null;
            }
        }

        public static FieldReference GenericField(this FieldReference self, ModuleDefinition module, GenericInstanceType tr)
        {
            return module.ImportReference(new FieldReference(self.Name, self.FieldType, tr));
        }

        public static GenericInstanceType MakeGeneric(this TypeReference self, params TypeReference[] parameters)
        {
            var tr = new GenericInstanceType(self);
            foreach (var param in parameters)
            {
                tr.GenericArguments.Add(param);
            }

            return tr;
        }

        public static GenericInstanceMethod MakeGeneric(this MethodReference self, TypeReference tr)
        {
            var method = new GenericInstanceMethod(self);
            method.GenericArguments.Add(tr);
            return method;
        }

        public static FieldReference MakeGeneric(this FieldReference self)
        {
            var tr = new GenericInstanceType(self.DeclaringType);
            foreach (var param in self.DeclaringType.GenericParameters)
            {
                tr.GenericArguments.Add(param);
            }

            return new FieldReference(self.Name, self.FieldType, tr);
        }

        public static MethodReference GenericInstance(this MethodReference self, ModuleDefinition md, GenericInstanceType tr)
        {
            var mr = new MethodReference(self.Name, self.ReturnType, tr) { HasThis = self.HasThis, ExplicitThis = self.ExplicitThis, CallingConvention = self.CallingConvention };

            foreach (var param in self.Parameters)
            {
                mr.Parameters.Add(new ParameterDefinition(param.ParameterType));
            }

            foreach (var param in self.GenericParameters)
            {
                mr.GenericParameters.Add(new GenericParameter(param.Name, mr));
            }

            return md.ImportReference(mr);
        }

        public static MethodReference GenericInstance(this MethodReference self, ModuleDefinition md, TypeReference tr)
        {
            var method = new GenericInstanceMethod(self);
            method.GenericArguments.Add(tr);
            return md.ImportReference(method);
        }
    }
}