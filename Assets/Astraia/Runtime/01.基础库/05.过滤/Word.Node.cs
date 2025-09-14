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

using System.Collections.Generic;

namespace Astraia
{
    public static partial class Service
    {
        internal static partial class Word
        {
            private class Node
            {
                public readonly Dictionary<char, Node> nodes = new Dictionary<char, Node>();
                public bool finish;
            }
        }
    }

    public static partial class Extensions
    {
        public static string Mask(this string result, char mask = '*')
        {
            return Service.Word.Replace(result, mask);
        }
    }
}