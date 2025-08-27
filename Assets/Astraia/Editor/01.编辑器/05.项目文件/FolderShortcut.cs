// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-08-28 01:08:19
// // # Recently: 2025-08-28 01:08:19
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System.Collections.Generic;
using UnityEditor;
#if UNITY_6000_2_OR_NEWER
using TreeViewState = UnityEditor.IMGUI.Controls.TreeViewState<int>;
#endif

namespace Astraia
{
    using static Reflection;
    using static EditorInput;

    internal static partial class Folder
    {
        public static void Shortcuts()
        {
            var window = EditorWindow.mouseOverWindow;
            if (!window)
            {
                return;
            }

            if (window.GetType() != Browser)
            {
                return;
            }

            if (isShiftE)
            {
                Expand(window);
                window.Repaint();
                Use();
            }

            if (isShiftR)
            {
                Expand(window, 1);
                window.Repaint();
                Use();
            }
        }

        private static void Expand(EditorWindow window, int index = 0)
        {
            var mode = window.GetValue<int>("m_ViewMode") == 0;
            var tree = window.GetValue(mode ? "m_AssetTree" : "m_FolderTree");
            var data = tree.GetValue("data");
            var item = tree.GetValue<TreeViewState>("state").expandedIDs;

            var roots = new HashSet<int>();
            foreach (var id in item)
            {
                var path = AssetDatabase.GetAssetPath(EditorUtility.InstanceIDToObject(id));

                if (path.LastIndexOf('/') > 0)
                {
                    if (index == 0)
                    {
                        if (path.Substring(0, path.LastIndexOf('/')) is "Assets" or "Packages")
                        {
                            roots.Add(id);
                        }
                    }
                    else
                    {
                        if (path is "Assets" or "Packages")
                        {
                            roots.Add(id);
                        }
                    }
                }
            }

            for (int i = item.Count - 1; i >= 0; i--)
            {
                var id = item[i];
                var path = AssetDatabase.GetAssetPath(EditorUtility.InstanceIDToObject(id));
                if (!path.IsNullOrEmpty())
                {
                    if (path != "Assets" && path != "Packages" && !roots.Contains(id))
                    {
                        data.Invoke("SetExpanded", id, false);
                    }
                }
            }
        }
    }
}