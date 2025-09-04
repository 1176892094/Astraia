// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2024-12-23 18:12:21
// # Recently: 2025-01-08 17:01:43
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System.IO;
using UnityEngine;

namespace Astraia.Common
{
    public static partial class JsonManager
    {
        public static void Save<T>(T data, string name)
        {
            var path = LoadPath(name);
            var json = ToJson(data);
            File.WriteAllText(path, json);
        }

        public static void Load<T>(T data, string name)
        {
            var path = LoadPath(name);
            if (!File.Exists(path))
            {
                Save(data, name);
            }

            var json = File.ReadAllText(path);
            FromJson(json, data);
        }

        public static T Load<T>(string name, T data = default)
        {
            var path = LoadPath(name);
            if (!File.Exists(path))
            {
                Save(data, name);
            }

            var json = File.ReadAllText(path);
            return FromJson<T>(json);
        }

        public static void Encrypt<T>(T data, string name)
        {
            var path = LoadPath(name);
            var json = ToJson(data);
            json = Service.Zip.Compress(json);
            var item = Service.Text.GetBytes(json);
            item = Service.Xor.Encrypt(item, GlobalSetting.Instance.secretVersion);
            File.WriteAllBytes(path, item);
        }

        public static void Decrypt<T>(T data, string name)
        {
            var path = LoadPath(name);
            if (!File.Exists(path))
            {
                Encrypt(data, name);
            }

            var item = File.ReadAllBytes(path);
            item = Service.Xor.Decrypt(item);
            var json = Service.Text.GetString(item);
            json = Service.Zip.Decompress(json);
            FromJson(json, data);
        }

        public static T Decrypt<T>(string name, T data = default)
        {
            var path = LoadPath(name);
            if (!File.Exists(path))
            {
                Encrypt(data, name);
            }

            var item = File.ReadAllBytes(path);
            item = Service.Xor.Decrypt(item);
            var json = Service.Text.GetString(item);
            json = Service.Zip.Decompress(json);
            return FromJson<T>(json);
        }

        private static string LoadPath(string fileName)
        {
            var jsonPath = Path.Combine(Application.streamingAssetsPath, nameof(JsonManager));
            if (Directory.Exists(jsonPath))
            {
                var filePath = Path.Combine(jsonPath, Service.Text.Format("{0}.json", fileName));
                if (File.Exists(filePath))
                {
                    return filePath;
                }
            }

            jsonPath = Path.Combine(Application.persistentDataPath, nameof(JsonManager));
            if (!Directory.Exists(jsonPath))
            {
                Directory.CreateDirectory(jsonPath);
            }

            return Path.Combine(jsonPath, Service.Text.Format("{0}.json", fileName));
        }
    }
}