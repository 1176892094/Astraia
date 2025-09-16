// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-09-16 19:09:06
// // # Recently: 2025-09-16 19:09:06
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using System.Threading.Tasks;
using Threading = System.Threading.Tasks;

namespace Astraia
{
    [Serializable]
    public abstract partial class TaskNode
    {
        public TaskNode[] nodes = Array.Empty<TaskNode>();
        public abstract Task<NodeState> Execute(int id);

        protected static class Task
        {
            public static readonly Task<NodeState> Success = Threading.Task.FromResult(NodeState.Success);
            public static readonly Task<NodeState> Failure = Threading.Task.FromResult(NodeState.Failure);
        }
    }

    public partial class TaskNode
    {
        internal sealed class Sequence : TaskNode
        {
            public override async Task<NodeState> Execute(int id)
            {
                foreach (var node in nodes)
                {
                    var state = await node.Execute(id);
                    if (state == NodeState.Failure)
                    {
                        return NodeState.Failure;
                    }
                }

                return NodeState.Success;
            }
        }

        internal sealed class Selector : TaskNode
        {
            public override async Task<NodeState> Execute(int id)
            {
                foreach (var node in nodes)
                {
                    var state = await node.Execute(id);
                    if (state == NodeState.Success)
                    {
                        return NodeState.Success;
                    }
                }

                return NodeState.Failure;
            }
        }

        internal sealed class Actuator : TaskNode
        {
            public override async Task<NodeState> Execute(int id)
            {
                if (nodes == null || nodes.Length == 0)
                {
                    return NodeState.Success;
                }

                var index = Service.Random.Next(nodes.Length);
                var state = await nodes[index].Execute(id);
                if (state == NodeState.Success)
                {
                    return NodeState.Success;
                }

                return NodeState.Failure;
            }
        }

        internal sealed class Repeater : TaskNode
        {
            public int count;

            public override async Task<NodeState> Execute(int id)
            {
                foreach (var node in nodes)
                {
                    for (int i = 0; i < count; i++)
                    {
                        var state = await node.Execute(id);
                        if (state == NodeState.Failure)
                        {
                            return NodeState.Failure;
                        }
                    }
                }

                return NodeState.Success;
            }
        }
    }
}