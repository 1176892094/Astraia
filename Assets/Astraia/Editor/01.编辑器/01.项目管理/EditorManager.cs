// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-04-09 18:04:34
// // # Recently: 2025-04-09 18:04:34
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using Astraia.Common;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
#if UNITY_6000_3_OR_NEWER
using TreeViewItem = UnityEditor.IMGUI.Controls.TreeViewItem<UnityEngine.EntityId>;
#elif UNITY_6000_2_OR_NEWER
using TreeViewItem = UnityEditor.IMGUI.Controls.TreeViewItem<int>;
else
using TreeViewItem = UnityEditor.IMGUI.Controls.TreeViewItem;
#endif

namespace Astraia
{
    using EventHandler = EditorApplication.CallbackFunction;

    [InitializeOnLoad]
    internal static class EditorManager
    {
        private static EditorWindow focusedWindow;
        private static bool isMaximized;

        static EditorManager()
        {
            Service.Ref.OnLoad -= EditorSetting.LoadWindows;
            Service.Ref.OnLoad += EditorSetting.LoadWindows;
            Service.Ref.OnLoad -= GlobalSetting.LoadSetting;
            Service.Ref.OnLoad += GlobalSetting.LoadSetting;
            Service.Ref.OnLoadComplete -= GlobalSetting.LoadComplete;
            Service.Ref.OnLoadComplete += GlobalSetting.LoadComplete;

            EditorApplication.update -= Update;
            EditorApplication.update += Update;
            EditorApplication.update -= Menubar.Update;
            EditorApplication.update += Menubar.Update;
            EditorApplication.hierarchyWindowItemOnGUI -= Hierarchy.OnGUI;
            EditorApplication.hierarchyWindowItemOnGUI += Hierarchy.OnGUI;
            EditorApplication.projectWindowItemInstanceOnGUI -= Folder.OnGUI;
            EditorApplication.projectWindowItemInstanceOnGUI += Folder.OnGUI;
            EditorApplication.projectChanged -= Folder.OnProjectChanged;
            EditorApplication.projectChanged += Folder.OnProjectChanged;
            
            Selection.selectionChanged -= Inspector.SelectionChanged;
            Selection.selectionChanged += Inspector.SelectionChanged;
            EditorApplication.delayCall -= Inspector.SelectionChanged;
            EditorApplication.delayCall += Inspector.SelectionChanged;

            SceneManager.activeSceneChanged -= Toolbar.ActiveSceneChanged;
            SceneManager.activeSceneChanged += Toolbar.ActiveSceneChanged;
            EditorSceneManager.sceneOpened -= Toolbar.OnSceneOpened;
            EditorSceneManager.sceneOpened += Toolbar.OnSceneOpened;
            EditorSceneManager.activeSceneChangedInEditMode -= Toolbar.ActiveSceneChanged;
            EditorSceneManager.activeSceneChangedInEditMode += Toolbar.ActiveSceneChanged;
            EditorApplication.projectChanged -= Toolbar.OnProjectChanged;
            EditorApplication.projectChanged += Toolbar.OnProjectChanged;

            focusedWindow = EditorWindow.focusedWindow;
            isMaximized = focusedWindow && focusedWindow.maximized;

            var eventHandler = typeof(EditorApplication).GetValue<EventHandler>("globalEventHandler");
            eventHandler = Folder.Shortcuts + (eventHandler - Folder.Shortcuts);
            eventHandler = Hierarchy.Shortcuts + (eventHandler - Hierarchy.Shortcuts);
            eventHandler = Inspector.Shortcuts + (eventHandler - Inspector.Shortcuts);
            typeof(EditorApplication).SetValue("globalEventHandler", eventHandler);
        }

        private static void Update()
        {
            if (focusedWindow != EditorWindow.focusedWindow)
            {
                focusedWindow = EditorWindow.focusedWindow;
                if (focusedWindow)
                {
                    if (focusedWindow.GetType() == EditorRef.Inspector)
                    {
                        Inspector.InitWindow(focusedWindow);
                    }
                }
            }

            if (focusedWindow)
            {
                if (isMaximized != focusedWindow.maximized)
                {
                    isMaximized = focusedWindow.maximized;
                    if (focusedWindow.GetType() == EditorRef.Inspector)
                    {
                        Inspector.InitWindow(focusedWindow);
                    }
                }
            }
        }
    }

    internal static class EventManager
    {
        private static readonly Event current;
        private static readonly Event Event = current ??= typeof(Event).GetValue<Event>("s_Current");
        public static int mouseButton => Event.button;
        public static Vector2 mousePosition => Event.mousePosition;
        public static bool isLayout => Event.type == EventType.Layout;
        public static bool isRepaint => Event.type == EventType.Repaint;
        public static bool isMouseUp => Event.type == EventType.MouseUp;
        public static bool isMouseDown => Event.type == EventType.MouseDown;
        public static bool isAlt => Event.alt;
        public static bool isShift => Event.shift;
        public static bool isCtrl => Event.control || Event.command;
        public static bool Q => Event.type == EventType.KeyDown && Event.keyCode == KeyCode.Q && Event.modifiers == EventModifiers.None;
        public static bool W => Event.type == EventType.KeyDown && Event.keyCode == KeyCode.W && Event.modifiers == EventModifiers.None;
        public static bool E => Event.type == EventType.KeyDown && Event.keyCode == KeyCode.E && Event.modifiers == EventModifiers.None;
        public static bool R => Event.type == EventType.KeyDown && Event.keyCode == KeyCode.R && Event.modifiers == EventModifiers.None;
        public static bool isExpand => Event.type == EventType.KeyDown && Event.keyCode == KeyCode.E && Event.shift;
        public static bool isEscape => Event.type == EventType.KeyDown && Event.keyCode == KeyCode.Escape && Event.modifiers == EventModifiers.None;
        public static void Use() => Event?.Use();
    }

    internal static class EditorIcon
    {
        private static readonly Dictionary<Type, Texture2D> cache = new Dictionary<Type, Texture2D>();
        private static readonly Dictionary<string, Texture2D> icons = new Dictionary<string, Texture2D>();
        private static readonly Dictionary<string, string> items;

        static EditorIcon()
        {
            var data = Service.Zip.Decompress(GlobalSetting.LoadAsset(AssetData.Icons));
            items = JsonManager.FromJson<List<KeyValue>>(data).ToDictionary(p => p.Key, p => p.Value);
        }

        public static Texture2D GetIcon(Object target)
        {
            return target ? GetIcon(target.GetType()) : null;
        }

        public static Texture2D GetIcon(Type target)
        {
            if (!cache.TryGetValue(target, out var icon))
            {
                icon = AssetPreview.GetMiniTypeThumbnail(target);
                cache[target] = icon;
            }

            return icon;
        }

        public static Texture2D GetIcon(string target)
        {
            if (icons.TryGetValue(target, out var icon))
            {
                return icon;
            }

            if (items.TryGetValue(target, out var result))
            {
                icon = new Texture2D(4, 4, TextureFormat.DXT5, false)
                {
                    wrapMode = TextureWrapMode.Clamp,
                    filterMode = FilterMode.Bilinear,
                    hideFlags = HideFlags.HideAndDontSave
                };
                icon.LoadImage(Convert.FromBase64String(result));
            }

            if (!icon)
            {
                icon = typeof(EditorGUIUtility).Invoke<Texture2D>("LoadIcon", target);
            }

            if (!icon)
            {
                icon = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                icon.SetPixel(0, 0, Color.clear);
                icon.Apply();
            }

            return icons[target] = icon;
        }

        [Serializable]
        private struct KeyValue
        {
            public string Key;
            public string Value;

            public KeyValue(string key, string value)
            {
                Key = key;
                Value = value;
            }
        }
    }

    internal static class EditorRef
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

        public static float CalcSize(string name)
        {
            var text = GUIContent.none.text;
            GUIContent.none.text = name;
            var size = GUI.skin.label.CalcSize(GUIContent.none).x;
            GUIContent.none.text = text;
            return size;
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