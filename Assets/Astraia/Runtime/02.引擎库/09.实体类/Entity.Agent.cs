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
using UnityEngine;

namespace Astraia
{
    public interface IAgent
    {
        void SetData(Entity id);
        void Dequeue();
        void OnShow();
        void OnHide();
        void Enqueue();
    }

    [Serializable]
    public abstract class Agent<T> : IAgent where T : Entity
    {
        public T owner;
        public Transform transform => owner?.transform;
        public GameObject gameObject => owner?.gameObject;

        void IAgent.SetData(Entity owner)
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