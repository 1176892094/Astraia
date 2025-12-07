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
using UnityEngine;
using UnityEngine.SceneManagement;

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

        public static void ActiveSceneChanged(Scene oldScene, Scene newScene)
        {
            MainToolbar.Refresh("Astraia/Scene Selector");
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