// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-08-05 16:08:17
// // # Recently: 2025-08-05 16:08:17
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************


using System;
using System.IO;
using Astraia.Common;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Astraia
{
    public class EditorProcessor : AssetPostprocessor
    {
        private static string root => GlobalSetting.Instance.assetSourcePath;
        private static int offset => root.EndsWith("/") ? root.Length : root.Length + 1;

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

                if (newPath.StartsWith(root))
                {
                    ProcessAsset(newPath);
                }
                else if (oldPath.StartsWith(root))
                {
                    var importer = AssetImporter.GetAtPath(newPath);
                    if (importer != null && !string.IsNullOrEmpty(importer.assetBundleName))
                    {
                        Debug.Log(Service.Text.Format("移除 {0} 资源: {1}", importer.assetBundleName, oldPath));
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
            if (!path.StartsWith(root))
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

            if (asset is SceneAsset && !GlobalSetting.Instance.sceneAssets.Contains(path))
            {
                GlobalSetting.Instance.sceneAssets.Add(path);
            }

            var split = path.Substring(offset).TrimStart('/');
            var index = split.IndexOf('/');
            if (index >= 0)
            {
                var folder = split.Substring(0, index).ToLower();
                var importer = AssetImporter.GetAtPath(path);

                if (!string.Equals(importer.assetBundleName, folder, StringComparison.Ordinal))
                {
                    Debug.Log(Service.Text.Format("设置 {0} 资源: {1}", folder, path));
                    importer.assetBundleName = folder;
                    importer.SaveAndReimport();
                }
            }
        }

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
                        GlobalManager.assetPath[Service.Text.Format("{0}/{1}", folder, result)] = path;
                    }
                }
            }
        }
    }
}