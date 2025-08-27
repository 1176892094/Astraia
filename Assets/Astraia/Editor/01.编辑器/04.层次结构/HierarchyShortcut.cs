// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-08-28 02:08:29
// // # Recently: 2025-08-28 02:08:29
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using UnityEditor;
using UnityEngine;
#if UNITY_6000_2_OR_NEWER
using TreeViewState = UnityEditor.IMGUI.Controls.TreeViewState<int>;
#endif

namespace Astraia
{
    using static EditorInput;

    internal static partial class Hierarchy
    {
        public static void Shortcuts()
        {
            var window = EditorWindow.mouseOverWindow;
            if (!window)
            {
                return;
            }

            if (window.GetType() != Reflection.Hierarchy)
            {
                return;
            }

            if (isShiftE)
            {
                Expand(window);
            }

            if (isShiftR)
            {
                Expand(window, 1);
            }

            if (isEscape)
            {
                copiedData.Clear();
            }
        }

        private static void Expand(EditorWindow window, int index = 0)
        {
            var scene = window.GetValue("m_SceneHierarchy");
            var tree = scene.GetValue("m_TreeView");
            var data = tree.GetValue("data");
            var item = tree.GetValue<TreeViewState>("state").expandedIDs;

            for (int i = item.Count - 1; i >= 0; i--)
            {
                var id = item[i];
                if (EditorUtility.InstanceIDToObject(id) is GameObject o)
                {
                    if (index == 0)
                    {
                        if (o.transform.parent)
                        {
                            data.Invoke("SetExpanded", id, false);
                        }
                    }
                    else
                    {
                        data.Invoke("SetExpanded", id, false);
                    }
                }
            }
        }
    }
}