// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-04-09 21:04:38
// // # Recently: 2025-04-09 21:04:38
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System.Runtime.CompilerServices;

namespace Astraia
{
    public static class Extensions
    {
        public static string Format<T>(this string format, T arg1)
        {
            return Service.Build.Format(format, arg1);
        }

        public static string Format<T1, T2>(this string format, T1 arg1, T2 arg2)
        {
            return Service.Build.Format(format, arg1, arg2);
        }

        public static string Format<T1, T2, T3>(this string format, T1 arg1, T2 arg2, T3 arg3)
        {
            return Service.Build.Format(format, arg1, arg2, arg3);
        }

        public static string Format<T1, T2, T3, T4>(this string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            return Service.Build.Format(format, arg1, arg2, arg3, arg4);
        }

        public static bool IsNullOrEmpty(this string result)
        {
            return string.IsNullOrEmpty(result);
        }

        public static bool IsNullOrWhiteSpace(this string result)
        {
            return string.IsNullOrWhiteSpace(result);
        }

        public static string Mask(this string result, char mask = '*')
        {
            return Service.Input.Mask(result, mask);
        }

        public static string Limit(this string result, int count)
        {
            var value = string.Empty;
            var input = 0;

            foreach (var c in result)
            {
                var width = c > 255 ? 2 : 1;
                if (input + width > count)
                {
                    break;
                }

                input += width;
                value += c;
            }

            return value;
        }

        public static string Align(this string result, int count)
        {
            var width = 0;
            foreach (var c in result)
            {
                width += c > 255 ? 2 : 1;
            }

            while (width++ < count)
            {
                result += ' ';
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static object GetValue(this object target, string name)
        {
            return Service.Emit.GetValue(target, name);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static T GetValue<T>(this object target, string name)
        {
            return (T)Service.Emit.GetValue(target, name);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void SetValue(this object target, string name, object value)
        {
            Service.Emit.SetValue(target, name, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void SetValue<T>(this object target, string name, T value)
        {
            Service.Emit.SetValue(target, name, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static object Invoke(this object target, string name, params object[] args)
        {
            return Service.Emit.Invoke(target, name, args);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static T Invoke<T>(this object target, string name, params object[] args)
        {
            return (T)Service.Emit.Invoke(target, name, args);
        }
    }
}