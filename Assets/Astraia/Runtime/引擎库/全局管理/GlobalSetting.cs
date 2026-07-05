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
using System.Linq;
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
        public const string TARGET = "{0}/AssetBundles";
        public const string DEFINE = "HotUpdate.Data";
        public const string VERIFY = "AssetBundle.json";
        public const string BUNDLE = "Assets/AssetBundles";

#if UNITY_EDITOR && ODIN_INSPECTOR
        [EnumToggleButtons]
#endif
        public AssetPlatform BuildTarget = AssetPlatform.StandaloneWindows;
#if UNITY_EDITOR && ODIN_INSPECTOR
        [EnumToggleButtons]
#endif
        public bool BuildLoader;
#if UNITY_EDITOR && ODIN_INSPECTOR
        [ShowIf("BuildLoader")] [ValueDropdown("UpdateEncryptKey")]
#endif
        public int BuildEncrypt;
#if UNITY_EDITOR && ODIN_INSPECTOR
        [ShowIf("BuildLoader")]
#endif
        public string RemotePath = "https://cdn.jsdelivr.net/gh/1176892094/AssetBundles@main";

#if UNITY_EDITOR && ODIN_INSPECTOR
        [PropertyOrder(1)]
#endif
        public string[] EncryptGroup;

        public static string EditorPath => TARGET.Format(Path.GetDirectoryName(Application.dataPath));
        public static string BundlePath => TARGET.Format(Application.persistentDataPath);
        public static string TargetPath => TARGET.Format(Application.persistentDataPath) + "/{0}";
        public static string ServerPath => Path.Combine(Instance.RemotePath, Instance.BuildTarget + "/{0}");
        public static string ClientPath => Path.Combine(Application.streamingAssetsPath, Instance.BuildTarget + "/{0}");

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RuntimeInitializeOnLoad()
        {
            Xor.LoadData(Instance.EncryptGroup);
            Bad.LoadData(LoadAsset(AssetData.BadWord));
            Log.Setup(Debug.Log, Debug.LogWarning, Debug.LogError);
        }

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
        public static string RemoteBuildPath => Instance.BuildPath == BuildMode.AssetBundlePath ? EditorPath : Application.streamingAssetsPath;
#if ODIN_INSPECTOR
        [ShowInInspector]
#endif
        public static string RemoteAssetPath => Path.Combine(RemoteBuildPath, Instance.BuildTarget.ToString());
#if ODIN_INSPECTOR
        [ShowInInspector]
#endif
        public static string RemoteAssetData => Path.Combine(RemoteAssetPath, VERIFY);
#if ODIN_INSPECTOR
        private IEnumerable<ValueDropdownItem<int>> UpdateEncryptKey => EncryptGroup.Select((item, i) => new ValueDropdownItem<int>(item, i));
#endif
#endif
    }
}