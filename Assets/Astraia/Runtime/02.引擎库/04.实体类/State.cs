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

        public void Create<T>(int key) where T : IState
        {
            var item = HeapManager.Dequeue<IState>(typeof(T));
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
    public sealed class Blackboard<TEnum> where TEnum : Enum
    {
        private Dictionary<TEnum, object> properties = new Dictionary<TEnum, object>();

        public void Set<T>(TEnum key, T value)
        {
            properties[key] = value;
        }

        public T Get<T>(TEnum key)
        {
            if (properties.TryGetValue(key, out var value))
            {
                return (T)value;
            }

            return default;
        }
    }

    [Serializable]
    public abstract class Blackboard<TEnum, TEntity> : Module<TEntity> where TEnum : Enum where TEntity : Entity
    {
        private Dictionary<TEnum, int> properties = new Dictionary<TEnum, int>();
        private const int SCALE = 100;

        public int GetInt(TEnum key)
        {
            properties.TryAdd(key, 0);
            return properties[key] / SCALE;
        }

        public void SetInt(TEnum key, int value)
        {
            properties[key] = value * SCALE;
        }

        public void AddInt(TEnum key, int value)
        {
            properties.TryAdd(key, 0);
            properties[key] += value * SCALE;
        }

        public void SubInt(TEnum key, int value)
        {
            properties.TryAdd(key, 0);
            properties[key] -= value * SCALE;
        }

        public float GetFloat(TEnum key)
        {
            properties.TryAdd(key, 0);
            return properties[key] / (float)SCALE;
        }

        public void SetFloat(TEnum key, float value)
        {
            properties[key] = (int)Math.Round(value * SCALE);
        }

        public void AddFloat(TEnum key, float value)
        {
            properties.TryAdd(key, 0);
            properties[key] += (int)Math.Round(value * SCALE);
        }

        public void SubFloat(TEnum key, float value)
        {
            properties.TryAdd(key, 0);
            properties[key] -= (int)Math.Round(value * SCALE);
        }

        public override void Enqueue()
        {
            properties.Clear();
        }
    }
}