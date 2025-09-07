// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-04-09 22:04:25
// // # Recently: 2025-04-09 22:04:25
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
                var assetData = await LoadSceneAsset(GlobalSetting.GetScenePath(assetPath));
                if (assetData != null)
                {
                    EventManager.Invoke(new SceneAwake(assetPath));
                    var operation = SceneManager.LoadSceneAsync(assetPath, LoadSceneMode.Single);
                    await Instance.Wait(operation).OnUpdate(progress => EventManager.Invoke(new SceneUpdate(progress)));
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

        private static async Task<string> LoadSceneAsset(string assetPath)
        {
            if (GlobalSetting.Instance.assetLoadMode == AssetMode.Authentic)
            {
                var assetItem = await LoadAssetData(assetPath);
                var assetPack = await LoadAssetPack(assetItem.path);
                var assetData = assetPack.GetAllScenePaths();
                foreach (var data in assetData)
                {
                    if (data == assetItem.name)
                    {
                        return data;
                    }
                }
            }

            return assetPath.Substring(assetPath.LastIndexOf('/') + 1);
        }
    }
}