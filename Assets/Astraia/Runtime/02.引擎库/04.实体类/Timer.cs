// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2025-09-03 19:09:09
// # Recently: 2025-09-03 19:09:09
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using UnityEngine;

namespace Astraia
{
    [Serializable]
    public sealed class Timer : Tick<Component>
    {
        private int progress;
        private Action onUpdate;

        internal static Timer Create(Component owner, float duration)
        {
            var item = HeapManager.Dequeue<Timer>();
            ((ISystem)item).AddEvent();
            item.owner = owner;
            item.progress = 1;
            item.complete = 0;
            item.duration = duration;
            item.nextTime = TimeManager.Time + duration;
            item.onComplete = OnComplete;
            return item;

            void OnComplete()
            {
                item.complete = 1;
                item.owner = null;
                item.onNext = null;
                item.onUpdate = null;
                ((ISystem)item).SubEvent();
                HeapManager.Enqueue(item);
            }
        }

        protected override bool IsActive()
        {
            return owner && owner.gameObject.activeInHierarchy;
        }

        protected override void OnTick()
        {
            if (nextTime < TimeManager.Time)
            {
                nextTime = TimeManager.Time + duration;
                if (onUpdate != null)
                {
                    onUpdate.Invoke();
                }

                progress--;
                if (progress == 0)
                {
                    complete = 2;
                    onComplete += onNext;
                    onComplete.Invoke();
                }
            }
        }

        public Timer OnUpdate(Action onUpdate)
        {
            this.onUpdate += onUpdate;
            return this;
        }

        public Timer Set(float duration)
        {
            this.duration = duration;
            nextTime = TimeManager.Time + duration;
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
    }

    [Serializable]
    public sealed class Tween : Tick<Component>
    {
        private float progress;
        private Action<float> onUpdate;

        internal static Tween Create(Component owner, float duration)
        {
            var item = HeapManager.Dequeue<Tween>();
            ((ISystem)item).AddEvent();
            item.owner = owner;
            item.progress = 0;
            item.complete = 0;
            item.duration = duration;
            item.nextTime = TimeManager.Time;
            item.onComplete = OnComplete;
            return item;

            void OnComplete()
            {
                item.complete = 1;
                item.owner = null;
                item.onNext = null;
                item.onUpdate = null;
                ((ISystem)item).SubEvent();
                HeapManager.Enqueue(item);
            }
        }

        protected override bool IsActive()
        {
            return owner && owner.gameObject.activeInHierarchy;
        }

        protected override void OnTick()
        {
            if (nextTime < TimeManager.Time)
            {
                progress = (TimeManager.Time - nextTime) / duration;
                if (progress > 1)
                {
                    progress = 1;
                }

                onUpdate.Invoke(progress);
                if (progress >= 1)
                {
                    complete = 2;
                    onComplete += onNext;
                    onComplete.Invoke();
                }
            }
        }

        public Tween OnUpdate(Action<float> onUpdate)
        {
            this.onUpdate += onUpdate;
            return this;
        }
    }
}