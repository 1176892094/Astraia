// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-04-09 18:04:34
// // # Recently: 2025-04-09 18:04:34
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using UnityEditor;

namespace Astraia
{
    [InitializeOnLoad]
    internal static class EditorManager
    {
        private static bool maximized;
        private static EditorWindow focusedWindow;
        public static event Action<EditorWindow> OnWindowFocused;
        public static event Action<EditorWindow> OnWindowMaximized;

        static EditorManager()
        {
            EditorApplication.update -= Update;
            EditorApplication.update += Update;
            EditorApplication.update -= TabPro.Update;
            EditorApplication.update += TabPro.Update;
            EditorApplication.update -= Favourite.Update;
            EditorApplication.update += Favourite.Update;
            EditorApplication.update -= EditorInput.Update;
            EditorApplication.update += EditorInput.Update;
            EditorApplication.hierarchyWindowItemOnGUI -= Hierarchy.OnGUI;
            EditorApplication.hierarchyWindowItemOnGUI += Hierarchy.OnGUI;
            EditorApplication.projectWindowItemInstanceOnGUI -= Folder.OnGUI;
            EditorApplication.projectWindowItemInstanceOnGUI += Folder.OnGUI;
            EditorApplication.projectChanged -= Folder.OnProjectChanged;
            EditorApplication.projectChanged += Folder.OnProjectChanged;
            focusedWindow = EditorWindow.focusedWindow;
            maximized = focusedWindow != null && focusedWindow.maximized;
        }

        private static void Update()
        {
            if (focusedWindow != EditorWindow.focusedWindow)
            {
                focusedWindow = EditorWindow.focusedWindow;
                OnWindowFocused?.Invoke(focusedWindow);
            }

            if (focusedWindow == null)
            {
                return;
            }

            if (maximized != focusedWindow.maximized)
            {
                maximized = focusedWindow.maximized;
                OnWindowMaximized?.Invoke(focusedWindow);
            }
        }
    }
}