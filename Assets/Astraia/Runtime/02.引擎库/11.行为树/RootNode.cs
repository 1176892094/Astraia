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
    [Serializable]
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
            [SerializeField] private RootNode[] nodes;

            public Sequence(RootNode[] nodes)
            {
                this.nodes = nodes;
            }

            public static Sequence Create(RootNode[] nodes)
            {
                return new Sequence(nodes);
            }

            protected override State OnUpdate()
            {
                foreach (var node in nodes)
                {
                    var result = node.Tick();
                    if (result == State.Running)
                    {
                        return State.Running;
                    }

                    if (result == State.Failure)
                    {
                        return State.Failure;
                    }
                }

                return State.Success;
            }
        }

        [Serializable]
        public sealed class Selector : RootNode
        {
            [SerializeField] private RootNode[] nodes;

            public Selector(RootNode[] nodes)
            {
                this.nodes = nodes;
            }

            public static Selector Create(RootNode[] nodes)
            {
                return new Selector(nodes);
            }

            protected override State OnUpdate()
            {
                foreach (var node in nodes)
                {
                    var result = node.Tick();
                    if (result == State.Running)
                    {
                        return State.Running;
                    }

                    if (result == State.Success)
                    {
                        return State.Success;
                    }
                }

                return State.Failure;
            }
        }

        [Serializable]
        public sealed class Parallel : RootNode
        {
            [SerializeField] private RootNode[] nodes;
            private int success;
            private int failure;

            public Parallel(RootNode[] nodes, int success = -1, int failure = -1)
            {
                this.nodes = nodes;
                this.success = success < 0 ? nodes.Length : success;
                this.failure = failure < 0 ? nodes.Length : failure;
            }

            public static Parallel Create(RootNode[] nodes)
            {
                return new Parallel(nodes);
            }

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
        }

        [Serializable]
        public sealed class Operator : RootNode
        {
            [SerializeField] private RootNode[] nodes;

            public Operator(RootNode[] nodes)
            {
                this.nodes = nodes;
            }

            public static Operator Create(RootNode[] nodes)
            {
                return new Operator(nodes);
            }

            protected override State OnUpdate()
            {
                return nodes[Service.Random.Next(nodes.Length)].Tick();
            }
        }

        [Serializable]
        public sealed class Repeater : RootNode
        {
            [SerializeField] private RootNode node;
            private int count;
            private int repeat;

            public Repeater(RootNode node, int repeat = -1)
            {
                this.node = node;
                this.repeat = repeat;
            }

            public static Repeater Create(RootNode[] nodes)
            {
                return new Repeater(nodes[0]);
            }

            protected override void OnEnter()
            {
                count = 0;
            }

            protected override State OnUpdate()
            {
                if (repeat >= 0 && count >= repeat)
                {
                    return State.Success;
                }

                var result = node.Tick();
                if (result == State.Success || result == State.Failure)
                {
                    count++;
                }

                return State.Running;
            }
        }

        [Serializable]
        public sealed class Inverter : RootNode
        {
            [SerializeField] private RootNode node;

            public Inverter(RootNode node)
            {
                this.node = node;
            }

            public static Inverter Create(RootNode[] nodes)
            {
                return new Inverter(nodes[0]);
            }

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
        }

        [Serializable]
        public sealed class Success : RootNode
        {
            [SerializeField] private RootNode node;

            public Success(RootNode node)
            {
                this.node = node;
            }

            public static Success Create(RootNode[] nodes)
            {
                return new Success(nodes[0]);
            }

            protected override State OnUpdate()
            {
                return node.Tick() == State.Running ? State.Running : State.Success;
            }
        }

        [Serializable]
        public sealed class Failure : RootNode
        {
            [SerializeField] private RootNode node;

            public Failure(RootNode node)
            {
                this.node = node;
            }

            public static Failure Create(RootNode[] nodes)
            {
                return new Failure(nodes[0]);
            }

            protected override State OnUpdate()
            {
                return node.Tick() == State.Running ? State.Running : State.Failure;
            }
        }

        [Serializable]
        public sealed class WaitTime : RootNode
        {
            [SerializeField] private float duration;
            private float waitTime;


            public WaitTime()
            {
            }

            public WaitTime(float duration)
            {
                this.duration = duration;
            }

            public static WaitTime Create(float duration)
            {
                return new WaitTime(duration);
            }

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
        }
    }
}