// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-07-12 18:07:19
// // # Recently: 2025-07-12 18:07:19
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using Astraia.Common;
using UnityEngine;

namespace Astraia
{
    [Serializable]
    public abstract class Agent<T> : IAgent where T : Entity
    {
        public T owner;
        public Transform transform => owner?.transform;
        public GameObject gameObject => owner?.gameObject;

        void IAgent.Create(Entity owner)
        {
            this.owner = (T)owner;
        }

        public virtual void Dequeue()
        {
        }

        public virtual void OnShow()
        {
        }

        public virtual void OnHide()
        {
        }

        public virtual void Enqueue()
        {
        }

        public static implicit operator bool(Agent<T> agent)
        {
            return agent != null && agent.owner && agent.owner.isActiveAndEnabled;
        }
    }
}