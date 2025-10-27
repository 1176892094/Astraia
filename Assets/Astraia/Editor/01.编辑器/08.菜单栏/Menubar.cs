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

    internal static class Menubar
    {
        private static readonly Dictionary<Object, Menu> menus = new Dictionary<Object, Menu>();
        private static readonly List<Object> copies = new List<Object>();
        private static IEnumerable<Object> pages;

        public static void Update()
        {
            if (pages == null)
            {
                var windows = typeof(EditorWindow).GetValue<List<EditorWindow>>("activeEditorWindows");
                pages = windows.Where(w => w.hasFocus && w.docked && !w.maximized).Select(w => w.GetValue<Object>("m_Parent"));
            }

            foreach (var page in pages)
            {
                if (!menus.TryGetValue(page, out var menu))
                {
                    menu = new Menu(page);
                    menus[page] = menu;
                    menu.Register();
                }
            }

            foreach (var menu in menus)
            {
                if (!menu.Key)
                {
                    menu.Value.Unregister();
                    copies.Add(menu.Key);
                }
            }

            foreach (var menu in copies)
            {
                menus.Remove(menu);
            }

            copies.Clear();
        }

        private class Menu
        {
            private IPanel panel;
            private List<EditorWindow> panes;

            public Menu(Object dockArea)
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

            private void Event(EventBase item)
            {
                if (item is not WheelEvent result)
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
                var menu = panes.FirstOrDefault(r => r.hasFocus);
                if (isCtrl)
                {
                    var dir = move > 0 ? 1 : -1;
                    var i = panes.IndexOf(menu);
                    var j = Mathf.Clamp(i + dir, 0, panes.Count - 1);
                    (panes[i], panes[j]) = (panes[j], panes[i]);
                    panes[j].Focus();
                }
                else
                {
                    var dir = move > 0 ? 1 : -1;
                    var i = panes.IndexOf(menu);
                    var j = Mathf.Clamp(i + dir, 0, panes.Count - 1);
                    panes[j].Focus();
                }
            }
        }
    }
}