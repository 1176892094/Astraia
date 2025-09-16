// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-09-16 20:09:28
// // # Recently: 2025-09-16 20:09:28
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using System.Collections.Generic;

namespace Astraia.Common
{
    public static class NodeManager
    {
        private static readonly Dictionary<int, Queue<TaskNode>> poolData = new Dictionary<int, Queue<TaskNode>>();

        public static TaskNode Dequeue(int id, string root, Func<NodeData, TaskNode> func)
        {
            if (!poolData.TryGetValue(id, out var pool))
            {
                pool = new Queue<TaskNode>();
                poolData.Add(id, pool);
            }

            return pool.Count > 0 ? pool.Dequeue() : LoadNode(GetRoot(root), func);
        }

        public static void Enqueue(int id, TaskNode node)
        {
            if (!poolData.TryGetValue(id, out var pool))
            {
                pool = new Queue<TaskNode>();
                poolData.Add(id, pool);
            }

            pool.Enqueue(node);
        }

        private static TaskNode LoadNode(NodeData root, Func<NodeData, TaskNode> func)
        {
            if (root.name == null)
            {
                return null;
            }

            var result = func.Invoke(root);
            if (result == null)
            {
                return null;
            }

            result.nodes = new TaskNode[root.items.Count];
            for (int i = 0; i < root.items.Count; i++)
            {
                var node = LoadNode(root.items[i], func);
                if (node != null)
                {
                    result.nodes[i] = node;
                }
            }

            return result;
        }

        private static NodeData GetRoot(string reason)
        {
            if (string.IsNullOrEmpty(reason))
            {
                return default;
            }

            var index = reason.IndexOf('(');
            if (index < 0)
            {
                return new NodeData(reason);
            }

            var result = reason.Substring(0, index).Trim();
            var braced = Parse(reason, index);

            var node = new NodeData(result);
            foreach (var child in Split(braced))
            {
                node.items.Add(GetRoot(child));
            }

            return node;
        }

        private static string Parse(string reason, int index)
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
    }
}