// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-09-27 13:09:31
// // # Recently: 2025-09-27 13:09:31
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

namespace Astraia
{
    public static partial class Extensions
    {
        public static void Info(this string format)
        {
            Log.Info(format);
        }

        public static void Info<T>(this string format, T arg1)
        {
            Log.Info(format, arg1);
        }

        public static void Info<T1, T2>(this string format, T1 arg1, T2 arg2)
        {
            Log.Info(format, arg1, arg2);
        }

        public static void Info<T1, T2, T3>(this string format, T1 arg1, T2 arg2, T3 arg3)
        {
            Log.Info(format, arg1, arg2, arg3);
        }

        public static void Info<T1, T2, T3, T4>(this string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            Log.Info(format, arg1, arg2, arg3, arg4);
        }

        public static void Warn(this string format)
        {
            Log.Warn(format);
        }

        public static void Warn<T>(this string format, T arg1)
        {
            Log.Warn(format, arg1);
        }

        public static void Warn<T1, T2>(this string format, T1 arg1, T2 arg2)
        {
            Log.Warn(format, arg1, arg2);
        }

        public static void Warn<T1, T2, T3>(this string format, T1 arg1, T2 arg2, T3 arg3)
        {
            Log.Warn(format, arg1, arg2, arg3);
        }

        public static void Warn<T1, T2, T3, T4>(this string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            Log.Warn(format, arg1, arg2, arg3, arg4);
        }

        public static void Error(this string format)
        {
            Log.Error(format);
        }

        public static void Error<T>(this string format, T arg1)
        {
            Log.Error(format, arg1);
        }

        public static void Error<T1, T2>(this string format, T1 arg1, T2 arg2)
        {
            Log.Error(format, arg1, arg2);
        }

        public static void Error<T1, T2, T3>(this string format, T1 arg1, T2 arg2, T3 arg3)
        {
            Log.Error(format, arg1, arg2, arg3);
        }

        public static void Error<T1, T2, T3, T4>(this string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            Log.Error(format, arg1, arg2, arg3, arg4);
        }
    }
}