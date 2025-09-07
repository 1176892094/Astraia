// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-04-09 23:04:01
// // # Recently: 2025-04-09 23:04:01
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using System.Collections.Generic;

namespace Astraia
{
    public static partial class Extensions
    {
        public static bool IsNullOrEmpty(this string result)
        {
            return string.IsNullOrEmpty(result);
        }

        public static bool IsNullOrWhiteSpace(this string result)
        {
            return string.IsNullOrWhiteSpace(result);
        }

        public static string Bold(this string result)
        {
            return "<b>{0}</b>".Format(result);
        }

        public static string Line(this string result)
        {
            return "<u>{0}</u>".Format(result);
        }

        public static string Link(this string result, string format)
        {
            return "<a href=\"{0}\">{1}</a>".Format(format, result);
        }

        public static string Color(this string result, string format)
        {
            return format switch
            {
                "R" => "<color=#FF0000>{0}</color>".Format(result), //红
                "G" => "<color=#00FF00>{0}</color>".Format(result), //绿
                "B" => "<color=#0000FF>{0}</color>".Format(result), //蓝
                "Y" => "<color=#FFFF00>{0}</color>".Format(result), //黄
                "O" => "<color=#FFAA00>{0}</color>".Format(result), //橙
                "S" => "<color=#00FFFF>{0}</color>".Format(result), //青
                "P" => "<color=#FFAAFF>{0}</color>".Format(result), //紫
                "W" => "<color=#FFFFFF>{0}</color>".Format(result), //白
                _ => "<color=#{0}>{1}</color>".Format(format, result)
            };
        }

        public static T ToEnum<T>(this string result) where T : struct, Enum
        {
            return Enum.TryParse(result, out T value) ? value : default;
        }

        public static T ToNext<T>(this T current) where T : struct, Enum
        {
            var enumArray = (T[])Enum.GetValues(typeof(T));
            var currIndex = Array.IndexOf(enumArray, current);
            var nextIndex = (currIndex + 1) % enumArray.Length;
            return enumArray[nextIndex];
        }

        public static T ToLast<T>(this T current) where T : struct, Enum
        {
            var enumArray = (T[])Enum.GetValues(typeof(T));
            var currIndex = Array.IndexOf(enumArray, current);
            var lastIndex = (currIndex - 1 + enumArray.Length) % enumArray.Length;
            return enumArray[lastIndex];
        }

        public static T ToRandom<T>(this T current) where T : struct, Enum
        {
            var enumArray = (T[])Enum.GetValues(typeof(T));
            var enumFlags = new List<T>();
            foreach (var item in enumArray)
            {
                if (current.HasFlag(item))
                {
                    enumFlags.Add(item);
                }
            }

            return enumFlags[Service.Random.Next(enumFlags.Count)];
        }
    }
}