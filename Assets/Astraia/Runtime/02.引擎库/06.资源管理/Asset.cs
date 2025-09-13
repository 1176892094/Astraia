// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-04-09 22:04:43
// // # Recently: 2025-04-09 22:04:43
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Astraia.Common
{
    using static GlobalManager;

    public static partial class AssetManager
    {
        public static async void LoadAssetData()
        {
            var platform = await LoadAssetPack(GlobalSetting.Instance.assetPlatform.ToString());
            manifest ??= platform.LoadAsset<AssetBundleManifest>(nameof(AssetBundleManifest));
            EventManager.Invoke(new AssetAwake(manifest.GetAllAssetBundles()));

            var assetPacks = manifest.GetAllAssetBundles();
            foreach (var assetPack in assetPacks)
            {
                _ = LoadAssetPack(assetPack);
            }

            await Task.WhenAll(assetTask.Values);
            EventManager.Invoke(new AssetComplete());
        }

        public static async Task<T> Load<T>(string assetPath) where T : Object
        {
            try
            {
                if (!Instance) return null;
                var assetData = await LoadAsset(assetPath, typeof(T));
                if (assetData != null)
                {
                    return (T)assetData;
                }

                Debug.LogWarning("加载资源 {0} 为空!".Format(assetPath));
            }
            catch (Exception e)
            {
                Debug.LogWarning("加载资源 {0} 失败!\n{1}".Format(assetPath, e));
            }

            return null;
        }

        public static async void Load<T>(string assetPath, Action<T> assetAction) where T : Object
        {
            try
            {
                if (!Instance) return;
                var assetData = await LoadAsset(assetPath, typeof(T));
                if (assetData != null)
                {
                    assetAction.Invoke((T)assetData);
                    return;
                }

                Debug.LogWarning("加载资源 {0} 为空!".Format(assetPath));
            }
            catch (Exception e)
            {
                Debug.LogWarning("加载资源 {0} 失败!\n{1}".Format(assetPath, e));
            }
        }

        private static async Task<Object> LoadAsset(string assetPath, Type assetType)
        {
            if (GlobalSetting.Instance.assetLoadMode == AssetMode.Authentic)
            {
                var assetItem = await LoadAssetData(assetPath);
                var assetPack = await LoadAssetPack(assetItem.path);
                var assetData = LoadByAssetPack(assetItem.name, assetType, assetPack);
                assetData ??= LoadByResources(assetPath, assetType);
                return assetData;
            }
            else
            {
                var assetData = LoadBySimulates(assetPath, assetType);
                assetData ??= LoadByResources(assetPath, assetType);
                return assetData;
            }
        }

        private static async Task<(string path, string name)> LoadAssetData(string assetPath)
        {
            if (!GlobalManager.assetData.TryGetValue(assetPath, out var assetData))
            {
                var index = assetPath.LastIndexOf('/');
                if (index < 0)
                {
                    assetData = (string.Empty, assetPath);
                }
                else
                {
                    var assetPack = assetPath.Substring(0, index).ToLower();
                    assetData = (assetPack, assetPath.Substring(index + 1));
                }

                GlobalManager.assetData.Add(assetPath, assetData);
            }

            var platform = await LoadAssetPack(GlobalSetting.Instance.assetPlatform.ToString());
            manifest ??= platform.LoadAsset<AssetBundleManifest>(nameof(AssetBundleManifest));

            var assetPacks = manifest.GetAllDependencies(assetData.Item1);
            foreach (var assetPack in assetPacks)
            {
                _ = LoadAssetPack(assetPack);
            }

            return assetData;
        }

        private static async Task<AssetBundle> LoadAssetPack(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                return null;
            }

            if (GlobalManager.assetPack.TryGetValue(assetPath, out var assetPack))
            {
                return assetPack;
            }

            if (GlobalManager.assetTask.TryGetValue(assetPath, out var assetTask))
            {
                return await assetTask;
            }

            var persistentData = GlobalSetting.GetPacketPath(assetPath);
            var streamingAsset = GlobalSetting.GetClientPath(assetPath);
            assetTask = PackManager.LoadAssetRequest(persistentData, streamingAsset);
            GlobalManager.assetTask.Add(assetPath, assetTask);
            try
            {
                assetPack = await assetTask;
                GlobalManager.assetPack.Add(assetPath, assetPack);
                EventManager.Invoke(new AssetUpdate(assetPath));
                return assetPack;
            }
            finally
            {
                GlobalManager.assetTask.Remove(assetPath);
            }
        }

        private static Object LoadByAssetPack(string assetPath, Type assetType, AssetBundle assetPack)
        {
            if (assetPack == null) return null;
            var request = assetPack.LoadAssetAsync(assetPath, assetType);
            return request.asset is GameObject ? Object.Instantiate(request.asset) : request.asset;
        }

        private static Object LoadByResources(string assetPath, Type assetType)
        {
            var request = Resources.Load(assetPath, assetType);
            return request is GameObject ? Object.Instantiate(request) : request;
        }

        private static Object LoadBySimulates(string assetPath, Type assetType)
        {
#if UNITY_EDITOR
            if (!GlobalManager.assetPath.TryGetValue(assetPath, out var assetData)) return null;
            var request = UnityEditor.AssetDatabase.LoadAssetAtPath(assetData, assetType);
            return request is GameObject ? Object.Instantiate(request) : request;
#else
            return null;
#endif
        }

        internal static void Dispose()
        {
            assetPath.Clear();
            assetData.Clear();
            assetTask.Clear();
            assetPack.Clear();
            AssetBundle.UnloadAllAssetBundles(true);
        }
    }
}