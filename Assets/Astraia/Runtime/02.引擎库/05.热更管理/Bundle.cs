// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-04-09 22:04:41
// // # Recently: 2025-04-09 22:04:41
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

#pragma warning disable CS1998 // 异步方法缺少 "await" 运算符，将以同步方式运行

namespace Astraia.Common
{
    using static GlobalManager;

    internal static class BundleManager
    {
        public static async void Update()
        {
            if (!Instance) return;
            if (GlobalSetting.Instance.assetLoadMode == AssetMode.Resources)
            {
                EventManager.Invoke(new OnBundleComplete(0, "启动本地资源加载。"));
                return;
            }

            if (!Directory.Exists(GlobalSetting.downloadLocalPath))
            {
                Directory.CreateDirectory(GlobalSetting.downloadLocalPath);
            }

            var serverFile = GlobalSetting.GetServerPath(GlobalSetting.ASSET_JSON);
            var serverRequest = await LoadServerRequest(GlobalSetting.ASSET_JSON, serverFile);
            var serverData = new Dictionary<string, BundleData>();
            if (!string.IsNullOrEmpty(serverRequest))
            {
                var bundles = JsonManager.FromJson<List<BundleData>>(serverRequest);
                foreach (var bundle in bundles)
                {
                    serverData.Add(bundle.name, bundle);
                }
            }
            else
            {
                EventManager.Invoke(new OnBundleComplete(-1, "没有连接到服务器!"));
                return;
            }

            var persistentData = GlobalSetting.GetBundlePath(GlobalSetting.ASSET_JSON);
            var streamingAsset = GlobalSetting.GetClientPath(GlobalSetting.ASSET_JSON);
            var clientRequest = await LoadClientRequest(persistentData, streamingAsset);
            var clientData = new Dictionary<string, BundleData>();
            if (!string.IsNullOrEmpty(clientRequest))
            {
                var bundles = JsonManager.FromJson<List<BundleData>>(clientRequest);
                foreach (var bundle in bundles)
                {
                    clientData.Add(bundle.name, bundle);
                }
            }

            var modifyData = new HashSet<string>();
            foreach (var fileName in serverData.Keys)
            {
                if (clientData.TryGetValue(fileName, out var bundle))
                {
                    if (serverData[fileName] != bundle)
                    {
                        modifyData.Add(fileName);
                    }

                    clientData.Remove(fileName);
                }
                else
                {
                    modifyData.Add(fileName);
                }
            }

            var modifySize = 0L;
            foreach (var modifyName in modifyData)
            {
                if (serverData.TryGetValue(modifyName, out var bundle))
                {
                    modifySize += bundle.size;
                }
            }

            EventManager.Invoke(new OnLoadBundle(modifyData.Count, modifySize));
            foreach (var deleteData in clientData.Keys)
            {
                var filePath = GlobalSetting.GetBundlePath(deleteData);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }

            var success = await LoadBundleRequest(modifyData);
            if (success)
            {
                var filePath = GlobalSetting.GetBundlePath(GlobalSetting.ASSET_JSON);
                await File.WriteAllTextAsync(filePath, serverRequest);
            }

            EventManager.Invoke(new OnBundleComplete(1, success ? "更新完成!" : "更新失败!"));
        }

        private static async Task<bool> LoadBundleRequest(HashSet<string> fileNames)
        {
            var copies = new HashSet<string>(fileNames);
            for (var i = 0; i < 5; i++)
            {
                foreach (var fileName in fileNames)
                {
                    var serverFile = GlobalSetting.GetServerPath(fileName);
                    var bundleData = await LoadBundleRequest(fileName, serverFile);
                    var bundlePath = GlobalSetting.GetBundlePath(fileName);
                    await Task.Run(() => File.WriteAllBytes(bundlePath, bundleData));
                    if (copies.Contains(fileName))
                    {
                        copies.Remove(fileName);
                    }
                }

                if (copies.Count == 0)
                {
                    break;
                }
            }

            return copies.Count == 0;
        }

        private static async Task<string> LoadServerRequest(string bundleName, string bundleUri)
        {
            for (var i = 0; i < 5; i++)
            {
                using (var request = UnityWebRequest.Head(bundleUri))
                {
                    request.timeout = 1;
                    await request.SendWebRequest();
                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        continue;
                    }
                }

                using (var request = UnityWebRequest.Get(bundleUri))
                {
                    await request.SendWebRequest();
                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        Debug.Log("请求服务器下载 {0} 失败!\n".Format(bundleName));
                        continue;
                    }

                    return request.downloadHandler.text;
                }
            }

            return null;
        }

        private static async Task<byte[]> LoadBundleRequest(string bundleName, string bundleUri)
        {
            using (var request = UnityWebRequest.Head(bundleUri))
            {
                request.timeout = 1;
                await request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log("请求服务器校验 {0} 失败!\n".Format(bundleName));
                    return null;
                }
            }

            using (var request = UnityWebRequest.Get(bundleUri))
            {
                var result = request.SendWebRequest();
                while (!result.isDone && Instance)
                {
                    EventManager.Invoke(new OnBundleUpdate(bundleName, request.downloadProgress));
                    await Task.Yield();
                }

                EventManager.Invoke(new OnBundleUpdate(bundleName, 1));
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log("请求服务器下载 {0} 失败!\n".Format(bundleName));
                    return null;
                }

                return request.downloadHandler.data;
            }
        }

        private static async Task<string> LoadClientRequest(string persistentData, string streamingAsset)
        {
            var assetData = await LoadRequest(persistentData, streamingAsset);
            string result = null;
            if (assetData.mode == 1)
            {
                result = await File.ReadAllTextAsync(assetData.path);
            }
            else if (assetData.mode == 2)
            {
                using var request = UnityWebRequest.Get(assetData.path);
                await request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success)
                {
                    result = request.downloadHandler.text;
                }
            }

            return result;
        }


        internal static async Task<(int mode, string path)> LoadRequest(string persistentData, string streamingAsset)
        {
            if (File.Exists(persistentData))
            {
                return (1, persistentData);
            }
#if UNITY_ANDROID && !UNITY_EDITOR
            if (!streamingAsset.StartsWith("jar:"))
            {
                streamingAsset = "jar:" + streamingAsset;
            }
            using var request = UnityWebRequest.Head(streamingAsset);
            await request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                return (2, streamingAsset);
            }
#else
            if (File.Exists(streamingAsset))
            {
                return (1, streamingAsset);
            }
#endif
            return (0, string.Empty);
        }
    }
}