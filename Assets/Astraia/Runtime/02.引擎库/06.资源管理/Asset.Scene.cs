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
        public static async void LoadScene(string assetPath)
        {
            try
            {
                if (!Instance) return;
                var scene = LoadSceneAsset(GlobalSetting.GetScenePath(assetPath));
                if (!string.IsNullOrEmpty(scene))
                {
                    EventManager.Invoke(new SceneAwake(assetPath));
                    var request = SceneManager.LoadSceneAsync(assetPath, LoadSceneMode.Single);
                    if (request != null)
                    {
                        while (!request.isDone && Instance)
                        {
                            EventManager.Invoke(new SceneUpdate(request.progress));
                            await Task.Yield();
                        }
                    }

                    EventManager.Invoke(new SceneComplete());
                    return;
                }

                Debug.LogWarning("加载资源 {0} 为空!".Format(assetPath));
            }
            catch (Exception e)
            {
                Debug.LogWarning("加载场景 {0} 失败!\n{1}".Format(assetPath, e));
            }
        }

        private static string LoadSceneAsset(string path)
        {
            if (GlobalSetting.Instance.assetLoadMode == AssetMode.Authentic)
            {
                var item = LoadAssetData(path);
                var data = assetPack[item.path].GetAllScenePaths();
                foreach (var scene in data)
                {
                    if (scene == item.name)
                    {
                        return scene;
                    }
                }
            }

            return path.Substring(path.LastIndexOf('/') + 1);
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