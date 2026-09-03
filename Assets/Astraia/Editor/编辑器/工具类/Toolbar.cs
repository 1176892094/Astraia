// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-09-02 18:09:26
// # Recently: 2026-09-03 14:21:08
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

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
        private static Object[] scenePaths => GlobalSetting.Instance.sceneAssets;

        [MainToolbarElement("Astraia/Preference Setting", defaultDockPosition = MainToolbarDockPosition.Left)]
        public static MainToolbarElement PreferenceSettings()
        {
            var content = new MainToolbarContent(EditorReference.projectIcon.image as Texture2D);
            return new MainToolbarButton(content, () => SettingsService.OpenUserPreferences());
        }

        [MainToolbarElement("Astraia/Build Setting", defaultDockPosition = MainToolbarDockPosition.Left)]
        public static MainToolbarElement BuildSettings()
        {
            var content = new MainToolbarContent(EditorReference.buildIcon.image as Texture2D);

            return new MainToolbarButton(content, () => EditorApplication.ExecuteMenuItem("File/Build Profiles"));
        }

        [MainToolbarElement("Astraia/Framework Setting", defaultDockPosition = MainToolbarDockPosition.Right)]
        public static MainToolbarElement FrameworkSettings()
        {
            var content = new MainToolbarContent(EditorReference.customIcon.image as Texture2D);
            return new MainToolbarButton(content, EditorSetting.ShowWindow);
        }

        [MainToolbarElement("Astraia/Project Setting", defaultDockPosition = MainToolbarDockPosition.Right)]
        public static MainToolbarElement ProjectSettings()
        {
            var content = new MainToolbarContent(EditorReference.settingIcon.image as Texture2D);
            return new MainToolbarButton(content, () => SettingsService.OpenProjectSettings());
        }

        [MainToolbarElement("Astraia/Timescale", defaultDockPosition = MainToolbarDockPosition.Middle)]
        public static MainToolbarElement Timescale()
        {
            var content = new MainToolbarContent("{0:F2}".Format(Time.timeScale), EditorReference.windowIcon.image as Texture2D, null);
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

            var content = new MainToolbarContent(sceneName, EditorReference.sceneIcon.image as Texture2D, null);
            return new MainToolbarDropdown(content, ShowDropdownMenu);
        }

        private static void ShowDropdownMenu(Rect dropDownRect)
        {
            var menu = new GenericMenu();
            for (var i = 1; i < scenePaths.Length; i++)
            {
                var scene = scenePaths[i];
                if (scene)
                {
                    var scenePath = AssetDatabase.GetAssetPath(scene);
                    var sceneName = Path.GetFileNameWithoutExtension(scenePath);
                    menu.AddItem(new GUIContent(sceneName), false, () => SwitchScene(scenePath));
                }
            }

            if (menu.GetItemCount() == 0)
            {
                menu.AddDisabledItem(new GUIContent("Empty"));
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
                    Log.Error($"Scene '{sceneName}' is not in the Build Settings.");
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
                    Log.Error($"Scene at path '{scenePath}' does not exist.");
                }
            }
        }

        public static void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            if (!EditorApplication.isPlaying)
            {
                var sceneData = AssetDatabase.LoadAssetAtPath<Object>(scene.path);

                for (var index = 0; index < scenePaths.Length; index++)
                {
                    if (scenePaths[index] == sceneData)
                    {
                        for (var i = index; i > 0; i--)
                        {
                            scenePaths[i] = scenePaths[i - 1];
                        }

                        scenePaths[0] = sceneData;
                        return;
                    }
                }

                for (var index = scenePaths.Length - 1; index > 0; index--)
                {
                    scenePaths[index] = scenePaths[index - 1];
                }

                scenePaths[0] = sceneData;
            }
        }

        public static void ActiveSceneChanged(Scene oldScene, Scene newScene)
        {
            MainToolbar.Refresh("Astraia/Scene Selector");
        }
    }
}