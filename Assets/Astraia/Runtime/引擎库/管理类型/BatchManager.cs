// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2025-04-09 22:04:41
// # Recently: 2025-04-09 22:04:41
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Astraia
{
    public partial class AssetManager
    {
        public static event Action<long> OnLoadBatch;
        public static event Action<string, long> OnBatchUpdate;
        public static event Action<string, bool> OnBatchComplete;

        internal static async void Update()
        {
            if (GlobalSetting.Instance.AssetSimulate)
            {
                OnBatchComplete?.Invoke("启动本地资源加载。", false);
                return;
            }

            Directory.CreateDirectory(GlobalSetting.PersistentData);

            var streamResult = await LoadManifestFromStreamingAsync();
            var clientResult = await LoadManifestFromPersistentAsync();
            var serverResult = await LoadManifestFromServerAsync();

            var selected = SelectManifest(streamResult, clientResult, serverResult);
            Instance.SetVersion(selected);

            if (!selected.IsLoaded)
            {
                OnBatchComplete?.Invoke("没有找到可用资源。", false);
                return;
            }

            if (!selected.IsRemote && clientResult.Version >= selected.Version)
            {
                OnBatchComplete?.Invoke("本地资源无需更新。", true);
                return;
            }

            var toDownload = new Dictionary<string, Bundle>();
            var toDelete = new List<string>();

            var clientData = clientResult.Bundles;
            foreach (var (name, bundle) in selected.Bundles)
            {
                if (clientData.TryGetValue(name, out var clientBundle))
                {
                    if (bundle.Hash != clientBundle.Hash)
                    {
                        toDownload.Add(name, bundle);
                    }

                    clientData.Remove(name);
                }
                else
                {
                    toDownload.Add(name, bundle);
                }
            }

            toDelete.AddRange(clientData.Keys);
            OnLoadBatch?.Invoke(toDownload.Values.Sum(download => download.Size));

            foreach (var delete in toDelete)
            {
                var path = BuildPersistentPath(delete);
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }

            var success = await DownloadBundlesAsync(toDownload, selected.IsRemote);
            if (success)
            {
                await SaveManifestToPersistentAsync(selected);
                OnBatchComplete?.Invoke("更新完成！", true);
            }
            else
            {
                OnBatchComplete?.Invoke("更新失败！", true);
            }
        }

        private static async Task<bool> DownloadBundlesAsync(Dictionary<string, Bundle> toDownload, bool isRemote)
        {
            try
            {
                if (toDownload.Count == 0)
                {
                    return true;
                }

                var downloadTasks = new List<Task<bool>>();
                var downloadBytes = new Dictionary<string, long>();

                using var semaphore = new SemaphoreSlim(5);
                foreach (var name in toDownload.Keys)
                {
                    downloadTasks.Add(DownloadBundleAsync(name, isRemote, downloadBytes, semaphore));
                }

                var successes = await Task.WhenAll(downloadTasks);
                return successes.All(success => success);
            }
            catch
            {
                return false;
            }
        }

        private static async Task<bool> DownloadBundleAsync(string name, bool isRemote, Dictionary<string, long> downloadBytes, SemaphoreSlim semaphore)
        {
            await semaphore.WaitAsync();
            try
            {
                for (var retry = 0; retry < 5; retry++)
                {
                    if (retry > 0)
                    {
                        await Task.Delay(200 * retry);
                    }

                    var path = isRemote ? BuildRemotePath(name) : BuildStreamingPath(name);
                    var data = await ReadBundleDataAsync(name, path, isRemote, downloadBytes);
                    if (data != null && data.Length != 0)
                    {
                        await File.WriteAllBytesAsync(BuildPersistentPath(name), data);
                        return true;
                    }
                }

                Log.Warn("请求服务器下载 {0} 失败!\n".Format(name));
                return false;
            }
            finally
            {
                semaphore.Release();
            }
        }

        private static async Task<byte[]> ReadBundleDataAsync(string name, string path, bool isRemote, Dictionary<string, long> downloadBytes)
        {
            if (isRemote)
            {
                using var request = UnityWebRequest.Get(path);
                var operation = request.SendWebRequest();

                while (!operation.isDone)
                {
                    downloadBytes[name] = (long)request.downloadedBytes;
                    OnBatchUpdate?.Invoke(name, downloadBytes.Values.Sum());

                    await Task.Yield();
                }

                downloadBytes[name] = (long)request.downloadedBytes;
                OnBatchUpdate?.Invoke(name, downloadBytes.Values.Sum());
                return request.result == UnityWebRequest.Result.Success ? request.downloadHandler.data : null;
            }

            return await ReadDataAsync(path);
        }

        private static string BuildRemotePath(string name)
        {
            return GlobalSetting.ServerDataPath.Format(name);
        }

        private static string BuildStreamingPath(string name)
        {
            return GlobalSetting.StreamingAsset.Format(name);
        }

        private static string BuildPersistentPath(string name)
        {
            return GlobalSetting.PersistentPath.Format(name);
        }

        private static async Task SaveManifestToPersistentAsync(Manifest manifest)
        {
            var json = JsonManager.ToJson(new Package(manifest.Version, manifest.Bundles.Values.ToList()));
            var path = GlobalSetting.PersistentPath.Format(GlobalSetting.VERIFY);
            await File.WriteAllTextAsync(path, json);
        }

        private static async Task<byte[]> ReadDataAsync(string path)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            using var request = UnityWebRequest.Get(path);
            await request.SendWebRequest();
            return request.result == UnityWebRequest.Result.Success ? request.downloadHandler.data : null;
#else
            return File.Exists(path) ? await File.ReadAllBytesAsync(path) : null;
#endif
        }

        private static async Task<string> ReadTextAsync(string path)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            using var request = UnityWebRequest.Get(path);
            await request.SendWebRequest();
            return request.result == UnityWebRequest.Result.Success ? request.downloadHandler.text : null;
#else
            return File.Exists(path) ? await File.ReadAllTextAsync(path) : null;
#endif
        }

        private static async Task<Manifest> LoadManifestFromStreamingAsync()
        {
            var path = GlobalSetting.StreamingAsset.Format(GlobalSetting.VERIFY);
            var json = await ReadTextAsync(path);
            return LoadManifestFromJson(json, false);
        }

        private static async Task<Manifest> LoadManifestFromPersistentAsync()
        {
            var path = GlobalSetting.PersistentPath.Format(GlobalSetting.VERIFY);
            var json = File.Exists(path) ? await File.ReadAllTextAsync(path) : null;
            return LoadManifestFromJson(json, false);
        }

        private static async Task<Manifest> LoadManifestFromServerAsync()
        {
            var path = GlobalSetting.ServerListData.Format(GlobalSetting.VERIFY);
            var json = string.Empty;
            try
            {
                for (var i = 0; i < 5; i++)
                {
                    using var request = UnityWebRequest.Get(path);
                    request.timeout = 5;
                    await request.SendWebRequest();
                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        Log.Warn("请求服务器下载 {0} 失败!\n".Format(GlobalSetting.VERIFY));
                        await Task.Delay(200);
                        continue;
                    }

                    json = request.downloadHandler.text;
                    break;
                }

                return LoadManifestFromJson(json, true);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("{0}\n{1}\n{2}".Format(path, json, e));
            }
        }

        private static Manifest LoadManifestFromJson(string json, bool remote)
        {
            var bundles = new Dictionary<string, Bundle>();
            if (string.IsNullOrEmpty(json))
            {
                return new Manifest(-1, bundles, false, remote);
            }

            var result = JsonManager.FromJson<Package>(json);

            if (result.Bundles != null)
            {
                foreach (var bundle in result.Bundles)
                {
                    bundles[bundle.Name] = bundle;
                }
            }

            return new Manifest(result.Version, bundles, true, remote);
        }

        private static Manifest SelectManifest(Manifest stream, Manifest client, Manifest server)
        {
            if (server.IsLoaded)
            {
                return server.Version > stream.Version ? server : stream;
            }

            if (client.IsLoaded)
            {
                return client.Version > stream.Version ? client : stream;
            }

            return stream.IsLoaded ? stream : default;
        }
    }

    internal readonly struct Manifest
    {
        public readonly int Version;
        public readonly bool IsLoaded;
        public readonly bool IsRemote;
        public readonly Dictionary<string, Bundle> Bundles;

        public Manifest(int version, Dictionary<string, Bundle> bundles, bool isLoaded, bool isRemote)
        {
            Version = version;
            Bundles = bundles;
            IsLoaded = isLoaded;
            IsRemote = isRemote;
        }
    }

    [Serializable]
    internal struct Package
    {
        public int Version;
        public List<Bundle> Bundles;

        public Package(int version, List<Bundle> bundles)
        {
            Version = version;
            Bundles = bundles;
        }
    }

    [Serializable]
    internal struct Bundle
    {
        public long Size;
        public string Name;
        public string Hash;

        public Bundle(long size, string name, string hash)
        {
            Size = size;
            Name = name;
            Hash = hash;
        }
    }
}