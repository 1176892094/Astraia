// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-09-10 22:09:08
// // # Recently: 2025-09-10 22:09:08
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************


namespace Astraia.Common
{
    using static GlobalManager;

    internal static class SystemManager
    {
        internal static void Update()
        {
            foreach (var system in systemLoop)
            {
                system.Update();
            }

            for (int i = panelLoop.Count - 1; i >= 0; i--)
            {
                panelLoop[i].Update();
            }

            for (int i = timerLoop.Count - 1; i >= 0; i--)
            {
                timerLoop[i].Update();
            }
        }

        internal static void Dispose()
        {
            panelLoop.Clear();
            timerLoop.Clear();
            systemLoop.Clear();
        }
    }
}