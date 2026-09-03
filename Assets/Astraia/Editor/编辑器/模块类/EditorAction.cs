// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-09-02 19:09:01
// # Recently: 2026-09-03 14:21:07
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Astraia
{
    internal static class EditorAction
    {
        private static readonly Event current;
        private static readonly Event Event = current ??= typeof(Event).GetValue<Event>("s_Current");
        public static int mouseButton => Event.button;
        public static Vector2 mousePosition => Event.mousePosition;
        public static bool isLayout => Event.type == EventType.Layout;
        public static bool isRepaint => Event.type == EventType.Repaint;
        public static bool isMouseUp => Event.type == EventType.MouseUp;
        public static bool isMouseDown => Event.type == EventType.MouseDown;
        public static bool Q => Event.type == EventType.KeyDown && Event.keyCode == KeyCode.Q && Event.modifiers == EventModifiers.None;
        public static bool W => Event.type == EventType.KeyDown && Event.keyCode == KeyCode.W && Event.modifiers == EventModifiers.None;
        public static bool E => Event.type == EventType.KeyDown && Event.keyCode == KeyCode.E && Event.modifiers == EventModifiers.None;
        public static bool R => Event.type == EventType.KeyDown && Event.keyCode == KeyCode.R && Event.modifiers == EventModifiers.None;
        public static bool isExpand => Event.type == EventType.KeyDown && Event.keyCode == KeyCode.E && Event.shift;
        public static void Use() => Event?.Use();
    }

    internal static class EditorIcon
    {
        private static readonly Dictionary<Type, Texture2D> cache = new Dictionary<Type, Texture2D>();
        private static readonly Dictionary<string, Texture2D> icons = new Dictionary<string, Texture2D>();
        private static readonly Dictionary<string, string> items;

        static EditorIcon()
        {
            var data = Zip.Decompress(GlobalSetting.LoadText(TextAsset.编辑器));
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
}