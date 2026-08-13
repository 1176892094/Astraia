// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-08-13 22:08:05
// # Recently: 2026-08-13 22:26:05
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;

namespace Astraia
{
    [Serializable]
    public sealed class Tween : Async
    {
        private int easeType;
        private float progress;
        private Action<float> onUpdate;

        internal static Tween Create(object owner, float duration)
        {
            var item = HeapManager.Dequeue<Tween>();
            item.state = 0;
            item.owner = owner;
            item.progress = 0;
            item.easeType = 0;
            item.duration = duration;
            item.waitTime = TimeManager.renderTime;
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
            if (waitTime < elapseTime)
            {
                progress = (elapseTime - waitTime) / duration;
                if (progress > 1)
                {
                    progress = 1;
                }

                onUpdate(Evaluate(progress));
                if (progress >= 1)
                {
                    Interrupt(State.Success);
                }
            }
        }

        public Tween OnUpdate(Action<float> update)
        {
            onUpdate += update;
            return this;
        }

        public Tween Ease(int ease = Astraia.Ease.Linear)
        {
            easeType = ease;
            return this;
        }

        private float Evaluate(float t)
        {
            switch (easeType)
            {
                case Astraia.Ease.InQuad:
                    return t * t;
                case Astraia.Ease.OutQuad:
                    return t * (2 - t);
                case Astraia.Ease.InOutQuad:
                    return t < 0.5f ? 2 * t * t : -1 + (4 - 2 * t) * t;
                case Astraia.Ease.SmoothStep:
                    return t * t * (3 - 2 * t);
                case Astraia.Ease.PingPong:
                    return t < 0.5f ? t * 2 : 2 - t * 2;
            }

            return t;
        }
    }

    public static class Ease
    {
        public const int Linear = 0; // 匀速
        public const int InQuad = 1; // 先慢后快
        public const int OutQuad = 2; //先快后慢
        public const int InOutQuad = 3; //慢快慢
        public const int SmoothStep = 4; //慢快慢
        public const int PingPong = 5; // 往返
    }
}