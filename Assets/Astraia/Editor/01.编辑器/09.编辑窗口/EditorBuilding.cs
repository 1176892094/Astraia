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
using Astraia.Common;
using UnityEditor;
using UnityEngine;

namespace Astraia
{
    internal static class EditorBuilding
    {
        [MenuItem("Tools/Astraia/热更资源构建", priority = 3)]
        private static async void BuildAsset()
        {
            var folderPath = Directory.CreateDirectory(GlobalSetting.remoteAssetPath);
            var buildTarget = (BuildTarget)GlobalSetting.Instance.assetPlatform;
            BuildPipeline.BuildAssetBundles(GlobalSetting.remoteAssetPath, BuildAssetBundleOptions.None, buildTarget);
            var elapseTime = EditorApplication.timeSinceStartup;

            var fileHash = new HashSet<string>();
            var isExists = File.Exists(GlobalSetting.remoteAssetData);
            if (isExists)
            {
                var readJson = await File.ReadAllTextAsync(GlobalSetting.remoteAssetData);
                fileHash = JsonManager.FromJson<List<PackData>>(readJson).Select(data => data.code).ToHashSet();
            }

            var filePacks = new List<PackData>();
            var fileInfos = folderPath.GetFiles();
            foreach (var fileInfo in fileInfos)
            {
                if (fileInfo.Extension != "")
                {
                    continue;
                }

                if (isExists && fileHash.Contains(GetHashCode(fileInfo.FullName)))
                {
                    filePacks.Add(new PackData(GetHashCode(fileInfo.FullName), fileInfo.Name, (int)fileInfo.Length));
                    continue;
                }

                await Task.Run(() =>
                {
                    var readBytes = File.ReadAllBytes(fileInfo.FullName);
                    readBytes = Service.Xor.Encrypt(readBytes);
                    File.WriteAllBytes(fileInfo.FullName, readBytes);
                });
                filePacks.Add(new PackData(GetHashCode(fileInfo.FullName), fileInfo.Name, (int)fileInfo.Length));
                Debug.Log(Service.Text.Format("加密AB包: {0}", fileInfo.FullName));
            }

            var saveJson = JsonManager.ToJson(filePacks);
            await File.WriteAllTextAsync(GlobalSetting.remoteAssetData, saveJson);
            elapseTime = EditorApplication.timeSinceStartup - elapseTime;
            Debug.Log(Service.Text.Format("加密 AssetBundle 完成。耗时:<color=#00FF00> {0:F} </color>秒", elapseTime));
            AssetDatabase.Refresh();
        }

        private static string GetHashCode(string filePath)
        {
            using var provider = MD5.Create();
            using var fileStream = File.OpenRead(filePath);
            var computeHash = provider.ComputeHash(fileStream);
            var builder = new StringBuilder();
            foreach (var buffer in computeHash)
            {
                builder.Append(buffer.ToString("x2"));
            }

            return builder.ToString();
        }
    }
}