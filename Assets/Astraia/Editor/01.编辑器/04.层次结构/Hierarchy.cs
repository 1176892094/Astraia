// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-04-09 19:04:31
// // # Recently: 2025-04-09 19:04:31
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Astraia
{
    using static EditorInput;

    internal static partial class Hierarchy
    {
        private static readonly HashSet<int> windows = new HashSet<int>();
        private static readonly List<Object> items = new List<Object>();
        private static bool pressed;

        public static void OnGUI(int id, Rect rect)
        {
            var target = (GameObject)EditorUtility.InstanceIDToObject(id);

            if (isLayout)
            {
                DrawWindow();
            }
            else if (isRepaint)
            {
                DrawTransform(rect, target);
                DrawTexCoords(rect, target);
                DrawComponent(rect, target);
            }
            else if (isMouseDown || isMouseUp)
            {
                DrawComponent(rect, target);
            }

            DrawToggle();
            return;

            void DrawWindow()
            {
                var window = Reflection.GetHierarchy();
                if (window != null)
                {
                    var instance = window.GetInstanceID();
                    if (!windows.Contains(instance))
                    {
                        var root = window.rootVisualElement.parent.Query<IMGUIContainer>().First();
                        root.onGUIHandler = HideIcon + root.onGUIHandler;
                        Reflection.HideIcon(window);
                        windows.Add(instance);
                    }
                }

                return;

                void HideIcon()
                {
                    if (isLayout)
                    {
                        Reflection.HideIcon(window);
                    }
                }
            }

            void DrawToggle()
            {
                var button = new Rect(rect);
                button.x -= 60;
                button.width = rect.width + 76;
                if (button.Contains(mousePosition) && target)
                {
                    var activeSelf = target.activeSelf;
                    button.x = 33;
                    button.width = 16;
                    target.SetActive(EditorGUI.Toggle(button, target.activeSelf));
                    if (activeSelf != target.activeSelf)
                    {
                        EditorUtility.SetDirty(target);
                    }
                }
            }
        }

        private static void DrawTransform(Rect rect, GameObject target)
        {
            var itemRect = new Rect(rect)
            {
                width = 16,
                height = 16
            };
            itemRect.y += (16 - rect.height) / 2;
            GUI.DrawTexture(itemRect, DrawIcon(), ScaleMode.ScaleToFit);

            if (!target) return;
            if (Mathf.FloorToInt((rect.y - 4) / 16 % 2) != 0)
            {
                itemRect.x = 32;
                itemRect.width = rect.width + rect.x - 16;
                EditorGUI.DrawRect(itemRect, Color.black * 0.05f);
            }

            rect.xMin = 32;
            rect.width += 16;
            rect.y += 15;
            rect.height = 1;
            EditorGUI.DrawRect(rect, Color.black * 0.2f);

            Texture DrawIcon()
            {
                if (!target)
                {
                    return Reflection.sceneIcon.image;
                }

                Texture icon = AssetPreview.GetMiniThumbnail(target);
                if (icon.name is "d_Prefab Icon" or "Prefab Icon")
                {
                    if (PrefabUtility.IsAnyPrefabInstanceRoot(target))
                    {
                        return icon;
                    }
                }

                Component component;
                var components = target.GetComponents<Component>();
                if (components.Length > 1)
                {
                    component = components[1];
                    if (components.Length > 2 && component is CanvasRenderer)
                    {
                        component = components[2];
                        if (components.Length > 3 && component is ICanvasRaycastFilter)
                        {
                            var image = components[3];
                            icon = AssetPreview.GetMiniThumbnail(image);
                            if (icon)
                            {
                                component = image;
                            }
                        }
                    }
                }
                else
                {
                    component = components[0];
                }

                icon = AssetPreview.GetMiniThumbnail(component);
                if (icon.name is "cs Script Icon" or "d_cs Script Icon" or "dll Script Icon")
                {
                    return AssetPreview.GetMiniThumbnail(components[0]);
                }

                return icon;
            }
        }


        private static void DrawTexCoords(Rect rect, GameObject target)
        {
            if (!target) return;
            var x = Mathf.Max(28, rect.x - 128 - 16);
            var width = Mathf.Min(128, rect.x - 28 - 16);
            var position = new Rect(x, rect.y, width, rect.height);
            var texCoords = new Rect(1 - width / 128, 0, width / 128, 1);
            GUI.DrawTextureWithTexCoords(position, EditorIcon.GetIcon("Normal"), texCoords);
            if (target.transform.childCount == 0)
            {
                var item = "Height";
                var index = target.transform.GetSiblingIndex();
                if (index != 0)
                {
                    var parent = target.transform.parent;
                    var amount = parent != null ? parent.childCount : target.scene.rootCount;
                    item = index == amount - 1 ? "Bottom" : "Middle";
                }

                position.width = 16;
                position.x = rect.x - 16;
                GUI.DrawTexture(position, EditorIcon.GetIcon(item));
            }
        }

        private static void DrawComponent(Rect rect, GameObject target)
        {
            if (!target) return;
            items.Clear();
            items.AddRange(target.GetComponents<Component>());

            var render = target.GetComponent<Renderer>();
            var shared = render && render.sharedMaterials != null;
            if (shared)
            {
                items.AddRange(render.sharedMaterials);
            }

            var distance = rect.xMax + 16;
            var nameSize = rect.x + Reflection.NameLength(target.name) + 15;
            for (var i = 0; i < items.Count; ++i)
            {
                if (!items[i])
                {
                    continue;
                }

                if (i == 0 && PrefabUtility.IsAnyPrefabInstanceRoot(target))
                {
                    distance -= 14;
                    continue;
                }

                Rect itemRect;
                if (shared && i == items.Count - render.sharedMaterials.Length)
                {
                    foreach (var material in render.sharedMaterials)
                    {
                        if (material)
                        {
                            LoadIcon();
                        }
                    }

                    break;
                }

                LoadIcon();

                void LoadIcon()
                {
                    distance -= 14;
                    itemRect = new Rect(distance, rect.y, 12, rect.height);
                    if (nameSize < itemRect.x)
                    {
                        Button(itemRect, items[i]);
                    }
                }
            }
        }

        private static void Button(Rect rect, Object item)
        {
            var color = GUI.color;
            GUI.color = rect.Contains(mousePosition) && pressed ? Color.white : Color.white * 0.6F;
            GUI.DrawTexture(rect, AssetPreview.GetMiniThumbnail(item), ScaleMode.ScaleToFit);
            GUI.color = color;

            if (isMouseDown)
            {
                pressed = true;
            }

            if (isMouseUp)
            {
                pressed = false;
                if (rect.Contains(mousePosition) && mouseButton == 0)
                {
                    rect.x += 16;
                    rect.y -= 16;
                    Reflection.ShowContext(rect, item);
                    Use();
                }
            }
        }
    }
}