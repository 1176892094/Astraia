// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2024-12-23 18:12:21
// # Recently: 2025-01-08 17:01:24
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using Astraia.Common;
using UnityEngine;

namespace Astraia
{
    public abstract class Agent<T> : IAgent where T : Component
    {
        public T owner { get; private set; }

        void IAgent.OnShow(Component owner)
        {
            this.owner = (T)owner;
            OnShow();
        }

        void IAgent.OnUpdate()
        {
            if (owner != null)
            {
                OnUpdate();
            }
        }

        void IAgent.OnHide()
        {
            OnHide();
            owner = null;
        }

        protected virtual void OnShow()
        {
        }

        protected virtual void OnUpdate()
        {
        }

        protected virtual void OnHide()
        {
        }
    }
}