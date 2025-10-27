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
using UnityEngine;

namespace Astraia.Common
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
        private Dictionary<int, IState> states = new Dictionary<int, IState>();
        [SerializeReference] private IState state;

        public void Create(int key, Type value)
        {
            if (!states.TryGetValue(key, out var item))
            {
                item = HeapManager.Dequeue<IState>(value);
                states.Add(key, item);
                item.Acquire(owner);
            }
        }

        public void Update()
        {
            state?.OnUpdate();
        }

        public void Switch(int key)
        {
            state?.OnExit();
            states.TryGetValue(key, out state);
            state?.OnEnter();
        }

        public override void Enqueue()
        {
            foreach (var value in states.Values)
            {
                HeapManager.Enqueue(value, value.GetType());
            }

            state = null;
            states.Clear();
        }
    }

    [Serializable]
    public abstract class State<T> : IState where T : Entity
    {
        public T owner { get; protected set; }
        public INode node;

        void IState.Acquire(Entity owner)
        {
            this.owner = (T)owner;
            node = Acquire();
        }

        void IState.OnEnter() => OnEnter();
        void IState.OnUpdate() => node.Tick();
        void IState.OnExit() => OnExit();

        protected virtual INode Acquire()
        {
            return Node.Create(OnUpdate);
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

        private class Node : INode
        {
            private Action OnUpdate;

            public NodeState Tick()
            {
                OnUpdate.Invoke();
                return NodeState.Success;
            }

            public static Node Create(Action OnUpdate)
            {
                return new Node { OnUpdate = OnUpdate };
            }
        }
    }

    [Serializable]
    public abstract class Feature<TEntity> : Module<TEntity> where TEntity : Entity
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