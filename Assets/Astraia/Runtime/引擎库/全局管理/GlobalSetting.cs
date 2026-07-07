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
using Astraia.Core;
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
#if UNITY_EDITOR && ODIN_INSPECTOR
        [PropertyOrder(1)]
#endif
        public string[] EncryptGroup;

        public int AssetVersion;

        public string RemotePath = "https://cdn.jsdelivr.net/gh/1176892094/AssetBundles@main";

        public static string BundlePath => Path.Combine(Application.persistentDataPath, "AssetBundles");
        public static string TargetPath => Path.Combine(Application.persistentDataPath, "AssetBundles", "{0}");
        public static string OutputPath => Path.Combine(Application.temporaryCachePath, "AssetBundles");
        public static string StreamPath => Path.Combine(Application.streamingAssetsPath, Instance.BuildTarget.ToString(), "{0}");
        public static string ServerPath => Path.Combine(Instance.RemotePath, Instance.BuildTarget.ToString(), "{0}");
        public static string PacketPath => Path.Combine(Instance.RemotePath, Instance.BuildTarget.ToString(), Instance.AssetVersion.ToString(), "{0}");

        private static readonly Dictionary<AssetData, TextAsset> TextAssets = new Dictionary<AssetData, TextAsset>();

        public static string LoadAsset(AssetData option)
        {
            if (TextAssets.Count <= 0)
            {
                var items = Resources.LoadAll<TextAsset>(nameof(GlobalSetting));
                for (var i = 0; i < items.Length; i++)
                {
                    TextAssets[(AssetData)i] = items[i];
                }
            }

            return TextAssets[option].text;
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
        public static string BuildLocalPath => Path.Combine(Environment.CurrentDirectory, "AssetBundles");
#if ODIN_INSPECTOR
        [ShowInInspector]
#endif
        public static string BuildLocalData => Instance.BuildPath == BuildMode.AssetBundlePath ? BuildLocalPath : Application.streamingAssetsPath;
#if ODIN_INSPECTOR
        [ShowInInspector]
#endif
        public static string BuildAssetPath => Path.Combine(BuildLocalData, Instance.BuildTarget.ToString());
#if ODIN_INSPECTOR
        [ShowInInspector]
#endif
        public static string BuildCryptPath => Path.Combine(BuildLocalData, Instance.BuildTarget.ToString(), Instance.AssetVersion.ToString());
#if ODIN_INSPECTOR
        [ShowInInspector]
#endif
        public static string BuildAssetData => Path.Combine(BuildLocalData, Instance.BuildTarget.ToString(), VERIFY);
#endif
    }
}