// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-08-13 22:08:12
// # Recently: 2026-08-13 22:46:13
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;

namespace Astraia
{
    [Serializable]
    public abstract class State<T> : IState
    {
        public T owner { get; private set; }
        void IState.Acquire(object value) => owner = (T)value;
        void IState.Release() => owner = default;
        void IState.OnEnter() => OnEnter();
        void IState.OnUpdate() => OnUpdate();
        void IState.OnExit() => OnExit();
        protected virtual void OnEnter() { }
        protected virtual void OnUpdate() { }
        protected virtual void OnExit() { }
    }

    internal interface IState
    {
        void Acquire(object value);
        void Release();
        void OnEnter();
        void OnUpdate();
        void OnExit();
    }
}