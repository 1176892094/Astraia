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
    internal static class Folder
    {
        private static readonly Dictionary<string, Icon> icons = new()
        {
            { "Animations", Icon.Animations },
            { "Audios", Icon.Audios },
            { "Editor", Icon.Editor },
            { "Lights", Icon.Lights },
            { "Fonts", Icon.Fonts },
            { "Materials", Icon.Materials },
            { "Meshes", Icon.Meshes },
            { "Physics", Icon.Physics },
            { "Plugins", Icon.Plugins },
            { "Prefabs", Icon.Prefabs },
            { "Project", Icon.Project },
            { "Resources", Icon.Resources },
            { "Scenes", Icon.Scenes },
            { "Scripts", Icon.Scripts },
            { "Shaders", Icon.Shaders },
            { "Terrains", Icon.Terrains },
            { "Textures", Icon.Textures },
            { "Android", Icon.Android },
            { "iOS", Icon.IPhone },
            { "MacOS", Icon.MacOS },
            { "WebGL", Icon.WebGL },
            { "Windows", Icon.Windows },

            { "Atlas", Icon.Meshes },
            { "Models", Icon.Meshes },
            { "DataTable", Icon.Project },
            { "HotUpdate", Icon.Scripts },
            { "Settings", Icon.Project },
            { "Tilemaps", Icon.Terrains },
            { "Extensions", Icon.Editor },
            { "Template", Icon.Resources },
            { "StreamingAssets", Icon.Resources },
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

        private static readonly Dictionary<string, List<string>> itemNames = new();

        public static void OnGUI(int id, Rect rect)
        {
            if (Event.current.type == EventType.Repaint)
            {
                var path = AssetDatabase.GetAssetPath(EditorUtility.InstanceIDToObject(id));
                if (rect.height > 16)
                {
                    return;
                }

                DrawTexture(id, rect, path);

                if (AssetDatabase.IsValidFolder(path))
                {
                    var name = Path.GetFileName(path);
                    if (icons.TryGetValue(name, out var icon))
                    {
                        var iconRect = new Rect(rect);
                        iconRect.width = iconRect.height;
                        GUI.DrawTexture(iconRect, EditorIcon.GetIcon(icon));
                    }

                    var iconNames = LoadIcon(AssetDatabase.AssetPathToGUID(path));
                    var fullRect = new Rect(rect);
                    fullRect.x += fullRect.width;
                    fullRect.width = rect.width + rect.x;
                    fullRect.x -= fullRect.width;
                    Minimap();

                    void Minimap()
                    {
                        var minButtonX = rect.x + Reflection.NameLength(Path.GetFileName(path)) + 15;
                        var iconRect = new Rect(fullRect);
                        iconRect.x += iconRect.width - 2;
                        iconRect.width = 13;
                        iconRect.x -= iconRect.width;


                        foreach (var iconName in iconNames)
                        {
                            if (iconRect.x > minButtonX)
                            {
                                var color = GUI.color;
                                GUI.color = Color.white * 0.6F;
                                GUI.DrawTexture(iconRect, EditorIcon.GetIcon(iconName), ScaleMode.ScaleToFit);
                                GUI.color = color;
                                iconRect.x -= 13;
                            }
                        }
                    }
                }
            }
        }

        private static void DrawTexture(int id, Rect rect, string path)
        {
            var x = Mathf.Max(0, rect.x - 128 - 16);
            var width = Mathf.Min(128, rect.x - 16);
            var position = new Rect(x, rect.y, width, rect.height);
            var texCoords = new Rect(1 - width / 128, 0, width / 128, 1);
            GUI.DrawTextureWithTexCoords(position, EditorIcon.GetIcon(Tree.Normal), texCoords);

            if (!string.IsNullOrEmpty(path))
            {
                if (!Reflection.HasChild(id))
                {
                    position.width = 16;
                    position.x = rect.x - 16;
                    GUI.DrawTexture(position, EditorIcon.GetIcon(Tree.Middle));
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
            if (itemNames.TryGetValue(guid, out var state))
            {
                return state;
            }

            var paths = Directory.GetFiles(AssetDatabase.GUIDToAssetPath(guid), "*.*");

            var iconNames = new List<string>();
            foreach (var path in paths)
            {
                var result = AssetDatabase.GetMainAssetTypeAtPath(path);
                if (result != null)
                {
                    if (result == typeof(Texture2D))
                    {
                        iconNames.Add("Texture Icon");
                    }
                    else if (result == typeof(GameObject))
                    {
                        iconNames.Add("Prefab Icon");
                    }
                    else if (typeof(ScriptableObject).IsAssignableFrom(result))
                    {
                        iconNames.Add("ScriptableObject Icon");
                    }
                    else if (result == typeof(MonoScript))
                    {
                        iconNames.Add("cs Script Icon");
                    }
                    else
                    {
                        var icon = AssetPreview.GetMiniTypeThumbnail(result);
                        if (icon != null)
                        {
                            iconNames.Add(icon.name);
                        }
                    }
                }
            }

            iconNames = iconNames.Distinct().ToList();

            for (var i = 0; i < iconNames.Count; i++)
            {
                if (iconNames[i].StartsWith("d_"))
                {
                    iconNames[i] = iconNames[i].Substring(2);
                }
            }

            iconNames.Remove("DefaultAsset Icon");
            if (iconNames.Contains("cs Script Icon"))
            {
                iconNames.Remove("AssemblyDefinitionAsset Icon");
            }

            if (iconNames.Contains("Shader Icon"))
            {
                iconNames.Remove("ShaderInclude Icon");
            }

            iconNames.Sort((a, b) =>
            {
                var indexA = sorts.IndexOf(a);
                var indexB = sorts.IndexOf(b);
                if (indexA == -1) indexA = 1232;
                if (indexB == -1) indexB = 1232;
                var compare = indexA.CompareTo(indexB);
                return compare != 0 ? compare : string.Compare(a, b, StringComparison.Ordinal);
            });
            itemNames.Add(guid, iconNames);
            return iconNames;
        }
    }
}