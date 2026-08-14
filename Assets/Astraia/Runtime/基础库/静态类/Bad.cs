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

using System;
using System.Collections.Generic;

namespace Astraia
{
    internal static class Bad
    {
        private sealed class Node
        {
            public readonly Dictionary<char, int> Next = new();
            public int Fail;
            public int Length;
        }

        private static readonly List<Node> nodes = new List<Node>() { new Node() };

        public static void SetUp(string text)
        {
            var start = 0;
            for (var i = 0; i <= text.Length; i++)
            {
                if (i == text.Length || text[i] == '\n')
                {
                    var count = i - start;
                    if (count > 0)
                    {
                        var word = text.AsSpan(start, count).Trim();
                        if (word.Length > 0)
                        {
                            Add(word);
                        }
                    }

                    start = i + 1;
                }
            }

            Build();
        }

        private static void Add(ReadOnlySpan<char> word)
        {
            var current = 0;

            foreach (var c in word)
            {
                if (!nodes[current].Next.TryGetValue(c, out var next))
                {
                    next = nodes.Count;
                    nodes[current].Next.Add(c, next);
                    nodes.Add(new Node());
                }

                current = next;
            }

            nodes[current].Length = Math.Max(nodes[current].Length, word.Length);
        }

        private static void Build()
        {
            var queue = new Queue<int>();

            foreach (var item in nodes[0].Next)
            {
                queue.Enqueue(item.Value);
                nodes[item.Value].Fail = 0;
            }

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                foreach (var (c, child) in nodes[current].Next)
                {
                    queue.Enqueue(child);

                    var fail = nodes[current].Fail;

                    while (fail != 0 && !nodes[fail].Next.ContainsKey(c))
                    {
                        fail = nodes[fail].Fail;
                    }

                    nodes[child].Fail = nodes[fail].Next.GetValueOrDefault(c, 0);

                    var failNode = nodes[child].Fail;

                    if (nodes[failNode].Length > nodes[child].Length)
                    {
                        nodes[child].Length = nodes[failNode].Length;
                    }
                }
            }
        }

        public static string Filter(string text, char mask = '*')
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            var buffer = text.Length <= 256 ? stackalloc char[text.Length] : new char[text.Length];

            if (TryFilter(text.AsSpan(), buffer, mask))
            {
                return new string(buffer);
            }

            return text;
        }

        private static bool TryFilter(ReadOnlySpan<char> input, Span<char> output, char mask = '*')
        {
            input.CopyTo(output);

            var bad = false;

            var state = 0;

            for (var i = 0; i < input.Length; i++)
            {
                state = Move(state, input[i]);

                var length = nodes[state].Length;

                if (length > 0)
                {
                    bad = true;

                    var start = i - length + 1;

                    for (var j = start; j <= i; j++)
                    {
                        output[j] = mask;
                    }
                }
            }

            return bad;
        }

        private static int Move(int state, char c)
        {
            while (state != 0 && !nodes[state].Next.ContainsKey(c))
            {
                state = nodes[state].Fail;
            }

            return nodes[state].Next.GetValueOrDefault(c, 0);
        }
    }
}