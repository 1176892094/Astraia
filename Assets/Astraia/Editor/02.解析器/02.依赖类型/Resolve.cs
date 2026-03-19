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

namespace Astraia.Editor
{
    internal class Comparer : IEqualityComparer<TypeReference>
    {
        public bool Equals(TypeReference x, TypeReference y)
        {
            return x?.FullName == y?.FullName;
        }

        public int GetHashCode(TypeReference obj)
        {
            return obj.FullName.GetHashCode();
        }
    }

    internal static class Resolve
    {
        public static MethodReference GetProperty(TypeReference tr, AssemblyDefinition ad, string name)
        {
            return tr.Resolve().Properties.Where(pd => pd.Name == name).Select(pd => ad.MainModule.ImportReference(pd.GetMethod)).FirstOrDefault();
        }

        private static MethodReference GetMethod(TypeReference tr, AssemblyDefinition ad, Predicate<MethodDefinition> match)
        {
            return tr.Resolve().Methods.Where(match.Invoke).Select(md => ad.MainModule.ImportReference(md)).FirstOrDefault();
        }

        public static MethodDefinition GetConstructor(TypeReference tr)
        {
            return tr.Resolve().Methods.FirstOrDefault(md => md.Name == Weaver.CTOR && md.Resolve().IsPublic && md.Parameters.Count == 0);
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
                foreach (var md in tr.Resolve().Methods)
                {
                    if (md.Name == name)
                    {
                        MethodReference mr = md;
                        if (tr.IsGenericInstance)
                        {
                            mr = mr.MakeHostInstanceGeneric(tr.Module, (GenericInstanceType)tr);
                        }

                        return ad.MainModule.ImportReference(mr);
                    }
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

                throw new InvalidOperationException("没有找到匹配的泛型参数。");
            }

            throw new InvalidOperationException("方法带有泛型参数，在子类中找不到它们。");
        }
    }
}