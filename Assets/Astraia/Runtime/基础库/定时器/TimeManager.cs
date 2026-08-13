// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-08-13 22:08:51
// # Recently: 2026-08-13 22:23:39
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;

namespace Astraia
{
    [Serializable]
    public static class TimeManager
    {
        private const int LENGTH = 1024;
        private const int RENDER = 0;
        private const int PHYSIC = 1;
        private const int ACCEPT = 2;

        private const int RENDER_OFFSET = LENGTH * RENDER;
        private const int PHYSIC_OFFSET = LENGTH * PHYSIC;
        private const int ACCEPT_OFFSET = LENGTH * ACCEPT;

        private static readonly IAsync[] items = new IAsync[LENGTH * 3];
        private static readonly int[] counts = new int[3];

        public static Fixation renderTime;
        public static Fixation physicTime;
        public static Fixation acceptTime;

        public static void RenderUpdate(Fixation elapseTime)
        {
            renderTime = elapseTime;
            UpdateModule(RENDER_OFFSET, counts[RENDER], elapseTime);
        }

        public static void PhysicUpdate(Fixation elapseTime)
        {
            physicTime = elapseTime;
            UpdateModule(PHYSIC_OFFSET, counts[PHYSIC], elapseTime);
        }

        public static void AcceptUpdate(Fixation elapseTime)
        {
            acceptTime = elapseTime;
            UpdateModule(ACCEPT_OFFSET, counts[ACCEPT], elapseTime);
        }

        private static void UpdateModule(int offset, int count, Fixation elapseTime)
        {
            for (var i = count - 1; i >= 0; i--)
            {
                items[offset + i].Update(elapseTime);
            }
        }

        internal static void Register<T>(T item) where T : IAsync
        {
            var module = item.Index;

            var offset = module * LENGTH;

            var count = counts[module];

            var id = offset + count;

            counts[module]++;

            item.Id = id;

            items[id] = item;
        }

        internal static void UnRegister<T>(T item) where T : IAsync
        {
            var module = item.Index;

            var offset = module * LENGTH;

            var count = --counts[module];

            var last = offset + count;

            if (item.Id != last)
            {
                var swap = items[last];

                items[item.Id] = swap;

                swap.Id = item.Id;
            }

            items[last] = null;

            item.Id = -1;
        }

        internal static T Render<T>(T item) where T : IAsync
        {
            if (item.Index != RENDER)
            {
                UnRegister(item);
                item.Index = RENDER;
                Register(item);
            }

            return item;
        }

        internal static T Physic<T>(T item) where T : IAsync
        {
            if (item.Index != PHYSIC)
            {
                UnRegister(item);
                item.Index = PHYSIC;
                Register(item);
            }

            return item;
        }

        internal static T Accept<T>(T item) where T : IAsync
        {
            if (item.Index != ACCEPT)
            {
                UnRegister(item);
                item.Index = ACCEPT;
                Register(item);
            }

            return item;
        }
    }


}