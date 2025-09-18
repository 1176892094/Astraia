// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-04-30 21:04:34
// // # Recently: 2025-04-30 21:04:34
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System.Collections.Generic;

namespace Astraia
{
    public static partial class Service
    {
        internal static class Input
        {
            private static readonly Node root = new Node();

            public static void LoadData(string text)
            {
                var cache = Zip.Decompress(text);
                var words = cache.Split('\n');
                foreach (var word in words)
                {
                    Search(word);
                }
            }

            private static void Search(string word)
            {
                var current = root;
                foreach (var ch in word)
                {
                    if (!current.nodes.TryGetValue(ch, out var node))
                    {
                        node = new Node();
                        current.nodes[ch] = node;
                    }

                    current = node;
                }

                current.finish = true;
            }

            public static string Mask(string text, char mask)
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

            private class Node
            {
                public readonly Dictionary<char, Node> nodes = new Dictionary<char, Node>();
                public bool finish;
            }
        }
    }
}