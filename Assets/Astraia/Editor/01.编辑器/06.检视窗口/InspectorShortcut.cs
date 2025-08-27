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

    public static partial class Inspector
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

            if (isExpand)
            {
                Set(window);
                window.Repaint();
                Use();
            }
        }

        private static void Set(EditorWindow window)
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

                flag |= Get(window, target);
                if (editor.targets.Length != editors[0].targets.Length)
                {
                    continue;
                }

                if (target is not ParticleSystemRenderer)
                {
                    components.Add(target);
                }
            }

            foreach (var entity in components)
            {
                Set(window, entity, !flag);
            }
        }

        private static bool Get(EditorWindow window, Component component)
        {
            var tracker = window.GetValue<ActiveEditorTracker>("m_Tracker");
            var index = Array.FindIndex(tracker.activeEditors, r => r.target == component);
            if (index >= tracker.activeEditors.Length)
            {
                return false;
            }

            return tracker.GetVisible(index) == 1;
        }

        private static void Set(EditorWindow window, Component component, bool state)
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