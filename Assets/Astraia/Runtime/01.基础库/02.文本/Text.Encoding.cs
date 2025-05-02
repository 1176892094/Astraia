// // *********************************************************************************
// // # Project: JFramework
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
        public static partial class Text
        {
            [ThreadStatic] 
            private static UTF8Encoding encoding;
            private static UTF8Encoding Encoding => encoding ??= new UTF8Encoding(false, true);

            public static byte[] GetBytes(string message)
            {
                return Encoding.GetBytes(message);
            }

            public static string GetString(byte[] bytes)
            {
                return Encoding.GetString(bytes);
            }

            public static string GetString(byte[] bytes, int index, int count)
            {
                return Encoding.GetString(bytes, index, count);
            }

            public static int GetByteCount(int count)
            {
                return Encoding.GetMaxByteCount(count);
            }

            public static int GetByteCount(string message, int count, byte[] buffer, int index)
            {
                return Encoding.GetBytes(message, 0, count, buffer, index);
            }
        }
    }
}