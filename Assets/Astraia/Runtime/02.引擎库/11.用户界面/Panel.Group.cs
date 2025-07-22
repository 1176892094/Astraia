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

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Astraia.Common
{
    public static partial class UIManager
    {
        public static void Listen(int group, UIPanel panel)
        {
            if (!GlobalManager.Instance) return;
            if (!GlobalManager.groupData.TryGetValue(group, out var panels))
            {
                panels = new List<UIPanel>();
                GlobalManager.groupData.Add(group, panels);
            }

            if (panel.groups.Add(group))
            {
                panels.Add(panel);
            }
        }

        public static void Remove(int group, UIPanel panel)
        {
            if (!GlobalManager.Instance) return;
            if (!GlobalManager.groupData.TryGetValue(group, out var panels))
            {
                panels = new List<UIPanel>();
                GlobalManager.groupData.Add(group, panels);
            }

            if (panel.groups.Remove(group))
            {
                panels.Remove(panel);
            }
        }

        public static void Show(int group)
        {
            if (!GlobalManager.Instance) return;
            if (GlobalManager.groupData.TryGetValue(group, out var panels))
            {
                foreach (var panel in panels)
                {
                    try
                    {
                        panel.gameObject.SetActive(true);
                    }
                    catch (Exception e)
                    {
                        Debug.Log(Service.Text.Format("游戏对象为空: {0}\n{1}", panel, e));
                    }
                }
            }
        }

        public static void Hide(int group)
        {
            if (!GlobalManager.Instance) return;
            if (GlobalManager.groupData.TryGetValue(group, out var panels))
            {
                foreach (var panel in panels)
                {
                    try
                    {
                        panel.gameObject.SetActive(false);
                    }
                    catch (Exception e)
                    {
                        Debug.Log(Service.Text.Format("游戏对象为空: {0}\n{1}", panel, e));
                    }
                }
            }
        }

        private static void ShowInGroup(UIPanel panel)
        {
            foreach (var group in panel.groups)
            {
                if (GlobalManager.groupData.TryGetValue(group, out var panels))
                {
                    foreach (var other in panels.Where(other => panel != other))
                    {
                        try
                        {
                            other.gameObject.SetActive(false);
                        }
                        catch (Exception e)
                        {
                            Debug.Log(Service.Text.Format("游戏对象为空: {0}\n{1}", other, e));
                        }
                    }
                }
            }

            panel.gameObject.SetActive(true);
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