// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-08-14 19:08:27
// # Recently: 2026-08-14 19:23:27
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Astraia
{
    public static class Search
    {
        private static readonly Dictionary<string, Assembly> assemblies = new();
        private static readonly Dictionary<string, Type> cacheTypes = new();

        public const BindingFlags Static = (BindingFlags)56;
        public const BindingFlags Instance = (BindingFlags)52;

        public static event Action<Type> OnLoad;

        public static void LoadData(params string[] args)
        {
            var assemblyData = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblyData)
            {
                var name = assembly.GetName().Name;
                assemblies[name] = assembly;
                if (args.Contains(name) || name.StartsWith("Assembly-CSharp"))
                {
                    foreach (var result in assembly.GetTypes())
                    {
                        cacheTypes["{0},{1}".Format(result.FullName, name)] = result;
                        OnLoad?.Invoke(result);
                    }
                }
            }
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