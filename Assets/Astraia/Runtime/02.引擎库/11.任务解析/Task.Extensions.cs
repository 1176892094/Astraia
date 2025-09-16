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
using Astraia.Common;

namespace Astraia
{
    public static partial class Extensions
    {
        public static TaskNode ToNode(this string reason, Func<TaskData, TaskNode> OnResolve)
        {
            var root = reason.GetRoot();
            return string.IsNullOrEmpty(root.name) ? root.LoadNode(OnResolve) : null;
        }

        private static TaskNode LoadNode(this TaskData root, Func<TaskData, TaskNode> OnResolve)
        {
            TaskNode result = null;
            if (OnResolve != null)
            {
                result = OnResolve.Invoke(root);
            }

            foreach (var node in root.items)
            {
                LoadNode(node, OnResolve);
            }

            return result;
        }

        private static TaskData GetRoot(this string reason)
        {
            if (string.IsNullOrEmpty(reason))
            {
                return default;
            }

            var index = reason.IndexOf('(');
            if (index < 0)
            {
                return new TaskData(reason);
            }

            var result = reason.Substring(0, index).Trim();
            var braced = Parse(reason, index);

            var node = new TaskData(result);
            foreach (var child in Split(braced))
            {
                node.items.Add(child.GetRoot());
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