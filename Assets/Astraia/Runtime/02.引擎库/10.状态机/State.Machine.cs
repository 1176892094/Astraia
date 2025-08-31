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
        
        public void OnUpdate()
        {
            state?.OnUpdate();
        }

        public void AddState(int key, Type type)
        {
            if (!states.TryGetValue(key, out var item))
            {
                item = (IState)Activator.CreateInstance(type);
                states.Add(key, item);
            }

            item.Create(owner);
        }

        public void ChangeState(int key)
        {
            state?.OnExit();
            states.TryGetValue(key, out state);
            state?.OnEnter();
        }

        public override void Enqueue()
        {
            states.Clear();
        }
    }
}