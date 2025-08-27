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

namespace Astraia
{
    internal sealed class Toolbar
    {
        private static Toolbar instance;
        private static List<string> scenes;
        private static ToolbarMenu toolbarMenu;

        private Toolbar()
        {
            EditorApplication.delayCall += OnInitialized;
            EditorSceneManager.sceneOpened += OnSceneOpened;
            Selection.selectionChanged += SelectionChanged;
        }


        [InitializeOnLoadMethod]
        private static void Enable() => instance ??= new Toolbar();

        private void OnInitialized()
        {
            if (Selection.activeObject == null)
            {
                SelectionChanged();
            }
        }

        private static void SelectionChanged()
        {
            var objects = Resources.FindObjectsOfTypeAll(Reflection.Toolbar);
            foreach (var obj in objects)
            {
                if (obj is ScriptableObject window)
                {
                    var parent = FindElement(Reflection.GetRoot(window), "unity-editor-toolbar-container");
                    var element = parent.Q<VisualElement>("ToolbarLeftAlign");
                    if (element == null)
                    {
                        parent.Q<VisualElement>("ToolbarZoneLeftAlign").Add(LeftElement("ToolbarLeftAlign"));
                    }

                    element = parent.Q<VisualElement>("RightLeftAlign");
                    if (element == null)
                    {
                        parent.Q<VisualElement>("ToolbarZoneRightAlign").Add(RightElement("RightLeftAlign"));
                    }
                }
            }
        }

        private static VisualElement FindElement(VisualElement parent, string className)
        {
            for (var i = 0; i < parent.childCount; i++)
            {
                var element = parent[i];
                if (element.ClassListContains(className))
                {
                    return element;
                }

                element = FindElement(element, className);
                if (element != null)
                {
                    return element;
                }
            }

            return null;
        }

        private static VisualElement LeftElement(string name)
        {
            var parent = new VisualElement
            {
                name = name,
                style =
                {
                    flexGrow = 1,
                    flexDirection = FlexDirection.RowReverse,
                }
            };
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
                    backgroundImage = Reflection.windowIcon.image as Texture2D,
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
            parent.Add(dropdown);
            SetButton(parent, Reflection.customIcon.image, EditorSetting.ShowWindow);
            SetButton(parent, Reflection.settingIcon.image, () => EditorApplication.ExecuteMenuItem("Edit/Project Settings..."));
            SetButton(parent, Reflection.buildIcon.image, () => EditorApplication.ExecuteMenuItem("File/Build Profiles"));
            return parent;
        }

        private static void SetButton(VisualElement parent, Texture texture, Action assetAction)
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
            parent.Add(button);
        }


        private static VisualElement RightElement(string name)
        {
            var parent = new VisualElement
            {
                name = name,
                style =
                {
                    flexGrow = 1,
                    flexDirection = FlexDirection.Row,
                }
            };

            toolbarMenu = new ToolbarMenu
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
            };

            var menuIcon = new VisualElement
            {
                style =
                {
                    width = 16,
                    height = 16,
                    backgroundImage = Reflection.sceneIcon.image as Texture2D,
                },
            };
            toolbarMenu.Insert(0, menuIcon);
            SetButton(parent, Reflection.packageIcon.image, () => EditorApplication.ExecuteMenuItem("Window/Package Management/Package Manager"));
            parent.Add(toolbarMenu);

            var assets = EditorPrefs.GetString(nameof(CacheScene));
            if (string.IsNullOrEmpty(assets))
            {
                assets = "{\"value\":[\"\", \"\", \"\", \"\", \"\"]}";
                EditorPrefs.SetString(nameof(CacheScene), assets);
            }

            scenes = JsonUtility.FromJson<CacheScene>(assets).value;
            SetToolbarMenu(Path.GetFileNameWithoutExtension(scenes[0]));
            return parent;
        }

        private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            if (EditorApplication.isPlaying) return;
            var assets = EditorPrefs.GetString(nameof(CacheScene));
            if (string.IsNullOrEmpty(assets))
            {
                assets = "{\"value\":[\"\", \"\", \"\", \"\", \"\"]}";
                EditorPrefs.SetString(nameof(CacheScene), assets);
            }

            scenes = JsonUtility.FromJson<CacheScene>(assets).value;

            if (scenes.Contains(scene.path))
            {
                scenes.Remove(scene.path);
            }
            else
            {
                scenes.RemoveAt(scenes.Count - 1);
            }

            scenes.Insert(0, scene.path);
            SetToolbarMenu(Path.GetFileNameWithoutExtension(scenes[0]));
            assets = JsonUtility.ToJson(new CacheScene(scenes));
            EditorPrefs.SetString(nameof(CacheScene), assets);
        }

        private static void SetToolbarMenu(string sceneName)
        {
            toolbarMenu.text = string.IsNullOrEmpty(sceneName) ? "Empty Scene" : sceneName;
            toolbarMenu.menu.ClearItems();
            for (var i = 0; i < scenes.Count; i++)
            {
                var index = i;
                toolbarMenu.menu.AppendAction(Path.GetFileNameWithoutExtension(scenes[index]), LoadScene);
                continue;

                void LoadScene(DropdownMenuAction action)
                {
                    try
                    {
                        if (EditorApplication.isPlaying) return;
                        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                        {
                            EditorSceneManager.OpenScene(scenes[index], OpenSceneMode.Single);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("打开 " + scenes[index] + " 场景失败!\n" + e);
                    }
                }
            }
        }

        [Serializable]
        private class CacheScene
        {
            public List<string> value;

            public CacheScene(List<string> value)
            {
                this.value = value;
            }
        }
    }
}