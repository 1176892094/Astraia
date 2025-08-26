// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-08-27 01:08:50
// // # Recently: 2025-08-27 01:08:50
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using UnityEngine;

namespace Astraia
{

    public static class TabPro
    {
        private static readonly List<Object, TabProPage> DockAreas = new List<Object, TabProPage>();

        public static void Update()
        {
            foreach (var dockArea in Reflection.AllDockAreas)
            {
                if (!DockAreas.TryGetValue(dockArea, out var page))
                {
                    page = new TabProPage(dockArea);
                    DockAreas[dockArea] = page;
                    page.Register();
                }
            }

            foreach (var dockArea in DockAreas)
            {
                if (!dockArea.Key)
                {
                    dockArea.Value.Unregister();
                    DockAreas.Remove(dockArea.Key);
                }
            }
        }
    }
}