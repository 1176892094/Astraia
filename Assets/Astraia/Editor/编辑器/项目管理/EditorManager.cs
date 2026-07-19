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
using System.Reflection;
using System.Runtime.CompilerServices;
using Astraia;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using TreeViewItem = UnityEditor.IMGUI.Controls.TreeViewItem<UnityEngine.EntityId>;

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
            Search.OnLoad -= EditorSetting.LoadWindows;
            Search.OnLoad += EditorSetting.LoadWindows;

            EditorApplication.update -= Update;
            EditorApplication.update += Update;
#if UNITY_6000_4_OR_NEWER
            EditorApplication.hierarchyWindowItemByEntityIdOnGUI -= Hierarchy.OnGUI;
            EditorApplication.hierarchyWindowItemByEntityIdOnGUI += Hierarchy.OnGUI;
            EditorApplication.projectWindowItemByEntityIdOnGUI -= Folder.OnGUI;
            EditorApplication.projectWindowItemByEntityIdOnGUI += Folder.OnGUI;
#else
            EditorApplication.hierarchyWindowItemOnGUI -= Hierarchy.OnGUI;
            EditorApplication.hierarchyWindowItemOnGUI += Hierarchy.OnGUI;
            EditorApplication.projectWindowItemInstanceOnGUI -= Folder.OnGUI;
            EditorApplication.projectWindowItemInstanceOnGUI += Folder.OnGUI;
#endif
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

    internal static class EditorEvent
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
            var data = Zip.Decompress(GlobalSetting.LoadText(AssetData.Icons));
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
                icon = new Texture2D(4, 4, TextureFormat.DXT5, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear, hideFlags = HideFlags.HideAndDontSave };
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

    internal static class Emit
    {
        private static readonly Dictionary<Type, Dictionary<string, FieldInfo>> fieldData = new();
        private static readonly Dictionary<Type, Dictionary<string, MethodInfo>> methodData = new();
        private static readonly Dictionary<Type, Dictionary<string, PropertyInfo>> propertyData = new();

        public static object Invoke(object target, string name, params object[] args)
        {
            var result = target as Type ?? target.GetType();
            var method = GetMethod(result, name, args.Select(r => r.GetType()).ToArray());
            if (method != null)
            {
                return method.Invoke(target is Type ? null : target, args);
            }

            throw new MissingMethodException(result.FullName, name);
        }

        public static object GetValue(object target, string name)
        {
            var source = target as Type ?? target.GetType();
            var field = GetField(source, name);
            if (field != null)
            {
                return field.GetValue(target is Type ? null : target);
            }

            var property = GetProperty(source, name);
            if (property != null)
            {
                return property.GetValue(target is Type ? null : target);
            }

            throw new MissingMemberException(source.FullName, name);
        }

        public static void SetValue(object target, string name, object value)
        {
            var source = target as Type ?? target.GetType();
            var field = GetField(source, name);
            if (field != null)
            {
                field.SetValue(target is Type ? null : target, value);
                return;
            }

            var property = GetProperty(source, name);
            if (property != null)
            {
                property.SetValue(target is Type ? null : target, value);
                return;
            }

            throw new MissingMemberException(source.FullName, name);
        }

        private static MethodInfo GetMethod(Type type, string name, params Type[] args)
        {
            if (!methodData.TryGetValue(type, out var results))
            {
                results = new Dictionary<string, MethodInfo>();
                methodData[type] = results;
            }

            if (!results.TryGetValue(name, out var result))
            {
                for (var current = type; current != null; current = current.BaseType)
                {
                    result = current.GetMethod(name, (BindingFlags)62, null, args, null);
                    if (result != null)
                    {
                        return results[name] = result;
                    }
                }

                foreach (var current in type.GetInterfaces())
                {
                    result = current.GetMethod(name, (BindingFlags)62, null, args, null);
                    if (result != null)
                    {
                        return results[name] = result;
                    }
                }
            }

            return result;
        }

        private static FieldInfo GetField(Type type, string name)
        {
            if (!fieldData.TryGetValue(type, out var results))
            {
                results = new Dictionary<string, FieldInfo>();
                fieldData[type] = results;
            }

            if (!results.TryGetValue(name, out var result))
            {
                for (var current = type; current != null; current = current.BaseType)
                {
                    result = current.GetField(name, (BindingFlags)62);
                    if (result != null)
                    {
                        return results[name] = result;
                    }
                }
            }

            return result;
        }

        private static PropertyInfo GetProperty(Type type, string name)
        {
            if (!propertyData.TryGetValue(type, out var results))
            {
                results = new Dictionary<string, PropertyInfo>();
                propertyData[type] = results;
            }

            if (!results.TryGetValue(name, out var result))
            {
                for (var current = type; current != null; current = current.BaseType)
                {
                    result = current.GetProperty(name, (BindingFlags)62);
                    if (result != null)
                    {
                        return results[name] = result;
                    }
                }
            }

            return result;
        }
    }

    internal static class Extensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static object GetValue(this object target, string name)
        {
            return Emit.GetValue(target, name);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static T GetValue<T>(this object target, string name)
        {
            return (T)Emit.GetValue(target, name);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void SetValue(this object target, string name, object value)
        {
            Emit.SetValue(target, name, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void SetValue<T>(this object target, string name, T value)
        {
            Emit.SetValue(target, name, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static object Invoke(this object target, string name, params object[] args)
        {
            return Emit.Invoke(target, name, args);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static T Invoke<T>(this object target, string name, params object[] args)
        {
            return (T)Emit.Invoke(target, name, args);
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
        public static readonly GUIContent projectIcon = EditorGUIUtility.IconContent("d_VerticalLayoutGroup Icon");

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