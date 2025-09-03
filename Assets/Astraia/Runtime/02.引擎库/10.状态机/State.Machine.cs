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
        private Dictionary<int, IState> states = new Dictionary<int, IState>();
        private IState current;

        public void OnUpdate()
        {
            current?.OnUpdate();
        }
        
        public void Create(int state, Type value)
        {
            if (!states.TryGetValue(state, out var item))
            {
                item = (IState)Activator.CreateInstance(value);
                states.Add(state, item);
            }

            item.Create(owner);
        }

        public void Switch(int state)
        {
            current?.OnExit();
            states.TryGetValue(state, out current);
            current?.OnEnter();
        }

        public override void Enqueue()
        {
            states.Clear();
        }
    }
}