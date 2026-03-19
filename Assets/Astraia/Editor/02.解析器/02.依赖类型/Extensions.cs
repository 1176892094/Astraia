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
        public static bool Is(this TypeReference self, Type t)
        {
            return t.IsGenericType ? self.GetElementType().FullName == t.FullName : self.FullName == t.FullName;
        }

        public static bool Is<T>(this TypeReference self)
        {
            return self.Is(typeof(T));
        }

        public static bool HasAttribute<T>(this ICustomAttributeProvider self)
        {
            return self.CustomAttributes.Any(custom => custom.AttributeType.Is<T>());
        }

        public static CustomAttribute GetAttribute<T>(this ICustomAttributeProvider self)
        {
            return self.CustomAttributes.FirstOrDefault(custom => custom.AttributeType.Is<T>());
        }

        public static MethodDefinition GetMethod(this TypeDefinition self, string name)
        {
            return self.Methods.FirstOrDefault(method => method.Name == name);
        }

        public static IEnumerable<MethodDefinition> GetConstructors(this TypeDefinition self)
        {
            return self.Methods.Where(method => method.IsConstructor);
        }

        public static object GetArgument(this ICustomAttribute self)
        {
            return self.ConstructorArguments[0].Value;
        }

        public static bool Support(this TypeReference self)
        {
            return self.Is<GameObject>() || self.Is<NetworkEntity>() || self.IsDerivedFrom<NetworkModule>() || self.Is<NetworkModule>();
        }

        public static string GetName(this MethodDefinition self, string name)
        {
            return self.Name + name;
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


        public static IEnumerable<MethodDefinition> GetMethods(this TypeDefinition self, string name)
        {
            while (self != null)
            {
                foreach (var md in self.Methods.Where(md => md.Name == name))
                {
                    yield return md;
                }

                self = self.GetBaseType();
            }
        }

        public static bool HasInterface<T>(this TypeDefinition self)
        {
            while (self != null)
            {
                if (self.Interfaces.Any(ii => ii.InterfaceType.Is<T>()))
                {
                    return true;
                }

                self = self.GetBaseType();
            }

            return false;
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

        private static bool IsDerivedFrom(this TypeReference self, Type t)
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

            return tr.CanResolve() && tr.Resolve().IsDerivedFrom(t);
        }

        public static bool IsDerivedFrom<T>(this TypeReference self)
        {
            return self.IsDerivedFrom(typeof(T));
        }

        public static bool CanResolve(this TypeReference self)
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

        public static FieldReference MakeHostInstanceGeneric(this FieldReference self)
        {
            var tr = new GenericInstanceType(self.DeclaringType);
            foreach (var param in self.DeclaringType.GenericParameters)
            {
                tr.GenericArguments.Add(param);
            }

            return new FieldReference(self.Name, self.FieldType, tr);
        }

        public static MethodReference MakeHostInstanceGeneric(this MethodReference self, ModuleDefinition md, GenericInstanceType type)
        {
            var mr = new MethodReference(self.Name, self.ReturnType, type)
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

        public static GenericInstanceType MakeGenericInstanceType(this TypeReference self, params TypeReference[] parameters)
        {
            var tr = new GenericInstanceType(self);
            foreach (var param in parameters)
            {
                tr.GenericArguments.Add(param);
            }

            return tr;
        }

        public static MethodReference MakeGenericInstanceType(this MethodReference self, ModuleDefinition md, TypeReference tr)
        {
            var method = new GenericInstanceMethod(self);
            method.GenericArguments.Add(tr);
            return md.ImportReference(method);
        }


        public static FieldReference SpecializeField(this FieldReference self, ModuleDefinition md, GenericInstanceType type)
        {
            return md.ImportReference(new FieldReference(self.Name, self.FieldType, type));
        }

        public static TypeReference GetEnumUnderlyingType(this TypeDefinition self)
        {
            foreach (var field in self.Fields.Where(field => !field.IsStatic))
            {
                return field.FieldType;
            }

            throw new ArgumentException("无效的枚举类型: " + self.FullName);
        }
    }
}