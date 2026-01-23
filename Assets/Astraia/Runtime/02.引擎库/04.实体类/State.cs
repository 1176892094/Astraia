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
        private IState state;

        public void Create(int key, Type value)
        {
            var item = HeapManager.Dequeue<IState>(value);
            item.Acquire(owner);
            states[key] = item;
        }

        public void Switch(int key)
        {
            state?.OnExit();
            states.TryGetValue(key, out state);
            state?.OnEnter();
        }

        public void Update()
        {
            state?.OnUpdate();
        }

        public override void Enqueue()
        {
            foreach (var item in states.Values)
            {
                HeapManager.Enqueue(item, item.GetType());
            }

            state = null;
            states.Clear();
        }
    }

    [Serializable]
    public abstract class State<T> : IState where T : Entity
    {
        public T owner;

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
        private Dictionary<Enum, int> features = new Dictionary<Enum, int>();
        private const int SCALE = 100;

        public int GetInt(Enum key)
        {
            features.TryAdd(key, 0);
            return features[key];
        }

        public void SetInt(Enum key, int value)
        {
            features[key] = value;
        }

        public void AddInt(Enum key, int value)
        {
            features.TryAdd(key, 0);
            features[key] += value;
        }

        public void SubInt(Enum key, int value)
        {
            features.TryAdd(key, 0);
            features[key] -= value;
        }

        public float GetFloat(Enum key)
        {
            features.TryAdd(key, 0);
            return features[key] / (float)SCALE;
        }

        public void SetFloat(Enum key, float value)
        {
            features[key] = (int)Math.Round(value * SCALE);
        }

        public void AddFloat(Enum key, float value)
        {
            features.TryAdd(key, 0);
            features[key] += (int)Math.Round(value * SCALE);
        }

        public void SubFloat(Enum key, float value)
        {
            features.TryAdd(key, 0);
            features[key] -= (int)Math.Round(value * SCALE);
        }

        public override void Enqueue()
        {
            features.Clear();
        }
    }
}