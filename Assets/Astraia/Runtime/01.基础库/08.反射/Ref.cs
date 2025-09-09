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
        public static class Ref
        {
            private static readonly Dictionary<string, Type> cacheTypes = new();
            private static readonly Dictionary<string, Assembly> assemblies = new();
            public const BindingFlags Static = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            public const BindingFlags Instance = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            public static event Action<Type> OnLoad;
            public static event Action OnLoadComplete;

            public static void LoadData()
            {
                var assemblyData = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in assemblyData)
                {
                    var assemblyName = assembly.GetName().Name;
                    assemblies[assemblyName] = assembly;
                    var types = assembly.GetTypes();
                    foreach (var type in types)
                    {
                        var typeName = "{0},{1}".Format(type.FullName, assemblyName);
                        cacheTypes[typeName] = type;
                        OnLoad?.Invoke(type);
                    }
                }
                OnLoadComplete?.Invoke();
            }

            public static Assembly GetAssembly(string name)
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

            public static Type GetType(string name)
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
                    var assembly = GetAssembly(name.Substring(index + 1).Trim());
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