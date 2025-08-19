// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-04-30 22:04:55
// // # Recently: 2025-04-30 22:04:55
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System.Collections.Generic;

namespace Astraia
{
    public static partial class Service
    {
        public static partial class Word
        {
            private class Node
            {
                public readonly Dictionary<char, Node> nodes = new Dictionary<char, Node>();
                public bool finish;
            }
        }
    }
}