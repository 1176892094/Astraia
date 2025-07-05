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
    public abstract class StateMachine<TOwner> : Agent<TOwner> where TOwner : Component
    {
        private readonly Dictionary<Type, IState> states = new Dictionary<Type, IState>();
        private IState state;

        protected override void OnHide()
        {
            var copies = new List<IState>(states.Values);
            foreach (var item in copies)
            {
                item.OnHide();
                HeapManager.Enqueue(item, item.GetType());
            }

            states.Clear();
            state = null;
        }

        public void OnUpdate()
        {
            state?.OnUpdate();
        }
        
        public void AddState<T>(Type type)
        {
            var item = HeapManager.Dequeue<IState>(type);
            states[typeof(T)] = item;
            item.OnShow(owner);
        }

        public void ChangeState<T>()
        {
            state?.OnExit();
            state = states[typeof(T)];
            state?.OnEnter();
        }

        public void RemoveState<T>()
        {
            if (states.TryGetValue(typeof(T), out var item))
            {
                item.OnHide();
                states.Remove(typeof(T));
                HeapManager.Enqueue(item, item.GetType());
            }
        }
    }
}