// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-09-02 21:09:02
// # Recently: 2026-09-03 14:21:08
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using Object = UnityEngine.Object;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
#endif

namespace Astraia
{
    internal class EditorSetting
#if ODIN_INSPECTOR
        : OdinMenuEditorWindow
#endif
    {
        private static readonly Dictionary<Type, Object> windows = new Dictionary<Type, Object>();

        private static bool AssetLoadKey
        {
            get => EditorPrefs.GetBool(nameof(AssetLoadKey), false);
            set => EditorPrefs.SetBool(nameof(AssetLoadKey), value);
        }

        private static string ExcelPathKey
        {
            get => EditorPrefs.GetString(nameof(ExcelPathKey), Environment.CurrentDirectory);
            set => EditorPrefs.SetString(nameof(ExcelPathKey), value);
        }

        public static void LoadWindows(Type result)
        {
            if (!result.IsAbstract && !result.IsGenericType)
            {
                var parent = result.BaseType;
                if (parent != null)
                {
                    if (parent.IsGenericType && parent.GetGenericTypeDefinition() == typeof(EditorSingleton<>))
                    {
                        windows[result] = result.GetValue<ScriptableObject>("Instance");
                    }
                }
            }
        }

        [MenuItem("Tools/Astraia/表格数据导入", priority = 5)]
        private static async void ExcelToScripts()
        {
            var folderPath = ExcelPathKey;
            if (string.IsNullOrEmpty(folderPath))
            {
                folderPath = Environment.CurrentDirectory;
            }

            folderPath = EditorUtility.OpenFolderPanel("选择文件夹", folderPath, "");
            if (!string.IsNullOrEmpty(folderPath))
            {
                try
                {
                    AssetLoadKey = false;
                    ExcelPathKey = folderPath;
                    AssetLoadKey = await FormManager.WriteScripts(folderPath);
                }
                finally
                {
                    AssetDatabase.Refresh();
                    EditorUtility.ClearProgressBar();
                }
            }
        }

        [DidReloadScripts]
        private static async void CompileScripts()
        {
            if (AssetLoadKey)
            {
                try
                {
                    AssetLoadKey = false;
                    EditorUtility.DisplayProgressBar("", "", 0);
                    await FormManager.WriteAssets(ExcelPathKey);
                }
                finally
                {
                    AssetDatabase.Refresh();
                    EditorUtility.ClearProgressBar();
                }
            }
            else
            {
                EditorApplication.delayCall += DataManager.LoadDataTable;
            }
        }

        [MenuItem("Tools/Astraia/框架配置窗口 _F1", priority = 2)]
        public static void ShowWindow()
        {
#if ODIN_INSPECTOR
            var window = GetWindow<EditorSetting>();
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(1000, 600);
            window.Show();
#endif
        }
#if ODIN_INSPECTOR
        protected override OdinMenuTree BuildMenuTree()
        {
            var menuTree = new OdinMenuTree();
            foreach (var window in windows)
            {
                menuTree.Add(window.Key.Name, window.Value, EditorIcons.UnityFolderIcon);
            }

            menuTree.SortMenuItemsByName();
            var menuItem = new OdinMenuItem(menuTree, nameof(GlobalSetting), GlobalSetting.Instance) { Icon = EditorIcons.UnityFolderIcon };
            menuTree.MenuItems.Insert(0, menuItem);
            return menuTree;
        }

        private class BooleanDrawer : OdinValueDrawer<bool>
        {
            protected override void DrawPropertyLayout(GUIContent label)
            {
                GUILayout.BeginHorizontal();
                var value = ValueEntry.SmartValue;

                EditorGUILayout.LabelField(label, GUILayout.Width(EditorGUIUtility.labelWidth));

                var color = GUI.backgroundColor;
                GUI.backgroundColor = value ? Color.green : color * 0.8f;
                if (GUILayout.Button("Yes", SirenixGUIStyles.ButtonLeft))
                {
                    ValueEntry.SmartValue = true;
                }

                GUI.backgroundColor = !value ? Color.yellow : color * 0.8f;
                if (GUILayout.Button("No", SirenixGUIStyles.ButtonRight))
                {
                    ValueEntry.SmartValue = false;
                }

                GUI.backgroundColor = color;
                GUILayout.EndHorizontal();
            }
        }

        private class FixationDrawer : OdinValueDrawer<Fixation>
        {
            protected override void DrawPropertyLayout(GUIContent label)
            {
                GUILayout.BeginHorizontal();
                var value = ValueEntry.SmartValue;
                var field = SirenixEditorFields.FloatField(label, value);
                ValueEntry.SmartValue = field;
                GUILayout.EndHorizontal();
            }
        }

        private class PositionDrawer : OdinValueDrawer<Position>
        {
            protected override void DrawPropertyLayout(GUIContent label)
            {
                GUILayout.BeginHorizontal();
                var value = ValueEntry.SmartValue;
                var field = SirenixEditorFields.Vector4Field(label, new Vector4(value.x, value.y, value.x.value, value.y.value));
                ValueEntry.SmartValue = new Position(field.x, field.y);
                GUILayout.EndHorizontal();
            }
        }
#endif
    }
}