// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-08-13 22:08:34
// # Recently: 2026-08-13 22:24:34
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Astraia
{
    [Serializable]
    public abstract class Async : IAsync, INotifyCompletion
    {
        public static readonly Task<State> Success = Task.FromResult(State.Success);
        public static readonly Task<State> Failure = Task.FromResult(State.Failure);

        protected State state;
        protected object owner;
        protected float waitTime;
        protected float duration;

        private Action onWaitable;
        private Action onComplete;

        public bool IsCompleted => state != State.Running;

        protected abstract void Update(float elapseTime);
        protected abstract void Release();

        public void Interrupt(State value = State.Failure)
        {
            state = value;
            var complete = onComplete;
            var waitable = onWaitable;
            onComplete = null;
            onWaitable = null;
            try
            {
                complete?.Invoke();
            }
            catch (Exception e)
            {
                Log.Info($"打断异步方法：\n{e}");
            }
            finally
            {
                Release();

                if (value == State.Success)
                {
                    waitable?.Invoke();
                }
            }
        }

        public void OnComplete(Action complete)
        {
            onComplete += complete;
        }

        public Async GetAwaiter()
        {
            return this;
        }

        public State GetResult()
        {
            return state;
        }

        int IAsync.Id { get; set; }
        int IAsync.Index { get; set; }

        void IAsync.Update(float elapseTime)
        {
            if (owner.GetHashCode() == 0)
            {
                Interrupt();
                return;
            }

            Update(elapseTime);
        }

        void INotifyCompletion.OnCompleted(Action waitable)
        {
            if (owner.GetHashCode() == 0)
            {
                Interrupt();
                return;
            }

            onWaitable = waitable;
        }

        public enum State
        {
            Running,
            Success,
            Failure
        }
    }

    internal interface IAsync
    {
        int Id { get; set; }
        int Index { get; set; }
        void Update(float elapseTime);
    }
}