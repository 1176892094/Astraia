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
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using System.Runtime.CompilerServices;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace Astraia.Core
{
    [Serializable]
    public class AssetManager : Singleton<AssetManager>
    {
        private static readonly Dictionary<string, AssetData> assetData = new();
        private static readonly Dictionary<string, AssetBundle> assetPack = new();
        private static Manifest package;
        private static AssetBundleManifest manifest;

        public int version;

        internal void SetVersion(Manifest package)
        {
            version = package.Version;
            AssetManager.package = package;
        }

        public override void Enqueue()
        {
            Instance = null;
            manifest = null;
            assetData.Clear();
            assetPack.Clear();
            AssetBundle.UnloadAllAssetBundles(true);
        }

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
            if (Instance != null && manifest)
            {
                var asset = LoadFirst<T>(LoadAssetData(reason));
                return asset ?? LoadSecond<T>(reason);
            }
            else
            {
                var asset = LoadThird<T>(LoadAssetData(reason));
                return asset ?? LoadSecond<T>(reason);
            }
        }

        private static T[] LoadAssetAll<T>(string reason) where T : Object
        {
            if (Instance != null && manifest)
            {
                var asset = LoadFirstAll<T>(LoadAssetData(reason));
                return asset ?? LoadSecondAll<T>(reason);
            }
            else
            {
                var asset = LoadThirdAll<T>(LoadAssetData(reason));
                return asset ?? LoadSecondAll<T>(reason);
            }
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
            if (Instance != null)
            {
                var requests = new Dictionary<string, Task<AssetBundle>>();
                var platform = await LoadAssetBundle(GlobalSetting.TargetPlatform, requests);

                if (manifest == null)
                {
                    manifest = platform.LoadAsset<AssetBundleManifest>(nameof(AssetBundleManifest));
                }

                EventManager.Invoke(new OnLoadAsset(manifest.GetAllAssetBundles()));

                var bundles = manifest.GetAllAssetBundles();
                foreach (var bundle in bundles)
                {
                    _ = LoadAssetBundle(bundle, requests);
                }

                await Task.WhenAll(requests.Values);
                EventManager.Invoke(new OnAssetComplete(assetPack.Values.All(bundle => bundle)));
            }
        }

        public static async Task<AssetBundle> LoadAssetBundle(string reason, Dictionary<string, Task<AssetBundle>> requests)
        {
            if (string.IsNullOrEmpty(reason))
            {
                return null;
            }

            if (assetPack.TryGetValue(reason, out var bundle))
            {
                return bundle;
            }

            if (requests.TryGetValue(reason, out var request))
            {
                return await request;
            }

            request = LoadRequest(reason);
            requests.Add(reason, request);
            try
            {
                bundle = await request;
                assetPack.Add(reason, bundle);
                EventManager.Invoke(new OnAssetUpdate(reason));
                return bundle;
            }
            finally
            {
                requests.Remove(reason);
            }
        }

        private static async Task<AssetBundle> LoadRequest(string reason)
        {
            var result = GlobalSetting.PersistentPath.Format(reason);
            if (package.Bundles.TryGetValue(reason, out var bundle))
            {
                if (bundle.Hash == Zip.ComputeHash(result))
                {
                    var request = AssetBundle.LoadFromFileAsync(result);
                    await request;
                    return request.assetBundle;
                }
            }

            return null;
        }

        public static async void LoadScene(string reason)
        {
            try
            {
                if (Instance != null)
                {
                    EventManager.Invoke(new OnLoadScene(reason));
                    var request = LoadSceneAsset(GlobalSetting.SCENES.Format(reason));
                    while (!request.isDone && Instance != null)
                    {
                        EventManager.Invoke(new OnSceneUpdate(request.progress));
                        await Task.Yield();
                    }

                    EventManager.Invoke(new OnSceneComplete(reason));
                }
            }
            catch (Exception e)
            {
                Log.Warn("加载场景 {0} 失败!\n{1}".Format(reason, e));
            }
        }

        private static AsyncOperation LoadSceneAsset(string reason)
        {
            var item = LoadAssetData(reason);

            if (Instance != null && manifest)
            {
                var sceneData = assetPack.GetValueOrDefault(item.Path);
                var scenePath = sceneData.GetAllScenePaths().FirstOrDefault(path => Path.GetFileNameWithoutExtension(path) == item.Name);
                if (!string.IsNullOrEmpty(scenePath))
                {
                    return SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Single);
                }
            }
            else
            {
#if UNITY_EDITOR
                var sceneData = LoadThird<SceneAsset>(item);
                var scenePath = AssetDatabase.GetAssetPath(sceneData);
                if (!string.IsNullOrEmpty(scenePath))
                {
                    return EditorSceneManager.LoadSceneAsyncInPlayMode(scenePath, new LoadSceneParameters(LoadSceneMode.Single));
                }
#endif
            }

            var sceneName = reason.Substring(reason.LastIndexOf('/') + 1);
            return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
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
                var asset = AssetDatabase.LoadAssetAtPath<T>(result);
                if (asset) return Instantiate(asset);
            }
#endif
            return null;
        }

        private static T[] LoadThirdAll<T>(AssetData reason) where T : Object
        {
#if UNITY_EDITOR
            foreach (var result in AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(reason.Path, reason.Name))
            {
                var source = AssetDatabase.LoadAllAssetRepresentationsAtPath(result);
                return Unsafe.As<Object[], T[]>(ref source);
            }
#endif
            return null;
        }

        public struct AssetData
        {
            public string Name;
            public string Path;
        }
    }
}