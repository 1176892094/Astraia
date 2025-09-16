// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2024-12-23 18:12:21
// # Recently: 2025-01-08 17:01:32
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using Astraia.Common;

namespace Astraia.Common
{
    public interface IState
    {
        void Create(Entity owner);
        void OnEnter();
        void OnUpdate();
        void OnExit();
    }
}

namespace Astraia
{
    [Serializable]
    public abstract class State<T> : IState where T : Entity
    {
        public INode node;
        public T owner;

        void IState.Create(Entity owner)
        {
            this.owner = (T)owner;
            OnCreate();
        }

        void IState.OnEnter() => OnEnter();
        void IState.OnUpdate() => node.Tick();
        void IState.OnExit() => OnExit();

        private NodeState Tick()
        {
            OnUpdate();
            return NodeState.Success;
        }

        protected virtual void OnCreate()
        {
            node = ActionNode.Create(Tick);
        }

        protected virtual void OnEnter()
        {
        }

        protected virtual void OnUpdate()
        {
        }

        protected virtual void OnExit()
        {
        }
    }
}