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
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
#if UNITY_6000_2_OR_NEWER
using TreeViewItem = UnityEditor.IMGUI.Controls.TreeViewItem<int>;
#endif

namespace Astraia
{
    internal static class Reflection
    {
        public static readonly Type toolbar;
        public static readonly Type importer;
        public static readonly Type inspector;

        private static readonly Type browser;
        private static readonly Type hierarchy;

        public static readonly GUIContent collapse;
        public static readonly GUIContent expansion;
        public static readonly GUIContent buildIcon;
        public static readonly GUIContent settingIcon;
        public static readonly GUIContent unityIcon;
        public static readonly GUIContent prefabIcon;
        public static readonly GUIContent objectIcon;
        public static readonly GUIContent scriptIcon;
        public static readonly GUIContent customIcon;
        public static readonly GUIContent windowIcon;

        static Reflection()
        {
            toolbar = typeof(Editor).Assembly.GetType("UnityEditor.Toolbar");
            browser = typeof(Editor).Assembly.GetType("UnityEditor.ProjectBrowser");
            importer = typeof(Editor).Assembly.GetType("UnityEditor.PrefabImporter");
            inspector = typeof(Editor).Assembly.GetType("UnityEditor.InspectorWindow");
            hierarchy = typeof(Editor).Assembly.GetType("UnityEditor.SceneHierarchyWindow");

            unityIcon = EditorGUIUtility.IconContent("UnityLogo");
            prefabIcon = EditorGUIUtility.IconContent("Prefab Icon");

            objectIcon = EditorGUIUtility.IconContent("GameObject Icon");
            scriptIcon = EditorGUIUtility.IconContent("cs Script Icon");

            buildIcon = EditorGUIUtility.IconContent("BuildSettings.Standalone");
            windowIcon = EditorGUIUtility.IconContent("UnityEditor.AnimationWindow");
            customIcon = EditorGUIUtility.IconContent("CustomTool");
            settingIcon = EditorGUIUtility.IconContent("SettingsIcon");

            collapse = EditorGUIUtility.IconContent("Download-Available");
            expansion = EditorGUIUtility.IconContent("Toolbar Plus More");
        }

        public static EditorWindow ShowHierarchy()
        {
            return hierarchy.GetValue<EditorWindow>("s_LastInteractedHierarchy");
        }

        public static void HideHierarchy(EditorWindow window)
        {
            if (window == null) return;
            var cached = window.GetValue("m_SceneHierarchy");
            if (cached == null) return;
            cached = cached.GetValue("m_TreeView");
            if (cached == null) return;
            cached = cached.GetValue("gui");
            if (cached == null) return;
            cached.SetValue("k_IconWidth", 0F);
            cached.SetValue("k_SpaceBetweenIconAndText", 18F);
        }

        public static void ShowContext(Rect position, Object context)
        {
            typeof(EditorUtility).Invoke("DisplayObjectContextMenu", position, context, 0);
        }

        public static bool HasChild(int assetId)
        {
            var window = browser.GetValue<EditorWindow>("s_LastInteractedProjectBrowser");
            if (window == null) return false;
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

            return items != null && items.Where(item => item.id == assetId).Any(item => item.hasChildren);
        }

        public static Editor[] GetEditors(object instance)
        {
            return instance.GetValue<Editor[]>("m_Editors");
        }

        public static ActiveEditorTracker GetTracker(object instance)
        {
            return instance.GetValue<ActiveEditorTracker>("tracker");
        }

        public static VisualElement GetRoot(ScriptableObject instance)
        {
            return instance.GetValue<VisualElement>("m_Root");
        }
    }
}