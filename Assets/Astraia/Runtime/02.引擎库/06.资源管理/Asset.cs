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
        public static T Load<T>(string path) where T : Object
        {
            try
            {
                if (!Instance) return null;
                var asset = LoadAsset<T>(path);
                if (asset != null)
                {
                    return asset;
                }

                Debug.LogWarning("加载资源 {0} 为空!".Format(path));
            }
            catch (Exception e)
            {
                Debug.LogWarning("加载资源 {0} 失败!\n{1}".Format(path, e));
            }

            return null;
        }

        public static T[] LoadAll<T>(string path) where T : Object
        {
            try
            {
                if (!Instance) return null;
                var asset = LoadAssetAll<T>(path);
                if (asset != null)
                {
                    return asset;
                }

                Debug.LogWarning("加载资源 {0} 为空!".Format(path));
            }
            catch (Exception e)
            {
                Debug.LogWarning("加载资源 {0} 失败!\n{1}".Format(path, e));
            }

            return null;
        }

        private static T LoadAsset<T>(string path) where T : Object
        {
            T asset;
            if (GlobalSetting.Instance.assetLoadMode == AssetMode.Authentic)
            {
                var item = LoadAssetData(path);
                assetPack.TryGetValue(item.path, out var data);
                asset = Actuator.LoadAt<T>(item.name, data);
            }
            else
            {
                asset = Simulate.LoadAt<T>(path);
            }

            asset ??= Resource.LoadAt<T>(path);
            return asset;
        }

        private static T[] LoadAssetAll<T>(string path) where T : Object
        {
            T[] asset;
            if (GlobalSetting.Instance.assetLoadMode == AssetMode.Authentic)
            {
                var item = LoadAssetData(path);
                assetPack.TryGetValue(item.path, out var data);
                asset = Actuator.LoadBy<T>(item.name, data);
            }
            else
            {
                asset = Simulate.LoadBy<T>(path);
            }

            asset ??= Resource.LoadBy<T>(path);
            return asset;
        }

        private static (string path, string name) LoadAssetData(string path)
        {
            if (!assetData.TryGetValue(path, out var asset))
            {
                var index = path.LastIndexOf('/');
                asset = index < 0 ? (null, path) : (path.Substring(0, index).ToLower(), path.Substring(index + 1));
                assetData.Add(path, asset);
            }

            return asset;
        }

        public static async void LoadAssetBundle()
        {
            var platform = await LoadAssetBundle(GlobalSetting.Instance.assetPlatform.ToString());
            manifest ??= platform.LoadAsset<AssetBundleManifest>(nameof(AssetBundleManifest));
            EventManager.Invoke(new AssetAwake(manifest.GetAllAssetBundles()));

            var assetPacks = manifest.GetAllAssetBundles();
            foreach (var assetPack in assetPacks)
            {
                _ = LoadAssetBundle(assetPack);
            }

            await Task.WhenAll(assetTask.Values);
            EventManager.Invoke(new AssetComplete());
        }

        public static async Task<AssetBundle> LoadAssetBundle(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            if (assetPack.TryGetValue(path, out var pack))
            {
                return pack;
            }

            if (assetTask.TryGetValue(path, out var request))
            {
                return await request;
            }

            request = LoadRequest(GlobalSetting.GetPacketPath(path), GlobalSetting.GetClientPath(path));
            assetTask.Add(path, request);
            try
            {
                pack = await request;
                assetPack.Add(path, pack);
                EventManager.Invoke(new AssetUpdate(path));
                return pack;
            }
            finally
            {
                assetTask.Remove(path);
            }
        }

        private static async Task<AssetBundle> LoadRequest(string persistentData, string streamingAsset)
        {
            var item = await PackManager.LoadRequest(persistentData, streamingAsset);
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