// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-04-09 23:04:12
// // # Recently: 2025-04-09 23:04:12
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.Windows;
using Debug = UnityEngine.Debug;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
#endif


namespace Astraia
{
    internal partial class EditorSetting
#if ODIN_INSPECTOR
        : OdinMenuEditorWindow
#endif
    {
        public static readonly Dictionary<Type, ScriptableObject> windows = new Dictionary<Type, ScriptableObject>();

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

        [MenuItem("Tools/Astraia/框架配置窗口 _F1", priority = 2)]
        public static void ShowWindow()
        {
#if ODIN_INSPECTOR
            var window = GetWindow<EditorSetting>();
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(800, 600);
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
            var menuItem = new OdinMenuItem(menuTree, nameof(GlobalSetting), GlobalSetting.Instance)
            {
                Icon = EditorIcons.UnityFolderIcon
            };
            menuTree.MenuItems.Insert(0, menuItem);
            return menuTree;
        }
#endif
        [MenuItem("Tools/Astraia/转化表格数据", priority = 5)]
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
                    var sinceTime = EditorApplication.timeSinceStartup;
                    EditorUtility.DisplayProgressBar("", "", 0);
                    AssetLoadKey = await FormManager.WriteScripts(folderPath);
                    if (!AssetLoadKey)
                    {
                        UpdateAsset();
                    }

                    var elapsedTime = EditorApplication.timeSinceStartup - sinceTime;
                    Debug.Log(Service.Text.Format("自动生成脚本完成。耗时: {0}秒", elapsedTime.ToString("F").Color("G")));
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
                    var sinceTime = EditorApplication.timeSinceStartup;
                    EditorUtility.DisplayProgressBar("", "", 0);
                    AssetLoadKey = false;
                    await FormManager.WriteAssets(ExcelPathKey);
                    UpdateAsset();
                    var elapsedTime = EditorApplication.timeSinceStartup - sinceTime;
                    Debug.Log(Service.Text.Format("自动生成资源完成。耗时: {0}秒", elapsedTime.ToString("F").Color("G")));
                }
                finally
                {
                    AssetDatabase.Refresh();
                    EditorUtility.ClearProgressBar();
                }
            }
        }

        [MenuItem("Tools/Astraia/项目工程路径", priority = 6)]
        private static void ProjectDirectories() => Process.Start(Environment.CurrentDirectory);


        [MenuItem("Tools/Astraia/脚本编译路径", priority = 7)]
        private static void AssemblyDefinitionPath()
        {
            if (!Directory.Exists(Environment.CurrentDirectory + "/Library/ScriptAssemblies"))
            {
                Directory.CreateDirectory(Environment.CurrentDirectory + "/Library/ScriptAssemblies");
                AssetDatabase.Refresh();
            }

            Process.Start(Environment.CurrentDirectory + "/Library/ScriptAssemblies");
        }

        [MenuItem("Tools/Astraia/持久存储路径", priority = 8)]
        private static void PersistentDataPath() => Process.Start(Application.persistentDataPath);

        [MenuItem("Tools/Astraia/流动资源路径", priority = 9)]
        private static void StreamingAssetPath()
        {
            if (!Directory.Exists(Application.streamingAssetsPath))
            {
                Directory.CreateDirectory(Application.dataPath + "/StreamingAssets");
                AssetDatabase.Refresh();
            }

            Process.Start(Application.streamingAssetsPath);
        }
    }
}