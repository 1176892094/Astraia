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
using Astraia.Common;

namespace Astraia
{
    using static GlobalManager;

    public static class UIGroup
    {
        public static void Listen(int group, UIPanel panel)
        {
            if (!Instance) return;
            if (!groupData.TryGetValue(group, out var panels))
            {
                panels = new HashSet<UIPanel>();
                groupData.Add(group, panels);
            }

            if (panelData.TryGetValue(panel, out var groups))
            {
                if (groups.Add(group))
                {
                    panels.Add(panel);
                }
            }
        }

        public static void Remove(int group, UIPanel panel)
        {
            if (!Instance) return;
            if (!groupData.TryGetValue(group, out var panels))
            {
                panels = new HashSet<UIPanel>();
                groupData.Add(group, panels);
            }

            if (panelData.TryGetValue(panel, out var groups))
            {
                if (groups.Remove(group))
                {
                    panels.Remove(panel);
                }
            }
        }

        public static void Show(int group)
        {
            if (!Instance) return;
            if (groupData.TryGetValue(group, out var panels))
            {
                foreach (var panel in panels)
                {
                    panel.gameObject.SetActive(true);
                }
            }
        }

        public static void Hide(int group)
        {
            if (!Instance) return;
            if (groupData.TryGetValue(group, out var panels))
            {
                foreach (var panel in panels)
                {
                    panel.gameObject.SetActive(false);
                }
            }
        }

        internal static void Show(UIPanel panel)
        {
            if (panelData.TryGetValue(panel, out var groups))
            {
                foreach (var group in groups)
                {
                    if (groupData.TryGetValue(group, out var panels))
                    {
                        foreach (var other in panels)
                        {
                            if (panel != other)
                            {
                                other.gameObject.SetActive(false);
                            }
                        }
                    }
                }
            }

            panel.gameObject.SetActive(true);
        }
    }
}