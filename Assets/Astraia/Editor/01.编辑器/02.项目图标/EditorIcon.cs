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
using UnityEngine;

namespace Astraia
{
    internal static class EditorIcon
    {
        private static readonly Dictionary<int, Texture2D> icons = new Dictionary<int, Texture2D>();
        private static readonly Dictionary<string, string> items;


        static EditorIcon()
        {
            var data = Service.Zip.Decompress(GlobalSetting.GetTextByIndex(AssetText.Icons));
            items = JsonManager.FromJson<List<KeyValue>>(data).ToDictionary(p => p.Key, p => p.Value);
        }

        public static Texture2D GetIcon<T>(T value) where T : Enum
        {
            var index = value.GetHashCode();
            if (icons.TryGetValue(index, out var icon))
            {
                return icon;
            }

            if (!items.TryGetValue(value.ToString(), out var result))
            {
                icon = Texture2D.grayTexture;
                icons.Add(index, icon);
                return icon;
            }

            icon = new Texture2D(4, 4, TextureFormat.DXT5, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                hideFlags = HideFlags.HideAndDontSave
            };
            icon.LoadImage(Convert.FromBase64String(result));
            icons.Add(index, icon);
            return icon;
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

    internal enum Icon
    {
        Animations = 1,
        Audios = 2,
        Editor = 3,
        Lights = 4,
        Fonts = 5,
        Materials = 6,
        Meshes = 7,
        Physics = 8,
        Plugins = 9,
        Prefabs = 10,
        Project = 11,
        Resources = 12,
        Scenes = 13,
        Scripts = 14,
        Shaders = 15,
        Terrains = 16,
        Textures = 17,
        Android = 18,
        IPhone = 19,
        MacOS = 20,
        WebGL = 21,
        Windows = 22,
    }

    internal enum Item
    {
        Normal = 23,
        Bottom = 24,
        Middle = 25,
        Height = 26,
    }
}