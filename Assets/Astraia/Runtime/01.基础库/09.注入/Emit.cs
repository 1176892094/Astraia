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
    public static class Emit
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object GetValue(object target, string name)
        {
            return EmitHelper.GetValue(target, name);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetValue<T>(object target, string name)
        {
            return (T)EmitHelper.GetValue(target, name);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValue(object target, string name, object value)
        {
            EmitHelper.SetValue(target, name, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValue<T>(object target, string name, T value)
        {
            EmitHelper.SetValue(target, name, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object Invoke(object target, string name, params object[] args)
        {
            return EmitHelper.Invoke(target, name, args);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Invoke<T>(object target, string name, params object[] args)
        {
            return (T)EmitHelper.Invoke(target, name, args);
        }
    }
}