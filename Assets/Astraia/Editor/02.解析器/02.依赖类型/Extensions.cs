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
using System.Linq;
using Mono.Cecil;
using UnityEngine;
using Astraia.Net;

namespace Astraia.Editor
{
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

        public static bool Support(this TypeReference self)
        {
            return self.Is<GameObject>() || self.Is<NetworkEntity>() || self.Is<NetworkModule>() || self.IsSubclassOf<NetworkModule>();
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
}