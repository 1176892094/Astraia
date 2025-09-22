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
using System.Collections.Generic;
using System.Threading.Tasks;
using Threading = System.Threading;

namespace Astraia
{
    [Serializable]
    public abstract partial class TaskNode
    {
        public TaskNode[] nodes = Array.Empty<TaskNode>();
        public abstract Task<State> Execute(Entity owner, int id);

        public enum State
        {
            Success,
            Failure
        }

        [Serializable]
        public struct Node
        {
            public List<Node> items;
            public string name;

            public Node(string name)
            {
                this.name = name;
                items = new List<Node>();
            }
        }

        protected static class Task
        {
            public static readonly Task<State> Success = Threading.Tasks.Task.FromResult(State.Success);
            public static readonly Task<State> Failure = Threading.Tasks.Task.FromResult(State.Failure);
        }
    }

    public partial class TaskNode
    {
        private sealed class Sequence : TaskNode
        {
            public override async Task<State> Execute(Entity owner, int id)
            {
                foreach (var node in nodes)
                {
                    var state = await node.Execute(owner, id);
                    if (state == State.Failure)
                    {
                        return State.Failure;
                    }
                }

                return State.Success;
            }
        }

        private sealed class Selector : TaskNode
        {
            public override async Task<State> Execute(Entity owner, int id)
            {
                foreach (var node in nodes)
                {
                    var state = await node.Execute(owner, id);
                    if (state == State.Success)
                    {
                        return State.Success;
                    }
                }

                return State.Failure;
            }
        }

        private sealed class Actuator : TaskNode
        {
            public override async Task<State> Execute(Entity owner, int id)
            {
                if (nodes == null || nodes.Length == 0)
                {
                    return State.Success;
                }

                var index = Service.Random.Next(nodes.Length);
                var state = await nodes[index].Execute(owner, id);
                if (state == State.Success)
                {
                    return State.Success;
                }

                return State.Failure;
            }
        }

        private sealed class Repeater : TaskNode
        {
            public int count;

            public override async Task<State> Execute(Entity owner, int id)
            {
                foreach (var node in nodes)
                {
                    for (int i = 0; i < count; i++)
                    {
                        var state = await node.Execute(owner, id);
                        if (state == State.Failure)
                        {
                            return State.Failure;
                        }
                    }
                }

                return State.Success;
            }
        }

        private sealed class WaitTime : TaskNode
        {
            public float waitTime;

            public override async Task<State> Execute(Entity owner, int id)
            {
                await owner.Wait(waitTime);
                return State.Success;
            }
        }
    }
}