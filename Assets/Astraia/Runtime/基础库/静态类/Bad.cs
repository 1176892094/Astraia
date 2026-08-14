// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-08-14 19:08:46
// # Recently: 2026-08-14 19:24:46
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System.Collections.Generic;

namespace Astraia
{
    internal static class Bad
    {
        private class Node
        {
            public readonly Dictionary<char, Node> nodes = new();
            public bool finish;
        }

        private static readonly Node root = new();

        public static void SetUp(string text)
        {
            var splits = Zip.Decompress(text).Split('\n');
            foreach (var chars in splits)
            {
                var current = root;
                foreach (var c in chars)
                {
                    if (!current.nodes.TryGetValue(c, out var node))
                    {
                        node = new Node();
                        current.nodes[c] = node;
                    }

                    current = node;
                }

                current.finish = true;
            }
        }

        public static string Invoke(string text, char mask)
        {
            var chars = text.ToCharArray();
            for (var i = 0; i < chars.Length; i++)
            {
                var current = root;
                var j = i;
                while (j < chars.Length && current.nodes.TryGetValue(chars[j], out var next))
                {
                    if (next.finish)
                    {
                        for (var k = i; k <= j; k++)
                        {
                            chars[k] = mask;
                        }

                        break;
                    }

                    current = next;
                    j++;
                }
            }

            return new string(chars);
        }
    }
}