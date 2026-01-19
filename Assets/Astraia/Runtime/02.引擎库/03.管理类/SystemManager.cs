// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-07-12 20:07:36
// // # Recently: 2025-07-12 20:07:36
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System.Collections.Generic;

namespace Astraia.Core
{
    using static GlobalManager;

    internal static class SystemManager
    {
        public static void AddSystem(int index, ISystem system)
        {
            if (!systemLoop.TryGetValue(index, out var systems))
            {
                systems = new List<ISystem>();
                systemLoop.Add(index, systems);
            }

            systems.Add(system);
        }

        public static void SubSystem(int index, ISystem system)
        {
            if (systemLoop.TryGetValue(index, out var systems))
            {
                systems.Remove(system);
            }
        }

        public static void Update()
        {
            foreach (var systems in systemLoop.Values)
            {
                for (int i = systems.Count - 1; i >= 0; i--)
                {
                    systems[i].Update();
                }
            }

            SoundManager.Update();
        }

        public static void Dispose()
        {
            systemLoop.Clear();
        }
    }

    public interface ISystem
    {
        int index => 0;
        void AddSystem() => SystemManager.AddSystem(index, this);
        void SubSystem() => SystemManager.SubSystem(index, this);
        void Update();
    }
}