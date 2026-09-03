// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-09-02 19:09:22
// # Recently: 2026-09-02 21:22:59
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Astraia
{
    using EventHandler = EditorApplication.CallbackFunction;

    [InitializeOnLoad]
    internal static class EditorManager
    {
        private static EditorWindow focusedWindow;
        private static bool isMaximized;

        static EditorManager()
        {
            Search.OnLoad -= EditorSetting.LoadWindows;
            Search.OnLoad += EditorSetting.LoadWindows;

            EditorApplication.update -= Update;
            EditorApplication.update += Update;

#if UNITY_6000_4_OR_NEWER
            EditorApplication.hierarchyWindowItemByEntityIdOnGUI -= Hierarchy.OnGUI;
            EditorApplication.hierarchyWindowItemByEntityIdOnGUI += Hierarchy.OnGUI;
            EditorApplication.projectWindowItemByEntityIdOnGUI -= Folder.OnGUI;
            EditorApplication.projectWindowItemByEntityIdOnGUI += Folder.OnGUI;
#else
            EditorApplication.hierarchyWindowItemOnGUI -= Hierarchy.OnGUI;
            EditorApplication.hierarchyWindowItemOnGUI += Hierarchy.OnGUI;
            EditorApplication.projectWindowItemInstanceOnGUI -= Folder.OnGUI;
            EditorApplication.projectWindowItemInstanceOnGUI += Folder.OnGUI;
#endif
            EditorApplication.projectChanged -= Folder.OnProjectChanged;
            EditorApplication.projectChanged += Folder.OnProjectChanged;

            Selection.selectionChanged -= Inspector.SelectionChanged;
            Selection.selectionChanged += Inspector.SelectionChanged;
            EditorApplication.delayCall -= Inspector.SelectionChanged;
            EditorApplication.delayCall += Inspector.SelectionChanged;

            SceneManager.activeSceneChanged -= Toolbar.ActiveSceneChanged;
            SceneManager.activeSceneChanged += Toolbar.ActiveSceneChanged;
            EditorSceneManager.sceneOpened -= Toolbar.OnSceneOpened;
            EditorSceneManager.sceneOpened += Toolbar.OnSceneOpened;
            EditorSceneManager.activeSceneChangedInEditMode -= Toolbar.ActiveSceneChanged;
            EditorSceneManager.activeSceneChangedInEditMode += Toolbar.ActiveSceneChanged;

            focusedWindow = EditorWindow.focusedWindow;
            isMaximized = focusedWindow && focusedWindow.maximized;

            var eventHandler = typeof(EditorApplication).GetValue<EventHandler>("globalEventHandler");
            eventHandler = Folder.Shortcuts + (eventHandler - Folder.Shortcuts);
            eventHandler = Hierarchy.Shortcuts + (eventHandler - Hierarchy.Shortcuts);
            eventHandler = Inspector.Shortcuts + (eventHandler - Inspector.Shortcuts);
            typeof(EditorApplication).SetValue("globalEventHandler", eventHandler);
        }

        private static void Update()
        {
            if (focusedWindow != EditorWindow.focusedWindow)
            {
                focusedWindow = EditorWindow.focusedWindow;
                if (focusedWindow)
                {
                    if (focusedWindow.GetType() == EditorReference.Inspector)
                    {
                        Inspector.InitWindow(focusedWindow);
                    }
                }
            }

            if (focusedWindow)
            {
                if (isMaximized != focusedWindow.maximized)
                {
                    isMaximized = focusedWindow.maximized;
                    if (focusedWindow.GetType() == EditorReference.Inspector)
                    {
                        Inspector.InitWindow(focusedWindow);
                    }
                }
            }
        }
    }
}