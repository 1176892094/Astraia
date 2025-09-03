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
using UnityEngine;

namespace Astraia.Common
{
    using static GlobalManager;
    internal static class TimerManager
    {
        public static void Update(float time)
        {
            foreach (var timer in TaskData.Values)
            {
                timer.Update(time);
            }
        }

        public static T Load<T>(Component entity, float duration) where T : class, ITask
        {
            if (!Instance) return null;
            var item = HeapManager.Dequeue<T>();
            item.Start(entity, duration, OnComplete);
            TaskData.Add(entity, item);
            return item;

            void OnComplete()
            {
                item.Dispose();
                TaskData.Remove(entity);
                HeapManager.Enqueue(item, typeof(T));
            }
        }

        internal static void Dispose()
        {
            TaskData.Clear();
        }
    }

    internal interface ITask : IDisposable
    {
        void Start(Component owner, float duration, Action OnDispose);

        void Update(float time);
    }
}