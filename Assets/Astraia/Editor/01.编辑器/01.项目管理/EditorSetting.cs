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
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Astraia.Common;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using System.Threading.Tasks;
using Directory = System.IO.Directory;
using File = System.IO.File;
using Debug = UnityEngine.Debug;
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
                    var sinceTime = EditorApplication.timeSinceStartup;
                    EditorUtility.DisplayProgressBar("", "", 0);
                    AssetLoadKey = await FormManager.WriteScripts(folderPath);
                    var elapsedTime = EditorApplication.timeSinceStartup - sinceTime;
                    Debug.Log("自动生成脚本完成。耗时: {0}秒".Format(elapsedTime.ToString("F").Color("G")));
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
                    var sinceTime = EditorApplication.timeSinceStartup;
                    EditorUtility.DisplayProgressBar("", "", 0);
                    await FormManager.WriteAssets(ExcelPathKey);
                    var elapsedTime = EditorApplication.timeSinceStartup - sinceTime;
                    Debug.Log("自动生成资源完成。耗时: {0}秒".Format(elapsedTime.ToString("F").Color("G")));
                }
                finally
                {
                    AssetDatabase.Refresh();
                    EditorUtility.ClearProgressBar();
                }
            }

            DataManager.isLoaded = false;
            EditorApplication.delayCall -= DataManager.LoadDataTable;
            EditorApplication.delayCall += DataManager.LoadDataTable;
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
            var menuItem = new OdinMenuItem(menuTree, nameof(GlobalSetting), GlobalSetting.Instance)
            {
                Icon = EditorIcons.UnityFolderIcon
            };
            menuTree.MenuItems.Insert(0, menuItem);
            return menuTree;
        }

        public class BooleanDrawer : OdinValueDrawer<bool>
        {
            protected override void DrawPropertyLayout(GUIContent label)
            {
                GUILayout.BeginHorizontal();
                var value = ValueEntry.SmartValue;

                GUILayout.Label(label, GUILayout.Width(EditorGUIUtility.labelWidth - 4));

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
#endif
    }

    internal static class EditorBuilder
    {
        private static string ComputeMD5(string filePath)
        {
            using var md5 = MD5.Create();
            using var stream = File.OpenRead(filePath);
            var hash = md5.ComputeHash(stream);
            var sb = new StringBuilder(hash.Length * 2);
            foreach (var b in hash)
            {
                sb.Append(b.ToString("X2"));
            }

            return sb.ToString();
        }

        [MenuItem("Tools/Astraia/热更资源构建", priority = 3)]
        private static async void BuildAsset()
        {
            var startTime = EditorApplication.timeSinceStartup;
            var folderPath = Directory.CreateDirectory(GlobalSetting.RemoteAssetPath);
            BuildPipeline.BuildAssetBundles(GlobalSetting.RemoteAssetPath, BuildAssetBundleOptions.None, (BuildTarget)GlobalSetting.Instance.BuildTarget);

            var oldHashes = new Dictionary<string, string>();
            if (File.Exists(GlobalSetting.RemoteAssetData))
            {
                var readJson = await File.ReadAllTextAsync(GlobalSetting.RemoteAssetData);
                var oldData = JsonManager.FromJson<List<BundleData>>(readJson);
                oldHashes = oldData.ToDictionary(d => d.name, d => d.code);
            }

            var filePacks = new List<BundleData>();
            var fileInfos = folderPath.GetFiles();

            foreach (var fileInfo in fileInfos)
            {
                if (fileInfo.Extension != "")
                {
                    continue;
                }

                var md5Before = ComputeMD5(fileInfo.FullName);
                if (oldHashes.TryGetValue(fileInfo.Name, out var oldMd5) && md5Before == oldMd5)
                {
                    filePacks.Add(new BundleData(md5Before, fileInfo.Name, (int)fileInfo.Length));
                    Debug.Log("跳过未变更文件: {0}".Format(fileInfo.Name));
                    continue;
                }

                await Task.Run(() =>
                {
                    var bytes = File.ReadAllBytes(fileInfo.FullName);
                    bytes = Service.Xor.Encrypt(bytes);
                    File.WriteAllBytes(fileInfo.FullName, bytes);
                });

                var md5After = ComputeMD5(fileInfo.FullName);
                filePacks.Add(new BundleData(md5After, fileInfo.Name, (int)fileInfo.Length));
                Debug.Log("加密并更新文件: {0}".Color("G").Format(fileInfo.Name));
            }

            await File.WriteAllTextAsync(GlobalSetting.RemoteAssetData, JsonManager.ToJson(filePacks));
            var elapsed = EditorApplication.timeSinceStartup - startTime;
            Debug.Log("加密 AssetBundle 完成。耗时: <color=#00FF00>{0:F2}</color> 秒".Format(elapsed));
            AssetDatabase.Refresh();
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

    public class EditorProcess : AssetPostprocessor
    {
        private static int offset => GlobalSetting.Bundle.EndsWith("/") ? GlobalSetting.Bundle.Length : GlobalSetting.Bundle.Length + 1;

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (var path in importedAssets)
            {
                ProcessAsset(path);
            }

            for (int i = 0; i < movedAssets.Length; i++)
            {
                var newPath = movedAssets[i];
                var oldPath = movedFromAssetPaths[i];

                if (newPath.StartsWith(GlobalSetting.Bundle))
                {
                    ProcessAsset(newPath);
                }
                else if (oldPath.StartsWith(GlobalSetting.Bundle))
                {
                    var importer = AssetImporter.GetAtPath(newPath);
                    if (importer != null && !string.IsNullOrEmpty(importer.assetBundleName))
                    {
                        Debug.Log("移除 {0} 资源: {1}".Format(importer.assetBundleName, oldPath));
                        importer.assetBundleName = null;
                        importer.SaveAndReimport();
                    }
                }
            }

            AssetDatabase.RemoveUnusedAssetBundleNames();
            AssetDatabase.Refresh();
        }

        private static void ProcessAsset(string path)
        {
            if (!path.StartsWith(GlobalSetting.Bundle))
            {
                return;
            }

            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            var asset = AssetDatabase.LoadAssetAtPath<Object>(path);
            if (asset == null || asset is DefaultAsset)
            {
                return;
            }

            if (GlobalSetting.Instance.ignoreAssets.Contains(asset))
            {
                return;
            }

            if (asset is SceneAsset && !GlobalSetting.Instance.sceneAssets.Contains(asset))
            {
                GlobalSetting.Instance.sceneAssets.Add(asset);
            }

            var split = path.Substring(offset).TrimStart('/');
            var index = split.IndexOf('/');
            if (index >= 0)
            {
                var folder = split.Substring(0, index).ToLower();
                var importer = AssetImporter.GetAtPath(path);

                if (!string.Equals(importer.assetBundleName, folder, StringComparison.Ordinal))
                {
                    Debug.Log("设置 {0} 资源: {1}".Format(folder, path), asset);
                    importer.assetBundleName = folder;
                    importer.SaveAndReimport();
                }
            }
        }

        [InitializeOnLoadMethod]
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void RuntimeInitializeOnLoad()
        {
            var assetBundles = AssetDatabase.GetAllAssetBundleNames();
            foreach (var assetBundle in assetBundles)
            {
                var assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(assetBundle);
                foreach (var path in assetPaths)
                {
                    var split = path.Substring(offset).TrimStart('/');
                    var index = split.IndexOf('/');
                    if (index >= 0)
                    {
                        var folder = split.Substring(0, index);
                        var result = Path.GetFileNameWithoutExtension(path);
                        GlobalManager.assetPath["{0}/{1}".Format(folder, result)] = path;
                    }
                }
            }
        }
    }

    public abstract class EditorSingleton<T> : ScriptableObject where T : EditorSingleton<T>
    {
        private static T instance;

        public static T Instance
        {
            get
            {
                if (instance)
                {
                    return instance;
                }

                var name = "Assets/Editor/Resources/Settings/{0}.asset".Format(typeof(T).Name);
                instance = AssetDatabase.LoadAssetAtPath<T>(name);
                if (instance)
                {
                    return instance;
                }

                var path = Path.GetDirectoryName(name);
                if (!Directory.Exists(path) && !string.IsNullOrEmpty(path))
                {
                    Directory.CreateDirectory(path);
                }

                instance = CreateInstance<T>();
                AssetDatabase.CreateAsset(instance, name);
                AssetDatabase.Refresh();
                return instance;
            }
        }
    }
}