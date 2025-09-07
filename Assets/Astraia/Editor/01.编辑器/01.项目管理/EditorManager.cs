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
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Astraia
{
    [InitializeOnLoad]
    internal static class EditorManager
    {
        private static bool wasAlt;
        private static bool wasShift;
        private static bool isMaximized;
        private static Event inputEventLast;
        private static EditorWindow focusedWindow;
        private static EditorWindow mouseOverWindow;

        static EditorManager()
        {
            EditorApplication.update -= Update;
            EditorApplication.update += Update;
            EditorApplication.update -= TabPro.Update;
            EditorApplication.update += TabPro.Update;
            EditorApplication.hierarchyWindowItemOnGUI -= Hierarchy.OnGUI;
            EditorApplication.hierarchyWindowItemOnGUI += Hierarchy.OnGUI;
            EditorApplication.projectWindowItemInstanceOnGUI -= Folder.OnGUI;
            EditorApplication.projectWindowItemInstanceOnGUI += Folder.OnGUI;
            EditorApplication.projectChanged -= Folder.OnProjectChanged;
            EditorApplication.projectChanged += Folder.OnProjectChanged;
            
            Selection.selectionChanged -= Toolbar.SelectionChanged;
            Selection.selectionChanged += Toolbar.SelectionChanged;
            Selection.selectionChanged -= Inspector.SelectionChanged;
            Selection.selectionChanged += Inspector.SelectionChanged;
            
            EditorApplication.delayCall -= Toolbar.SelectionChanged;
            EditorApplication.delayCall += Toolbar.SelectionChanged;
            EditorApplication.delayCall -= Inspector.SelectionChanged;
            EditorApplication.delayCall += Inspector.SelectionChanged;
            
            EditorSceneManager.sceneOpened -= Toolbar.OnSceneOpened;
            EditorSceneManager.sceneOpened += Toolbar.OnSceneOpened;
            

            focusedWindow = EditorWindow.focusedWindow;
            isMaximized = focusedWindow && focusedWindow.maximized;

            var eventHandler = typeof(EditorApplication).GetValue<EditorApplication.CallbackFunction>("globalEventHandler");
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
                    if (focusedWindow.GetType() == Reflection.Inspector)
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
                    if (focusedWindow.GetType() == Reflection.Inspector)
                    {
                        Inspector.InitWindow(focusedWindow);
                    }
                }
            }

            inputEventLast = typeof(Event).GetValue<Event>("s_Current");
            mouseOverWindow = EditorWindow.mouseOverWindow;
            if (mouseOverWindow)
            {
                if (wasAlt && !inputEventLast.alt)
                {
                    if (mouseOverWindow.GetType() == Reflection.Browser)
                    {
                        mouseOverWindow.Repaint();
                    }
                }

                if (wasShift && !inputEventLast.shift)
                {
                    if (mouseOverWindow.GetType() == Reflection.Hierarchy)
                    {
                        mouseOverWindow.Repaint();
                    }
                }
            }

            wasAlt = inputEventLast.alt;
            wasShift = inputEventLast.shift;
        }
    }
}