// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-08-13 22:08:36
// # Recently: 2026-08-13 22:49:36
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;

namespace Astraia
{
    public static class Nodes
    {
        private static readonly Dictionary<Type, Func<Node, Func<Node, Type>, INode>> Func = new();

        static Nodes()
        {
            Func[typeof(Sequence)] = SequenceInternal;
            Func[typeof(Selector)] = SelectorInternal;
            Func[typeof(Parallel)] = ParallelInternal;
            Func[typeof(Randomer)] = RandomerInternal;
            Func[typeof(Repeater)] = RepeaterInternal;
            Func[typeof(Inverter)] = InverterInternal;
            Func[typeof(Success)] = SuccessInternal;
            Func[typeof(Failure)] = FailureInternal;
        }

        private static INode SequenceInternal(Node node, Func<Node, Type> func)
        {
            return new Sequence(node.Index, node.Nodes.Select(i => i.Build(func)).ToArray());
        }

        private static INode SelectorInternal(Node node, Func<Node, Type> func)
        {
            return new Selector(node.Index, node.Nodes.Select(i => i.Build(func)).ToArray());
        }

        private static INode ParallelInternal(Node node, Func<Node, Type> func)
        {
            return new Parallel(node.Data, node.Nodes.Select(i => i.Build(func)).ToArray());
        }

        private static INode RandomerInternal(Node node, Func<Node, Type> func)
        {
            return new Randomer(node.Index, node.Nodes.Select(i => i.Build(func)).ToArray());
        }

        private static INode RepeaterInternal(Node node, Func<Node, Type> func)
        {
            return new Repeater(node.Index, int.Parse(node.Data), node.Nodes.Select(i => i.Build(func)).First());
        }

        private static INode InverterInternal(Node node, Func<Node, Type> func)
        {
            return new Inverter(node.Nodes.Select(i => i.Build(func)).First());
        }

        private static INode SuccessInternal(Node node, Func<Node, Type> func)
        {
            return new Success(node.Nodes.Select(i => i.Build(func)).First());
        }

        private static INode FailureInternal(Node node, Func<Node, Type> func)
        {
            return new Failure(node.Nodes.Select(i => i.Build(func)).First());
        }

        public static Node Load(string reason, ref int i)
        {
            if (string.IsNullOrEmpty(reason))
            {
                return default;
            }

            var index = FindFirstBracket(reason);
            if (index < 0)
            {
                return new Node(reason, i++);
            }

            var result = new Node(reason.Substring(0, index).Trim(), i++);
            foreach (var child in LoadNode(Checked(reason, index)))
            {
                result.Nodes.Add(Load(child, ref i));
            }

            return result;
        }

        private static string Checked(string reason, int index)
        {
            var depth = 0;
            var count = index;
            while (count < reason.Length)
            {
                if (IsLeftBracket(reason[count]))
                {
                    depth++;
                }
                else if (IsRightBracket(reason[count]))
                {
                    depth--;
                }

                if (depth == 0)
                {
                    break;
                }

                count++;
            }

            return reason.Substring(index + 1, count - index - 1);
        }

        private static List<string> LoadNode(string reason)
        {
            var result = new List<string>();
            var depth = 0;
            var index = 0;

            for (var i = 0; i < reason.Length; i++)
            {
                var c = reason[i];
                if (IsLeftBracket(c))
                {
                    depth++;
                }
                else if (IsRightBracket(c))
                {
                    depth--;
                }
                else if (depth == 0 && IsSeparator(c))
                {
                    result.Add(reason.Substring(index, i - index).Trim());
                    index = i + 1;
                }
            }

            result.Add(reason.Substring(index).Trim());
            return result;
        }

        private static int FindFirstBracket(string text)
        {
            var englishIndex = text.IndexOf('(');
            var chineseIndex = text.IndexOf('（');

            if (englishIndex < 0) return chineseIndex;
            if (chineseIndex < 0) return englishIndex;

            return Math.Min(englishIndex, chineseIndex);
        }

        private static int FindColon(string text)
        {
            var englishIndex = text.IndexOf(':');
            var chineseIndex = text.IndexOf('：');

            if (englishIndex < 0) return chineseIndex;
            if (chineseIndex < 0) return englishIndex;

            return Math.Min(englishIndex, chineseIndex);
        }

        private static bool IsLeftBracket(char c)
        {
            return c is '(' or '（';
        }

        private static bool IsRightBracket(char c)
        {
            return c is ')' or '）';
        }

        private static bool IsSeparator(char c)
        {
            return c is ',' or '，';
        }

        [Serializable]
        public struct Node
        {
            public int Index;
            public string Name;
            public string Data;
            public List<Node> Nodes;

            public Node(string name, int index)
            {
                var i = FindColon(name);
                if (i < 0)
                {
                    Name = name;
                    Data = null;
                }
                else
                {
                    Name = name.Substring(0, i);
                    Data = name.Substring(i + 1);
                }

                Index = index;
                Nodes = new List<Node>();
            }

            public INode Build(Func<Node, Type> func)
            {
                if (Name.IsNullOrEmpty())
                {
                    throw new NullReferenceException();
                }

                var reason = func.Invoke(this);
                if (Func.TryGetValue(reason, out var result))
                {
                    return result.Invoke(this, func);
                }

                return (INode)Activator.CreateInstance(reason);
            }
        }
    }
}