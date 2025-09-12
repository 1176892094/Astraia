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
        public static bool IsEditor(AssemblyDefinition ad)
        {
            return ad.MainModule.AssemblyReferences.Any(r => r.Name.StartsWith(nameof(UnityEditor)));
        }

        public static MethodReference GetProperty(TypeReference tr, AssemblyDefinition ad, string name)
        {
            return tr.Resolve().Properties.Where(pd => pd.Name == name).Select(pd => ad.MainModule.ImportReference(pd.GetMethod)).FirstOrDefault();
        }

        public static MethodDefinition GetConstructor(TypeReference tr)
        {
            return tr.Resolve().Methods.FirstOrDefault(md => md.Name == Const.CTOR && md.Resolve().IsPublic && md.Parameters.Count == 0);
        }

        public static MethodReference GetMethod(TypeReference tr, AssemblyDefinition ad, Predicate<MethodDefinition> match)
        {
            return tr.Resolve().Methods.Where(match.Invoke).Select(md => ad.MainModule.ImportReference(md)).FirstOrDefault();
        }

        public static MethodReference GetMethod(TypeReference tr, AssemblyDefinition ad, ILog log, Predicate<MethodDefinition> match, ref bool failed)
        {
            var mr = GetMethod(tr, ad, match);
            if (mr == null)
            {
                log.Error("在类型 {0} 中没有找到方法".Format(tr.Name), tr);
                failed = true;
            }

            return mr;
        }

        public static MethodReference GetMethod(TypeReference tr, AssemblyDefinition ad, ILog log, string name, ref bool failed)
        {
            var mr = GetMethod(tr, ad, method => method.Name == name);
            if (mr == null)
            {
                log.Error("在类型 {0} 中没有找到名称 {1} 的方法".Format(tr.Name,name), tr);
                failed = true;
            }

            return mr;
        }

        public static MethodReference GetMethodByParent(TypeReference tr, AssemblyDefinition ad, string name)
        {
            while (true)
            {
                if (tr == null)
                {
                    return null;
                }

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

                tr = tr.Resolve().BaseType.ApplyGenericParameters(tr);
            }
        }
    }
}