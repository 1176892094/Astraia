// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-09-27 13:09:05
// // # Recently: 2025-09-27 13:09:05
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using System.Text;

namespace Astraia
{
    public static partial class Service
    {
        public static partial class Text
        {
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