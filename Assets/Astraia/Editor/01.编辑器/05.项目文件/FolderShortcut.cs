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

using UnityEditor;
using UnityEngine;
#if UNITY_6000_3_OR_NEWER
using TreeViewState = UnityEditor.IMGUI.Controls.TreeViewState<UnityEngine.EntityId>;
#elif UNITY_6000_2_OR_NEWER
using TreeViewState = UnityEditor.IMGUI.Controls.TreeViewState<int>;
#else
using TreeViewState = UnityEditor.IMGUI.Controls.TreeViewState;
#endif

namespace Astraia
{
    using static EditorRef;
    using static EventManager;

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

            if (isExpand)
            {
                ExpandOrCompose(window);
                window.Repaint();
                Use();
            }
        }

        private static void ExpandOrCompose(EditorWindow window)
        {
            var mode = window.GetValue<int>("m_ViewMode") == 0;
            var tree = window.GetValue(mode ? "m_AssetTree" : "m_FolderTree");
            var data = tree.GetValue("data");
            var state = tree.GetValue<TreeViewState>("state");

            string[] roots = { "Assets", "Packages" };

            foreach (var root in roots)
            {
                var rootObj = AssetDatabase.LoadAssetAtPath<Object>(root);
                if (rootObj == null) continue;

                int rootId = rootObj.GetInstanceID();
                if (!state.expandedIDs.Contains(rootId))
                {
                    data.Invoke("SetExpanded", rootId, true);
                }

                var subDirs = AssetDatabase.GetSubFolders(root);
                var hasChild = false;

                foreach (var subDir in subDirs)
                {
                    var subObj = AssetDatabase.LoadAssetAtPath<Object>(subDir);
                    if (subObj != null)
                    {
                        if (state.expandedIDs.Contains(subObj.GetInstanceID()))
                        {
                            hasChild = true;
                            break;
                        }
                    }
                }

                if (!hasChild)
                {
                    foreach (var subDir in subDirs)
                    {
                        var subObj = AssetDatabase.LoadAssetAtPath<Object>(subDir);
                        if (subObj != null)
                        {
                            data.Invoke("SetExpanded", subObj.GetInstanceID(), true);
                        }
                    }
                }
                else
                {
                    foreach (var subDir in subDirs)
                    {
                        CollapseRecursive(data, subDir);
                    }
                }
            }

            window.Repaint();
            Use();
        }

        private static void CollapseRecursive(object data, string folder)
        {
            var subObj = AssetDatabase.LoadAssetAtPath<Object>(folder);
            if (subObj != null)
            {
                data.Invoke("SetExpanded", subObj.GetInstanceID(), false);
                var subDirs = AssetDatabase.GetSubFolders(folder);
                foreach (var subDir in subDirs)
                {
                    CollapseRecursive(data, subDir);
                }
            }
        }
    }
}