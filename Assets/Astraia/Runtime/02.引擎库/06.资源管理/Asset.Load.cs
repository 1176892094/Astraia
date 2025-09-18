// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-09-18 01:09:26
// // # Recently: 2025-09-18 01:09:26
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Astraia.Common
{
    using static GlobalManager;

    public static partial class AssetManager
    {
        private static class Actuator
        {
            public static T LoadAt<T>(string path, AssetBundle pack) where T : Object
            {
                if (pack == null) return null;
                var asset = pack.LoadAsset<T>(path);
                return asset is GameObject ? Object.Instantiate(asset) : asset;
            }

            public static T[] LoadBy<T>(string path, AssetBundle pack) where T : Object
            {
                return pack.LoadAssetWithSubAssets<T>(path);
            }
        }

        private static class Resource
        {
            public static T LoadAt<T>(string path) where T : Object
            {
                var asset = Resources.Load<T>(path);
                return asset is GameObject ? Object.Instantiate(asset) : asset;
            }

            public static T[] LoadBy<T>(string path) where T : Object
            {
                return Resources.LoadAll<T>(path);
            }
        }

        private static class Simulate
        {
            public static T LoadAt<T>(string path) where T : Object
            {
#if UNITY_EDITOR
                if (assetPath.TryGetValue(path, out var data))
                {
                    var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(data);
                    return asset is GameObject ? Object.Instantiate(asset) : asset;
                }

                return null;
#else
                return null;
#endif
            }

            public static T[] LoadBy<T>(string path) where T : Object
            {
#if UNITY_EDITOR
                if (assetPath.TryGetValue(path, out var data))
                {
                    return UnityEditor.AssetDatabase.LoadAllAssetRepresentationsAtPath(data).Cast<T>().ToArray();
                }

                return null;
#else
                return null;
#endif
            }
        }
    }
}