// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-04-09 20:04:48
// // # Recently: 2025-04-09 20:04:48
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Toolbars;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Astraia
{
    internal static class Toolbar
    {
        private static List<Object> scenePaths => GlobalSetting.Instance.sceneCaches;
        private static ToolbarMenu sceneBar;

        public static void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            if (!EditorApplication.isPlaying)
            {
                SetToolbarMenu(AssetDatabase.LoadAssetAtPath<Object>(scene.path));
            }
        }

        public static void SelectionChanged()
        {
            var toolbars = Resources.FindObjectsOfTypeAll(EditorRef.Toolbar);
            foreach (var toolbar in toolbars)
            {
                var root = (VisualElement)toolbar.GetValue("m_Root");
                if (root != null)
                {
                    var parent = root.ElementAt(0).ElementAt(0).ElementAt(1);
                    parent.Add(SetButton(EditorRef.customIcon.image, EditorSetting.ShowWindow));
                    parent.Add(SetDropDown());
                    parent.Insert(0, SetTimescale());
                    parent.Insert(0, SetButton(EditorRef.settingIcon.image, () => EditorApplication.ExecuteMenuItem("Edit/Project Settings...")));
                    parent.Insert(0, SetButton(EditorRef.buildIcon.image, () => EditorApplication.ExecuteMenuItem("File/Build Profiles")));
                    return;
                }
            }
        }

        private static EditorToolbarButton SetButton(Texture texture, Action assetAction)
        {
            var button = new EditorToolbarButton(assetAction)
            {
                style =
                {
                    width = 30,
                    backgroundColor = Color.white * 0.5f,
                    unityTextAlign = TextAnchor.MiddleCenter,
                    borderBottomWidth = 0,
                    borderTopWidth = 0,
                    borderLeftWidth = 0,
                    borderRightWidth = 0,
                    marginLeft = 1,
                    marginRight = 1,
                },
                iconImage = texture as Texture2D,
            };
            button.RegisterCallback<MouseEnterEvent>(_ => button.style.backgroundColor = Color.white * 0.6f);
            button.RegisterCallback<MouseLeaveEvent>(_ => button.style.backgroundColor = Color.white * 0.5f);
            return button;
        }

        private static ToolbarMenu SetDropDown()
        {
            sceneBar = new ToolbarMenu()
            {
                style =
                {
                    backgroundColor = Color.white * 0.5f,
                    unityTextAlign = TextAnchor.MiddleCenter,
                    borderTopLeftRadius = 4,
                    borderTopRightRadius = 4,
                    borderBottomLeftRadius = 4,
                    borderBottomRightRadius = 4,
                    marginBottom = 0,
                    marginLeft = 2,
                    marginRight = 2,
                    marginTop = 0,
                    paddingBottom = 2,
                    paddingLeft = 4,
                    paddingRight = 4,
                    paddingTop = 2,
                    height = 20,
                },
                text = "Empty Scene"
            };

            var menuIcon = new VisualElement
            {
                style =
                {
                    width = 16,
                    height = 16,
                    backgroundImage = EditorRef.sceneIcon.image as Texture2D,
                },
            };
            sceneBar.Insert(0, menuIcon);
            foreach (var scenePath in scenePaths)
            {
                SetToolbarMenu(scenePath);
                break;
            }

            sceneBar.RegisterCallback<MouseEnterEvent>(_ => sceneBar.style.backgroundColor = Color.white * 0.6f);
            sceneBar.RegisterCallback<MouseLeaveEvent>(_ => sceneBar.style.backgroundColor = Color.white * 0.5f);
            return sceneBar;
        }

        private static void SetToolbarMenu(Object sceneData)
        {
            if (scenePaths.Contains(sceneData))
            {
                scenePaths.Remove(sceneData);
            }

            scenePaths.Insert(0, sceneData);
            if (scenePaths.Count > 5)
            {
                scenePaths.RemoveAt(scenePaths.Count - 1);
            }

            sceneBar.menu.ClearItems();
            sceneBar.text = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(sceneData));
            for (var i = 1; i < scenePaths.Count; i++)
            {
                var index = i;
                sceneBar.menu.AppendAction(Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(scenePaths[index])), _ =>
                {
                    try
                    {
                        if (!EditorApplication.isPlaying && EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                        {
                            EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(scenePaths[index]), OpenSceneMode.Single);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("打开 " + scenePaths[index] + " 场景失败!\n" + e);
                    }
                });
            }
        }

        private static ToolbarMenu SetTimescale()
        {
            var dropdown = new ToolbarMenu
            {
                text = Time.timeScale.ToString("F2"),
                style =
                {
                    backgroundColor = Color.white * 0.5f,
                    unityTextAlign = TextAnchor.MiddleCenter,
                    borderTopLeftRadius = 4,
                    borderTopRightRadius = 4,
                    borderBottomLeftRadius = 4,
                    borderBottomRightRadius = 4,
                    marginBottom = 0,
                    marginLeft = 2,
                    marginRight = 2,
                    marginTop = 0,
                    paddingBottom = 2,
                    paddingLeft = 4,
                    paddingRight = 4,
                    paddingTop = 2,
                    height = 20,
                },
            };
            var menuIcon = new VisualElement
            {
                style =
                {
                    width = 16,
                    height = 16,
                    backgroundImage = EditorRef.windowIcon.image as Texture2D,
                },
            };
            dropdown.Insert(0, menuIcon);
            dropdown.menu.AppendAction("0", _ =>
            {
                Time.timeScale = 0;
                dropdown.text = Time.timeScale.ToString("F2");
            });
            dropdown.menu.AppendAction("0.25", _ =>
            {
                Time.timeScale = 0.25f;
                dropdown.text = Time.timeScale.ToString("F2");
            });
            dropdown.menu.AppendAction("0.5", _ =>
            {
                Time.timeScale = 0.5f;
                dropdown.text = Time.timeScale.ToString("F2");
            });
            dropdown.menu.AppendAction("0.75", _ =>
            {
                Time.timeScale = 0.75f;
                dropdown.text = Time.timeScale.ToString("F2");
            });
            dropdown.menu.AppendAction("1", _ =>
            {
                Time.timeScale = 1;
                dropdown.text = Time.timeScale.ToString("F2");
            });
            dropdown.menu.AppendAction("1.5", _ =>
            {
                Time.timeScale = 1.5f;
                dropdown.text = Time.timeScale.ToString("F2");
            });
            dropdown.menu.AppendAction("2", _ =>
            {
                Time.timeScale = 2f;
                dropdown.text = Time.timeScale.ToString("F2");
            });
            dropdown.menu.AppendAction("2.5", _ =>
            {
                Time.timeScale = 2.5f;
                dropdown.text = Time.timeScale.ToString("F2");
            });
            dropdown.menu.AppendAction("3", _ =>
            {
                Time.timeScale = 3;
                dropdown.text = Time.timeScale.ToString("F2");
            });
            dropdown.RegisterCallback<MouseEnterEvent>(_ => dropdown.style.backgroundColor = Color.white * 0.6f);
            dropdown.RegisterCallback<MouseLeaveEvent>(_ => dropdown.style.backgroundColor = Color.white * 0.5f);
            return dropdown;
        }
    }
}