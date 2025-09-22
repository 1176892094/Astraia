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
        internal static void Listen(int group, UIPanel panel)
        {
            if (!Instance) return;
            if (!groupData.TryGetValue(group, out var panels))
            {
                panels = new HashSet<UIPanel>();
                groupData.Add(group, panels);
            }

            panels.Add(panel);
        }

        internal static void Remove(int group, UIPanel panel)
        {
            if (!Instance) return;
            if (groupData.TryGetValue(group, out var panels))
            {
                panels.Remove(panel);
                if (panels.Count == 0)
                {
                    groupData.Remove(group);
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
                    SetActive(panel, true);
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
                    SetActive(panel, false);
                }
            }
        }

        internal static void Show(UIPanel panel)
        {
            foreach (var pair in groupData)
            {
                if ((panel.groupMask & pair.Key) != 0)
                {
                    foreach (var other in pair.Value)
                    {
                        if (other != panel)
                        {
                            SetActive(other, false);
                        }
                    }
                }
            }

            SetActive(panel, true);
        }

        internal static async void SetActive(UIPanel panel, bool state)
        {
            if (panel is ITween Tween)
            {
                if (state)
                {
                    panel.gameObject.SetActive(true);
                    await Tween.OnShow();
                }
                else
                {
                    await Tween.OnHide();
                    panel.gameObject.SetActive(false);
                }
            }
            else
            {
                panel.gameObject.SetActive(state);
            }
        }
    }
}