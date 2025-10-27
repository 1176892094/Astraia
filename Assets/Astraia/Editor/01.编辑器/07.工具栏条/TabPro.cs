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

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Astraia
{
    using static EventManager;

    internal static class TabPro
    {
        private static readonly Dictionary<Object, TabProPage> DockAreas = new Dictionary<Object, TabProPage>();
        private static readonly List<Object> RemoveAreas = new List<Object>();
        private static IEnumerable<Object> ActiveEditors;

        public static void Update()
        {
            if (ActiveEditors == null)
            {
                var windows = typeof(EditorWindow).GetValue<List<EditorWindow>>("activeEditorWindows");
                ActiveEditors = windows.Where(w => w.hasFocus && w.docked && !w.maximized).Select(w => w.GetValue<Object>("m_Parent"));
            }

            foreach (var dockArea in ActiveEditors)
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
                    RemoveAreas.Add(dockArea.Key);
                }
            }

            foreach (var remove in RemoveAreas)
            {
                DockAreas.Remove(remove);
            }

            RemoveAreas.Clear();
        }

        private class TabProPage
        {
            private IPanel panel;
            private List<EditorWindow> panes;


            public TabProPage(Object dockArea)
            {
                panes = dockArea.GetValue<List<EditorWindow>>("m_Panes");
                panel = dockArea.GetValue<EditorWindow>("actualView").rootVisualElement.panel;
            }

            public void Register()
            {
                panel.visualTree.RegisterCallback<WheelEvent>(Event, TrickleDown.TrickleDown);
            }

            public void Unregister()
            {
                panel.visualTree.UnregisterCallback<WheelEvent>(Event);
                Destroy();
            }

            private void Destroy()
            {
                panes = null;
                panel = null;
            }

            private void Event(EventBase evt)
            {
                if (evt is not WheelEvent result)
                {
                    return;
                }

                if (!isShift)
                {
                    return;
                }

                var move = result.delta.x;
                if (move == 0)
                {
                    return;
                }

                result.StopPropagation();
                var moveTab = panes.FirstOrDefault(r => r.hasFocus);
                if (isCtrl)
                {
                    var dir = move > 0 ? 1 : -1;
                    var i0 = panes.IndexOf(moveTab);
                    var i1 = Mathf.Clamp(i0 + dir, 0, panes.Count - 1);
                    (panes[i0], panes[i1]) = (panes[i1], panes[i0]);
                    panes[i1].Focus();
                }
                else
                {
                    var dir = move > 0 ? 1 : -1;
                    var i0 = panes.IndexOf(moveTab);
                    var i1 = Mathf.Clamp(i0 + dir, 0, panes.Count - 1);
                    panes[i1].Focus();
                }
            }
        }
    }
}