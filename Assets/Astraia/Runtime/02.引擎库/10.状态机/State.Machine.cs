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
using UnityEngine;

namespace Astraia
{
    [Serializable]
    public abstract class StateMachine<TEntity> : Agent<TEntity> where TEntity : Entity
    {
        private readonly Dictionary<Type, StateBase> states = new Dictionary<Type, StateBase>();
        [SerializeField] private StateBase state;

        public virtual void OnUpdate()
        {
            state?.OnUpdate();
        }

        public void AddState<T>(Type type)
        {
            if (!states.TryGetValue(typeof(T), out var item))
            {
                item = HeapManager.Dequeue<StateBase>(type);
                states.Add(typeof(T), item);
                item.Id = Id;
            }
        }

        public void ChangeState<T>()
        {
            state?.OnExit();
            state = states[typeof(T)];
            state?.OnEnter();
        }
        
        public override void OnFade()
        {
            var copies = new List<StateBase>(states.Values);
            foreach (var item in copies)
            {
                HeapManager.Enqueue(item, item.GetType());
            }

            states.Clear();
        }
    }
}