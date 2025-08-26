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
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Astraia
{
    using static EditorEvent;

    internal static class Hierarchy
    {
        private static readonly HashSet<int> windows = new HashSet<int>();
        private static readonly GUIContent content = new GUIContent();

        public static void OnGUI(int id, Rect rect)
        {
            var target = (GameObject)EditorUtility.InstanceIDToObject(id);

            if (isMouseMove)
            {
                Debug.Log(isMouseMove );
            }
            if (isLayout)
            {
                InitWindow();
            }
            else if (isRepaint)
            {
                DrawTexture(rect, target);
                DrawIcon(rect, target);
            }
            else if (isMouseDown)
            {
                DrawIcon(rect, target);
            }

            var toggle = new Rect(rect)
            {
                x = rect.x - 60,
                width = rect.width + 76,
            };

            if (toggle.Contains(mousePosition) && target)
            {
                var oldState = target.activeSelf;
                target.SetActive(EditorGUI.Toggle(new Rect(33F, rect.y, 16, rect.height), target.activeSelf));
                if (oldState != target.activeSelf)
                {
                    EditorUtility.SetDirty(target);
                }
            }
        }

        private static void InitWindow()
        {
            var window = Reflection.ShowHierarchy();
            if (window != null)
            {
                if (!windows.Contains(window.GetInstanceID()))
                {
                    var element = window.rootVisualElement.parent.Query<IMGUIContainer>().First();
                    element.onGUIHandler = OnGUIHandler + element.onGUIHandler;
                    Reflection.HideHierarchy(window);
                    windows.Add(window.GetInstanceID());
                    return;

                    void OnGUIHandler()
                    {
                        if (Event.current.type == EventType.Layout)
                        {
                            Reflection.HideHierarchy(window);
                        }
                    }
                }
            }
        }

        private static void DrawTexture(Rect rect, GameObject target)
        {
            var itemRect = new Rect(rect.x, rect.y + (16 - rect.height) / 2, 16, 16);
            GUI.DrawTexture(itemRect, DrawImage(target), ScaleMode.ScaleToFit);
            if (target == null) return;
            var x = Mathf.Max(28, rect.x - 128 - 16);
            var width = Mathf.Min(128, rect.x - 28 - 16);
            var position = new Rect(x, rect.y, width, rect.height);
            var texCoords = new Rect(1 - width / 128, 0, width / 128, 1);
            GUI.DrawTextureWithTexCoords(position, EditorIcon.GetIcon(Tree.Normal), texCoords);
            if (target.transform.childCount == 0)
            {
                var item = Tree.Height;
                var index = target.transform.GetSiblingIndex();
                if (index != 0)
                {
                    var parent = target.transform.parent;
                    var amount = parent != null ? parent.childCount : target.scene.rootCount;
                    item = index == amount - 1 ? Tree.Bottom : Tree.Middle;
                }

                position.width = 16;
                position.x = rect.x - 16;
                GUI.DrawTexture(position, EditorIcon.GetIcon(item));
            }

            if (Mathf.FloorToInt((rect.y - 4) / 16 % 2) != 0)
            {
                itemRect = new Rect(32, rect.y, rect.width + rect.x - 16, rect.height);
                EditorGUI.DrawRect(itemRect, Color.black * 0.05f);
            }

            rect.width += 16;
            rect.height = 1;
            rect.xMin = 32;
            rect.y += 15.5f;
            EditorGUI.DrawRect(rect, Color.black * 0.2f);
        }

        private static Texture DrawImage(GameObject target)
        {
            if (target == null)
            {
                return Reflection.unityIcon.image;
            }

            Texture icon = AssetPreview.GetMiniThumbnail(target);

            if (icon.name == "d_Prefab Icon" || icon.name == "Prefab Icon")
            {
                if (PrefabUtility.IsAnyPrefabInstanceRoot(target))
                {
                    return Reflection.prefabIcon.image;
                }
            }

            var components = target.GetComponents<Component>();
            Component component;
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
                        if (icon != null)
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

            if (icon == null)
            {
                return Reflection.objectIcon.image;
            }

            if (icon.name == "cs Script Icon" || icon.name == "d_cs Script Icon" || icon.name == "dll Script Icon")
            {
                return AssetPreview.GetMiniThumbnail(components[0]);
            }

            return icon;
        }

        private static void DrawIcon(Rect rect, GameObject target)
        {
            if (target == null) return;
            content.text = target.name;
            var nameSize = GUI.skin.label.CalcSize(content).x;
            var nameRect = new Rect(rect.x, rect.y, nameSize + 14, rect.height);
            var distance = rect.xMax + 16;

            var render = target.GetComponent<Renderer>();
            var entity = target.GetComponents<Component>().ToList<Object>();
            var shared = render != null && render.sharedMaterials != null;
            if (shared)
            {
                entity.AddRange(render.sharedMaterials);
            }

            var isPrefabAsset = PrefabUtility.IsPartOfPrefabAsset(target);
            var isPrefabInstance = PrefabUtility.IsPartOfPrefabInstance(target);
            var isPrefab = isPrefabAsset || isPrefabInstance;

            for (var i = 0; i < entity.Count; ++i)
            {
                if (entity[i] != null)
                {
                    if (isPrefab && i == 0)
                    {
                        distance -= 14;
                        continue;
                    }

                    if (shared && i == entity.Count - render.sharedMaterials.Length)
                    {
                        foreach (var material in render.sharedMaterials)
                        {
                            if (material != null)
                            {
                                if (!DrawComponent())
                                {
                                    break;
                                }
                            }
                        }

                        break;
                    }

                    if (!DrawComponent())
                    {
                        break;
                    }

                    bool DrawComponent()
                    {
                        distance -= 14;
                        var itemRect = new Rect(distance, rect.y, 12, rect.height);
                        if (itemRect.xMax > nameRect.x && itemRect.x < nameRect.xMax)
                        {
                            return false;
                        }

                        ItemIcon(itemRect, entity[i]);
                        return true;
                    }
                }
            }
        }

        private static void ItemIcon(Rect rect, Object item)
        {
            if (isRepaint)
            {
                var icon = EditorGUIUtility.ObjectContent(item, item.GetType()).image;
                if (rect.Contains(mousePosition) && isShift)
                {
                    GUI.DrawTexture(rect, icon, ScaleMode.ScaleToFit);
                }
                else
                {
                    var color = GUI.color;
                    GUI.color = new Color(1, 1, 1, 0.3f);
                    GUI.DrawTexture(rect, icon, ScaleMode.ScaleToFit);
                    GUI.color = color;
                }
            }
            else if (isMouseDown)
            {
                if (rect.Contains(mousePosition) && isShift && mouseButton == 0)
                {
                    Reflection.ShowContext(rect, item);
                    Use();
                }
            }
        }
    }
}