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

namespace Astraia.Common
{
    internal static class PackManager
    {
        public static async void LoadAssetData()
        {
            if (!GlobalManager.Instance) return;
            if (GlobalSetting.Instance.assetLoadMode == AssetMode.Simulate)
            {
                EventManager.Invoke(new PackComplete(0, "启动本地资源加载。"));
                return;
            }

            if (GlobalSetting.Instance.assetLoadMode == AssetMode.Authentic)
            {
                if (!Directory.Exists(GlobalSetting.assetPackPath))
                {
                    Directory.CreateDirectory(GlobalSetting.assetPackPath);
                }
            }

            var fileUri = GlobalSetting.GetServerPath(GlobalSetting.assetPackData);
            var serverRequest = await LoadServerRequest(GlobalSetting.assetPackData, fileUri);
            if (!string.IsNullOrEmpty(serverRequest))
            {
                var assetPacks = JsonManager.FromJson<List<PackData>>(serverRequest);
                foreach (var assetPack in assetPacks)
                {
                    GlobalManager.serverPacks.Add(assetPack.name, assetPack);
                }
            }
            else
            {
                EventManager.Invoke(new PackComplete(-1, "没有连接到服务器!"));
                return;
            }

            var persistentData = GlobalSetting.GetPacketPath(GlobalSetting.assetPackData);
            var streamingAsset = GlobalSetting.GetClientPath(GlobalSetting.assetPackData);
            var clientRequest = await LoadClientRequest(persistentData, streamingAsset);
            if (!string.IsNullOrEmpty(clientRequest))
            {
                var assetPacks = JsonManager.FromJson<List<PackData>>(clientRequest);
                foreach (var assetPack in assetPacks)
                {
                    GlobalManager.clientPacks.Add(assetPack.name, assetPack);
                }
            }

            var fileNames = new List<string>();
            foreach (var fileName in GlobalManager.serverPacks.Keys)
            {
                if (GlobalManager.clientPacks.TryGetValue(fileName, out var assetPack))
                {
                    if (GlobalManager.serverPacks[fileName] != assetPack)
                    {
                        fileNames.Add(fileName);
                    }

                    GlobalManager.clientPacks.Remove(fileName);
                }
                else
                {
                    fileNames.Add(fileName);
                }
            }

            var fileSizes = new int[fileNames.Count];
            for (int i = 0; i < fileNames.Count; i++)
            {
                if (GlobalManager.serverPacks.TryGetValue(fileNames[i], out var assetPack))
                {
                    fileSizes[i] = assetPack.size;
                }
            }

            EventManager.Invoke(new PackAwake(fileSizes));
            foreach (var clientPack in GlobalManager.clientPacks.Keys)
            {
                var filePath = GlobalSetting.GetPacketPath(clientPack);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }

            var success = await LoadPacketRequest(fileNames);
            if (success)
            {
                var filePath = GlobalSetting.GetPacketPath(GlobalSetting.assetPackData);
                await File.WriteAllTextAsync(filePath, serverRequest);
            }

            EventManager.Invoke(new PackComplete(1, success ? "更新完成!" : "更新失败!"));
        }

        private static async Task<bool> LoadPacketRequest(List<string> fileNames)
        {
            var packNames = new HashSet<string>(fileNames);
            for (var i = 0; i < 5; i++)
            {
                foreach (var packName in packNames)
                {
                    var packUri = GlobalSetting.GetServerPath(packName);
                    var packData = await LoadPacketRequest(packName, packUri);
                    var packPath = GlobalSetting.GetPacketPath(packName);
                    await Task.Run(() => File.WriteAllBytes(packPath, packData));
                    if (fileNames.Contains(packName))
                    {
                        fileNames.Remove(packName);
                    }
                }

                if (fileNames.Count == 0)
                {
                    break;
                }
            }

            return fileNames.Count == 0;
        }

        private static async Task<string> LoadServerRequest(string packName, string packUri)
        {
            for (var i = 0; i < 5; i++)
            {
                using (var request = UnityWebRequest.Head(packUri))
                {
                    request.timeout = 1;
                    await request.SendWebRequest();
                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        continue;
                    }
                }

                using (var request = UnityWebRequest.Get(packUri))
                {
                    await request.SendWebRequest();
                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        Debug.Log(Service.Text.Format("请求服务器下载 {0} 失败!\n", packName));
                        continue;
                    }

                    return request.downloadHandler.text;
                }
            }

            return null;
        }

        private static async Task<byte[]> LoadPacketRequest(string packName, string packUri)
        {
            using (var request = UnityWebRequest.Head(packUri))
            {
                request.timeout = 1;
                await request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log(Service.Text.Format("请求服务器校验 {0} 失败!\n", packName));
                    return null;
                }
            }

            using (var request = UnityWebRequest.Get(packUri))
            {
                var result = request.SendWebRequest();
                while (!result.isDone && GlobalSetting.Instance != null)
                {
                    EventManager.Invoke(new PackUpdate(packName, request.downloadProgress));
                    await Task.Yield();
                }

                EventManager.Invoke(new PackUpdate(packName, 1));
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log(Service.Text.Format("请求服务器下载 {0} 失败!\n", packName));
                    return null;
                }

                return request.downloadHandler.data;
            }
        }

        private static async Task<string> LoadClientRequest(string persistentData, string streamingAsset)
        {
            var (assetMode, assetPath) = await LoadRequest(persistentData, streamingAsset);
            string result = null;
            if (assetMode == 1)
            {
                result = await File.ReadAllTextAsync(assetPath);
            }
            else if (assetMode == 2)
            {
                using var request = UnityWebRequest.Get(assetPath);
                await request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success)
                {
                    result = request.downloadHandler.text;
                }
            }

            return result;
        }

        internal static async Task<AssetBundle> LoadAssetRequest(string persistentData, string streamingAsset)
        {
            var (assetMode, assetPath) = await LoadRequest(persistentData, streamingAsset);
            byte[] result = null;
            if (assetMode == 1)
            {
                result = await Task.Run(() => Service.Xor.Decrypt(File.ReadAllBytes(assetPath)));
            }
            else if (assetMode == 2)
            {
                using var request = UnityWebRequest.Get(assetPath);
                await request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success)
                {
                    result = await Task.Run(() => Service.Xor.Decrypt(request.downloadHandler.data));
                }
            }

            return GlobalSetting.Instance ? AssetBundle.LoadFromMemory(result) : null;
        }

#pragma warning disable CS1998
        private static async ValueTask<(int, string)> LoadRequest(string persistentData, string streamingAsset)
#pragma warning restore CS1998
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

        internal static void Dispose()
        {
            GlobalManager.clientPacks.Clear();
            GlobalManager.serverPacks.Clear();
        }
    }
}