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
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Astraia.Common
{
    using static GlobalManager;

    public static class AssetManager
    {
        public static T Load<T>(string reason) where T : Object
        {
            try
            {
                var asset = LoadAsset<T>(reason);
                if (asset != null)
                {
                    return asset;
                }

                Service.Log.Warn("加载资源 {0} 为空!".Format(reason));
            }
            catch (Exception e)
            {
                Service.Log.Warn("加载资源 {0} 失败!\n{1}".Format(reason, e));
            }

            return null;
        }

        public static T[] LoadAll<T>(string reason) where T : Object
        {
            try
            {
                var asset = LoadAssetAll<T>(reason);
                if (asset != null)
                {
                    return asset;
                }

                Service.Log.Warn("加载资源 {0} 为空!".Format(reason));
            }
            catch (Exception e)
            {
                Service.Log.Warn("加载资源 {0} 失败!\n{1}".Format(reason, e));
            }

            return null;
        }

        private static T LoadAsset<T>(string reason) where T : Object
        {
            T asset;
            if (GlobalSetting.Instance.AssetMode != AssetMode.Resource && Application.isPlaying)
            {
                var item = LoadAssetData(reason);
                assetPack.TryGetValue(item.Key, out var result);
                asset = Actuator.LoadAt<T>(item.Value, result);
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
            if (GlobalSetting.Instance.AssetMode != AssetMode.Resource && Application.isPlaying)
            {
                var item = LoadAssetData(reason);
                assetPack.TryGetValue(item.Key, out var result);
                asset = Actuator.LoadBy<T>(item.Value, result);
            }
            else
            {
                asset = Simulate.LoadBy<T>(reason);
            }

            asset ??= Resource.LoadBy<T>(reason);
            return asset;
        }

        private static (string Key, string Value) LoadAssetData(string reason)
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
            var platform = await LoadAssetBundle(GlobalSetting.Instance.BuildTarget.ToString());
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

            request = LoadRequest(GlobalSetting.TargetPath.Format(reason), GlobalSetting.ClientPath.Format(reason));
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
            if (item.Key == 1)
            {
                bytes = await Task.Run(() => Service.Xor.Decrypt(File.ReadAllBytes(item.Value)));
            }
            else if (item.Key == 2)
            {
                using var request = UnityWebRequest.Get(item.Value);
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

        public static async void LoadScene(string reason)
        {
            try
            {
                if (!Instance) return;
                var scene = LoadSceneAsset(GlobalSetting.Scene.Format(reason));
                if (!string.IsNullOrEmpty(scene))
                {
                    EventManager.Invoke(new OnLoadScene(reason));
                    var request = SceneManager.LoadSceneAsync(reason, LoadSceneMode.Single);
                    if (request != null)
                    {
                        while (!request.isDone && Instance)
                        {
                            EventManager.Invoke(new OnSceneUpdate(request.progress));
                            await Task.Yield();
                        }
                    }

                    EventManager.Invoke(new OnSceneComplete());
                    return;
                }

                Service.Log.Warn("加载资源 {0} 为空!".Format(reason));
            }
            catch (Exception e)
            {
                Service.Log.Warn("加载场景 {0} 失败!\n{1}".Format(reason, e));
            }
        }

        private static string LoadSceneAsset(string reason)
        {
            if (GlobalSetting.Instance.AssetMode != AssetMode.Resource)
            {
                var item = LoadAssetData(reason);
                if (assetPack.TryGetValue(item.Key, out var result))
                {
                    var scenes = result.GetAllScenePaths();
                    foreach (var scene in scenes)
                    {
                        if (scene == item.Value)
                        {
                            return scene;
                        }
                    }
                }
            }

            return reason.Substring(reason.LastIndexOf('/') + 1);
        }

        internal static void Dispose()
        {
            assetPath.Clear();
            assetData.Clear();
            assetTask.Clear();
            assetPack.Clear();
            AssetBundle.UnloadAllAssetBundles(true);
        }

        private static class Actuator
        {
            public static T LoadAt<T>(string reason, AssetBundle bundle) where T : Object
            {
                if (bundle == null) return null;
                var asset = bundle.LoadAsset<T>(reason);
                return asset is GameObject ? Object.Instantiate(asset) : asset;
            }

            public static T[] LoadBy<T>(string reason, AssetBundle bundle) where T : Object
            {
                return bundle.LoadAssetWithSubAssets<T>(reason);
            }
        }

        private static class Resource
        {
            public static T LoadAt<T>(string reason) where T : Object
            {
                var asset = Resources.Load<T>(reason);
                return asset is GameObject ? Object.Instantiate(asset) : asset;
            }

            public static T[] LoadBy<T>(string reason) where T : Object
            {
                return Resources.LoadAll<T>(reason);
            }
        }

        private static class Simulate
        {
            public static T LoadAt<T>(string reason) where T : Object
            {
#if UNITY_EDITOR
                if (assetPath.TryGetValue(reason, out var result))
                {
                    var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(result);
                    return asset is GameObject ? Object.Instantiate(asset) : asset;
                }

                return null;
#else
                return null;
#endif
            }

            public static T[] LoadBy<T>(string reason) where T : Object
            {
#if UNITY_EDITOR
                if (assetPath.TryGetValue(reason, out var result))
                {
                    return UnityEditor.AssetDatabase.LoadAllAssetRepresentationsAtPath(result).Cast<T>().ToArray();
                }

                return null;
#else
                return null;
#endif
            }
        }
    }
}