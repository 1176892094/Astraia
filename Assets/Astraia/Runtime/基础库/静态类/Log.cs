// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-08-14 19:08:34
// # Recently: 2026-08-14 19:19:34
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

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
    }
}