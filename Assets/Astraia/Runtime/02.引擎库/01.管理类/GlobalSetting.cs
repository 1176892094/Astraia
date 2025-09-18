// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-04-09 21:04:33
// // # Recently: 2025-04-09 21:04:33
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Astraia.Common;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Astraia
{
    internal partial class GlobalSetting : ScriptableObject
    {
        private static readonly Dictionary<AssetText, TextAsset> itemText = new Dictionary<AssetText, TextAsset>();
        private static GlobalSetting instance;

        public static GlobalSetting Instance => instance ??= Resources.Load<GlobalSetting>(nameof(GlobalSetting));


        public const string ASSET_PACK = "AssetBundles";
        public const string ASSET_PATH = "Assets/AssetBundles";
        public const string ASSET_JSON = "AssetBundle.json";
        public const string ASSET_DATA = "HotUpdate.Data";

        public AssetPlatform assetPlatform = AssetPlatform.StandaloneWindows;
        public string smtpServer = "smtp.qq.com";
        public int smtpPort = 587;
        public string smtpUsername = "1176892094@qq.com";
        public string smtpPassword;
        public BadWordFilter badWordFilter = BadWordFilter.Enable;
        public byte secretVersion = 1;
#if UNITY_EDITOR && ODIN_INSPECTOR
        [OnValueChanged("UpdateSceneSetting")]
#endif
        public AssetMode assetLoadMode = AssetMode.Simulate;

        public string assetLocalPath = "http://192.168.0.3:8000/AssetBundles";
        public string assetRemotePath = "http://192.168.0.3:8000/AssetBundles";
#if UNITY_EDITOR && ODIN_INSPECTOR
        [PropertyOrder(2)]
#endif
        public string[] secretGroup;
#if UNITY_EDITOR && ODIN_INSPECTOR
        [ShowInInspector]
        [PropertyOrder(1)]
#endif
        public static string downloadLocalPath => "{0}/{1}".Format(Application.persistentDataPath, ASSET_PACK);
#if UNITY_EDITOR && ODIN_INSPECTOR
        [ShowInInspector]
        [PropertyOrder(1)]
#endif
        private static string downloadRemotePath => Instance.assetLoadMode == AssetMode.Remote ? Instance.assetRemotePath : Instance.assetLocalPath;
#if UNITY_EDITOR && ODIN_INSPECTOR
        [ShowInInspector]
        [PropertyOrder(1)]
#endif
        public static string downloadRemoteData => "{0}/{1}".Format(downloadRemotePath, ASSET_PACK);
        
        public static string GetTextByIndex(AssetText option)
        {
            if (itemText.Count > 0)
            {
                return itemText[option].text;
            }

            var items = Resources.LoadAll<TextAsset>(nameof(GlobalSetting));
            for (int i = 0; i < items.Length; i++)
            {
                itemText.Add((AssetText)i, items[i]);
            }

            return itemText[option].text;
        }

        public static string GetScenePath(string assetName) => "Scenes/{0}".Format(assetName);

        public static string GetAudioPath(string assetName) => "Audios/{0}".Format(assetName);

        public static string GetPanelPath(string assetName) => "Prefabs/{0}".Format(assetName);

        public static string GetTablePath(string assetName) => "DataTable/{0}".Format(assetName);

        public static string GetEditorPath(string assetName) => "{0}/{1}.asset".Format(ASSET_PATH, GetTablePath(assetName));

        public static string GetBundlePath(string fileName) => Path.Combine(downloadLocalPath, fileName);

        public static string GetServerPath(string fileName) => Path.Combine(downloadRemoteData, Path.Combine(Instance.assetPlatform.ToString(), fileName));

        public static string GetClientPath(string fileName) => Path.Combine(Application.streamingAssetsPath, Path.Combine(Instance.assetPlatform.ToString(), fileName));

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RuntimeInitializeOnLoad()
        {
            var source = new GameObject(nameof(PoolManager)).AddComponent<GlobalManager>();
            source.canvas = new GameObject(nameof(UIManager)).AddComponent<Canvas>();
            source.canvas.renderMode = RenderMode.ScreenSpaceCamera;
            source.canvas.gameObject.layer = LayerMask.NameToLayer("UI");

            source.canvas.gameObject.AddComponent<GraphicRaycaster>();
            var canvas = source.canvas.gameObject.AddComponent<CanvasScaler>();
            canvas.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvas.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvas.matchWidthOrHeight = 0.5f;
            canvas.referenceResolution = new Vector2(1920, 1080);
            canvas.referencePixelsPerUnit = 64;
            DontDestroyOnLoad(source.canvas);

            for (byte i = 1; i < Instance.secretGroup.Length; i++)
            {
                Service.Xor.LoadData(i, Instance.secretGroup[i]);
            }

            if (Instance.badWordFilter != BadWordFilter.Disable)
            {
                Service.Word.LoadData(GetTextByIndex(AssetText.BadWord));
            }
        }

        public enum BadWordFilter : byte
        {
            Enable,
            Disable
        }
    }
#if UNITY_EDITOR
    internal partial class GlobalSetting
    {
        public BuildMode BuildPath = BuildMode.StreamingAssets;
#if ODIN_INSPECTOR
        [PropertyOrder(2)]
#endif
        public List<Object> sceneAssets = new List<Object>();
#if ODIN_INSPECTOR
        [PropertyOrder(2)]
#endif
        public List<Object> ignoreAssets = new List<Object>();
#if ODIN_INSPECTOR
        [ShowInInspector]
#endif
        public static string ScriptPath
        {
            get => EditorPrefs.GetString(nameof(ScriptPath), "Assets/Scripts/DataTable");
            set => EditorPrefs.SetString(nameof(ScriptPath), value);
        }

        public static string dataTablePath => "{0}/DataTable".Format(ASSET_PATH);
#if ODIN_INSPECTOR
        [ShowInInspector]
#endif
        public static string assemblyPath => "{0}/{1}.asmdef".Format(ScriptPath, ASSET_DATA);
#if ODIN_INSPECTOR
        [ShowInInspector]
#endif
        public static string remoteBuildData => Path.GetDirectoryName(Application.dataPath);
#if ODIN_INSPECTOR
        [ShowInInspector]
#endif
        public static string remoteBuildPath => Instance.BuildPath == BuildMode.BuildPath ? Path.Combine(remoteBuildData, ASSET_PACK) : Application.streamingAssetsPath;
#if ODIN_INSPECTOR
        [ShowInInspector]
#endif
        public static string remoteAssetPath => Path.Combine(remoteBuildPath, Instance.assetPlatform.ToString());
#if ODIN_INSPECTOR
        [ShowInInspector]
#endif
        public static string remoteAssetData => "{0}/{1}".Format(remoteAssetPath, ASSET_JSON);

        public static string GetEnumPath(string name) => "{0}/01.枚举类/{1}.cs".Format(ScriptPath, name);

        public static string GetItemPath(string name) => "{0}/02.结构体/{1}.cs".Format(ScriptPath, name);

        public static string GetDataPath(string name) => "{0}/03.数据表/{1}DataTable.cs".Format(ScriptPath, name);

        public static string GetAssetPath(string name) => "{0}/{1}DataTable.asset".Format(dataTablePath, name);

        public static string GetDataName(string name) => "Astraia.Table.{0}Data,{1}".Format(name, ASSET_DATA);

        public static string GetTableName(string name) => "Astraia.Table.{0}DataTable".Format(name);

        public static void UpdateSceneSetting()
        {
            var assets = EditorBuildSettings.scenes.Select(scene => scene.path).ToHashSet();
            foreach (var sceneAsset in Instance.sceneAssets)
            {
                var scenePath = AssetDatabase.GetAssetPath(sceneAsset);
                if (assets.Contains(scenePath))
                {
                    if (Instance.assetLoadMode == AssetMode.Simulate) continue;
                    var scenes = EditorBuildSettings.scenes.Where(scene => scene.path != scenePath);
                    EditorBuildSettings.scenes = scenes.ToArray();
                }
                else
                {
                    if (Instance.assetLoadMode != AssetMode.Simulate) continue;
                    var scenes = EditorBuildSettings.scenes.ToList();
                    scenes.Add(new EditorBuildSettingsScene(scenePath, true));
                    EditorBuildSettings.scenes = scenes.ToArray();
                }
            }
        }

        public static readonly List<string> modules = new List<string>();
        public static readonly List<string> systems = new List<string>();

        public static void LoadSetting(Type result)
        {
            if (result.IsAbstract)
            {
                return;
            }

            if (result.IsGenericType)
            {
                return;
            }

            if (typeof(IModule).IsAssignableFrom(result))
            {
                modules.Add("{0}, {1}".Format(result.FullName, result.Assembly.GetName().Name));
            }

            if (typeof(ISystem).IsAssignableFrom(result))
            {
                systems.Add("{0}, {1}".Format(result.FullName, result.Assembly.GetName().Name));
            }
        }

        public static void LoadComplete()
        {
            modules.Sort(StringComparer.Ordinal);
            systems.Sort(StringComparer.Ordinal);
        }
    }
#endif
}