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
using Astraia.Common;

namespace Astraia
{
    using static GlobalManager;

    public interface ISystem
    {
        void Update(float deltaTime);
    }

    public static class SystemManager
    {
        public static void Listen<T>() where T : ISystem
        {
            var system = HeapManager.Dequeue<ISystem>(typeof(T));
            systemData.Add(typeof(T), system);
        }

        public static void Remove<T>() where T : ISystem
        {
            if (systemData.TryGetValue(typeof(T), out var system))
            {
                systemData.Remove(typeof(T));
                HeapManager.Enqueue(system, typeof(T));
            }
        }

        internal static void Update(float deltaTime)
        {
            foreach (var system in systemData.Values)
            {
                system.Update(deltaTime);
            }
        }

        public static IEnumerable<Entity> Query<T>() where T : IAgent
        {
            if (agentData.TryGetValue(typeof(T), out var query))
            {
                foreach (var entity in query.Keys)
                {
                    if (entity && entity.isActiveAndEnabled)
                    {
                        yield return entity;
                    }
                }
            }
        }

        public static T GetSystem<T>() where T : ISystem
        {
            return systemData.TryGetValue(typeof(T), out var system) ? (T)system : default;
        }

        internal static void Dispose()
        {
            foreach (var system in systemData.Values)
            {
                HeapManager.Enqueue(system);
            }

            systemData.Clear();
        }
    }
}