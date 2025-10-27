// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-09-16 16:09:08
// // # Recently: 2025-09-16 16:09:08
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Astraia.Common;
using UnityEngine;
using Threading = System.Threading;

namespace Astraia
{
    public abstract class CompositeNode : INode
    {
        protected IList<INode> nodes;
        public abstract NodeState Tick();
    }

    public abstract class DecoratorNode : INode
    {
        protected INode node;
        public abstract NodeState Tick();
    }

    [Serializable]
    public sealed class Sequence : CompositeNode
    {
        private int index;

        public override NodeState Tick()
        {
            while (index < nodes.Count)
            {
                var result = nodes[index].Tick();
                if (result == NodeState.Running)
                {
                    return NodeState.Running;
                }

                if (result == NodeState.Failure)
                {
                    index = 0;
                    return NodeState.Failure;
                }

                index++;
            }

            index = 0;
            return NodeState.Success;
        }

        public static Sequence Create(params INode[] nodes)
        {
            return new Sequence { nodes = nodes };
        }
    }

    [Serializable]
    public sealed class Selector : CompositeNode
    {
        private int index;

        public override NodeState Tick()
        {
            while (index < nodes.Count)
            {
                var result = nodes[index].Tick();
                if (result == NodeState.Running)
                {
                    return NodeState.Running;
                }

                if (result == NodeState.Success)
                {
                    index = 0;
                    return NodeState.Success;
                }

                index++;
            }

            index = 0;
            return NodeState.Failure;
        }

        public static Selector Create(params INode[] nodes)
        {
            return new Selector { nodes = nodes };
        }
    }

    [Serializable]
    public sealed class Parallel : CompositeNode
    {
        private Mode mode;

        public override NodeState Tick()
        {
            var isAll = true;
            var isAny = false;

            foreach (var child in nodes)
            {
                var result = child.Tick();
                switch (mode)
                {
                    case Mode.Any:
                        if (result == NodeState.Success)
                        {
                            return NodeState.Success;
                        }

                        if (result == NodeState.Failure)
                        {
                            return NodeState.Failure;
                        }

                        break;

                    case Mode.All:
                        if (result == NodeState.Failure)
                        {
                            return NodeState.Failure;
                        }

                        if (result != NodeState.Success)
                        {
                            isAll = false;
                        }

                        if (result == NodeState.Success)
                        {
                            isAny = true;
                        }

                        break;
                }
            }

            if (mode == Mode.All)
            {
                return isAll ? NodeState.Success : NodeState.Running;
            }

            return isAny ? NodeState.Success : NodeState.Running;
        }

        public static Parallel Create(params INode[] nodes)
        {
            return new Parallel { nodes = nodes };
        }

        public enum Mode
        {
            All,
            Any
        }
    }

    [Serializable]
    public sealed class Actuator : CompositeNode
    {
        private int index = -1;

        public override NodeState Tick()
        {
            if (index == -1)
            {
                index = Service.Seed.Next(nodes.Count);
            }

            var result = nodes[index].Tick();
            if (result == NodeState.Running)
            {
                return NodeState.Running;
            }

            index = -1;
            return result;
        }

        public static Actuator Create(params INode[] nodes)
        {
            return new Actuator { nodes = nodes };
        }
    }

    [Serializable]
    public sealed class Repeater : DecoratorNode
    {
        private int count = -1;
        private int index;

        public override NodeState Tick()
        {
            var result = node.Tick();
            if (result == NodeState.Running)
            {
                return NodeState.Running;
            }

            index++;
            if (count < 0 || index < count)
            {
                return NodeState.Running;
            }

            index = 0;
            return NodeState.Success;
        }

        public static Repeater Create(params INode[] nodes)
        {
            return new Repeater { node = nodes[0] };
        }
    }

    [Serializable]
    public sealed class Inverter : DecoratorNode
    {
        public override NodeState Tick()
        {
            var result = node.Tick();
            if (result == NodeState.Success)
            {
                return NodeState.Failure;
            }

            if (result == NodeState.Failure)
            {
                return NodeState.Success;
            }

            return NodeState.Running;
        }

        public static Inverter Create(params INode[] nodes)
        {
            return new Inverter { node = nodes[0] };
        }
    }

    [Serializable]
    public sealed class Success : DecoratorNode
    {
        public override NodeState Tick()
        {
            return node.Tick() == NodeState.Running ? NodeState.Running : NodeState.Success;
        }

        public static Success Create(params INode[] nodes)
        {
            return new Success { node = nodes[0] };
        }
    }

    [Serializable]
    public sealed class Failure : DecoratorNode
    {
        public override NodeState Tick()
        {
            return node.Tick() == NodeState.Running ? NodeState.Running : NodeState.Failure;
        }

        public static Failure Create(params INode[] nodes)
        {
            return new Failure { node = nodes[0] };
        }
    }

    [Serializable]
    public sealed class WaitTime : INode
    {
        private float duration;
        private float waitTime = -1;

        public NodeState Tick()
        {
            if (waitTime < 0)
            {
                waitTime = Time.realtimeSinceStartup;
            }

            if (waitTime + duration > Time.realtimeSinceStartup)
            {
                return NodeState.Running;
            }

            waitTime = -1;
            return NodeState.Success;
        }

        public static WaitTime Create(float duration)
        {
            return new WaitTime { duration = duration };
        }
    }

    [Serializable]
    public abstract class TaskNode
    {
        public TaskNode[] nodes = Array.Empty<TaskNode>();
        public abstract Task<State> Execute(object data);

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

        private sealed class Sequence : TaskNode
        {
            public override async Task<State> Execute(object data)
            {
                foreach (var node in nodes)
                {
                    var state = await node.Execute(data);
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
            public override async Task<State> Execute(object data)
            {
                foreach (var node in nodes)
                {
                    var state = await node.Execute(data);
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
            public override async Task<State> Execute(object data)
            {
                if (nodes == null || nodes.Length == 0)
                {
                    return State.Success;
                }

                var index = Service.Seed.Next(nodes.Length);
                var state = await nodes[index].Execute(data);
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

            public override async Task<State> Execute(object data)
            {
                foreach (var node in nodes)
                {
                    for (int i = 0; i < count; i++)
                    {
                        var state = await node.Execute(data);
                        if (state == State.Failure)
                        {
                            return State.Failure;
                        }
                    }
                }

                return State.Success;
            }
        }
    }
}