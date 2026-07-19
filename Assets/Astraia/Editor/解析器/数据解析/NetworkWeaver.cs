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
        public const string MED_T2 = nameof(EntityGenerator);
        public const string MED_T1 = nameof(NetworkProcessor);
        public const MA GEN_V1 = MA.HideBySig | MA.Family | MA.Static;
        public const MA GEN_V2 = MA.HideBySig | MA.Public | MA.Static;
        public const MA GEN_S1 = MA.HideBySig | MA.Public | MA.Virtual;
        public const MA GEN_S2 = MA.HideBySig | MA.Family | MA.Virtual;
        public const MA GEN_S3 = MA.HideBySig | MA.Public | MA.SpecialName;
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
                            var current = td;
                            while (current != null)
                            {
                                if (current.Is<NetworkModule>())
                                {
                                    break;
                                }

                                change |= new NetworkModuleGen(assembly, access, module, writer, reader, Log, current).Process(ref failed);
                                current = current.BaseType?.Resolve();
                            }
                        }
                    }

                    if (td.IsSubclassOf<Inject>())
                    {
                        modified |= EntityGenerator.Processed(assembly, td, module, Log);
                    }

                    if (td.IsSubclassOf(typeof(Module<>)))
                    {
                        modified |= ModuleGenerator.Processed(assembly, td, module, Log);
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
            InvokeDelegate = Import<HookFunc>().GetMethod(assembly, Weaver.MED_C1, Log, ref failed);
            AddArraySegment = Import(typeof(ArraySegment<>)).GetMethod(assembly, Weaver.MED_C1, Log, ref failed);
            GetTypeFromHandle = Import<Type>().GetMethod(assembly, "GetTypeFromHandle", Log, ref failed);
            ReadNetworkModule = Import(typeof(Net.Extensions)).GetMethod(assembly, ReadModule, Log, ref failed);

            Listen = Import(typeof(EventManager)).GetMethod(assembly, nameof(Listen), Log, ref failed);
            Remove = Import(typeof(EventManager)).GetMethod(assembly, nameof(Remove), Log, ref failed);
            Inject = Import(typeof(Astraia.EntityExtensions)).GetMethod(assembly, nameof(Inject), Log, ref failed);

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
            return md.Name == nameof(Net.Extensions.ReadNetworkModule) && md.HasGenericParameters;
        }
    }

    internal static class Common
    {
        public static MethodReference GetProperty(this TypeReference tr, AssemblyDefinition ad, string name)
        {
            return tr.Resolve().Properties.Where(pd => pd.Name == name).Select(pd => ad.MainModule.ImportReference(pd.GetMethod)).FirstOrDefault();
        }

        public static MethodDefinition GetConstructor(this TypeReference tr)
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

        public static MethodReference GetMethod(this TypeReference tr, AssemblyDefinition ad, string name)
        {
            while (tr != null)
            {
                foreach (var md in tr.Resolve().Methods)
                {
                    if (md.Name == name)
                    {
                        MethodReference mr = md;
                        if (tr is GenericInstanceType git) //如果当前类型是泛型实例（如 List<int>）
                        {
                            mr = mr.GenericInstance(tr.Module, git); // 替换为具体的泛型实参（如将 T 替换为 int）
                        }

                        return ad.MainModule.ImportReference(mr);
                    }
                }

                tr = tr.GetBaseType(); // 当前类型未找到，处理基类（并处理基类上的泛型参数映射）
            }

            return null;
        }

        private static TypeReference GetBaseType(this TypeReference self)
        {
            var parent = self.Resolve().BaseType;
            if (parent is GenericInstanceType git)
            {
                var result = new GenericInstanceType(parent.Resolve());
                foreach (var arg in git.GenericArguments)
                {
                    var tr = arg;
                    if (tr.IsGenericParameter) // 如果基类的某个泛型参数是泛型占位符（如 T）
                    {
                        var td = self.Resolve();
                        if (td.HasGenericParameters) // 检查当前类型是否有泛型参数列表
                        {
                            for (var i = 0; i < td.GenericParameters.Count; i++)
                            {
                                if (td.GenericParameters[i].Name == tr.Name)
                                {
                                    tr = ((GenericInstanceType)self).GenericArguments[i];
                                }
                            }
                        }
                        else
                        {
                            throw new InvalidOperationException("方法带有泛型参数，但是参数不匹配。");
                        }
                    }

                    result.GenericArguments.Add(parent.Module.ImportReference(tr));
                }

                return result;
            }

            return parent;
        }
    }

    internal static class Extensions
    {
        public static T GetArgument<T>(this ICustomAttribute self)
        {
            return (T)self.ConstructorArguments[0].Value;
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

        public static bool IsSubclassOf<T>(this TypeReference self)
        {
            return self.IsSubclassOf(typeof(T));
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

        public static bool HasInterface(this TypeReference self, Type t)
        {
            var current = self;
            while (current != null)
            {
                var td = current.Resolve();
                if (td == null)
                {
                    return false;
                }

                if (td.Interfaces.Any(it => it.InterfaceType.Is(t)))
                {
                    return true;
                }

                current = td.BaseType;
            }

            return false;
        }

        public static IEnumerable<FieldDefinition> GetFields(this TypeReference self)
        {
            var current = self;
            while (current != null)
            {
                var td = current.Resolve();
                if (td == null)
                {
                    yield break;
                }

                foreach (var fd in td.Fields.Where(field => field.IsPublic && !field.IsStatic))
                {
                    yield return fd;
                }

                current = td.BaseType;
            }
        }

        public static MethodDefinition GetBaseMethod(this TypeReference self, string name)
        {
            var current = self;
            while (current != null)
            {
                var td = current.Resolve();
                if (td == null)
                {
                    return null;
                }

                foreach (var md in td.Methods.Where(md => md.Name == name))
                {
                    return md;
                }

                current = td.BaseType;
            }

            return null;
        }

        public static FieldReference GenericField(this FieldReference self, ModuleDefinition module, GenericInstanceType tr)
        {
            return module.ImportReference(new FieldReference(self.Name, self.FieldType, tr));
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

        public static MethodReference MakeGeneric(this MethodReference self, ModuleDefinition module, TypeReference tr)
        {
            var method = new GenericInstanceMethod(self);
            method.GenericArguments.Add(tr);
            return module.ImportReference(method);
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

        public static MethodReference GenericInstance(this MethodReference self, ModuleDefinition module, GenericInstanceType tr)
        {
            var mr = new MethodReference(self.Name, self.ReturnType, tr);
            mr.HasThis = self.HasThis;
            mr.ExplicitThis = self.ExplicitThis;
            mr.CallingConvention = self.CallingConvention;

            foreach (var param in self.Parameters)
            {
                mr.Parameters.Add(new ParameterDefinition(param.ParameterType));
            }

            foreach (var param in self.GenericParameters)
            {
                mr.GenericParameters.Add(new GenericParameter(param.Name, mr));
            }

            return module.ImportReference(mr);
        }
    }
}