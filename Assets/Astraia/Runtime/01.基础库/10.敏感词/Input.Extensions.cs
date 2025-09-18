// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-09-14 23:09:47
// // # Recently: 2025-09-14 23:09:47
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

namespace Astraia
{
    public static partial class Extensions
    {
        public static string Mask(this string result, char mask = '*')
        {
            return Service.Input.Mask(result, mask);
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