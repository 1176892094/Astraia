// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-09-02 15:09:54
// # Recently: 2026-09-02 15:11:54
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Astraia
{
    internal class GlobalSetting : ScriptableObject
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

#if ODIN_INSPECTOR
        [EnumToggleButtons]
#endif
        public AssetPlatform BuildTarget = AssetPlatform.StandaloneWindows;
#if ODIN_INSPECTOR
        [ShowInInspector]
#endif
        public int AssetVersion;
#if ODIN_INSPECTOR
        [ShowInInspector]
#endif
        public string RemotePath = "https://cdn.jsdelivr.net/gh/1176892094/AssetBundles@main";
#if ODIN_INSPECTOR
        [ShowInInspector]
#endif
        public bool UseSimulate = true;
#if ODIN_INSPECTOR
        [PropertyOrder(1)]
#endif
        public UnityEngine.TextAsset[] TextAssets;

        public static string TargetPlatform => Instance.BuildTarget.ToString();
        public static string PersistentData => Path.Combine(Application.persistentDataPath, "AssetBundles");
        public static string PersistentPath => Path.Combine(Application.persistentDataPath, "AssetBundles", "{0}");
        public static string StreamingAsset => Path.Combine(Application.streamingAssetsPath, TargetPlatform, "{0}");
        public static string ServerListData => Path.Combine(Instance.RemotePath, TargetPlatform, "{0}");
        public static string ServerDataPath => Path.Combine(Instance.RemotePath, TargetPlatform, Instance.AssetVersion.ToString(), "{0}");
        public static string LoadText(TextAsset asset) => Instance.TextAssets[(int)asset].text;
#if UNITY_EDITOR
        public const string OWNING = "Astraia.Table";
        public const string SCRIPT = "Assets/Scripts/程序集B";
        public const string TABLES = BUNDLE + "/" + SHEETS + "DataTable.asset";
        public const string ASMDEF = SCRIPT + "/" + DEFINE + ".asmdef";
        public const string PATH_A = SCRIPT + "/枚举类/{0}.cs";
        public const string PATH_B = SCRIPT + "/结构体/{0}.cs";
        public const string PATH_C = SCRIPT + "/数据表/{0}DataTable.cs";
        public const string NAME_A = OWNING + ".{0}Data" + "," + DEFINE;
        public const string NAME_B = OWNING + ".{0}DataTable";
#if ODIN_INSPECTOR
        [ShowInInspector]
#endif
        public bool useStreaming = true;
#if ODIN_INSPECTOR
        [ShowInInspector]
#endif
        public BuildAssetBundleOptions BuildOptions = BuildAssetBundleOptions.ChunkBasedCompression;
#if ODIN_INSPECTOR
        [PropertyOrder(1)]
#endif
        public List<Object> ignoreAssets = new List<Object>();
#if ODIN_INSPECTOR
        [HideInInspector]
#endif
        public Object[] sceneAssets = new Object[5];
#if ODIN_INSPECTOR
        [ShowInInspector]
#endif
        private static string BuildFolder => Path.Combine(Environment.CurrentDirectory, "AssetBundles");
#if ODIN_INSPECTOR
        [ShowInInspector]
#endif
        private static string BuildFolderPath => Instance.useStreaming ? Application.streamingAssetsPath : BuildFolder;
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

    internal enum TextAsset : byte
    {
        敏感词,
        数据表,
        枚举类,
        程序集,
        结构体,
        编辑器
    }
}