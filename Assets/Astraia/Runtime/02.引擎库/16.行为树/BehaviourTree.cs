// // *********************************************************************************
// // # Project: JFramework
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
    public abstract class BehaviourTree
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
    }

    [Serializable]
    public sealed class Sequence : BehaviourTree
    {
        [SerializeField] private BehaviourTree[] nodes;
        [SerializeField] private int index;

        public Sequence(BehaviourTree[] nodes)
        {
            this.nodes = nodes;
        }

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
    }

    [Serializable]
    public sealed class Selector : BehaviourTree
    {
        [SerializeField] private BehaviourTree[] nodes;
        [SerializeField] private int index;

        public Selector(BehaviourTree[] nodes)
        {
            this.nodes = nodes;
        }

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
    }

    [Serializable]
    public sealed class Revealer : BehaviourTree
    {
        [SerializeField] private BehaviourTree[] nodes;
        [SerializeField] private int index;

        public Revealer(BehaviourTree[] nodes)
        {
            this.nodes = nodes;
        }

        protected override void OnEnter()
        {
            index = Service.Random.Next(nodes.Length);
        }

        protected override State OnUpdate()
        {
            return nodes[index].Tick();
        }
    }

    [Serializable]
    public sealed class Repeater : BehaviourTree
    {
        [SerializeField] private BehaviourTree node;
        [SerializeField] private int count;
        [SerializeField] private int repeat;

        public Repeater(BehaviourTree node, int repeat = -1)
        {
            this.node = node;
            this.repeat = repeat;
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
    public sealed class Parallel : BehaviourTree
    {
        [SerializeField] private BehaviourTree[] nodes;
        [SerializeField] private int success;
        [SerializeField] private int failure;

        public Parallel(BehaviourTree[] nodes, int success = -1, int failure = -1)
        {
            this.nodes = nodes;
            this.success = success < 0 ? nodes.Length : success;
            this.failure = failure < 0 ? nodes.Length : failure;
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
    public sealed class Inverter : BehaviourTree
    {
        [SerializeField] private BehaviourTree node;

        public Inverter(BehaviourTree node)
        {
            this.node = node;
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
    public sealed class Success : BehaviourTree
    {
        [SerializeField] private BehaviourTree node;

        public Success(BehaviourTree node)
        {
            this.node = node;
        }

        protected override State OnUpdate()
        {
            return node.Tick() == State.Running ? State.Running : State.Success;
        }
    }

    [Serializable]
    public sealed class Failure : BehaviourTree
    {
        [SerializeField] private BehaviourTree node;

        public Failure(BehaviourTree node)
        {
            this.node = node;
        }

        protected override State OnUpdate()
        {
            return node.Tick() == State.Running ? State.Running : State.Failure;
        }
    }

    [Serializable]
    public sealed class WaitTime : BehaviourTree
    {
        [SerializeField] private float waitTime;
        [SerializeField] private float duration;

        public WaitTime(float duration)
        {
            this.duration = duration;
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