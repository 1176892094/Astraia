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

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Astraia
{
    internal static class Folder
    {
        private static readonly Dictionary<string, Icon> icons = new Dictionary<string, Icon>()
        {
            { "Animations", Icon.Animations },
            { "Resources", Icon.Resources },
            { "Scenes", Icon.Scenes },
            { "Scripts", Icon.Scripts },
            { "Plugins", Icon.Plugins },
            { "Materials", Icon.Materials },
            { "Extensions", Icon.Editor },
            { "Audios", Icon.Audios },
            { "Prefabs", Icon.Prefabs },
            { "Models", Icon.Meshes },
            { "Settings", Icon.Project },
            { "Shaders", Icon.Shaders },
            { "Fonts", Icon.Fonts },
            { "Textures", Icon.Textures },
            { "StreamingAssets", Icon.Resources },
            { "Physics", Icon.Physics },
            { "Terrains", Icon.Terrains },
            { "Tilemaps", Icon.Terrains },
            { "Lights", Icon.Lights },
            { "Process", Icon.Lights },
            { "Editor", Icon.Editor },
            { "Android", Icon.Android },
            { "iOS", Icon.IPhone },
            { "Windows", Icon.Windows },
            { "MacOS", Icon.MacOS },
            { "WebGL", Icon.WebGL },
            { "DataTable", Icon.Project },
            { "Atlas", Icon.Meshes },
            { "Icons", Icon.Textures },
            { "HotUpdate", Icon.Scripts },
            { "Template", Icon.Resources },
        };

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
                    var folder = Path.GetFileName(path);

                    if (icons.TryGetValue(folder, out var icon))
                    {
                        rect.width = rect.height;
                        GUI.DrawTexture(rect, EditorIcon.GetIcon(icon));
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
            GUI.DrawTextureWithTexCoords(position, EditorIcon.GetIcon(Item.Normal), texCoords);

            if (!string.IsNullOrEmpty(path))
            {
                if (!Reflection.HasChild(id))
                {
                    position.width = 16;
                    position.x = rect.x - 16;
                    GUI.DrawTexture(position, EditorIcon.GetIcon(Item.Middle));
                }
            }

            if (Mathf.FloorToInt((rect.y - 4) / 16 % 2) != 0)
            {
                var itemRect = new Rect(0, rect.y, rect.width + rect.x, rect.height);
                EditorGUI.DrawRect(itemRect, Color.black * 0.05f);
            }

            rect.width += rect.x + 16;
            rect.height = 1;
            rect.x = 0;
            rect.y += 15.5f;
            EditorGUI.DrawRect(rect, Color.black * 0.1f);
        }
    }
}