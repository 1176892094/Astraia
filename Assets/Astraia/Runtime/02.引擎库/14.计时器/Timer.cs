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

    internal interface ITimer
    {
        void Update();
    }
    
    [Serializable]
    public sealed partial class Timer : ITimer
    {
        private Component owner;
        private int complete;
        private int progress;
        private float waitTime;
        private float nextTime;
        private float duration;
        private Action onUpdate;
        private Action onComplete;
        private Action onContinue;

        internal static Timer Create(Component owner, float duration)
        {
            var item = HeapManager.Dequeue<Timer>();
            timerLoop.Add(item);
            item.owner = owner;
            item.progress = 1;
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
                timerLoop.Remove(item);
                HeapManager.Enqueue(item);
            }
        }

        void ITimer.Update()
        {
            try
            {
                if (!owner.IsActive())
                {
                    Break();
                    return;
                }

                waitTime = Time.time;
                if (nextTime <= 0)
                {
                    nextTime = waitTime + duration;
                }

                if (waitTime <= nextTime)
                {
                    return;
                }

                progress--;
                nextTime = waitTime + duration;
                if (onUpdate != null)
                {
                    onUpdate.Invoke();
                }

                if (progress == 0)
                {
                    complete = 2;
                    onComplete += onContinue;
                    onComplete.Invoke();
                }
            }
            catch (Exception e)
            {
                Break();
                Debug.Log("无法执行异步方法：\n{0}".Format(e));
            }
        }

        public Timer OnUpdate(Action onUpdate)
        {
            this.onUpdate += onUpdate;
            return this;
        }

        public Timer OnComplete(Action onComplete)
        {
            this.onComplete += onComplete;
            return this;
        }

        public Timer Set(float duration)
        {
            this.duration = duration;
            nextTime = waitTime + duration;
            return this;
        }

        public Timer Add(float duration)
        {
            nextTime += duration;
            return this;
        }

        public Timer Loops(int progress = 0)
        {
            this.progress = progress;
            return this;
        }

        public Timer Break()
        {
            onComplete.Invoke();
            return this;
        }
    }

    public sealed partial class Timer : INotifyCompletion
    {
        public bool IsCompleted => complete > 0;

        public Timer GetAwaiter()
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

        public bool GetResult()
        {
            return complete == 2;
        }
    }
}