// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-09-05 15:09:36
// // # Recently: 2025-09-05 15:09:36
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using Astraia.Common;
using UnityEngine;

namespace Astraia.Common
{
    public interface IModule
    {
        void Create(Entity owner);

        void Dequeue();

        void Enqueue();
    }

    public interface IActive
    {
        void OnShow();

        void OnHide();
    }

    public interface IOnFade
    {
        void OnFade();
    }
}

namespace Astraia
{
    [Serializable]
    public abstract class Module<T> : IModule where T : Entity
    {
        public T owner { get; protected set; }
        public Transform transform => owner?.transform;
        public GameObject gameObject => owner?.gameObject;

        void IModule.Create(Entity owner)
        {
            this.owner = (T)owner;
        }

        public virtual void Dequeue()
        {
        }

        public virtual void Enqueue()
        {
        }

        public static implicit operator T(Module<T> module)
        {
            return module.owner;
        }

        public static implicit operator bool(Module<T> module)
        {
            return module != null && module.owner && module.owner.isActiveAndEnabled;
        }
    }

    public abstract class Singleton<T, TOwner> : Module<TOwner>, IModule, IActive where T : Singleton<T, TOwner> where TOwner : Entity
    {
        public static T Instance { get; private set; }

        void IModule.Create(Entity owner)
        {
            Instance = (T)this;
            this.owner = (TOwner)owner;
        }

        public virtual void OnShow()
        {
        }

        public virtual void OnHide()
        {
        }
    }
}