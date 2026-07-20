// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2025-04-09 21:04:33
// # Recently: 2025-04-09 21:04:33
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Astraia
{
    [Serializable]
    internal sealed class GlobalSetting : ScriptableObject
    {
        private static GlobalSetting instance;

        public static GlobalSetting Instance => instance ??= Resources.Load<GlobalSetting>(nameof(GlobalSetting));

        public const string SCENES = "Scenes/{0}";
        public const string AUDIOS = "Audios/{0}";
        public const string PREFAB = "Prefabs/{0}";
        public const string SHEETS = "DataTable/{0}";
        public const string DEFINE = "HotUpdate.Data";
        public const string VERIFY = "AssetBundle.json";
        public const string BUNDLE = "Assets/AssetBundles";

#if UNITY_EDITOR && ODIN_INSPECTOR
        [EnumToggleButtons]
#endif
        public AssetPlatform BuildTarget = AssetPlatform.StandaloneWindows;

        public int AssetVersion;

        public bool AssetSimulate = true;

        public string RemotePath = "https://cdn.jsdelivr.net/gh/1176892094/AssetBundles@main";

        public static string TargetPlatform => Instance.BuildTarget.ToString();
        public static string PersistentData => Path.Combine(Application.persistentDataPath, "AssetBundles");
        public static string PersistentPath => Path.Combine(Application.persistentDataPath, "AssetBundles", "{0}");
        public static string StreamingAsset => Path.Combine(Application.streamingAssetsPath, TargetPlatform, "{0}");
        public static string ServerListData => Path.Combine(Instance.RemotePath, TargetPlatform, "{0}");
        public static string ServerDataPath => Path.Combine(Instance.RemotePath, TargetPlatform, Instance.AssetVersion.ToString(), "{0}");

        private static readonly Dictionary<AssetData, TextAsset> TextCache = new Dictionary<AssetData, TextAsset>();

        public static string LoadText(AssetData option)
        {
            if (TextCache.Count <= 0)
            {
                var items = Resources.LoadAll<TextAsset>(nameof(GlobalSetting));
                for (var i = 0; i < items.Length; i++)
                {
                    TextCache[(AssetData)i] = items[i];
                }
            }

            return TextCache[option].text;
        }

#if UNITY_EDITOR
        public const string SCRIPT = "Assets/Scripts/程序集B";
        public const string TABLES = BUNDLE + "/" + SHEETS + "DataTable.asset";
        public const string ASMDEF = SCRIPT + "/" + DEFINE + ".asmdef";
        public const string PATH_A = SCRIPT + "/枚举类/{0}.cs";
        public const string PATH_B = SCRIPT + "/结构体/{0}.cs";
        public const string PATH_C = SCRIPT + "/数据表/{0}DataTable.cs";
        public const string NAME_A = "Astraia.Table.{0}Data" + "," + DEFINE;
        public const string NAME_B = "Astraia.Table.{0}DataTable";
#if ODIN_INSPECTOR
        [EnumToggleButtons]
#endif
        public BuildMode BuildPath = BuildMode.StreamingAssets;
#if ODIN_INSPECTOR
        [ShowInInspector]
#endif
        public UnityEditor.BuildAssetBundleOptions BuildOptions;
#if ODIN_INSPECTOR
        [PropertyOrder(1)]
#endif
        public List<Object> ignoreAssets = new List<Object>();
#if ODIN_INSPECTOR
        [HideInInspector]
#endif
        public List<Object> cacheAssets = new List<Object>();
#if ODIN_INSPECTOR
        [ShowInInspector]
#endif
        private static string BuildFolder => Path.Combine(Environment.CurrentDirectory, "AssetBundles");
#if ODIN_INSPECTOR
        [ShowInInspector]
#endif
        private static string BuildFolderPath => Instance.BuildPath == BuildMode.AssetBundlePath ? BuildFolder : Application.streamingAssetsPath;
#if ODIN_INSPECTOR
        [ShowInInspector]
#endif
        public static string BuildTargetPath => Path.Combine(BuildFolderPath, TargetPlatform, TargetPlatform);
#if ODIN_INSPECTOR
        [ShowInInspector]
#endif
        public static string BuildTargetJson => Path.Combine(BuildFolderPath, TargetPlatform, VERIFY);
#if ODIN_INSPECTOR
        [ShowInInspector]
#endif
        public static string BuildVersion => Path.Combine(BuildFolderPath, TargetPlatform, Instance.AssetVersion.ToString());
#endif
    }

    internal enum AssetPlatform : byte
    {
        StandaloneOSX = 2,
        StandaloneWindows = 5,
        IOS = 9,
        Android = 13
    }

    internal enum AssetData : byte
    {
        Assembly,
        Enum,
        Struct,
        DataTable,
        BadWord,
        Icons,
    }

    internal enum BuildMode : byte
    {
        AssetBundlePath,
        StreamingAssets
    }
}