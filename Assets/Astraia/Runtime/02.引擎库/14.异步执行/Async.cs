// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-09-03 19:09:09
// // # Recently: 2025-09-03 19:09:09
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using System.Runtime.CompilerServices;
using Astraia.Common;
using UnityEngine;

namespace Astraia
{
    using static GlobalManager;

    internal interface IAsync
    {
        void Update();
    }

    [Serializable]
    public sealed partial class Async<T> : IAsync where T : AsyncOperation
    {
        private Component owner;
        private T operation;
        private Action onComplete;
        private Action<float> onUpdate;

        internal static Async<T> Create(Component owner, T operation)
        {
            var item = HeapManager.Dequeue<Async<T>>();
            asyncData.Add(owner, item);
            item.owner = owner;
            item.operation = operation;
            item.onComplete = OnComplete;
            return item;

            void OnComplete()
            {
                item.owner = null;
                item.onUpdate = null;
                item.operation = null;
                asyncData.Remove(owner);
                HeapManager.Enqueue(item);
            }
        }

        void IAsync.Update()
        {
            try
            {
                if (!owner.IsActive())
                {
                    onComplete.Invoke();
                    return;
                }

                if (onUpdate != null)
                {
                    onUpdate.Invoke(operation.progress);
                }

                if (operation.isDone)
                {
                    onComplete.Invoke();
                }
            }
            catch
            {
                onComplete.Invoke();
            }
        }

        public Async<T> OnUpdate(Action<float> onUpdate)
        {
            this.onUpdate += onUpdate;
            return this;
        }

        public Async<T> OnComplete(Action onComplete)
        {
            this.onComplete += onComplete;
            return this;
        }
    }

    public sealed partial class Async<T> : INotifyCompletion
    {
        public bool IsCompleted => operation.isDone;

        public Async<T> GetAwaiter()
        {
            return this;
        }

        void INotifyCompletion.OnCompleted(Action continuation)
        {
            if (!owner.IsActive())
            {
                onComplete.Invoke();
                return;
            }

            onComplete += continuation;
        }

        public T GetResult()
        {
            return operation;
        }
    }
}