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
    internal static class Toolbar
    {
        private static List<string> scenePaths;

        static Toolbar()
        {
            OnProjectChanged();
        }

        public static void OnProjectChanged()
        {
            var assets = EditorPrefs.GetString(nameof(CacheScene));
            if (string.IsNullOrEmpty(assets))
            {
                assets = "{\"value\":[\"\", \"\", \"\", \"\", \"\"]}";
                EditorPrefs.SetString(nameof(CacheScene), assets);
            }

            scenePaths = JsonUtility.FromJson<CacheScene>(assets).value;
        }
#if UNITY_6000_3_OR_NEWER
        [MainToolbarElement("Astraia/Preference Setting", defaultDockPosition = MainToolbarDockPosition.Right)]
        public static MainToolbarElement PreferenceSettings()
        {
            var content = new MainToolbarContent(EditorRef.projectIcon.image as Texture2D);
            return new MainToolbarButton(content, () => SettingsService.OpenUserPreferences());
        }

        [MainToolbarElement("Astraia/Build Setting", defaultDockPosition = MainToolbarDockPosition.Right)]
        public static MainToolbarElement BuildSettings()
        {
            var content = new MainToolbarContent(EditorRef.buildIcon.image as Texture2D);

            return new MainToolbarButton(content, () => EditorApplication.ExecuteMenuItem("File/Build Profiles"));
        }

        [MainToolbarElement("Astraia/Framework Setting", defaultDockPosition = MainToolbarDockPosition.Right)]
        public static MainToolbarElement FrameworkSettings()
        {
            var content = new MainToolbarContent(EditorRef.customIcon.image as Texture2D);
            return new MainToolbarButton(content, EditorSetting.ShowWindow);
        }

        [MainToolbarElement("Astraia/Project Setting", defaultDockPosition = MainToolbarDockPosition.Right)]
        public static MainToolbarElement ProjectSettings()
        {
            var content = new MainToolbarContent(EditorRef.settingIcon.image as Texture2D);
            return new MainToolbarButton(content, () => SettingsService.OpenProjectSettings());
        }


        [MainToolbarElement("Astraia/Timescale", defaultDockPosition = MainToolbarDockPosition.Middle)]
        public static MainToolbarElement Timescale()
        {
            var content = new MainToolbarContent(EditorRef.windowIcon.image as Texture2D);
            return new MainToolbarSlider(content, 1, 0, 1, s => Time.timeScale = s);
        }

        [MainToolbarElement("Astraia/Scene Selector", defaultDockPosition = MainToolbarDockPosition.Middle)]
        public static MainToolbarElement SceneSelector()
        {
            var sceneName = SceneManager.GetActiveScene().name;
            if (sceneName.Length == 0)
            {
                sceneName = "Untitled";
            }

            var content = new MainToolbarContent(sceneName, EditorRef.sceneIcon.image as Texture2D, "Select active scene");
            return new MainToolbarDropdown(content, ShowDropdownMenu);
        }

        private static void ShowDropdownMenu(Rect dropDownRect)
        {
            var menu = new GenericMenu();
            if (scenePaths.Count == 0)
            {
                menu.AddDisabledItem(new GUIContent("No Scenes in Project"));
            }

            foreach (var scenePath in scenePaths)
            {
                if (!string.IsNullOrEmpty(scenePath))
                {
                    var sceneName = Path.GetFileNameWithoutExtension(scenePath);
                    menu.AddItem(new GUIContent(sceneName), false, () => SwitchScene(scenePath));
                }
            }

            menu.DropDown(dropDownRect);
        }

        private static void SwitchScene(string scenePath)
        {
            if (Application.isPlaying)
            {
                var sceneName = Path.GetFileNameWithoutExtension(scenePath);
                if (Application.CanStreamedLevelBeLoaded(sceneName))
                {
                    SceneManager.LoadScene(sceneName);
                }
                else
                {
                    Debug.LogError($"Scene '{sceneName}' is not in the Build Settings.");
                }
            }
            else
            {
                if (File.Exists(scenePath))
                {
                    if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    {
                        EditorSceneManager.OpenScene(scenePath);
                    }
                }
                else
                {
                    Debug.LogError($"Scene at path '{scenePath}' does not exist.");
                }
            }
        }
#endif
        public static void ActiveSceneChanged(Scene oldScene, Scene newScene)
        {
            //  MainToolbar.Refresh("Astraia/Scene Selector");
        }

        public static void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            if (!EditorApplication.isPlaying)
            {
                var assets = EditorPrefs.GetString(nameof(CacheScene));
                if (string.IsNullOrEmpty(assets))
                {
                    assets = "{\"value\":[\"\", \"\", \"\", \"\", \"\"]}";
                    EditorPrefs.SetString(nameof(CacheScene), assets);
                }

                scenePaths = JsonUtility.FromJson<CacheScene>(assets).value;

                if (scenePaths.Contains(scene.path))
                {
                    scenePaths.Remove(scene.path);
                }
                else
                {
                    scenePaths.RemoveAt(scenePaths.Count - 1);
                }

                scenePaths.Insert(0, scene.path);
                assets = JsonUtility.ToJson(new CacheScene(scenePaths));
                EditorPrefs.SetString(nameof(CacheScene), assets);
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
            var dropdown = new ToolbarMenu()
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
                    backgroundImage = EditorRef.sceneIcon.image as Texture2D,
                },
            };
            dropdown.Insert(0, menuIcon);
            SetToolbarMenu(dropdown);
            dropdown.RegisterCallback<MouseEnterEvent>(_ => dropdown.style.backgroundColor = Color.white * 0.6f);
            dropdown.RegisterCallback<MouseLeaveEvent>(_ => dropdown.style.backgroundColor = Color.white * 0.5f);
            return dropdown;
        }

        private static void SetToolbarMenu(ToolbarMenu toolbarMenu)
        {
            var sceneName = Path.GetFileNameWithoutExtension(scenePaths[0]);
            toolbarMenu.text = string.IsNullOrEmpty(sceneName) ? "Empty Scene" : sceneName;
            toolbarMenu.menu.ClearItems();
            for (var i = 0; i < scenePaths.Count; i++)
            {
                var index = i;
                toolbarMenu.menu.AppendAction(Path.GetFileNameWithoutExtension(scenePaths[index]), LoadScene);
                continue;

                void LoadScene(DropdownMenuAction action)
                {
                    try
                    {
                        if (EditorApplication.isPlaying) return;
                        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                        {
                            EditorSceneManager.OpenScene(scenePaths[index], OpenSceneMode.Single);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("打开 " + scenePaths[index] + " 场景失败!\n" + e);
                    }
                }
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