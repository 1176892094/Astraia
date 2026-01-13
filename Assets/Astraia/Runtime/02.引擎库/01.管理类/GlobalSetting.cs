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
    internal sealed partial class GlobalSetting : ScriptableObject
    {
        private static GlobalSetting instance;

        public static GlobalSetting Instance => instance ??= Resources.Load<GlobalSetting>(nameof(GlobalSetting));

        public const string Scene = "Scenes/{0}";
        public const string Audio = "Audios/{0}";
        public const string Table = "DataTable/{0}";
        public const string Prefab = "Prefabs/{0}";
        public const string Define = "HotUpdate.Data";
        public const string Target = "{0}/AssetBundles";
        public const string Verify = "AssetBundle.json";
        public const string Bundle = "Assets/AssetBundles";
#if UNITY_EDITOR && ODIN_INSPECTOR
        [EnumToggleButtons]
#endif
        public AssetPlatform BuildTarget = AssetPlatform.StandaloneWindows;
#if UNITY_EDITOR && ODIN_INSPECTOR
        [EnumToggleButtons] [OnValueChanged("UpdateSceneSetting")]
#endif
        public AssetMode AssetMode = AssetMode.Resource;
#if UNITY_EDITOR && ODIN_INSPECTOR
        [ShowIf("AssetMode", AssetMode.Resource)] [ValueDropdown("UpdateEncryptGroup")]
#endif
        public byte EncryptKey = 1;
#if UNITY_EDITOR && ODIN_INSPECTOR
        [ShowIf("AssetMode", AssetMode.Simulate)]
#endif
        public string LocalPath = "http://192.168.0.1:8000/AssetBundles";
#if UNITY_EDITOR && ODIN_INSPECTOR
        [ShowIf("AssetMode", AssetMode.Actuator)]
#endif
        public string RemotePath = "http://192.168.0.1:8000";
#if UNITY_EDITOR && ODIN_INSPECTOR
        [EnumToggleButtons]
#endif
        public InputMask InputMask = InputMask.Enable;
#if UNITY_EDITOR && ODIN_INSPECTOR
        [PropertyOrder(1)]
#endif
        public string[] EncryptGroup;

        public static string EditorPath => Target.Format(Path.GetDirectoryName(Application.dataPath));
        public static string OptionPath => Instance.AssetMode == AssetMode.Actuator ? Instance.RemotePath : Instance.LocalPath;
        public static string BundlePath => Target.Format(Application.persistentDataPath);
        public static string TargetPath => Target.Format(Application.persistentDataPath) + "/{0}";
        public static string ServerPath => Path.Combine(Target.Format(OptionPath), Instance.BuildTarget + "/{0}");
        public static string ClientPath => Path.Combine(Application.streamingAssetsPath, Instance.BuildTarget + "/{0}");

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
            canvas.matchWidthOrHeight = 0;
            canvas.referenceResolution = new Vector2(960, 540);
            canvas.referencePixelsPerUnit = 16;
            DontDestroyOnLoad(source.canvas);

            for (byte i = 1; i < Instance.EncryptGroup.Length; i++)
            {
                Service.Xor.LoadData(i, Instance.EncryptGroup[i]);
            }

            if (Instance.InputMask != InputMask.Disable)
            {
                Service.Input.LoadData(LoadAsset(AssetData.Input));
            }

            Service.Log.Setup(Debug.Log, Debug.LogWarning, Debug.LogError);
        }

        private static readonly Dictionary<AssetData, TextAsset> assetData = new Dictionary<AssetData, TextAsset>();

        public static string LoadAsset(AssetData option)
        {
            if (assetData.Count > 0)
            {
                return assetData[option].text;
            }

            var items = Resources.LoadAll<TextAsset>(nameof(GlobalSetting));
            for (int i = 0; i < items.Length; i++)
            {
                assetData.Add((AssetData)i, items[i]);
            }

            return assetData[option].text;
        }
    }
#if UNITY_EDITOR
    internal partial class GlobalSetting
    {
        public const string Scripts = "Assets/Scripts/02.数据系统";
        public const string Assembly = Scripts + "/" + Define + ".asmdef";
        public const string EnumPath = Scripts + "/01.枚举类/{0}.cs";
        public const string ItemPath = Scripts + "/02.结构体/{0}.cs";
        public const string DataPath = Scripts + "/03.数据表/{0}DataTable.cs";
        public const string EditTable = Bundle + "/" + Table + "DataTable.asset";
        public const string SheetData = "Astraia.Table.{0}Data," + Define;
        public const string SheetName = "Astraia.Table.{0}DataTable";
#if ODIN_INSPECTOR
        [EnumToggleButtons]
#endif
        public BuildMode BuildPath = BuildMode.StreamingAssets;
#if ODIN_INSPECTOR
        [PropertyOrder(1)]
#endif
        public List<Object> sceneAssets = new List<Object>();
#if ODIN_INSPECTOR
        [PropertyOrder(1)]
#endif
        public List<Object> ignoreAssets = new List<Object>();
#if UNITY_EDITOR
        [PropertyOrder(1)]
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
        public static string RemoteAssetData => Path.Combine(RemoteAssetPath, Verify);
#if ODIN_INSPECTOR
        private IEnumerable<ValueDropdownItem<byte>> UpdateEncryptGroup()
        {
            for (byte i = 1; i <= EncryptGroup.Length; i++)
            {
                yield return new ValueDropdownItem<byte>(EncryptGroup[i - 1], i);
            }
        }
#endif
        public static void UpdateSceneSetting()
        {
            var assets = EditorBuildSettings.scenes.Select(scene => scene.path).ToHashSet();
            foreach (var sceneAsset in Instance.sceneAssets)
            {
                var scenePath = AssetDatabase.GetAssetPath(sceneAsset);
                if (assets.Contains(scenePath))
                {
                    if (Instance.AssetMode == AssetMode.Resource) continue;
                    var scenes = EditorBuildSettings.scenes.Where(scene => scene.path != scenePath);
                    EditorBuildSettings.scenes = scenes.ToArray();
                }
                else
                {
                    if (Instance.AssetMode != AssetMode.Resource) continue;
                    var scenes = EditorBuildSettings.scenes.ToList();
                    scenes.Add(new EditorBuildSettingsScene(scenePath, true));
                    EditorBuildSettings.scenes = scenes.ToArray();
                }
            }
        }

        public static readonly List<string> modules = new List<string>();

        public static void LoadSetting(Type result)
        {
            if (!result.IsAbstract && !result.IsGenericType)
            {
                if (typeof(IModule).IsAssignableFrom(result))
                {
                    modules.Add("{0}, {1}".Format(result.FullName, result.Assembly.GetName().Name));
                }
            }
        }

        public static void LoadComplete()
        {
            modules.Sort(StringComparer.Ordinal);
        }
    }
#endif
}