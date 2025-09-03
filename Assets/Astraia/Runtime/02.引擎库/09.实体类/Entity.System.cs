// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-08-31 00:08:10
// // # Recently: 2025-08-31 00:08:10
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System.Collections.Generic;

namespace Astraia.Common
{
    using static GlobalManager;

    public interface ISystem
    {
        void Update();
    }

    public static class SystemManager
    {
        public static void Listen<T>() where T : ISystem
        {
            var system = HeapManager.Dequeue<ISystem>(typeof(T));
            SystemData.Add(typeof(T), system);
        }

        public static void Remove<T>() where T : ISystem
        {
            if (SystemData.TryGetValue(typeof(T), out var system))
            {
                SystemData.Remove(typeof(T));
                HeapManager.Enqueue(system, typeof(T));
            }
        }

        internal static void Update()
        {
            foreach (var system in SystemData.Values)
            {
                system.Update();
            }
        }

        public static IEnumerable<T> Query<T>() where T : IAgent
        {
            if (QueryData.TryGetValue(typeof(T), out var entities))
            {
                foreach (var entity in entities)
                {
                    if (entity.Key && entity.Key.isActiveAndEnabled)
                    {
                        yield return (T)entity.Value;
                    }
                }
            }
        }

        internal static void Dispose()
        {
            foreach (var system in SystemData.Values)
            {
                HeapManager.Enqueue(system);
            }

            SystemData.Clear();
        }
    }
}