// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-04-09 19:04:50
// // # Recently: 2025-04-09 19:04:50
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using Astraia.Common;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Astraia
{
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
            if (target)
            {
                var key = target.GetType();
                if (!cache.TryGetValue(key, out var icon))
                {
                    icon = AssetPreview.GetMiniTypeThumbnail(key);
                    cache[key] = icon;
                }

                return icon;
            }

            return null;
        }

        public static Texture2D GetIcon(Type key)
        {
            if (!cache.TryGetValue(key, out var icon))
            {
                icon = AssetPreview.GetMiniTypeThumbnail(key);
                cache[key] = icon;
            }

            return icon;
        }

        public static Texture2D GetIcon(string reason)
        {
            if (icons.TryGetValue(reason, out var icon))
            {
                return icon;
            }

            if (items.TryGetValue(reason, out var result))
            {
                icon = new Texture2D(4, 4, TextureFormat.DXT5, false)
                {
                    wrapMode = TextureWrapMode.Clamp,
                    filterMode = FilterMode.Bilinear,
                    hideFlags = HideFlags.HideAndDontSave
                };
                icon.LoadImage(Convert.FromBase64String(result));
            }

            if (icon == null)
            {
                icon = typeof(EditorGUIUtility).Invoke<Texture2D>("LoadIcon", reason);
            }

            if (icon == null)
            {
                icon = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                icon.SetPixel(0, 0, Color.clear);
                icon.Apply();
            }

            return icons[reason] = icon;
        }

        [Serializable]
        public struct KeyValue
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