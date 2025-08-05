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
using System.Threading.Tasks;
using Astraia.Common;
using UnityEditor;
using UnityEngine;

namespace Astraia
{
    internal static class EditorBuilding
    {
        private static readonly uint[] Table;

        static EditorBuilding()
        {
            Table = new uint[256];
            const uint POLYNOMIAL = 0xEDB88320;
            for (uint i = 0; i < Table.Length; ++i)
            {
                var result = i;
                for (var j = 0; j < 8; ++j)
                {
                    result = (result >> 1) ^ ((result & 1) == 1 ? POLYNOMIAL : 0);
                }

                Table[i] = result;
            }
        }

        private static uint Compute(string filePath)
        {
            using var stream = File.OpenRead(filePath);
            var buffer = new byte[8192];
            int bytesRead;
            var result = 0xFFFFFFFF;
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                for (var i = 0; i < bytesRead; i++)
                {
                    var index = (byte)((result & 0xFF) ^ buffer[i]);
                    result = (result >> 8) ^ Table[index];
                }
            }

            return ~result;
        }

        [MenuItem("Tools/Astraia/热更资源构建", priority = 3)]
        private static async void BuildAsset()
        {
            var folderPath = Directory.CreateDirectory(GlobalSetting.remoteAssetPath);
            BuildPipeline.BuildAssetBundles(GlobalSetting.remoteAssetPath, BuildAssetBundleOptions.None, (BuildTarget)GlobalSetting.Instance.assetPlatform);
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

                var nameHash = Compute(fileInfo.FullName).ToString("X8");
                if (isExists && fileHash.Contains(nameHash))
                {
                    filePacks.Add(new PackData(nameHash, fileInfo.Name, (int)fileInfo.Length));
                    continue;
                }

                await Task.Run(() =>
                {
                    var readBytes = File.ReadAllBytes(fileInfo.FullName);
                    readBytes = Service.Xor.Encrypt(readBytes);
                    File.WriteAllBytes(fileInfo.FullName, readBytes);
                });
                nameHash = Compute(fileInfo.FullName).ToString("X8");
                filePacks.Add(new PackData(nameHash, fileInfo.Name, (int)fileInfo.Length));
                Debug.Log(Service.Text.Format("加密AB包: {0}", fileInfo.FullName));
            }

            await File.WriteAllTextAsync(GlobalSetting.remoteAssetData, JsonManager.ToJson(filePacks));
            elapseTime = EditorApplication.timeSinceStartup - elapseTime;
            Debug.Log(Service.Text.Format("加密 AssetBundle 完成。耗时:<color=#00FF00> {0:F} </color>秒", elapseTime));
            AssetDatabase.Refresh();
        }
    }
}