// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2025-09-18 01:09:36
// # Recently: 2025-09-18 01:09:36
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Astraia.Core
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

                Log.Warn("加载资源 {0} 为空!".Format(reason));
            }
            catch (Exception e)
            {
                Log.Warn("加载资源 {0} 失败!\n{1}".Format(reason, e));
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

                Log.Warn("加载资源 {0} 为空!".Format(reason));
            }
            catch (Exception e)
            {
                Log.Warn("加载资源 {0} 失败!\n{1}".Format(reason, e));
            }

            return null;
        }

        private static T LoadAsset<T>(string reason) where T : Object
        {
            T asset;
            if (GlobalSetting.Instance.AssetMode != AssetMode.Resource && Application.isPlaying)
            {
                asset = LoadFirst<T>(LoadAssetData(reason));
            }
            else
            {
                asset = LoadThird<T>(LoadAssetData(reason));
            }

            asset ??= LoadSecond<T>(reason);
            return asset;
        }

        private static T[] LoadAssetAll<T>(string reason) where T : Object
        {
            T[] asset;
            if (GlobalSetting.Instance.AssetMode != AssetMode.Resource && Application.isPlaying)
            {
                asset = LoadFirstAll<T>(LoadAssetData(reason));
            }
            else
            {
                asset = LoadThirdAll<T>(LoadAssetData(reason));
            }

            asset ??= LoadSecondAll<T>(reason);
            return asset;
        }

        private static AssetData LoadAssetData(string reason)
        {
            if (!assetData.TryGetValue(reason, out var asset))
            {
                var index = reason.LastIndexOf('/');
                asset = new AssetData();
                if (index < 0)
                {
                    asset.Name = reason;
                }
                else
                {
                    asset.Path = reason.Substring(0, index).ToLower();
                    asset.Name = reason.Substring(index + 1);
                }

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
            var item = await LoadManager.LoadRequest(persistentData, streamingAsset);
            byte[] bytes = null;
            if (item.Key == 1)
            {
                bytes = await Task.Run(() => Xor.Decrypt(File.ReadAllBytes(item.Value)));
            }
            else if (item.Key == 2)
            {
                using var request = UnityWebRequest.Get(item.Value);
                await request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success)
                {
                    bytes = await Task.Run(() => Xor.Decrypt(request.downloadHandler.data));
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
                var scene = LoadSceneAsset(GlobalSetting.SCENES.Format(reason));
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

                    EventManager.Invoke(new OnSceneComplete(reason));
                    return;
                }

                Log.Warn("加载资源 {0} 为空!".Format(reason));
            }
            catch (Exception e)
            {
                Log.Warn("加载场景 {0} 失败!\n{1}".Format(reason, e));
            }
        }

        private static string LoadSceneAsset(string reason)
        {
            if (GlobalSetting.Instance.AssetMode != AssetMode.Resource)
            {
                var item = LoadAssetData(reason);
                if (assetPack.TryGetValue(item.Path, out var result))
                {
                    var scenes = result.GetAllScenePaths();
                    foreach (var scene in scenes)
                    {
                        if (scene == item.Name)
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
            assetData.Clear();
            assetTask.Clear();
            assetPack.Clear();
            AssetBundle.UnloadAllAssetBundles(true);
        }

        private static T Instantiate<T>(T asset) where T : Object
        {
            return asset is GameObject ? Object.Instantiate(asset) : asset;
        }

        private static T LoadFirst<T>(AssetData reason) where T : Object
        {
            return Instantiate(assetPack.GetValueOrDefault(reason.Path)?.LoadAsset<T>(reason.Name));
        }

        private static T[] LoadFirstAll<T>(AssetData reason) where T : Object
        {
            return assetPack.GetValueOrDefault(reason.Path)?.LoadAssetWithSubAssets<T>(reason.Name);
        }

        private static T LoadSecond<T>(string reason) where T : Object
        {
            return Instantiate(Resources.Load<T>(reason));
        }

        private static T[] LoadSecondAll<T>(string reason) where T : Object
        {
            return Resources.LoadAll<T>(reason);
        }

        private static T LoadThird<T>(AssetData reason) where T : Object
        {
#if UNITY_EDITOR
            foreach (var result in AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(reason.Path, reason.Name))
            {
                return Instantiate(AssetDatabase.LoadAssetAtPath<T>(result));
            }
#endif
            return null;
        }

        private static T[] LoadThirdAll<T>(AssetData reason) where T : Object
        {
#if UNITY_EDITOR
            foreach (var path in AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(reason.Path, reason.Name))
            {
                return AssetDatabase.LoadAllAssetRepresentationsAtPath(path).Cast<T>().ToArray();
            }
#endif
            return null;
        }
    }
}