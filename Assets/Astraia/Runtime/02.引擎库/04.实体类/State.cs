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
using System.Collections.Generic;

namespace Astraia.Core
{
    public interface INode
    {
        NodeState Tick();
    }

    public interface IState
    {
        void Acquire(Entity owner);
        void OnEnter();
        void OnUpdate();
        void OnExit();
    }

    [Serializable]
    public abstract class StateMachine<TEntity> : Module<TEntity> where TEntity : Entity
    {
        private Dictionary<int, State> states = new Dictionary<int, State>();
        private State state;

        public void Create(int key, Type value)
        {
            var item = HeapManager.Dequeue<IState>(value);
            states[key] = new State(item, new Node(item.OnUpdate));
            item.Acquire(owner);
        }

        public void Create(int key, Type value, INode node)
        {
            var item = HeapManager.Dequeue<IState>(value);
            states[key] = new State(item, node);
            item.Acquire(owner);
        }

        public void Switch(int key)
        {
            state?.state.OnExit();
            states.TryGetValue(key, out state);
            state?.state.OnEnter();
        }

        public void Update()
        {
            state?.node.Tick();
        }

        public override void Enqueue()
        {
            foreach (var item in states.Values)
            {
                HeapManager.Enqueue(item.state, item.state.GetType());
            }

            state = null;
            states.Clear();
        }

        private class Node : INode
        {
            private readonly Action OnUpdate;

            public Node(Action onUpdate)
            {
                OnUpdate = onUpdate;
            }

            public NodeState Tick()
            {
                OnUpdate.Invoke();
                return NodeState.Success;
            }
        }

        public record State(IState state, INode node);
    }

    [Serializable]
    public abstract class State<T> : IState where T : Entity
    {
        public T owner { get; protected set; }

        void IState.Acquire(Entity owner)
        {
            this.owner = (T)owner;
        }

        public virtual void OnEnter()
        {
        }

        public virtual void OnUpdate()
        {
        }

        public virtual void OnExit()
        {
        }
    }

    [Serializable]
    public abstract class Blackboard<TEntity> : Module<TEntity> where TEntity : Entity
    {
        private readonly Dictionary<Enum, Xor.Float> features = new Dictionary<Enum, Xor.Float>();

        public float GetFloat(Enum key, float value = 0)
        {
            features.TryAdd(key, value);
            return features[key];
        }

        public void SetFloat(Enum key, float value)
        {
            features.TryAdd(key, 0);
            features[key] = value;
        }

        public void AddFloat(Enum key, float value)
        {
            features.TryAdd(key, 0);
            features[key] += value;
        }

        public void SubFloat(Enum key, float value)
        {
            features.TryAdd(key, 0);
            features[key] -= value;
        }

        public int GetInt(Enum key, float value = 0)
        {
            features.TryAdd(key, value);
            return (int)features[key];
        }

        public void SetInt(Enum key, float value)
        {
            features.TryAdd(key, 0);
            features[key] = (int)value;
        }

        public void AddInt(Enum key, float value)
        {
            features.TryAdd(key, 0);
            features[key] += (int)value;
        }

        public void SubInt(Enum key, float value)
        {
            features.TryAdd(key, 0);
            features[key] -= (int)value;
        }

        public override void Enqueue()
        {
            features.Clear();
        }
    }
}