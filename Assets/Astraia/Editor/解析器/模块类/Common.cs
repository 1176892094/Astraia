// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-09-06 23:09:48
// # Recently: 2026-09-06 23:15:48
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

namespace Astraia
{
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

        public static MethodReference GetMethod(this TypeReference tr, AssemblyDefinition ad, Predicate<MethodDefinition> match, AssemblyDebugger Log, ref bool failed)
        {
            var mr = tr.GetMethod(ad, match);
            if (mr == null)
            {
                Log.Error("在类型 {0} 中没有找到方法".Format(tr), tr);
                failed = true;
            }

            return mr;
        }

        public static MethodReference GetMethod(this TypeReference tr, AssemblyDefinition ad, string name, AssemblyDebugger Log, ref bool failed)
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

        internal static TypeReference GetBaseType(this TypeReference self)
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