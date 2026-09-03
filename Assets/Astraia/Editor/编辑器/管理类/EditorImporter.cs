// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-09-02 21:09:47
// # Recently: 2026-09-02 21:22:59
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace Astraia
{
    internal class EditorImporter : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (var path in importedAssets)
            {
                ImportAsset(path);
            }

            for (int i = 0; i < movedAssets.Length; i++)
            {
                var newPath = movedAssets[i];
                var oldPath = movedFromAssetPaths[i];

                if (newPath.StartsWith(GlobalSetting.BUNDLE))
                {
                    ImportAsset(newPath);
                }
                else if (oldPath.StartsWith(GlobalSetting.BUNDLE))
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

        private static void ImportAsset(string path)
        {
            if (!path.StartsWith(GlobalSetting.BUNDLE))
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

            var count = GlobalSetting.BUNDLE.Length + 1;
            if (GlobalSetting.BUNDLE.EndsWith("/"))
            {
                count = GlobalSetting.BUNDLE.Length;
            }

            var value = path.Substring(count).TrimStart('/');
            var index = value.IndexOf('/');
            if (index >= 0)
            {
                var folder = value.Substring(0, index).ToLower();
                var importer = AssetImporter.GetAtPath(path);

                if (!string.Equals(importer.assetBundleName, folder, StringComparison.Ordinal))
                {
                    Debug.Log("设置 {0} 资源: {1}".Format(folder, path), asset);
                    importer.assetBundleName = folder;
                    importer.SaveAndReimport();
                }
            }
        }
    }

    internal static class EditorBuilder
    {
        [MenuItem("Tools/Astraia/热更资源构建", priority = 3)]
        private static void BuildAsset()
        {
            var watch = Stopwatch.StartNew();
            var build = Directory.CreateDirectory(GlobalSetting.BuildTargetPath);

            Directory.CreateDirectory(GlobalSetting.BuildVersion);
            BuildPipeline.BuildAssetBundles(GlobalSetting.BuildTargetPath, GlobalSetting.Instance.BuildOptions, (BuildTarget)GlobalSetting.Instance.BuildTarget);

            var package = new Package(GlobalSetting.Instance.AssetVersion, new List<Bundle>());
            foreach (var item in build.GetFiles())
            {
                if (item.Extension == string.Empty)
                {
                    var newHash = Zip.ComputeHash(item.FullName);
                    package.Bundles.Add(new Bundle(item.Length, item.Name, newHash));
                    File.Copy(item.FullName, Path.Combine(GlobalSetting.BuildVersion, item.Name), true);
                }
            }

            File.WriteAllText(GlobalSetting.BuildTargetJson, JsonManager.ToJson(package));
            watch.Stop();
            AssetDatabase.Refresh();
            Debug.Log("构建 AssetBundle 完成。耗时: <color=#00FF00>{0:F2}</color> 秒".Format(watch.ElapsedMilliseconds / 1000F));
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

    [CustomPropertyDrawer(typeof(Xor32))]
    internal class Xor32Drawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var color = GUI.color;
            GUI.color = Color.green;
            var content = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            GUI.color = color;

            var origin = property.FindPropertyRelative("origin");
            var offset = property.FindPropertyRelative("offset");
            var source = origin.intValue ^ offset.intValue;

            GUI.enabled = false;
            EditorGUI.IntField(content, source);
            GUI.enabled = true;

            EditorGUI.EndProperty();
        }
    }

    [CustomPropertyDrawer(typeof(Xor64))]
    internal class Xor64Drawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var color = GUI.color;
            GUI.color = Color.green;
            var content = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            GUI.color = color;

            var origin = property.FindPropertyRelative("origin");
            var offset = property.FindPropertyRelative("offset");
            var source = origin.longValue ^ offset.longValue;

            GUI.enabled = false;
            EditorGUI.LongField(content, source);
            GUI.enabled = true;

            EditorGUI.EndProperty();
        }
    }
}