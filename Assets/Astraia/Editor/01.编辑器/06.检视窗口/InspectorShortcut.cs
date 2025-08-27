// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-08-28 00:08:38
// // # Recently: 2025-08-28 00:08:38
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Astraia
{
    using static Reflection;
    using static EditorInput;

    internal static partial class Inspector
    {
        private static readonly List<Component> components = new List<Component>();

        public static void Shortcuts()
        {
            var window = EditorWindow.mouseOverWindow;
            if (!window)
            {
                return;
            }

            if (window.GetType() != Reflection.Inspector && window.GetType() != Property)
            {
                return;
            }

            if (Q)
            {
                SetExpand(window, 1);
                window.Repaint();
                Use();
            }
            
            if (W)
            {
                SetExpand(window, 2);
                window.Repaint();
                Use();
            }

            if (E)
            {
                SetExpand(window, 3);
                window.Repaint();
                Use();
            }
            
            if (R)
            {
                SetExpand(window, 4);
                window.Repaint();
                Use();
            }

            if (isShiftE)
            {
                SetExpand(window);
                window.Repaint();
                Use();
            }
        }

        private static void SetExpand(EditorWindow window, int selected = 0)
        {
            var editors = window.GetValue<ActiveEditorTracker>("m_Tracker").activeEditors;
            if (editors.Length == 0)
            {
                return;
            }

            var flag = false;
            components.Clear();
            foreach (var editor in editors)
            {
                if (editor.target is not Component target)
                {
                    continue;
                }

                flag |= GetExpand(window, target);
                if (editor.targets.Length != editors[0].targets.Length)
                {
                    continue;
                }

                if (target is not ParticleSystemRenderer)
                {
                    components.Add(target);
                }
            }

            for (int i = 0; i < components.Count; i++)
            {
                if (i == components.Count - selected)
                {
                    SetExpand(window, components[i], true);
                }
                else
                {
                    SetExpand(window, components[i], !flag);
                }
            }
        }

        private static bool GetExpand(EditorWindow window, Component component)
        {
            var tracker = window.GetValue<ActiveEditorTracker>("m_Tracker");
            var index = Array.FindIndex(tracker.activeEditors, r => r.target == component);
            if (index >= tracker.activeEditors.Length)
            {
                return false;
            }

            return tracker.GetVisible(index) == 1;
        }

        private static void SetExpand(EditorWindow window, Component component, bool state)
        {
            InternalEditorUtility.SetIsInspectorExpanded(component, state);
            var tracker = window.GetValue<ActiveEditorTracker>("m_Tracker");
            var index = Array.FindIndex(tracker.activeEditors, r => r.target == component);
            if (index >= tracker.activeEditors.Length)
            {
                return;
            }

            tracker.SetVisible(index, state ? 1 : 0);
        }
    }
}