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
    public abstract partial class StateMachine : Source
    {
        private readonly Dictionary<Type, State> states = new Dictionary<Type, State>();
        [SerializeField] private State state;

        public override void OnUpdate()
        {
            state?.OnUpdate();
        }

        public void AddState<T>(Type type)
        {
            if (!states.TryGetValue(typeof(T), out var item))
            {
                item = HeapManager.Dequeue<State>(type);
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

        public void RemoveState<T>()
        {
            if (states.TryGetValue(typeof(T), out var item))
            {
                states.Remove(typeof(T));
                HeapManager.Enqueue(item, item.GetType());
            }
        }

        public override void OnDestroy()
        {
            var copies = new List<State>(states.Values);
            foreach (var item in copies)
            {
                HeapManager.Enqueue(item, item.GetType());
            }

            states.Clear();
        }
    }
}