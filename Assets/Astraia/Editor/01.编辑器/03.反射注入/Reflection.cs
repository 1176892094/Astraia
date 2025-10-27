// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-04-09 19:04:00
// // # Recently: 2025-04-09 19:04:00
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_6000_2_OR_NEWER
using TreeViewItem = UnityEditor.IMGUI.Controls.TreeViewItem<int>;
#endif

namespace Astraia
{
    internal static class Reflection
    {
        public static readonly Type Toolbar = typeof(Editor).Assembly.GetType("UnityEditor.Toolbar");
        public static readonly Type Browser = typeof(Editor).Assembly.GetType("UnityEditor.ProjectBrowser");
        public static readonly Type Property = typeof(Editor).Assembly.GetType("UnityEditor.PropertyEditor");
        public static readonly Type Hierarchy = typeof(Editor).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
        public static readonly Type Inspector = typeof(Editor).Assembly.GetType("UnityEditor.InspectorWindow");
        public static readonly GUIContent buildIcon = EditorGUIUtility.IconContent("BuildSettings.Standalone");
        public static readonly GUIContent sceneIcon = EditorGUIUtility.IconContent("UnityLogo");
        public static readonly GUIContent customIcon = EditorGUIUtility.IconContent("CustomTool");
        public static readonly GUIContent windowIcon = EditorGUIUtility.IconContent("UnityEditor.AnimationWindow");
        public static readonly GUIContent settingIcon = EditorGUIUtility.IconContent("SettingsIcon");
        public static readonly GUIContent packageIcon = EditorGUIUtility.IconContent("Package Manager");

        private static readonly GUIContent content = new();

    

        public static float NameLength(string name)
        {
            content.text = name;
            return GUI.skin.label.CalcSize(content).x;
        }

        public static void HideIcon(EditorWindow window)
        {
            if (!window) return;
            var result = window.GetValue("m_SceneHierarchy");
            if (result == null) return;
            result = result.GetValue("m_TreeView");
            if (result == null) return;
            result = result.GetValue("gui");
            if (result == null) return;
            result.SetValue<float>("k_IconWidth", 0);
            result.SetValue<float>("k_SpaceBetweenIconAndText", 18);
        }

        public static IEnumerable<TreeViewItem> GetItems()
        {
            var window = Browser.GetValue<EditorWindow>("s_LastInteractedProjectBrowser");
            if (window == null) return null;
            IEnumerable<TreeViewItem> items = null;
            var cached = window.GetValue("m_AssetTree");
            if (cached != null)
            {
                cached = cached.GetValue("data");
                items = cached.Invoke<IEnumerable<TreeViewItem>>("GetRows");
            }

            cached = window.GetValue("m_FolderTree");
            if (cached != null)
            {
                cached = cached.GetValue("data");
                items = cached.Invoke<IEnumerable<TreeViewItem>>("GetRows");
            }

            return items;
        }
    }
}