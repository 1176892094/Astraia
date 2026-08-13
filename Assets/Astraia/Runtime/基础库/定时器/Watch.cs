// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-08-13 22:08:18
// # Recently: 2026-08-13 22:26:18
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;

namespace Astraia
{
    [Serializable]
    public sealed class Watch : Async
    {
        private int progress;
        private Action onUpdate;

        internal static Watch Create(object owner, float duration)
        {
            var item = HeapManager.Dequeue<Watch>();
            item.state = 0;
            item.owner = owner;
            item.progress = 1;
            item.duration = duration;
            item.waitTime = TimeManager.renderTime + duration;
            TimeManager.Register(item);
            return item;
        }

        protected override void Release()
        {
            TimeManager.UnRegister(this);
            owner = null;
            onUpdate = null;
            HeapManager.Enqueue(this);
        }

        protected override void Update(float elapseTime)
        {
            if (waitTime <= elapseTime)
            {
                waitTime = elapseTime + duration;
                if (onUpdate != null)
                {
                    onUpdate();
                }

                progress--;
                if (progress == 0)
                {
                    Interrupt(State.Success);
                }
            }
        }

        public Watch OnUpdate(Action update)
        {
            onUpdate += update;
            return this;
        }

        public Watch Set(float interval)
        {
            waitTime = waitTime - duration + interval;
            duration = interval;
            return this;
        }

        public Watch Add(float interval)
        {
            waitTime += interval;
            return this;
        }

        public Watch Loops(int count = 0)
        {
            progress = count;
            return this;
        }
    }
}