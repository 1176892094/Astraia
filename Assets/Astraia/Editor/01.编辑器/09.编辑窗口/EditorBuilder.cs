// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-04-09 23:04:28
// // # Recently: 2025-04-09 23:04:28
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Astraia;
using Astraia.Common;
using UnityEditor;
using UnityEngine;

internal static class EditorBuilder
{
    private static string ComputeMD5(string filePath)
    {
        using var md5 = MD5.Create();
        using var stream = File.OpenRead(filePath);
        var hash = md5.ComputeHash(stream);
        var sb = new StringBuilder(hash.Length * 2);
        foreach (var b in hash)
        {
            sb.Append(b.ToString("X2"));
        }

        return sb.ToString();
    }


    [MenuItem("Tools/Astraia/热更资源构建", priority = 3)]
    private static async void BuildAsset()
    {
        var startTime = EditorApplication.timeSinceStartup;
        var folderPath = Directory.CreateDirectory(GlobalSetting.remoteAssetPath);
        BuildPipeline.BuildAssetBundles(GlobalSetting.remoteAssetPath, BuildAssetBundleOptions.None, (BuildTarget)GlobalSetting.Instance.assetPlatform);

        var oldHashes = new Dictionary<string, string>();
        if (File.Exists(GlobalSetting.remoteAssetData))
        {
            var readJson = await File.ReadAllTextAsync(GlobalSetting.remoteAssetData);
            var oldData = JsonManager.FromJson<List<PackData>>(readJson);
            oldHashes = oldData.ToDictionary(d => d.name, d => d.code);
        }

        var filePacks = new List<PackData>();
        var fileInfos = folderPath.GetFiles();

        foreach (var fileInfo in fileInfos)
        {
            if (fileInfo.Extension != "")
            {
                continue;
            }

            var md5Before = ComputeMD5(fileInfo.FullName);
            if (oldHashes.TryGetValue(fileInfo.Name, out var oldMd5) && md5Before == oldMd5)
            {
                filePacks.Add(new PackData(md5Before, fileInfo.Name, (int)fileInfo.Length));
                Debug.Log("跳过未变更文件: {0}".Format(fileInfo.Name));
                continue;
            }

            await Task.Run(() =>
            {
                var bytes = File.ReadAllBytes(fileInfo.FullName);
                bytes = Service.Xor.Encrypt(bytes);
                File.WriteAllBytes(fileInfo.FullName, bytes);
            });

            var md5After = ComputeMD5(fileInfo.FullName);
            filePacks.Add(new PackData(md5After, fileInfo.Name, (int)fileInfo.Length));
            Debug.Log("加密并更新文件: {0}".Color("G").Format(fileInfo.Name));
        }

        await File.WriteAllTextAsync(GlobalSetting.remoteAssetData, JsonManager.ToJson(filePacks));
        var elapsed = EditorApplication.timeSinceStartup - startTime;
        Debug.Log("加密 AssetBundle 完成。耗时: <color=#00FF00>{0:F2}</color> 秒".Format(elapsed));
        AssetDatabase.Refresh();
    }
}