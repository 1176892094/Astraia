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
            private static readonly Dictionary<string, Type> cacheTypes = new();
            public static readonly Dictionary<string, Assembly> assemblies = new();
            public const BindingFlags Declared = Static | Instance | BindingFlags.DeclaredOnly;
            public const BindingFlags Static = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            public const BindingFlags Instance = BindingFlags.Public | BindingFlags.NonPublic| BindingFlags.Instance;
            
            static Find()
            {
                var assemblyData = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in assemblyData)
                {
                    var assemblyName = assembly.GetName().Name;
                    assemblies.Add(assemblyName, assembly);
                    var types = assembly.GetTypes();
                    foreach (var type in types)
                    {
                        var typeName = Text.Format("{0},{1}", type.FullName, assemblyName);
                        cacheTypes[typeName] = type;
                    }
                }
            }
            
            public static Assembly Assembly(string name)
            {
                if (assemblies.TryGetValue(name, out var result))
                {
                    return result;
                }

                var assemblyData = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in assemblyData)
                {
                    if (assembly.GetName().Name == name)
                    {
                        result = assembly;
                        break;
                    }
                }

                if (result != null)
                {
                    assemblies[name] = result;
                }

                return result;
            }

            public static Type Type(string name)
            {
                if (cacheTypes.TryGetValue(name, out var result))
                {
                    return result;
                }

                var index = name.LastIndexOf(',');
                if (index < 0)
                {
                    var assemblyData = AppDomain.CurrentDomain.GetAssemblies();
                    foreach (var assembly in assemblyData)
                    {
                        result = assembly.GetType(name);
                        if (result != null)
                        {
                            cacheTypes[name] = result;
                            assemblies[assembly.GetName().Name] = assembly;
                            break;
                        }
                    }
                }
                else
                {
                    var assembly = Assembly(name.Substring(index + 1).Trim());
                    if (assembly != null)
                    {
                        result = assembly.GetType(name.Substring(0, index));
                        if (result != null)
                        {
                            cacheTypes[name] = result;
                        }
                    }
                }

                return result;
            }
        }
    }
}