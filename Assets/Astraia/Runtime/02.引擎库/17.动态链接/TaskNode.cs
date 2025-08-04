// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-07-30 16:07:54
// // # Recently: 2025-07-30 16:07:54
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Astraia
{
    [Serializable]
    public abstract partial class TaskNode
    {
        public TaskNode[] nodes;
        public abstract Task<State> Execute(IDictionary<string,object> context);

        protected static class Task
        {
            public static readonly Task<State> Success = System.Threading.Tasks.Task.FromResult(State.Success);
            public static readonly Task<State> Failure = System.Threading.Tasks.Task.FromResult(State.Failure);
        }

        public enum State
        {
            Success,
            Failure
        }
    }

    public partial class TaskNode
    {
        [Serializable]
        public class Sequence : TaskNode
        {
            public override async Task<State> Execute(IDictionary<string,object> context)
            {
                foreach (var node in nodes)
                {
                    var state = await node.Execute(context);
                    if (state == State.Failure)
                    {
                        return State.Failure;
                    }
                }

                return State.Success;
            }
        }

        [Serializable]
        public class Selector : TaskNode
        {
            public override async Task<State> Execute(IDictionary<string,object> context)
            {
                foreach (var node in nodes)
                {
                    var state = await node.Execute(context);
                    if (state == State.Success)
                    {
                        return State.Success;
                    }
                }

                return State.Failure;
            }
        }

        [Serializable]
        public class Repeater : TaskNode
        {
            public int count;

            public override async Task<State> Execute(IDictionary<string,object> context)
            {
                foreach (var node in nodes)
                {
                    for (int i = 0; i < count; i++)
                    {
                        var state = await node.Execute(context);
                        if (state == State.Failure)
                        {
                            return State.Failure;
                        }
                    }
                }

                return State.Success;
            }
        }

        [Serializable]
        public class Operator : TaskNode
        {
            public override async Task<State> Execute(IDictionary<string,object> context)
            {
                var index = Service.Random.Next(nodes.Length);
                if (nodes.Length > 0)
                {
                    var state = await nodes[index].Execute(context);
                    if (state == State.Success)
                    {
                        return State.Success;
                    }
                }

                return State.Failure;
            }
        }

        [Serializable]
        public class WaitTime : TaskNode
        {
            public Entity owner;
            public float waitTime;

            public override async Task<State> Execute(IDictionary<string,object> context)
            {
                if (!owner)
                {
                    return State.Failure;
                }

                await owner.Wait(waitTime);
                foreach (var node in nodes)
                {
                    var state = await node.Execute(context);
                    if (state == State.Failure)
                    {
                        return State.Failure;
                    }
                }

                return State.Success;
            }
        }
    }
}