// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-04-23 15:04:38
// // # Recently: 2025-04-23 15:04:38
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;

namespace Astraia
{
    public static class Log
    {
        private static Action<string> onInfo = Console.WriteLine;
        private static Action<string> onWarn = Console.WriteLine;
        private static Action<string> onError = Console.Error.WriteLine;

        public static void Setup(Action<string> onInfo, Action<string> onWarn, Action<string> onError)
        {
            Log.onInfo = onInfo;
            Log.onWarn = onWarn;
            Log.onError = onError;
        }

        public static void Info(object message)
        {
            onInfo(message.ToString());
        }

        public static void Warn(object message)
        {
            onWarn(message.ToString());
        }

        public static void Error(object message)
        {
            onError(message.ToString());
        }

        public static void Info<T>(string format, T arg1)
        {
            onInfo(format.Format(arg1));
        }

        public static void Info<T1, T2>(string format, T1 arg1, T2 arg2)
        {
            onInfo(format.Format(arg1, arg2));
        }

        public static void Info<T1, T2, T3>(string format, T1 arg1, T2 arg2, T3 arg3)
        {
            onInfo(format.Format(arg1, arg2, arg3));
        }

        public static void Info<T1, T2, T3, T4>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            onInfo(format.Format(arg1, arg2, arg3, arg4));
        }

        public static void Warn<T>(string format, T arg1)
        {
            onWarn(format.Format(arg1));
        }

        public static void Warn<T1, T2>(string format, T1 arg1, T2 arg2)
        {
            onWarn(format.Format(arg1, arg2));
        }

        public static void Warn<T1, T2, T3>(string format, T1 arg1, T2 arg2, T3 arg3)
        {
            onWarn(format.Format(arg1, arg2, arg3));
        }

        public static void Warn<T1, T2, T3, T4>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            onWarn(format.Format(arg1, arg2, arg3, arg4));
        }

        public static void Error<T>(string format, T arg1)
        {
            onError(format.Format(arg1));
        }

        public static void Error<T1, T2>(string format, T1 arg1, T2 arg2)
        {
            onError(format.Format(arg1, arg2));
        }

        public static void Error<T1, T2, T3>(string format, T1 arg1, T2 arg2, T3 arg3)
        {
            onError(format.Format(arg1, arg2, arg3));
        }

        public static void Error<T1, T2, T3, T4>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            onError(format.Format(arg1, arg2, arg3, arg4));
        }
    }
}