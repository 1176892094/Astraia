// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-09-18 02:09:04
// // # Recently: 2025-09-18 02:09:04
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Astraia.Common
{
    using static GlobalManager;

    public static partial class AssetManager
    {
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

                Debug.LogWarning("加载资源 {0} 为空!".Format(reason));
            }
            catch (Exception e)
            {
                Debug.LogWarning("加载场景 {0} 失败!\n{1}".Format(reason, e));
            }
        }

        private static string LoadSceneAsset(string reason)
        {
            if (GlobalSetting.Instance.AssetMode != AssetMode.Resources)
            {
                var item = LoadAssetData(reason);
                if (assetPack.TryGetValue(item.path, out var result))
                {
                    var scenes = result.GetAllScenePaths();
                    foreach (var scene in scenes)
                    {
                        if (scene == item.name)
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
    }
}