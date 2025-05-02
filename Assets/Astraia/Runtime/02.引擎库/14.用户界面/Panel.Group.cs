// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2025-01-09 16:01:50
// # Recently: 2025-01-10 20:01:58
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System.Collections.Generic;

namespace Astraia.Common
{
    public static partial class UIManager
    {
        public static void Listen(string group, UIPanel panel)
        {
            if (!GlobalManager.Instance) return;
            if (!GlobalManager.groupData.TryGetValue(group, out var panels))
            {
                panels = new HashSet<UIPanel>();
                GlobalManager.groupData.Add(group, panels);
            }

            if (panels.Add(panel))
            {
                panel.groups.Add(group);
            }
        }

        public static void Remove(string group, UIPanel panel)
        {
            if (!GlobalManager.Instance) return;
            if (!GlobalManager.groupData.TryGetValue(group, out var panels))
            {
                panels = new HashSet<UIPanel>();
                GlobalManager.groupData.Add(group, panels);
            }

            if (panels.Remove(panel))
            {
                panel.groups.Remove(group);
            }
        }

        public static void Show(string group)
        {
            if (!GlobalManager.Instance) return;
            if (GlobalManager.groupData.TryGetValue(group, out var panels))
            {
                foreach (var panel in panels)
                {
                    if (!panel.gameObject.activeInHierarchy)
                    {
                        panel.Show();
                    }
                }
            }
        }

        public static void Hide(string group)
        {
            if (!GlobalManager.Instance) return;
            if (GlobalManager.groupData.TryGetValue(group, out var panels))
            {
                foreach (var panel in panels)
                {
                    if (panel.gameObject.activeInHierarchy)
                    {
                        panel.Hide();
                    }
                }
            }
        }

        private static bool ShowInGroup(UIPanel panel)
        {
            foreach (var group in panel.groups)
            {
                if (!GlobalManager.groupData.TryGetValue(group, out var panels))
                {
                    continue;
                }

                foreach (var other in panels)
                {
                    if (panel != other && other.gameObject.activeInHierarchy)
                    {
                        other.Hide();
                    }
                }
            }
            return !panel.gameObject.activeInHierarchy;
        }

        internal static void Dispose()
        {
            foreach (var group in GlobalManager.groupData.Values)
            {
                group.Clear();
            }

            GlobalManager.panelData.Clear();
            GlobalManager.groupData.Clear();
            GlobalManager.layerData.Clear();
        }
    }
}