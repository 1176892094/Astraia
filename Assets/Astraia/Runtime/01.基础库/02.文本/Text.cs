// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-04-26 16:04:36
// // # Recently: 2025-04-26 16:04:36
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using System.Text;

namespace Astraia
{
    public static partial class Service
    {
        public static class Text
        {
            [ThreadStatic] private static StringBuilder stringBuilder;

            private static StringBuilder StringBuilder => stringBuilder ??= new StringBuilder(1024);

            internal static string Format<T>(string format, T arg1)
            {
                StringBuilder.Length = 0;
                StringBuilder.AppendFormat(format, arg1);
                return StringBuilder.ToString();
            }

            internal static string Format<T1, T2>(string format, T1 arg1, T2 arg2)
            {
                StringBuilder.Length = 0;
                StringBuilder.AppendFormat(format, arg1, arg2);
                return StringBuilder.ToString();
            }

            internal static string Format<T1, T2, T3>(string format, T1 arg1, T2 arg2, T3 arg3)
            {
                StringBuilder.Length = 0;
                StringBuilder.AppendFormat(format, arg1, arg2, arg3);
                return StringBuilder.ToString();
            }

            internal static string Format<T1, T2, T3, T4>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
            {
                StringBuilder.Length = 0;
                StringBuilder.AppendFormat(format, arg1, arg2, arg3, arg4);
                return StringBuilder.ToString();
            }

            [ThreadStatic] private static UTF8Encoding stringEncode;

            private static UTF8Encoding StringEncode => stringEncode ??= new UTF8Encoding(false, true);

            public static byte[] GetBytes(string message)
            {
                return StringEncode.GetBytes(message);
            }

            public static int GetBytes(string message, int count, byte[] buffer, int index)
            {
                return StringEncode.GetBytes(message, 0, count, buffer, index);
            }

            public static string GetString(byte[] bytes)
            {
                return StringEncode.GetString(bytes);
            }

            public static string GetString(byte[] bytes, int index, int count)
            {
                return StringEncode.GetString(bytes, index, count);
            }

            public static int GetMaxByteCount(int count)
            {
                return StringEncode.GetMaxByteCount(count);
            }
        }
    }
}