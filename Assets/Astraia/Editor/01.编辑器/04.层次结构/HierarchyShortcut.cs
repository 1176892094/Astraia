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

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_6000_3_OR_NEWER
using TreeViewState = UnityEditor.IMGUI.Controls.TreeViewState<UnityEngine.EntityId>;
#elif UNITY_6000_2_OR_NEWER
using TreeViewState = UnityEditor.IMGUI.Controls.TreeViewState<int>;
#else
using TreeViewState = UnityEditor.IMGUI.Controls.TreeViewState;
#endif

namespace Astraia
{
    using static EventManager;

    internal static partial class Hierarchy
    {
        private static readonly Dictionary<int, bool> sceneExpandState = new Dictionary<int, bool>();
        private static readonly HashSet<int> expandedScenes = new HashSet<int>();

        public static void Shortcuts()
        {
            var window = EditorWindow.mouseOverWindow;
            if (!window)
            {
                return;
            }

            if (window.GetType() != EditorRef.Hierarchy)
            {
                return;
            }

            if (isExpand)
            {
                ExpandOrCompose(window);
            }

            if (isEscape)
            {
                copiedData.Clear();
            }
        }

        private static void ExpandOrCompose(EditorWindow window)
        {
            var scene = window.GetValue("m_SceneHierarchy");
            var tree = scene.GetValue("m_TreeView");
            var data = tree.GetValue("data");
            var state = tree.GetValue<TreeViewState>("state");

            var sceneActive = SceneManager.GetActiveScene();
            var sceneId = (EntityId)sceneActive.handle.GetHashCode();

            var isExpanded = sceneExpandState.ContainsKey(sceneId) && sceneExpandState[sceneId];

            if (isExpanded)
            {
                for (var i = state.expandedIDs.Count - 1; i >= 0; i--)
                {
                    var id = state.expandedIDs[i];
                    if (EditorUtility.EntityIdToObject(id) is GameObject)
                    {
                        data.Invoke("SetExpanded", id, false);
                    }
                }

                sceneExpandState[sceneId] = false;
            }
            else
            {
                if (!expandedScenes.Contains(sceneId))
                {
                    data.Invoke("SetExpanded", sceneId, true);
                    expandedScenes.Add(sceneId);
                }

                foreach (var root in sceneActive.GetRootGameObjects())
                {
                    var rootId = root.GetEntityId();
                    data.Invoke("SetExpanded", rootId, true);
                }

                sceneExpandState[sceneId] = true;
            }

            window.Repaint();
            Use();
        }
    }
}