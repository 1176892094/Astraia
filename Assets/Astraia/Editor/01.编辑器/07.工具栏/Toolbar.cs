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

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Astraia
{
    internal static class Toolbar
    {
        private static List<Object> scenePaths => GlobalSetting.Instance.cacheAssets;

        [MainToolbarElement("Astraia/Preference Setting", defaultDockPosition = MainToolbarDockPosition.Left)]
        public static MainToolbarElement PreferenceSettings()
        {
            var content = new MainToolbarContent(EditorRef.projectIcon.image as Texture2D);
            return new MainToolbarButton(content, () => SettingsService.OpenUserPreferences());
        }

        [MainToolbarElement("Astraia/Build Setting", defaultDockPosition = MainToolbarDockPosition.Left)]
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
            var content = new MainToolbarContent("{0:F2}".Format(Time.timeScale), EditorRef.windowIcon.image as Texture2D, null);
            return new MainToolbarDropdown(content, ShowTimeScaleMenu);
        }

        private static void ShowTimeScaleMenu(Rect dropDownRect)
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("0.00"), false, () => UpdateTimeScale(0.00F));
            menu.AddItem(new GUIContent("0.25"), false, () => UpdateTimeScale(0.25F));
            menu.AddItem(new GUIContent("0.50"), false, () => UpdateTimeScale(0.50F));
            menu.AddItem(new GUIContent("0.75"), false, () => UpdateTimeScale(0.75F));
            menu.AddItem(new GUIContent("1.00"), false, () => UpdateTimeScale(1.00F));
            menu.AddItem(new GUIContent("1.50"), false, () => UpdateTimeScale(1.50F));
            menu.AddItem(new GUIContent("2.00"), false, () => UpdateTimeScale(2.00F));
            menu.AddItem(new GUIContent("2.50"), false, () => UpdateTimeScale(2.50F));
            menu.AddItem(new GUIContent("3.00"), false, () => UpdateTimeScale(3.00F));
            menu.DropDown(dropDownRect);
            return;

            void UpdateTimeScale(float timeScale)
            {
                Time.timeScale = timeScale;
                MainToolbar.Refresh("Astraia/Timescale");
            }
        }

        [MainToolbarElement("Astraia/Scene Selector", defaultDockPosition = MainToolbarDockPosition.Middle)]
        public static MainToolbarElement SceneSelector()
        {
            var sceneName = SceneManager.GetActiveScene().name;
            if (sceneName.Length == 0)
            {
                sceneName = "Untitled";
            }

            var content = new MainToolbarContent(sceneName, EditorRef.sceneIcon.image as Texture2D, null);
            return new MainToolbarDropdown(content, ShowDropdownMenu);
        }

        private static void ShowDropdownMenu(Rect dropDownRect)
        {
            var menu = new GenericMenu();
            if (scenePaths.Count == 0)
            {
                menu.AddDisabledItem(new GUIContent("No Scenes in Project"));
            }

            for (var index = 1; index < scenePaths.Count; index++)
            {
                if (scenePaths[index])
                {
                    var scenePath = AssetDatabase.GetAssetPath(scenePaths[index]);
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
                    Service.Log.Error("Scene '{0}' is not in the Build Settings.", sceneName);
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
                    Service.Log.Error("Scene at path '{0}' does not exist.", scenePath);
                }
            }
        }

        public static void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            if (!EditorApplication.isPlaying)
            {
                var sceneData = AssetDatabase.LoadAssetAtPath<Object>(scene.path);
                if (scenePaths.Contains(sceneData))
                {
                    scenePaths.Remove(sceneData);
                }

                scenePaths.Insert(0, sceneData);
                if (scenePaths.Count > 5)
                {
                    scenePaths.RemoveAt(scenePaths.Count - 1);
                }
            }
        }


        public static void ActiveSceneChanged(Scene oldScene, Scene newScene)
        {
            MainToolbar.Refresh("Astraia/Scene Selector");
        }
    }
}