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
    public abstract class TaskNode
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

        public sealed class Sequence : TaskNode
        {
            private TaskNode[] nodes;

            public static Sequence Create(TaskNode[] nodes)
            {
                var sequence = Activator.CreateInstance<Sequence>();
                sequence.nodes = nodes;
                return sequence;
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

        public sealed class Selector : TaskNode
        {
            private TaskNode[] nodes;

            public static Selector Create(TaskNode[] nodes)
            {
                var selector = Activator.CreateInstance<Selector>();
                selector.nodes = nodes;
                return selector;
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

        public sealed class Parallel : TaskNode
        {
            private TaskNode[] nodes;
            private int success;
            private int failure;

            public static Parallel Create(TaskNode[] nodes, int success = -1, int failure = -1)
            {
                var parallel = Activator.CreateInstance<Parallel>();
                parallel.nodes = nodes;
                parallel.success = success < 0 ? nodes.Length : success;
                parallel.failure = failure < 0 ? nodes.Length : failure;
                return parallel;
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

        public sealed class Operator : TaskNode
        {
            private TaskNode[] nodes;

            public static Operator Create(TaskNode[] nodes)
            {
                var operation = Activator.CreateInstance<Operator>();
                operation.nodes = nodes;
                return operation;
            }

            protected override State OnUpdate()
            {
                return nodes[Service.Random.Next(nodes.Length)].Tick();
            }
        }

        public sealed class Repeater : TaskNode
        {
            private TaskNode node;
            private int count;
            private int repeat;

            public static Repeater Create(TaskNode node, int repeat = -1)
            {
                var repeater = Activator.CreateInstance<Repeater>();
                repeater.node = node;
                repeater.repeat = repeat;
                return repeater;
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

        public sealed class Inverter : TaskNode
        {
            private TaskNode node;

            public static Inverter Create(TaskNode node)
            {
                var inverter = Activator.CreateInstance<Inverter>();
                inverter.node = node;
                return inverter;
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

        public sealed class Success : TaskNode
        {
            private TaskNode node;

            public static Success Create(TaskNode node)
            {
                var success = Activator.CreateInstance<Success>();
                success.node = node;
                return success;
            }

            protected override State OnUpdate()
            {
                return node.Tick() == State.Running ? State.Running : State.Success;
            }
        }

        public sealed class Failure : TaskNode
        {
            private TaskNode node;

            public static Failure Create(TaskNode node)
            {
                var failure = Activator.CreateInstance<Failure>();
                failure.node = node;
                return failure;
            }

            protected override State OnUpdate()
            {
                return node.Tick() == State.Running ? State.Running : State.Failure;
            }
        }

        public sealed class WaitTime : TaskNode
        {
            private float waitTime;
            private float duration;

            public static WaitTime Create(float duration)
            {
                var failure = Activator.CreateInstance<WaitTime>();
                failure.duration = duration;
                return failure;
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