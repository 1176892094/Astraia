// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-07-30 17:07:54
// // # Recently: 2025-07-30 17:07:54
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using UnityEngine;

namespace Astraia
{
    public abstract class RootNode
    {
        private State state = State.Failure;
        private bool started;

        public State Tick()
        {
            if (!started)
            {
                OnEnter();
                started = true;
            }

            state = OnUpdate();

            if (state != State.Running)
            {
                OnExit();
                started = false;
            }

            return state;
        }

        protected abstract State OnUpdate();

        protected virtual void OnEnter()
        {
        }

        protected virtual void OnExit()
        {
        }

        public enum State
        {
            Running,
            Success,
            Failure
        }

        [Serializable]
        public sealed class Sequence : RootNode
        {
            public RootNode[] nodes;
            private int index;

            protected override void OnEnter()
            {
                index = 0;
            }

            protected override State OnUpdate()
            {
                while (index < nodes.Length)
                {
                    var result = nodes[index].Tick();

                    if (result == State.Running)
                    {
                        return State.Running;
                    }

                    if (result == State.Failure)
                    {
                        return State.Failure;
                    }

                    index++;
                }

                return State.Success;
            }

            public static Sequence Create(RootNode[] nodes)
            {
                return new Sequence() { nodes = nodes };
            }
        }

        [Serializable]
        public sealed class Selector : RootNode
        {
            public RootNode[] nodes;
            private int index;

            protected override void OnEnter()
            {
                index = 0;
            }

            protected override State OnUpdate()
            {
                while (index < nodes.Length)
                {
                    var result = nodes[index].Tick();

                    if (result == State.Running)
                    {
                        return State.Running;
                    }

                    if (result == State.Success)
                    {
                        return State.Success;
                    }

                    index++;
                }

                return State.Failure;
            }

            public static Selector Create(RootNode[] nodes)
            {
                return new Selector() { nodes = nodes };
            }
        }

        [Serializable]
        public sealed class Random : RootNode
        {
            public RootNode[] nodes;
            private int index;

            protected override void OnEnter()
            {
                index = Service.Random.Next(nodes.Length);
            }

            protected override State OnUpdate()
            {
                return nodes[index].Tick();
            }

            public static Random Create(RootNode[] nodes)
            {
                return new Random() { nodes = nodes };
            }
        }

        [Serializable]
        public sealed class Repeater : RootNode
        {
            public RootNode node;
            private int count;
            private int index;

            protected override void OnEnter()
            {
                index = 0;
            }

            protected override State OnUpdate()
            {
                if (count >= 0 && index >= count)
                {
                    return State.Success;
                }

                var result = node.Tick();
                if (result == State.Success || result == State.Failure)
                {
                    index++;
                }

                return State.Running;
            }

            public static Repeater Create(RootNode node, int count = -1)
            {
                return new Repeater() { node = node, count = count };
            }

            public static Repeater Create(RootNode[] nodes)
            {
                return new Repeater() { node = nodes[0], count = -1 };
            }
        }

        [Serializable]
        public sealed class Parallel : RootNode
        {
            public RootNode[] nodes;
            private int success;
            private int failure;

            protected override State OnUpdate()
            {
                var successCount = 0;
                var failureCount = 0;

                foreach (var node in nodes)
                {
                    switch (node.Tick())
                    {
                        case State.Success:
                            successCount++;
                            break;
                        case State.Failure:
                            failureCount++;
                            break;
                        case State.Running:
                            break;
                    }

                    if (successCount >= success)
                    {
                        return State.Success;
                    }

                    if (failureCount >= failure)
                    {
                        return State.Failure;
                    }
                }

                return State.Running;
            }

            public static Parallel Create(RootNode[] nodes, int success, int failure)
            {
                return new Parallel() { nodes = nodes, success = success < 0 ? nodes.Length : success, failure = failure < 0 ? nodes.Length : failure };
            }

            public static Parallel Create(RootNode[] nodes)
            {
                return new Parallel() { nodes = nodes, success = -1, failure = -1 };
            }
        }

        [Serializable]
        public sealed class Inverter : RootNode
        {
            public RootNode node;

            protected override State OnUpdate()
            {
                var result = node.Tick();
                if (result == State.Success)
                {
                    return State.Failure;
                }

                if (result == State.Failure)
                {
                    return State.Success;
                }

                return State.Running;
            }

            public static Inverter Create(RootNode node)
            {
                return new Inverter() { node = node };
            }

            public static Inverter Create(RootNode[] nodes)
            {
                return new Inverter() { node = nodes[0] };
            }
        }

        [Serializable]
        public sealed class Success : RootNode
        {
            public RootNode node;

            protected override State OnUpdate()
            {
                return node.Tick() == State.Running ? State.Running : State.Success;
            }

            public static Success Create(RootNode node)
            {
                return new Success() { node = node };
            }

            public static Success Create(RootNode[] nodes)
            {
                return new Success() { node = nodes[0] };
            }
        }

        [Serializable]
        public sealed class Failure : RootNode
        {
            public RootNode node;

            protected override State OnUpdate()
            {
                return node.Tick() == State.Running ? State.Running : State.Failure;
            }

            public static Failure Create(RootNode node)
            {
                return new Failure() { node = node };
            }

            public static Failure Create(RootNode[] nodes)
            {
                return new Failure() { node = nodes[0] };
            }
        }

        [Serializable]
        public sealed class WaitTime : RootNode
        {
            public float duration;
            private float waitTime;

            protected override void OnEnter()
            {
                waitTime = Time.time + duration;
            }

            protected override State OnUpdate()
            {
                return waitTime < Time.time ? State.Success : State.Running;
            }

            protected override void OnExit()
            {
                waitTime = 0;
            }

            public static WaitTime Create(float duration)
            {
                return new WaitTime() { duration = duration };
            }
        }
    }
}