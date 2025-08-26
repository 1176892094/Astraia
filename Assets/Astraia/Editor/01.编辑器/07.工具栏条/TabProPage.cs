// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-08-27 02:08:17
// // # Recently: 2025-08-27 02:08:17
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
    using static EditorEvent;

    internal class TabProPage
    {
        private readonly List<float> positions = new();
        private IPanel panel;
        private Object dockArea;
        private IMGUIContainer strip;
        private List<EditorWindow> panes;


        public TabProPage(Object dockArea)
        {
            this.dockArea = dockArea;
            panes = dockArea.GetValue<List<EditorWindow>>("m_Panes");
            panel = dockArea.GetValue<EditorWindow>("actualView").rootVisualElement.panel;
            strip = new IMGUIContainer
            {
                name = "TabPro-Window-Strip",
                style =
                {
                    width = Length.Percent(100),
                    height = 24,
                    position = Position.Absolute
                },
                pickingMode = PickingMode.Ignore,
                onGUIHandler = OnGUI
            };

            panel.visualTree.Add(strip);
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
            strip = null;
            dockArea = null;
        }

        private void Event(EventBase @event)
        {
            if (@event is not WheelEvent result)
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

        private void OnGUI()
        {
            if (isLayout)
            {
                UpdateState();
            }

            TabButton(strip.contentRect);
        }

        private void TabButton(Rect stripRect)
        {
            var buttonRect = new Rect(stripRect)
            {
                width = 24,
                x = positions.Last() + 1,
            };
            var distToRight = stripRect.xMax - buttonRect.xMax;

            if (distToRight < 10)
            {
                return;
            }

            var fade = Mathf.Pow(Mathf.Clamp01((distToRight - 25) / (25 - 10)), 2);
            var colorNormal = new Color(0.5f, 0.5f, 0.5f, fade);
            var colorHovered = new Color(1, 1, 1, 1);
            var colorPressed = new Color(0.75f, 0.75f, 0.75f, 1);

            // if (TabProPopup.instance && TabProPopup.instance.dockArea == dockArea)
            // {
            //     colorNormal = colorHovered;
            // }

            if (DragAndDrop.objectReferences.Any())
            {
                colorHovered = colorNormal;
            }

            if (!Button(buttonRect, "d_Toolbar Plus", 16, colorNormal, colorHovered, colorPressed))
            {
                return;
            }

            if (isAlt)
            {
                return;
            }

            // if (TabProPopup.instance)
            // {
            //     TabProPopup.instance.Close();
            // }
            // else
            // {
            //     TabProPopup.Open(dockArea);
            // }
        }

        private void UpdateState()
        {
            var offset = dockArea.GetValue<float>("m_ScrollOffset");
            if (offset != 0)
            {
                offset -= 5;
            }

            positions.Clear();
            var position = dockArea.GetValue<Rect>("m_TabAreaRect").x * 2 - offset;
            foreach (var pane in panes)
            {
                position += dockArea.Invoke<float>("GetTabWidth", GUI.skin.FindStyle("dragtab"), pane);
                positions.Add(Mathf.Round(position));
            }
        }
    }
}