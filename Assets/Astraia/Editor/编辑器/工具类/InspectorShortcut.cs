// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-09-02 19:09:50
// # Recently: 2026-09-02 21:22:59
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Astraia
{
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

            if (window.GetType() != EditorReference.Inspector && window.GetType() != EditorReference.Property)
            {
                return;
            }

            if (EditorAction.Q)
            {
                SetExpand(window, 1);
                window.Repaint();
                EditorAction.Use();
            }

            if (EditorAction.W)
            {
                SetExpand(window, 2);
                window.Repaint();
                EditorAction.Use();
            }

            if (EditorAction.E)
            {
                SetExpand(window, 3);
                window.Repaint();
                EditorAction.Use();
            }

            if (EditorAction.R)
            {
                SetExpand(window, 4);
                window.Repaint();
                EditorAction.Use();
            }

            if (EditorAction.isExpand)
            {
                SetExpand(window);
                window.Repaint();
                EditorAction.Use();
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