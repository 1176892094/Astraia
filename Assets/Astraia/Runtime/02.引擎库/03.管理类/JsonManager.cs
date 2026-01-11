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

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Astraia.Common
{
    public static class JsonManager
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
            item = Service.Xor.Encrypt(item, GlobalSetting.Instance.EncryptKey);
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
            var streamingRoot = Path.Combine(Application.streamingAssetsPath, nameof(JsonManager));
            var streamingName = Path.Combine(streamingRoot, fileName + ".json");
            if (File.Exists(streamingName))
            {
                return streamingName;
            }
            
            var persistentRoot = Path.Combine(Application.persistentDataPath, nameof(JsonManager));
            var persistentName = Path.GetDirectoryName(fileName);
            var persistentPath = string.IsNullOrEmpty(persistentName) ? persistentRoot : Path.Combine(persistentRoot, persistentName);
            Directory.CreateDirectory(persistentPath);
            return Path.Combine(persistentPath, Path.GetFileName(fileName) + ".json");
        }

        [Serializable]
        private class JsonMapper<T>
        {
            public T value;

            public JsonMapper(T value)
            {
                this.value = value;
            }

            public static bool IsArray
            {
                get
                {
                    if (typeof(T).IsArray)
                    {
                        return true;
                    }

                    if (typeof(T) == typeof(string))
                    {
                        return true;
                    }

                    if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(List<>))
                    {
                        return true;
                    }

                    return typeof(T).IsPrimitive;
                }
            }
        }

        public static string ToJson<T>(T data)
        {
            if (JsonMapper<T>.IsArray)
            {
                return JsonUtility.ToJson(new JsonMapper<T>(data));
            }

            return JsonUtility.ToJson(data);
        }

        public static void FromJson<T>(string json, T data)
        {
            if (JsonMapper<T>.IsArray)
            {
                JsonUtility.FromJsonOverwrite(json, new JsonMapper<T>(data));
            }

            JsonUtility.FromJsonOverwrite(json, data);
        }

        public static T FromJson<T>(string json)
        {
            if (JsonMapper<T>.IsArray)
            {
                return JsonUtility.FromJson<JsonMapper<T>>(json).value;
            }

            return JsonUtility.FromJson<T>(json);
        }
    }
}