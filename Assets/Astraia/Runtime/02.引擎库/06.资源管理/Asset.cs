// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-09-18 01:09:36
// // # Recently: 2025-09-18 01:09:36
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

namespace Astraia.Common
{
    using static GlobalManager;

    public static partial class AssetManager
    {
        public static T Load<T>(string reason) where T : Object
        {
            try
            {
                if (!Instance) return null;
                var asset = LoadAsset<T>(reason);
                if (asset != null)
                {
                    return asset;
                }

                Debug.LogWarning("加载资源 {0} 为空!".Format(reason));
            }
            catch (Exception e)
            {
                Debug.LogWarning("加载资源 {0} 失败!\n{1}".Format(reason, e));
            }

            return null;
        }

        public static T[] LoadAll<T>(string reason) where T : Object
        {
            try
            {
                if (!Instance) return null;
                var asset = LoadAssetAll<T>(reason);
                if (asset != null)
                {
                    return asset;
                }

                Debug.LogWarning("加载资源 {0} 为空!".Format(reason));
            }
            catch (Exception e)
            {
                Debug.LogWarning("加载资源 {0} 失败!\n{1}".Format(reason, e));
            }

            return null;
        }

        private static T LoadAsset<T>(string reason) where T : Object
        {
            T asset;
            if (GlobalSetting.Instance.assetLoadMode != AssetMode.Simulate)
            {
                var item = LoadAssetData(reason);
                assetPack.TryGetValue(item.path, out var result);
                asset = Actuator.LoadAt<T>(item.name, result);
            }
            else
            {
                asset = Simulate.LoadAt<T>(reason);
            }

            asset ??= Resource.LoadAt<T>(reason);
            return asset;
        }

        private static T[] LoadAssetAll<T>(string reason) where T : Object
        {
            T[] asset;
            if (GlobalSetting.Instance.assetLoadMode != AssetMode.Simulate)
            {
                var item = LoadAssetData(reason);
                assetPack.TryGetValue(item.path, out var result);
                asset = Actuator.LoadBy<T>(item.name, result);
            }
            else
            {
                asset = Simulate.LoadBy<T>(reason);
            }

            asset ??= Resource.LoadBy<T>(reason);
            return asset;
        }

        private static (string path, string name) LoadAssetData(string reason)
        {
            if (!assetData.TryGetValue(reason, out var asset))
            {
                var index = reason.LastIndexOf('/');
                asset = index < 0 ? (null, reason) : (reason.Substring(0, index).ToLower(), reason.Substring(index + 1));
                assetData.Add(reason, asset);
            }

            return asset;
        }

        public static async void LoadAssetBundle()
        {
            var platform = await LoadAssetBundle(GlobalSetting.Instance.assetPlatform.ToString());
            manifest ??= platform.LoadAsset<AssetBundleManifest>(nameof(AssetBundleManifest));
            EventManager.Invoke(new OnLoadAsset(manifest.GetAllAssetBundles()));

            var bundles = manifest.GetAllAssetBundles();
            foreach (var bundle in bundles)
            {
                _ = LoadAssetBundle(bundle);
            }

            await Task.WhenAll(assetTask.Values);
            EventManager.Invoke(new OnAssetComplete());
        }

        public static async Task<AssetBundle> LoadAssetBundle(string reason)
        {
            if (string.IsNullOrEmpty(reason))
            {
                return null;
            }

            if (assetPack.TryGetValue(reason, out var bundle))
            {
                return bundle;
            }

            if (assetTask.TryGetValue(reason, out var request))
            {
                return await request;
            }

            request = LoadRequest(GlobalSetting.GetBundlePath(reason), GlobalSetting.GetClientPath(reason));
            assetTask.Add(reason, request);
            try
            {
                bundle = await request;
                assetPack.Add(reason, bundle);
                EventManager.Invoke(new OnAssetUpdate(reason));
                return bundle;
            }
            finally
            {
                assetTask.Remove(reason);
            }
        }

        private static async Task<AssetBundle> LoadRequest(string persistentData, string streamingAsset)
        {
            var item = await BundleManager.LoadRequest(persistentData, streamingAsset);
            byte[] bytes = null;
            if (item.mode == 1)
            {
                bytes = await Task.Run(() => Service.Xor.Decrypt(File.ReadAllBytes(item.path)));
            }
            else if (item.mode == 2)
            {
                using var request = UnityWebRequest.Get(item.path);
                await request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success)
                {
                    bytes = await Task.Run(() => Service.Xor.Decrypt(request.downloadHandler.data));
                }
            }

            var result = AssetBundle.LoadFromMemoryAsync(bytes);
            while (!result.isDone && Instance)
            {
                await Task.Yield();
            }

            return result.assetBundle;
        }
    }
}