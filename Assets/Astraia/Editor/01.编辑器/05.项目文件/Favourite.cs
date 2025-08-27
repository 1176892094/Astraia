// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-08-27 19:08:35
// // # Recently: 2025-08-27 19:08:35
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;

namespace Astraia
{
    using static Reflection;
    using static EditorInput;

    public static class Favourite
    {
        private static readonly MethodInfo OnGUIUpdate = typeof(Favourite).GetMethod(nameof(OnUpdateGUI), FLAGS);
        private static Delegate OnGUI;
        private static EditorWindow Instance;

        public static void Update()
        {
            if (Instance)
            {
                if (Instance.hasFocus)
                {
                    if (isAlt)
                    {
                        return;
                    }
                }

                HideGUI();
                Instance.Repaint();
                Instance = null;
            }
            else
            {
                if (!InternalEditorUtility.isApplicationActive) //用来判断 Unity 编辑器窗口是否处于激活状态
                {
                    return;
                }

                if (!isAlt)
                {
                    return;
                }

                var window = EditorWindow.mouseOverWindow; //用于获取 当前鼠标悬停的编辑器窗口
                if (!window)
                {
                    return;
                }

                if (window.GetType() != Browser)
                {
                    return;
                }

                if (window.GetValue<bool>("isLocked"))
                {
                    return;
                }

                ShowGUI(window);

                if (!Instance)
                {
                    return;
                }

                Instance.Focus();
                Instance.Repaint();
                Browser.SetValue("s_LastInteractedProjectBrowser", Instance);
            }
        }

        private static void ShowGUI(EditorWindow window)
        {
            var parent = window.GetValue("m_Parent");
            OnGUI = parent.GetValue<Delegate>("m_OnGUI");
            parent.SetValue("m_OnGUI", OnGUIUpdate.CreateDelegate(GUIDelegate, window));
            Instance = window;
        }

        private static void HideGUI()
        {
            var parent = Instance.GetValue("m_Parent");
            if (parent.GetValue<Delegate>("m_OnGUI").Method == OnGUIUpdate)
            {
                parent.SetValue("m_OnGUI", OnGUI);
            }
        }

        public static void OnUpdateGUI(object window)
        {
        }
    }
}