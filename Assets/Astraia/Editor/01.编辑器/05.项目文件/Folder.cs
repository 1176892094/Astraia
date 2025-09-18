// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-04-09 20:04:37
// // # Recently: 2025-04-09 20:04:37
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;


namespace Astraia
{
    using static EventManager;

    internal static partial class Folder
    {
        private static readonly Dictionary<string, string> icons = new()
        {
            { "Animations", "Animations" },
            { "Audios", "Audios" },
            { "Editor", "Editor" },
            { "Lights", "Lights" },
            { "Fonts", "Fonts" },
            { "Materials", "Materials" },
            { "Meshes", "Meshes" },
            { "Physics", "Physics" },
            { "Plugins", "Plugins" },
            { "Prefabs", "Prefabs" },
            { "Project", "Project" },
            { "Resources", "Resources" },
            { "Scenes", "Scenes" },
            { "Scripts", "Scripts" },
            { "Shaders", "Shaders" },
            { "Terrains", "Terrains" },
            { "Textures", "Textures" },
            { "Android", "Android" },
            { "iOS", "iOS" },
            { "MacOS", "MacOS" },
            { "WebGL", "WebGL" },
            { "Windows", "Windows" },

            { "Atlas", "Meshes" },
            { "Models", "Meshes" },
            { "Settings", "Project" },
            { "DataTable", "Project" },
            { "HotUpdate", "Scripts" },
            { "Extensions", "Editor" },
            { "Tilemaps", "Terrains" },
            { "AssetBundles", "Resources" },
            { "StreamingAssets", "Resources" },
        };

        private static readonly List<string> sorts = new List<string>
        {
            "SceneAsset Icon",
            "Prefab Icon",
            "Mesh Icon",
            "Material Icon",
            "Texture Icon",
            "cs Script Icon",
            "Shader Icon",
            "ComputeShader Icon",
            "ShaderInclude Icon",
            "ScriptableObject Icon"
        };

        private static readonly Dictionary<string, List<string>> items = new();

        public static void OnGUI(int id, Rect rect)
        {
            if (!isRepaint)
            {
                return;
            }

            if (rect.height > 16)
            {
                return;
            }

            var data = EditorUtility.InstanceIDToObject(id);
            var path = AssetDatabase.GetAssetPath(data);
            DrawTexCoords(id, rect, path);

            if (!AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            var name = Path.GetFileName(path);
            if (icons.TryGetValue(name, out var icon))
            {
                var iconRect = new Rect(rect);
                iconRect.width = iconRect.height;
                GUI.DrawTexture(iconRect, EditorIcon.GetIcon(icon));
            }

            var iconItem = LoadIcon(AssetDatabase.AssetPathToGUID(path));
            var nameSize = rect.x + Reflection.NameLength(name) + 15;
            var itemRect = new Rect(rect);
            itemRect.x += rect.width - 15;
            itemRect.width = 13;
            foreach (var item in iconItem)
            {
                if (nameSize < itemRect.x)
                {
                    var color = GUI.color;
                    GUI.color = Color.white * 0.6F;
                    GUI.DrawTexture(itemRect, EditorIcon.GetIcon(item), ScaleMode.ScaleToFit);
                    GUI.color = color;
                    itemRect.x -= 13;
                }
            }
        }

        private static void DrawTexCoords(int id, Rect rect, string path)
        {
            var x = Mathf.Max(0, rect.x - 128 - 16);
            var width = Mathf.Min(128, rect.x - 16);
            var position = new Rect(x, rect.y, width, rect.height);
            var texCoords = new Rect(1 - width / 128, 0, width / 128, 1);
            GUI.DrawTextureWithTexCoords(position, EditorIcon.GetIcon("Normal"), texCoords);

            if (!string.IsNullOrEmpty(path))
            {
                if (!Reflection.GetItems().Where(item => item.id == id).Any(item => item.hasChildren))
                {
                    position.width = 16;
                    position.x = rect.x - 16;
                    GUI.DrawTexture(position, EditorIcon.GetIcon("Middle"));
                }
            }

            if (Mathf.FloorToInt((rect.y - 4) / 16 % 2) != 0)
            {
                var itemRect = new Rect(0, rect.y, rect.width + rect.x, rect.height);
                EditorGUI.DrawRect(itemRect, Color.black * 0.05f);
            }

            rect.width += rect.x + 16;
            rect.x = 0;
            rect.y += 16;
            rect.height = 1;
            EditorGUI.DrawRect(rect, Color.black * 0.1f);
        }

        private static List<string> LoadIcon(string guid)
        {
            if (items.TryGetValue(guid, out var item))
            {
                return item;
            }

            item = new List<string>();
            var paths = Directory.GetFiles(AssetDatabase.GUIDToAssetPath(guid), "*.*");
            foreach (var path in paths)
            {
                var result = AssetDatabase.GetMainAssetTypeAtPath(path);
                if (result != null)
                {
                    if (result == typeof(Texture2D))
                    {
                        item.Add("Texture Icon");
                    }
                    else if (result == typeof(GameObject))
                    {
                        item.Add("Prefab Icon");
                    }
                    else if (typeof(ScriptableObject).IsAssignableFrom(result))
                    {
                        item.Add("ScriptableObject Icon");
                    }
                    else if (result == typeof(MonoScript))
                    {
                        item.Add("cs Script Icon");
                    }
                    else
                    {
                        var icon = EditorIcon.GetIcon(result);
                        if (icon != null)
                        {
                            item.Add(icon.name);
                        }
                    }
                }
            }

            item = item.Distinct().ToList();

            for (var i = 0; i < item.Count; i++)
            {
                if (item[i].StartsWith("d_"))
                {
                    item[i] = item[i].Substring(2);
                }
            }

            item.Remove("DefaultAsset Icon");
            if (item.Contains("cs Script Icon"))
            {
                item.Remove("AssemblyDefinitionAsset Icon");
            }

            if (item.Contains("Shader Icon"))
            {
                item.Remove("ShaderInclude Icon");
            }

            item.Sort((a, b) =>
            {
                var indexA = sorts.IndexOf(a);
                var indexB = sorts.IndexOf(b);
                if (indexA == -1) indexA = 1232;
                if (indexB == -1) indexB = 1232;
                var compare = indexA.CompareTo(indexB);
                return compare != 0 ? compare : string.Compare(a, b, StringComparison.Ordinal);
            });
            items[guid] = item;
            return item;
        }

        public static void OnProjectChanged()
        {
            foreach (var item in items.Values)
            {
                item.Clear();
            }

            items.Clear();
        }
    }
}