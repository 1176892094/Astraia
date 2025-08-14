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
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Astraia
{
    internal static class Reflection
    {
        private static readonly Assembly assembly;

        public static readonly Type toolbar;
        public static readonly Type importer;
        public static readonly Type inspector;
        private static readonly FieldInfo iconWidth;
        private static readonly FieldInfo iconLabel;
        private static readonly FieldInfo hierarchyItem;
        private static readonly FieldInfo HierarchyLast;
        private static readonly FieldInfo hierarchyTree;

        private static readonly FieldInfo toolbarRoot;
        private static readonly FieldInfo importAssets;
        private static readonly FieldInfo importFolder;
        private static readonly FieldInfo importBrowser;

        private static readonly PropertyInfo importTreeItem;
        private static readonly PropertyInfo importTreeData;
        private static readonly PropertyInfo inspectorElement;
        private static readonly PropertyInfo inspectorTracker;

        private static readonly MethodInfo folderGetRows;
        private static readonly MethodInfo assetsGetRows;
        private static readonly MethodInfo contextMenuMethod;

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
            assembly = Assembly.Load("UnityEditor");

            toolbar = GetEditor("Toolbar");
            toolbarRoot = toolbar.GetField("m_Root", Service.Find.Entity);
            importer = GetEditor("PrefabImporter");
            inspector = GetEditor("InspectorWindow");
            inspectorTracker = inspector.GetProperty("tracker", Service.Find.Entity);
            var cacheType = GetEditor("SceneHierarchyWindow");
            hierarchyItem = cacheType.GetField("m_SceneHierarchy", Service.Find.Entity);
            HierarchyLast = cacheType.GetField("s_LastInteractedHierarchy", Service.Find.Static);
            cacheType = GetEditor("SceneHierarchy");
            hierarchyTree = cacheType.GetField("m_TreeView", Service.Find.Entity);
#if UNITY_6000_2_OR_NEWER
            cacheType = GetEditor("IMGUI.Controls.TreeViewController`1");
            cacheType = cacheType.MakeGenericType(typeof(int));
#else
            cacheType = GetEditor("IMGUI.Controls.TreeViewController");
#endif
            importTreeItem = cacheType.GetProperty("gui", Service.Find.Entity);
            importTreeData = cacheType.GetProperty("data", Service.Find.Entity);
            cacheType = GetEditor("IMGUI.Controls.TreeViewGUI");
            iconWidth = cacheType.GetField("k_IconWidth", Service.Find.Entity);
            iconLabel = cacheType.GetField("k_SpaceBetweenIconAndText", Service.Find.Entity);
            cacheType = GetEditor("ProjectBrowser");
            importBrowser = cacheType.GetField("s_LastInteractedProjectBrowser", Service.Find.Static);
            importAssets = cacheType.GetField("m_AssetTree", Service.Find.Entity);
            importFolder = cacheType.GetField("m_FolderTree", Service.Find.Entity);
            cacheType = GetEditor("ProjectBrowserColumnOneTreeViewDataSource");
            folderGetRows = cacheType.GetMethod("GetRows", Service.Find.Entity);
            cacheType = GetEditor("AssetsTreeViewDataSource");
            assetsGetRows = cacheType.GetMethod("GetRows", Service.Find.Entity);
            cacheType = GetEditor("UIElements.EditorElement");
            inspectorElement = cacheType.GetProperty("m_Editors", Service.Find.Entity);

            contextMenuMethod = GetMethod(typeof(EditorUtility), "DisplayObjectContextMenu", Service.Find.Static, new[] { typeof(Rect), typeof(Object), typeof(int) });

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

        private static Type GetEditor(string name)
        {
            return assembly.GetType("UnityEditor." + name);
        }

        private static MethodInfo GetMethod(Type type, string name, BindingFlags flags, Type[] types)
        {
            return type.GetMethod(name, flags, null, types, null);
        }

        public static EditorWindow ShowHierarchy()
        {
            return HierarchyLast.GetValue(null) as EditorWindow;
        }

        public static void HideHierarchy(EditorWindow window)
        {
            if (window == null) return;
            var cached = hierarchyItem.GetValue(window);
            if (cached == null) return;
            cached = hierarchyTree.GetValue(cached);
            if (cached == null) return;
            cached = importTreeItem.GetValue(cached);
            if (cached == null) return;
            iconWidth.SetValue(cached, 0);
            iconLabel.SetValue(cached, 18);
        }

        public static void ShowContext(Rect position, Object context)
        {
            contextMenuMethod?.Invoke(null, new object[] { position, context, 0 });
        }

        public static bool HasChild(int assetId)
        {
            var window = importBrowser.GetValue(null) as EditorWindow;
            if (window == null) return false;
#if UNITY_6000_2_OR_NEWER
            IEnumerable<TreeViewItem<int>> items = null;
#else
            IEnumerable<TreeViewItem> items = null;
#endif
            var cached = importAssets.GetValue(window);
            if (cached != null)
            {
                cached = importTreeData.GetValue(cached, null);
#if UNITY_6000_2_OR_NEWER
                items = (IEnumerable<TreeViewItem<int>>)folderGetRows.Invoke(cached, null);
#else
                items = (IEnumerable<TreeViewItem>)folderGetRows.Invoke(cached, null);
#endif
            }

            cached = importFolder.GetValue(window);
            if (cached != null)
            {
                cached = importTreeData.GetValue(cached, null);
#if UNITY_6000_2_OR_NEWER
                items = (IEnumerable<TreeViewItem<int>>)assetsGetRows.Invoke(cached, null);
#else
                items = (IEnumerable<TreeViewItem>)assetsGetRows.Invoke(cached, null);
#endif
            }

            return items != null && items.Where(item => item.id == assetId).Any(item => item.hasChildren);
        }

        public static ActiveEditorTracker GetTracker(object obj)
        {
            return (ActiveEditorTracker)inspectorTracker.GetValue(obj);
        }

        public static Editor[] GetEditors(object instance)
        {
            return inspectorElement.GetValue(instance) as Editor[];
        }

        public static VisualElement GetRoot(ScriptableObject toolbar)
        {
            return (VisualElement)toolbarRoot.GetValue(toolbar);
        }
    }
}