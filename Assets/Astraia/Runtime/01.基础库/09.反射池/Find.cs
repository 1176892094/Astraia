// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2025-01-10 21:01:21
// # Recently: 2025-01-11 18:01:32
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Astraia
{
    public static partial class Service
    {
        public static partial class Find
        {
            private static readonly IDictionary<string, Assembly> assemblies = new Dictionary<string, Assembly>();
            private static readonly IDictionary<string, Type> cacheTypes = new Dictionary<string, Type>();
            public const BindingFlags Entity = (BindingFlags)52;
            public const BindingFlags Static = (BindingFlags)56;

            public static Assembly Assembly(string name)
            {
                if (assemblies.TryGetValue(name, out var assembly))
                {
                    return assembly;
                }

                var assemblyData = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var data in assemblyData)
                {
                    if (data.GetName().Name == name)
                    {
                        assembly = data;
                        break;
                    }
                }

                if (assembly != null)
                {
                    assemblies[name] = assembly;
                }

                return assembly;
            }

            public static Type Type(string name)
            {
                if (cacheTypes.TryGetValue(name, out var cacheType))
                {
                    return cacheType;
                }

                var index = name.LastIndexOf(',');
                if (index < 0)
                {
                    return System.Type.GetType(name);
                }

                var assembly = Assembly(name.Substring(index + 1).Trim());
                if (assembly != null)
                {
                    cacheType = assembly.GetType(name.Substring(0, index));
                    cacheTypes.Add(name, cacheType);
                }

                return cacheType;
            }
        }
    }
}