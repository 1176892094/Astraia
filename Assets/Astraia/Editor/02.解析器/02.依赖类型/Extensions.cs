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
using Astraia.Net;
using Mono.Cecil;
using UnityEngine;

namespace Astraia.Editor
{
    internal static class Extensions
    {
        /// <summary>
        /// 指定类型判断
        /// </summary>
        public static bool Is(this TypeReference self, Type t)
        {
            if (t.IsGenericType)
            {
                return self.GetElementType().FullName == t.FullName;
            }

            return self.FullName == t.FullName;
        }

        /// <summary>
        /// 指定类型判断
        /// </summary>
        public static bool Is<T>(this TypeReference self)
        {
            return Is(self, typeof(T));
        }

        /// <summary>
        /// 是指定类型的派生类型
        /// </summary>
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

            return tr.IsResolve() && IsDerivedFrom(tr.Resolve(), t);
        }

        /// <summary>
        /// 是指定类型的派生类型
        /// </summary>
        public static bool IsDerivedFrom<T>(this TypeReference self)
        {
            return IsDerivedFrom(self, typeof(T));
        }

        /// <summary>
        /// 是能够解析的
        /// </summary>
        public static bool IsResolve(this TypeReference self)
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

        /// <summary>
        /// 从指定类型中获取方法
        /// </summary>
        public static MethodDefinition GetMethod(this TypeDefinition self, string name)
        {
            foreach (var method in self.Methods)
            {
                if (method.Name == name)
                {
                    return method;
                }
            }

            return null;
        }

        /// <summary>
        /// 从指定类型中获取方法组
        /// </summary>
        public static IEnumerable<MethodDefinition> GetMethods(this TypeDefinition self, string name)
        {
            return self.Methods.Where(method => method.Name == name);
        }

        /// <summary>
        /// 判断自定义特性
        /// </summary>
        public static bool HasCustomAttribute<T>(this ICustomAttributeProvider self)
        {
            return self.CustomAttributes.Any(custom => custom.AttributeType.Is<T>());
        }

        /// <summary>
        /// 获取自定义特性
        /// </summary>
        public static CustomAttribute GetCustomAttribute<T>(this ICustomAttributeProvider self)
        {
            return self.CustomAttributes.FirstOrDefault(custom => custom.AttributeType.Is<T>());
        }

        /// <summary>
        /// 获取宿主实例泛型方法
        /// </summary>
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

        /// <summary>
        /// 获取宿主实例泛型字段
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static FieldReference MakeHostInstanceGeneric(this FieldReference self)
        {
            var type = new GenericInstanceType(self.DeclaringType);
            foreach (var parameter in self.DeclaringType.GenericParameters)
            {
                type.GenericArguments.Add(parameter);
            }

            return new FieldReference(self.Name, self.FieldType, type);
        }

        /// <summary>
        /// 获取泛型实例类型
        /// </summary>
        public static GenericInstanceType MakeGenericInstanceType(this TypeReference self, params TypeReference[] arguments)
        {
            var type = new GenericInstanceType(self);
            foreach (var tr in arguments)
            {
                type.GenericArguments.Add(tr);
            }

            return type;
        }

        /// <summary>
        /// 获取泛型实例方法
        /// </summary>
        public static MethodReference MakeGenericInstanceType(this MethodReference self, ModuleDefinition md, TypeReference tr)
        {
            var instance = new GenericInstanceMethod(self);
            instance.GenericArguments.Add(tr);
            return md.ImportReference(instance);
        }

        /// <summary>
        /// 从构造方法中获取字段
        /// </summary>
        public static T GetField<T>(this CustomAttribute self)
        {
            foreach (var custom in self.ConstructorArguments)
            {
                if (custom.Type.FullName == typeof(T).FullName)
                {
                    return (T)custom.Value;
                }
            }

            return default;
        }

        /// <summary>
        /// 找到所有公开字段
        /// </summary>
        public static IEnumerable<FieldDefinition> FindPublicFields(this TypeDefinition self)
        {
            while (self != null)
            {
                foreach (var field in self.Fields.Where(field => !field.IsStatic && !field.IsPrivate && !field.IsFamily && !field.IsAssembly && !field.IsNotSerialized))
                {
                    yield return field;
                }

                try
                {
                    self = self.BaseType?.Resolve();
                }
                catch (AssemblyResolutionException)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// 获取构造函数
        /// </summary>
        public static IEnumerable<MethodDefinition> GetConstructors(this TypeDefinition self)
        {
            if (self == null)
            {
                throw new ArgumentNullException(nameof(self));
            }

            if (self.HasMethods)
            {
                foreach (var method in self.Methods.Where(method => method.IsConstructor))
                {
                    yield return method;
                }
            }
        }

        /// <summary>
        /// 是实现指定接口的
        /// </summary>
        public static bool IsImplement<T>(this TypeDefinition self)
        {
            var td = self;
            while (td != null)
            {
                if (td.Interfaces.Any(i => i.InterfaceType.Is<T>()))
                {
                    return true;
                }

                try
                {
                    td = td.BaseType?.Resolve();
                }
                catch
                {
                    break;
                }
            }

            return false;
        }

        /// <summary>
        /// 导入特定字段
        /// </summary>
        public static FieldReference SpecializeField(this FieldReference self, ModuleDefinition md, GenericInstanceType type)
        {
            return md.ImportReference(new FieldReference(self.Name, self.FieldType, type));
        }

        /// <summary>
        /// 添加枚举类型
        /// </summary>
        public static TypeReference GetEnumUnderlyingType(this TypeDefinition self)
        {
            foreach (var field in self.Fields.Where(field => !field.IsStatic))
            {
                return field.FieldType;
            }

            throw new ArgumentException("无效的枚举类型: " + self.FullName);
        }

        /// <summary>
        /// 获取父类的方法
        /// </summary>
        public static MethodDefinition GetMethodInBaseType(this TypeDefinition self, string name)
        {
            var td = self;
            while (td != null)
            {
                foreach (var md in td.Methods.Where(md => md.Name == name))
                {
                    return md;
                }

                try
                {
                    td = td.BaseType?.Resolve();
                }
                catch
                {
                    break;
                }
            }

            return null;
        }

        /// <summary>
        /// 应用泛型参数
        /// </summary>
        public static TypeReference ApplyGenericParameters(this TypeReference self, TypeReference child)
        {
            if (!self.IsGenericInstance)
            {
                return self;
            }

            var args = (GenericInstanceType)self;
            var it = new GenericInstanceType(self.Resolve());
            foreach (var tr in args.GenericArguments)
            {
                it.GenericArguments.Add(tr);
            }

            for (var i = 0; i < it.GenericArguments.Count; i++)
            {
                if (!it.GenericArguments[i].IsGenericParameter)
                {
                    continue;
                }

                var tr = child.FindMatchingGenericArgument(it.GenericArguments[i].Name);
                it.GenericArguments[i] = self.Module.ImportReference(tr);
            }

            return it;
        }

        /// <summary>
        /// 查找匹配的泛型参数
        /// </summary>
        private static TypeReference FindMatchingGenericArgument(this TypeReference self, string name)
        {
            var td = self.Resolve();
            if (!td.HasGenericParameters)
            {
                throw new InvalidOperationException("方法带有泛型参数，在子类中找不到它们。");
            }

            for (var i = 0; i < td.GenericParameters.Count; i++)
            {
                if (td.GenericParameters[i].Name == name)
                {
                    return ((GenericInstanceType)self).GenericArguments[i];
                }
            }

            throw new InvalidOperationException("没有找到匹配的泛型参数。");
        }

        /// <summary>
        /// 判断网络对象
        /// </summary>
        /// <param name="tr"></param>
        /// <returns></returns>
        public static bool IsNetworkEntity(this TypeReference tr)
        {
            if (tr.Is<GameObject>())
            {
                return true;
            }

            if (tr.Is<NetworkEntity>())
            {
                return true;
            }

            if (tr.IsDerivedFrom<NetworkModule>())
            {
                return true;
            }

            return tr.Is<NetworkModule>();
        }
    }
}