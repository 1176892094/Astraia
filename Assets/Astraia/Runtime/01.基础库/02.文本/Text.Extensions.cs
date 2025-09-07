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

using System;
using System.Text;

namespace Astraia
{
    public static partial class Extensions
    {
        [ThreadStatic] private static StringBuilder builder;

        private static StringBuilder Builder => builder ??= new StringBuilder(1024);

        public static string Format<T>(this string format, T arg1)
        {
            Builder.Length = 0;
            Builder.AppendFormat(format, arg1);
            return Builder.ToString();
        }

        public static string Format<T1, T2>(this string format, T1 arg1, T2 arg2)
        {
            Builder.Length = 0;
            Builder.AppendFormat(format, arg1, arg2);
            return Builder.ToString();
        }

        public static string Format<T1, T2, T3>(this string format, T1 arg1, T2 arg2, T3 arg3)
        {
            Builder.Length = 0;
            Builder.AppendFormat(format, arg1, arg2, arg3);
            return Builder.ToString();
        }

        public static string Format<T1, T2, T3, T4>(this string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            Builder.Length = 0;
            Builder.AppendFormat(format, arg1, arg2, arg3, arg4);
            return Builder.ToString();
        }

        public static string Format<T1, T2, T3, T4, T5>(this string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            Builder.Length = 0;
            Builder.AppendFormat(format, arg1, arg2, arg3, arg4, arg5);
            return Builder.ToString();
        }

        public static string Format<T1, T2, T3, T4, T5, T6>(this string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            Builder.Length = 0;
            Builder.AppendFormat(format, arg1, arg2, arg3, arg4, arg5, arg6);
            return Builder.ToString();
        }

        public static bool IsNullOrEmpty(this string result)
        {
            return string.IsNullOrEmpty(result);
        }

        public static bool IsNullOrWhiteSpace(this string result)
        {
            return string.IsNullOrWhiteSpace(result);
        }

        public static string Color(this string result, string format)
        {
            return format switch
            {
                "R" => "<color=#FF0000>{0}</color>".Format(result), // 红
                "G" => "<color=#00FF00>{0}</color>".Format(result), // 绿
                "B" => "<color=#0000FF>{0}</color>".Format(result), // 蓝
                "Y" => "<color=#FFFF00>{0}</color>".Format(result), // 黄
                "O" => "<color=#FFAA00>{0}</color>".Format(result), // 橙
                "S" => "<color=#00FFFF>{0}</color>".Format(result), // 青
                "P" => "<color=#FFAAFF>{0}</color>".Format(result), // 紫
                "W" => "<color=#FFFFFF>{0}</color>".Format(result), // 白
                _ => "<color=#{0}>{1}</color>".Format(format, result)
            };
        }
    }
}