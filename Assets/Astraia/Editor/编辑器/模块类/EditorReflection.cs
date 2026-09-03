// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-09-02 20:09:56
// # Recently: 2026-09-02 21:22:59
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Astraia
{
    internal static class EditorReflection
    {
        private static readonly Dictionary<Type, Dictionary<string, FieldInfo>> fieldData = new();
        private static readonly Dictionary<Type, Dictionary<string, MethodInfo>> methodData = new();
        private static readonly Dictionary<Type, Dictionary<string, PropertyInfo>> propertyData = new();

        public static object Invoke(object target, string name, params object[] args)
        {
            var result = target as Type ?? target.GetType();
            var method = GetMethod(result, name, args.Select(r => r.GetType()).ToArray());
            if (method != null)
            {
                return method.Invoke(target is Type ? null : target, args);
            }

            throw new MissingMethodException(result.FullName, name);
        }

        public static object GetValue(object target, string name)
        {
            var source = target as Type ?? target.GetType();
            var field = GetField(source, name);
            if (field != null)
            {
                return field.GetValue(target is Type ? null : target);
            }

            var property = GetProperty(source, name);
            if (property != null)
            {
                return property.GetValue(target is Type ? null : target);
            }

            throw new MissingMemberException(source.FullName, name);
        }

        public static void SetValue(object target, string name, object value)
        {
            var source = target as Type ?? target.GetType();
            var field = GetField(source, name);
            if (field != null)
            {
                field.SetValue(target is Type ? null : target, value);
                return;
            }

            var property = GetProperty(source, name);
            if (property != null)
            {
                property.SetValue(target is Type ? null : target, value);
                return;
            }

            throw new MissingMemberException(source.FullName, name);
        }

        private static MethodInfo GetMethod(Type type, string name, params Type[] args)
        {
            if (!methodData.TryGetValue(type, out var results))
            {
                results = new Dictionary<string, MethodInfo>();
                methodData[type] = results;
            }

            if (!results.TryGetValue(name, out var result))
            {
                for (var current = type; current != null; current = current.BaseType)
                {
                    result = current.GetMethod(name, (BindingFlags)62, null, args, null);
                    if (result != null)
                    {
                        return results[name] = result;
                    }
                }

                foreach (var current in type.GetInterfaces())
                {
                    result = current.GetMethod(name, (BindingFlags)62, null, args, null);
                    if (result != null)
                    {
                        return results[name] = result;
                    }
                }
            }

            return result;
        }

        private static FieldInfo GetField(Type type, string name)
        {
            if (!fieldData.TryGetValue(type, out var results))
            {
                results = new Dictionary<string, FieldInfo>();
                fieldData[type] = results;
            }

            if (!results.TryGetValue(name, out var result))
            {
                for (var current = type; current != null; current = current.BaseType)
                {
                    result = current.GetField(name, (BindingFlags)62);
                    if (result != null)
                    {
                        return results[name] = result;
                    }
                }
            }

            return result;
        }

        private static PropertyInfo GetProperty(Type type, string name)
        {
            if (!propertyData.TryGetValue(type, out var results))
            {
                results = new Dictionary<string, PropertyInfo>();
                propertyData[type] = results;
            }

            if (!results.TryGetValue(name, out var result))
            {
                for (var current = type; current != null; current = current.BaseType)
                {
                    result = current.GetProperty(name, (BindingFlags)62);
                    if (result != null)
                    {
                        return results[name] = result;
                    }
                }
            }

            return result;
        }
    }

    internal static class EditorExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object GetValue(this object target, string name)
        {
            return EditorReflection.GetValue(target, name);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetValue<T>(this object target, string name)
        {
            return (T)EditorReflection.GetValue(target, name);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValue(this object target, string name, object value)
        {
            EditorReflection.SetValue(target, name, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValue<T>(this object target, string name, T value)
        {
            EditorReflection.SetValue(target, name, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object Invoke(this object target, string name, params object[] args)
        {
            return EditorReflection.Invoke(target, name, args);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Invoke<T>(this object target, string name, params object[] args)
        {
            return (T)EditorReflection.Invoke(target, name, args);
        }
    }
}