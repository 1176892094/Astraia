// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2025-04-09 21:04:23
// # Recently: 2025-04-09 21:04:23
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Text;

namespace Astraia
{
    internal static class String
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
    }

    public static class StringExtensions
    {
        public static string Format<T>(this string format, T arg1)
        {
            return String.Format(format, arg1);
        }

        public static string Format<T1, T2>(this string format, T1 arg1, T2 arg2)
        {
            return String.Format(format, arg1, arg2);
        }

        public static string Format<T1, T2, T3>(this string format, T1 arg1, T2 arg2, T3 arg3)
        {
            return String.Format(format, arg1, arg2, arg3);
        }

        public static string Format<T1, T2, T3, T4>(this string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            return String.Format(format, arg1, arg2, arg3, arg4);
        }

        public static bool IsNullOrEmpty(this string result)
        {
            return string.IsNullOrEmpty(result);
        }

        public static string Mask(this string result, char mask = '*')
        {
            return Bad.Invoke(result, mask);
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

        public static string Align(this string str, int count, string mask = "")
        {
            var width = 0;
            var i1 = str.Length;

            for (var i = 0; i < i1; i++)
            {
                width += str[i] > 255 ? 2 : 1;
            }

            if (width <= count)
            {
                return str + new string(' ', count - width);
            }

            var cur = 0;
            var i2 = 0;
            while (i2 < i1)
            {
                var w = str[i2] > 255 ? 2 : 1;

                if (cur + w + mask.Length > count)
                {
                    break;
                }

                cur += w;
                i2++;
            }

            return str.Substring(0, i2) + mask;
        }
    }
}