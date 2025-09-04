// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2024-12-23 18:12:21
// # Recently: 2025-01-08 17:01:28
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Runtime.CompilerServices;
using Astraia.Common;
using UnityEngine;

namespace Astraia
{
    using static GlobalManager;

    [Serializable]
    public sealed partial class Tween : IAsync
    {
        private Component owner;
        private int complete;
        private float progress;
        private float nextTime;
        private float duration;
        private Action onComplete;
        private Action onContinue;
        private Action<float> onUpdate;

        internal static Tween Create(Component owner, float duration)
        {
            var item = HeapManager.Dequeue<Tween>();
            asyncData.Add(owner, item);
            item.owner = owner;
            item.progress = 0;
            item.nextTime = 0;
            item.complete = 0;
            item.duration = duration;
            item.onComplete = OnComplete;
            return item;

            void OnComplete()
            {
                item.owner = null;
                item.complete = 1;
                item.onUpdate = null;
                item.onContinue = null;
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
                    Break();
                    return;
                }

                if (nextTime <= 0)
                {
                    nextTime = Time.time;
                }

                progress = (Time.time - nextTime) / duration;
                if (progress > 1)
                {
                    progress = 1;
                }

                onUpdate.Invoke(progress);

                if (progress >= 1)
                {
                    onComplete += onContinue;
                    onComplete.Invoke();
                }
            }
            catch (Exception e)
            {
                Break();
                Debug.Log(Service.Text.Format("无法执行异步方法：\n{0}", e));
            }
        }

        public Tween OnUpdate(Action<float> onUpdate)
        {
            this.onUpdate += onUpdate;
            return this;
        }

        public Tween OnComplete(Action onComplete)
        {
            this.onComplete += onComplete;
            return this;
        }

        public Tween Break()
        {
            onComplete.Invoke();
            return this;
        }
    }

    public sealed partial class Tween : INotifyCompletion
    {
        public bool IsCompleted => complete == 1;

        public Tween GetAwaiter()
        {
            return this;
        }

        void INotifyCompletion.OnCompleted(Action continuation)
        {
            if (!owner.IsActive())
            {
                Break();
                return;
            }

            onContinue = continuation;
        }

        public void GetResult()
        {
        }
    }
}