// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2024-12-23 18:12:21
// # Recently: 2025-01-08 17:01:34
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Collections.Generic;
using Astraia.Common;

namespace Astraia
{
    [Serializable]
    public abstract class StateMachine<TEntity> : Agent<TEntity> where TEntity : Entity
    {
        private readonly Dictionary<int, IState> states = new Dictionary<int, IState>();
        private IState state;

        public virtual void OnUpdate()
        {
            state?.OnUpdate();
        }

        public IState GetState(int key)
        {
            return states.GetValueOrDefault(key);
        }

        public void AddState(int key, Type type)
        {
            if (!states.TryGetValue(key, out var item))
            {
                item = HeapManager.Dequeue<IState>(type);
                states.Add(key, item);
                item.OnAwake(owner);
            }
        }

        public void ChangeState(int key)
        {
            state?.OnExit();
            states.TryGetValue(key, out state);
            state?.OnEnter();
        }

        public void RemoveState(int key)
        {
            if (states.TryGetValue(key, out var item))
            {
                item.OnDestroy();
                states.Remove(key);
                HeapManager.Enqueue(item, item.GetType());
            }
        }

        public override void OnDestroy()
        {
            foreach (var item in states.Values)
            {
                item.OnDestroy();
                HeapManager.Enqueue(item, item.GetType());
            }

            states.Clear();
        }
    }
}