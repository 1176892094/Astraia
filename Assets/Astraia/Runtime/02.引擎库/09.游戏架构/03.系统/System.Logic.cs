// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-09-05 15:09:25
// // # Recently: 2025-09-05 15:09:25
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System.Collections.Generic;

namespace Astraia.Common
{
    using static GlobalManager;

    public static class SystemManager
    {
        public static IEnumerable<T> Query<T>() where T : IAgent
        {
            if (queryData.TryGetValue(typeof(T), out var entities))
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

        internal static void Listen<T>() where T : struct, ISystem
        {
            systemData.Add(typeof(T), new T());
        }

        internal static void Remove<T>() where T : struct, ISystem
        {
            systemData.Remove(typeof(T));
        }

        internal static void Update()
        {
            foreach (var system in systemData.Values)
            {
                system.Update();
            }

            for (int i = asyncData.Count - 1; i >= 0; i--)
            {
                asyncData[i].Update();
            }
        }

        internal static void Dispose()
        {
            asyncData.Clear();
            systemData.Clear();
        }
    }
}