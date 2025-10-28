// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-10-28 13:10:25
// // # Recently: 2025-10-28 13:10:25
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Astraia.Common;

namespace Astraia
{
    [Serializable]
    public abstract class TaskNode
    {
        protected static readonly Task<bool> Success = Task.FromResult(true);
        protected static readonly Task<bool> Failure = Task.FromResult(false);

        public TaskNode[] nodes = Array.Empty<TaskNode>();

        public abstract Task<bool> Execute(object data);

        [Serializable]
        public struct Node
        {
            public string name;
            public List<Node> items;

            public Node(string name)
            {
                this.name = name;
                items = new List<Node>();
            }
        }

        public static void Enqueue(TaskNode node)
        {
            foreach (var child in node.nodes)
            {
                Enqueue(child);
            }

            node.nodes = Array.Empty<TaskNode>();
            HeapManager.Enqueue(node, node.GetType());
        }

        public static TaskNode Dequeue(string node, Func<Node, TaskNode> func)
        {
            var root = LoadRoot(node);
            return root.name != null ? LoadNode(root, func) : null;
        }

        private static TaskNode LoadNode(Node node, Func<Node, TaskNode> func)
        {
            var root = func.Invoke(node);
            if (root != null)
            {
                root.nodes = new TaskNode[node.items.Count];
                for (var i = 0; i < node.items.Count; i++)
                {
                    root.nodes[i] = LoadNode(node.items[i], func);
                }
            }

            return root;
        }

        private static Node LoadRoot(string reason)
        {
            if (string.IsNullOrEmpty(reason))
            {
                return default;
            }

            var index = reason.IndexOf('(');
            if (index < 0)
            {
                return new Node(reason);
            }

            var result = reason.Substring(0, index).Trim();
            var target = Split(reason, index);

            var node = new Node(result);
            foreach (var child in Split(target))
            {
                node.items.Add(LoadRoot(child));
            }

            return node;
        }

        private static string Split(string reason, int index)
        {
            var depth = 0;
            var count = index;
            while (count < reason.Length)
            {
                if (reason[count] == '(')
                {
                    depth++;
                }
                else if (reason[count] == ')')
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

        private static List<string> Split(string reason)
        {
            var result = new List<string>();
            var depth = 0;
            var index = 0;

            for (var i = 0; i < reason.Length; i++)
            {
                if (reason[i] == '(')
                {
                    depth++;
                }
                else if (reason[i] == ')')
                {
                    depth--;
                }
                else if ((reason[i] == ',' || reason[i] == '，') && depth == 0)
                {
                    result.Add(reason.Substring(index, i - index).Trim());
                    index = i + 1;
                }
            }

            result.Add(reason.Substring(index).Trim());
            return result;
        }

        private sealed class Sequence : TaskNode
        {
            public override async Task<bool> Execute(object data)
            {
                foreach (var node in nodes)
                {
                    var state = await node.Execute(data);
                    if (!state)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        private sealed class Selector : TaskNode
        {
            public override async Task<bool> Execute(object data)
            {
                foreach (var node in nodes)
                {
                    var state = await node.Execute(data);
                    if (state)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        private sealed class Actuator : TaskNode
        {
            public override async Task<bool> Execute(object data)
            {
                if (nodes == null || nodes.Length == 0)
                {
                    return true;
                }

                var index = Service.Seed.Next(nodes.Length);
                var state = await nodes[index].Execute(data);
                if (state)
                {
                    return true;
                }

                return false;
            }
        }

        private sealed class Repeater : TaskNode
        {
            public int count;

            public override async Task<bool> Execute(object data)
            {
                foreach (var node in nodes)
                {
                    for (int i = 0; i < count; i++)
                    {
                        var state = await node.Execute(data);
                        if (!state)
                        {
                            return false;
                        }
                    }
                }

                return true;
            }
        }
    }
}