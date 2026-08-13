// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-08-13 22:08:24
// # Recently: 2026-08-13 22:46:24
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;

namespace Astraia
{
    [Serializable]
    public abstract class Module<T> : IModule
    {
        public T owner { get; private set; }
        void IModule.Acquire(object value) => owner = (T)value;
        void IModule.Release() => owner = default;
        void IModule.Dequeue() => Dequeue();
        void IModule.Enqueue() => Enqueue();
        void IModule.OnShow() => OnShow();
        void IModule.OnHide() => OnHide();
        protected virtual void Dequeue() { }
        protected virtual void Enqueue() { }
        protected virtual void OnShow() { }
        protected virtual void OnHide() { }
    }
    internal interface IModule
    {
        void Acquire(object value);
        void Release();
        void Dequeue();
        void Enqueue();
        void OnShow();
        void OnHide();
    }
}