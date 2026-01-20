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
using UnityEngine;

namespace Astraia
{
    public interface INode
    {
        Node OnTick();
    }

    public abstract class CompositeNode : INode
    {
        protected IList<INode> nodes;
        public abstract Node OnTick();
    }

    public abstract class DecoratorNode : INode
    {
        protected INode node;
        public abstract Node OnTick();
    }

    [Serializable]
    public sealed class Sequence : CompositeNode
    {
        private int index;

        public override Node OnTick()
        {
            while (index < nodes.Count)
            {
                var result = nodes[index].OnTick();
                if (result == Node.Running)
                {
                    return Node.Running;
                }

                if (result == Node.Failure)
                {
                    index = 0;
                    return Node.Failure;
                }

                index++;
            }

            index = 0;
            return Node.Success;
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

        public override Node OnTick()
        {
            while (index < nodes.Count)
            {
                var result = nodes[index].OnTick();
                if (result == Node.Running)
                {
                    return Node.Running;
                }

                if (result == Node.Success)
                {
                    index = 0;
                    return Node.Success;
                }

                index++;
            }

            index = 0;
            return Node.Failure;
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

        public override Node OnTick()
        {
            var isAll = true;
            var isAny = false;

            foreach (var child in nodes)
            {
                var result = child.OnTick();
                switch (mode)
                {
                    case Mode.Any:
                        if (result == Node.Success)
                        {
                            return Node.Success;
                        }

                        if (result == Node.Failure)
                        {
                            return Node.Failure;
                        }

                        break;

                    case Mode.All:
                        if (result == Node.Failure)
                        {
                            return Node.Failure;
                        }

                        if (result != Node.Success)
                        {
                            isAll = false;
                        }

                        if (result == Node.Success)
                        {
                            isAny = true;
                        }

                        break;
                }
            }

            if (mode == Mode.All)
            {
                return isAll ? Node.Success : Node.Running;
            }

            return isAny ? Node.Success : Node.Running;
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

        public override Node OnTick()
        {
            if (index == -1)
            {
                index = Service.Seed.Next(nodes.Count);
            }

            var result = nodes[index].OnTick();
            if (result == Node.Running)
            {
                return Node.Running;
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

        public override Node OnTick()
        {
            var result = node.OnTick();
            if (result == Node.Running)
            {
                return Node.Running;
            }

            index++;
            if (count < 0 || index < count)
            {
                return Node.Running;
            }

            index = 0;
            return Node.Success;
        }

        public static Repeater Create(params INode[] nodes)
        {
            return new Repeater { node = nodes[0] };
        }
    }

    [Serializable]
    public sealed class Inverter : DecoratorNode
    {
        public override Node OnTick()
        {
            var result = node.OnTick();
            if (result == Node.Success)
            {
                return Node.Failure;
            }

            if (result == Node.Failure)
            {
                return Node.Success;
            }

            return Node.Running;
        }

        public static Inverter Create(params INode[] nodes)
        {
            return new Inverter { node = nodes[0] };
        }
    }

    [Serializable]
    public sealed class Success : DecoratorNode
    {
        public override Node OnTick()
        {
            return node.OnTick() == Node.Running ? Node.Running : Node.Success;
        }

        public static Success Create(params INode[] nodes)
        {
            return new Success { node = nodes[0] };
        }
    }

    [Serializable]
    public sealed class Failure : DecoratorNode
    {
        public override Node OnTick()
        {
            return node.OnTick() == Node.Running ? Node.Running : Node.Failure;
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

        public Node OnTick()
        {
            if (waitTime < 0)
            {
                waitTime = Time.realtimeSinceStartup;
            }

            if (waitTime + duration > Time.realtimeSinceStartup)
            {
                return Node.Running;
            }

            waitTime = -1;
            return Node.Success;
        }

        public static WaitTime Create(float duration)
        {
            return new WaitTime { duration = duration };
        }
    }
}