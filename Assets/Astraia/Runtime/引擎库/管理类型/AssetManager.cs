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
        private static readonly Dictionary<string, Task<AssetBundle>> AssetTask = new Dictionary<string, Task<AssetBundle>>();
        private static readonly Dictionary<string, AssetBundle> AssetPack = new Dictionary<string, AssetBundle>();
        private static readonly Dictionary<string, AssetData> AssetPath = new Dictionary<string, AssetData>();

        public long version;
        public AssetBundleManifest manifest;

        public override void Enqueue()
        {
            Instance = null;
            AssetPath.Clear();
            AssetTask.Clear();
            AssetPack.Clear();
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
            if (Instance != null && Instance.manifest)
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
            if (Instance != null && Instance.manifest)
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
            if (!AssetPath.TryGetValue(reason, out var asset))
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

                AssetPath.Add(reason, asset);
            }

            return asset;
        }

        public static async void LoadAssetBundle()
        {
            if (Instance != null)
            {
                var platform = await LoadAssetBundle(GlobalSetting.Instance.BuildTarget.ToString());
                var manifest = Instance.manifest;
                if (manifest == null)
                {
                    manifest = platform.LoadAsset<AssetBundleManifest>(nameof(AssetBundleManifest));
                }

                EventManager.Invoke(new OnLoadAsset(manifest.GetAllAssetBundles()));

                var bundles = manifest.GetAllAssetBundles();
                foreach (var bundle in bundles)
                {
                    _ = LoadAssetBundle(bundle);
                }

                await Task.WhenAll(AssetTask.Values);
                EventManager.Invoke(new OnAssetComplete());
            }
        }

        public static async Task<AssetBundle> LoadAssetBundle(string reason)
        {
            if (string.IsNullOrEmpty(reason))
            {
                return null;
            }

            if (AssetPack.TryGetValue(reason, out var bundle))
            {
                return bundle;
            }

            if (AssetTask.TryGetValue(reason, out var request))
            {
                return await request;
            }

            request = LoadRequest(GlobalSetting.TargetPath.Format(reason), GlobalSetting.ClientPath.Format(reason));
            AssetTask.Add(reason, request);
            try
            {
                bundle = await request;
                AssetPack.Add(reason, bundle);
                EventManager.Invoke(new OnAssetUpdate(reason));
                return bundle;
            }
            finally
            {
                AssetTask.Remove(reason);
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
            while (!result.isDone && Instance != null)
            {
                await Task.Yield();
            }

            return result.assetBundle;
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
            if (Instance != null && Instance.manifest)
            {
                var sceneData = AssetPack.GetValueOrDefault(item.Path);
                var scenePath = sceneData.GetAllScenePaths().FirstOrDefault(scene => scene == item.Name);
                return SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Single);
            }
            else
            {
#if UNITY_EDITOR
                var sceneData = LoadThird<SceneAsset>(item);
                if (sceneData)
                {
                    var scenePath = AssetDatabase.GetAssetPath(sceneData);
                    return EditorSceneManager.LoadSceneAsyncInPlayMode(scenePath, new LoadSceneParameters(LoadSceneMode.Single));
                }
#endif
                var sceneName = reason.Substring(reason.LastIndexOf('/') + 1);
                return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            }
        }

        private static T Instantiate<T>(T asset) where T : Object
        {
            return asset is GameObject ? Object.Instantiate(asset) : asset;
        }

        private static T LoadFirst<T>(AssetData reason) where T : Object
        {
            return Instantiate(AssetPack.GetValueOrDefault(reason.Path)?.LoadAsset<T>(reason.Name));
        }

        private static T[] LoadFirstAll<T>(AssetData reason) where T : Object
        {
            return AssetPack.GetValueOrDefault(reason.Path)?.LoadAssetWithSubAssets<T>(reason.Name);
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