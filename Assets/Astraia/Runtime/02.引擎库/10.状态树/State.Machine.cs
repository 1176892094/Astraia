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
}