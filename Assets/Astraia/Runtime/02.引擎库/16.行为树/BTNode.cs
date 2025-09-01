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
    public abstract class BTNode
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
    
    public sealed class Sequence : BTNode
    {
        private BTNode[] nodes;
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
    }
    
    public sealed class Selector : BTNode
    {
        private BTNode[] nodes;
        private int index;

        public Selector(BTNode[] nodes)
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
    
    public sealed class Operator : BTNode
    {
        private BTNode[] nodes;
        private int index;
        

        protected override void OnEnter()
        {
            index = Service.Random.Next(nodes.Length);
        }

        protected override State OnUpdate()
        {
            return nodes[index].Tick();
        }
    }
    
    public sealed class Repeater : BTNode
    {
        private BTNode node;
        private int count;
        private int repeat;

        public Repeater(BTNode node, int repeat = -1)
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
    
    public sealed class Parallel : BTNode
    {
        private BTNode[] nodes;
        private int success;
        private int failure;

        public Parallel(BTNode[] nodes, int success = -1, int failure = -1)
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
    
    public sealed class Inverter : BTNode
    {
        private BTNode node;

        public Inverter(BTNode node)
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
    
    public sealed class Success : BTNode
    {
        private BTNode node;

        public Success(BTNode node)
        {
            this.node = node;
        }

        protected override State OnUpdate()
        {
            return node.Tick() == State.Running ? State.Running : State.Success;
        }
    }
    
    public sealed class Failure : BTNode
    {
        private BTNode node;

        public Failure(BTNode node)
        {
            this.node = node;
        }

        protected override State OnUpdate()
        {
            return node.Tick() == State.Running ? State.Running : State.Failure;
        }
    }
    
    public sealed class WaitTime : BTNode
    {
        private float waitTime;
        private float duration;
        
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