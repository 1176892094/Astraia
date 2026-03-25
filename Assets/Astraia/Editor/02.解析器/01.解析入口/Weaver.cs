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
using System.Diagnostics;
using System.Collections.Generic;
using Mono.Cecil;
using Astraia.Net;

namespace Astraia.Editor
{
    using Member = TypeAttributes;
    using Method = MethodAttributes;

    [Serializable]
    internal sealed class Weaver
    {
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

                // var elapse = Stopwatch.StartNew();
                var access = new SyncVarAccess();
                var module = new Module(assembly, debugger, ref failed);
                var expand = new TypeDefinition(GEN_TYPE, GEN_FUN, GEN_ATTR, module.Import<object>());
                var writer = new Writer(assembly, module, expand, debugger);
                var reader = new Reader(assembly, module, expand, debugger);
                modified = NetworkMessageGen.Process(assembly, resolver, debugger, writer, reader, ref failed);

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

                        modified |= new NetworkModuleGen(assembly, access, module, writer, reader, debugger, parent).Process(ref failed);
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
                    NetworkMessageGen.Processed(assembly, module, writer, reader, expand);
                }

                // elapse.Stop();
                // debugger.Warn("{0:F2}ms ".Color("G").Format(elapse.ElapsedMilliseconds / 1000F) + assembly.Name.Name);
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
            Initialized = Import<UnityEngine.RuntimeInitializeOnLoadMethodAttribute>().Resolve();
            LogError = Common.GetMethod(Import<UnityEngine.Debug>(), assembly, OnLogError, debugger, ref failed);
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

    internal static class Common
    {
        public static MethodReference GetProperty(TypeReference tr, AssemblyDefinition ad, string name)
        {
            return tr.Resolve().Properties.Where(pd => pd.Name == name).Select(pd => ad.MainModule.ImportReference(pd.GetMethod)).FirstOrDefault();
        }

        public static MethodDefinition GetConstructor(TypeReference tr)
        {
            return tr.Resolve().Methods.FirstOrDefault(md => md.Name == Weaver.GEN_CTOR && md.Resolve().IsPublic && md.Parameters.Count == 0);
        }

        private static MethodReference GetMethod(TypeReference tr, AssemblyDefinition ad, Predicate<MethodDefinition> match)
        {
            return tr.Resolve().Methods.Where(match.Invoke).Select(md => ad.MainModule.ImportReference(md)).FirstOrDefault();
        }

        public static MethodReference GetMethod(TypeReference tr, AssemblyDefinition ad, Predicate<MethodDefinition> match, ILogPostProcessor Log, ref bool failure)
        {
            var mr = GetMethod(tr, ad, match);
            if (mr == null)
            {
                Log.Error("在类型 {0} 中没有找到方法".Format(tr), tr);
                failure = true;
            }

            return mr;
        }

        public static MethodReference GetMethod(TypeReference tr, AssemblyDefinition ad, string name, ILogPostProcessor Log, ref bool failure)
        {
            var mr = GetMethod(tr, ad, method => method.Name == name);
            if (mr == null)
            {
                Log.Error("在类型 {0} 中没有找到名称 {1} 的方法".Format(tr, name), tr);
                failure = true;
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

        private static bool IsSubclassOf(this TypeReference self, Type t)
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
            var mr = new MethodReference(self.Name, self.ReturnType, tr)
            {
                HasThis = self.HasThis,
                ExplicitThis = self.ExplicitThis,
                CallingConvention = self.CallingConvention
            };

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