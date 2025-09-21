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
            if (GlobalSetting.Instance.AssetMode == AssetMode.Resource)
            {
                EventManager.Invoke(new OnBundleComplete(0, "启动本地资源加载。"));
                return;
            }

            if (!Directory.Exists(GlobalSetting.BundlePath))
            {
                Directory.CreateDirectory(GlobalSetting.BundlePath);
            }

            var clientData = new Dictionary<string, BundleData>();
            var serverData = new Dictionary<string, BundleData>();
            var processingData = GlobalSetting.ServerPath.Format(GlobalSetting.Verify);
            var persistentData = GlobalSetting.TargetPath.Format(GlobalSetting.Verify);
            var streamingAsset = GlobalSetting.ClientPath.Format(GlobalSetting.Verify);

            var serverRequest = await LoadServerRequest(processingData, GlobalSetting.Verify);
            if (!string.IsNullOrEmpty(serverRequest))
            {
                var items = JsonManager.FromJson<List<BundleData>>(serverRequest);
                foreach (var item in items)
                {
                    serverData.Add(item.name, item);
                }
            }
            else
            {
                EventManager.Invoke(new OnBundleComplete(-1, "没有连接到服务器!"));
                return;
            }

            var clientRequest = await LoadClientRequest(persistentData, streamingAsset);
            if (!string.IsNullOrEmpty(clientRequest))
            {
                var items = JsonManager.FromJson<List<BundleData>>(clientRequest);
                foreach (var item in items)
                {
                    clientData.Add(item.name, item);
                }
            }

            var names = new HashSet<string>();
            foreach (var key in serverData.Keys)
            {
                if (clientData.TryGetValue(key, out var bundle))
                {
                    if (serverData[key] != bundle)
                    {
                        names.Add(key);
                    }

                    clientData.Remove(key);
                }
                else
                {
                    names.Add(key);
                }
            }

            var amount = 0L;
            foreach (var name in names)
            {
                if (serverData.TryGetValue(name, out var value))
                {
                    amount += value.size;
                }
            }

            EventManager.Invoke(new OnLoadBundle(names.Count, amount));
            foreach (var deleteData in clientData.Keys)
            {
                var targetPath = GlobalSetting.TargetPath.Format(deleteData);
                if (File.Exists(targetPath))
                {
                    File.Delete(targetPath);
                }
            }

            var success = await LoadBundleRequest(names);
            if (success)
            {
                await File.WriteAllTextAsync(persistentData, serverRequest);
            }

            EventManager.Invoke(new OnBundleComplete(1, success ? "更新完成!" : "更新失败!"));
        }

        private static async Task<bool> LoadBundleRequest(HashSet<string> names)
        {
            var copies = new HashSet<string>(names);
            for (var i = 0; i < 5; i++)
            {
                foreach (var name in names)
                {
                    var serverPath = GlobalSetting.ServerPath.Format(name);
                    var webRequest = await LoadBundleRequest(serverPath, name);
                    var targetPath = GlobalSetting.TargetPath.Format(name);
                    await Task.Run(() => File.WriteAllBytes(targetPath, webRequest));
                    if (copies.Contains(name))
                    {
                        copies.Remove(name);
                    }
                }

                if (copies.Count == 0)
                {
                    break;
                }
            }

            return copies.Count == 0;
        }

        private static async Task<string> LoadServerRequest(string uri, string name)
        {
            for (var i = 0; i < 5; i++)
            {
                using (var request = UnityWebRequest.Head(uri))
                {
                    request.timeout = 1;
                    await request.SendWebRequest();
                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        continue;
                    }
                }

                using (var request = UnityWebRequest.Get(uri))
                {
                    await request.SendWebRequest();
                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        Debug.Log("请求服务器下载 {0} 失败!\n".Format(name));
                        continue;
                    }

                    return request.downloadHandler.text;
                }
            }

            return null;
        }

        private static async Task<byte[]> LoadBundleRequest(string uri, string name)
        {
            using (var request = UnityWebRequest.Head(uri))
            {
                request.timeout = 1;
                await request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log("请求服务器校验 {0} 失败!\n".Format(name));
                    return null;
                }
            }

            using (var request = UnityWebRequest.Get(uri))
            {
                var result = request.SendWebRequest();
                while (!result.isDone && Instance)
                {
                    EventManager.Invoke(new OnBundleUpdate(name, request.downloadProgress));
                    await Task.Yield();
                }

                EventManager.Invoke(new OnBundleUpdate(name, 1));
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log("请求服务器下载 {0} 失败!\n".Format(name));
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