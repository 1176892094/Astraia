// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-08-26 18:08:03
// // # Recently: 2025-08-26 18:08:04
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System.Runtime.CompilerServices;

namespace Astraia
{
    public static class Extensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object GetValue(this object target, string name)
        {
            return Service.Find.GetValue(target, name);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetValue<T>(this object target, string name)
        {
            return (T)Service.Find.GetValue(target, name);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValue(this object target, string name, object value)
        {
            Service.Find.SetValue(target, name, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValue<T>(this object target, string name, T value)
        {
            Service.Find.SetValue(target, name, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object Invoke(this object target, string name, params object[] args)
        {
            return Service.Find.Invoke(target, name, args);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Invoke<T>(this object target, string name, params object[] args)
        {
            return (T)Service.Find.Invoke(target, name, args);
        }
    }
}