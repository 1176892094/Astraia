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
#if UNITY_EDITOR && ODIN_INSPECTOR
using Sirenix.OdinInspector;
using UnityEditor;
#endif

namespace Astraia
{
    internal partial class GlobalSetting : ScriptableObject
    {
        private static GlobalSetting instance;
        public static GlobalSetting Instance => instance ??= Resources.Load<GlobalSetting>(nameof(GlobalSetting));

#if UNITY_EDITOR && ODIN_INSPECTOR
        [FoldoutGroup("资源加载")] [LabelText("资源构建平台")] [PropertyOrder(-3)]
#endif
        public AssetPlatform assetPlatform = AssetPlatform.StandaloneWindows;
#if UNITY_EDITOR && ODIN_INSPECTOR
        [FoldoutGroup("其他设置")] [LabelText("敏感词过滤")]
#endif
        public BadWordFilter badWordFilter = BadWordFilter.Enable;
#if UNITY_EDITOR && ODIN_INSPECTOR
        [FoldoutGroup("邮件服务")]
#endif
        public string smtpServer = "smtp.qq.com";
#if UNITY_EDITOR && ODIN_INSPECTOR
        [FoldoutGroup("邮件服务")]
#endif
        public int smtpPort = 587;
#if UNITY_EDITOR && ODIN_INSPECTOR
        [FoldoutGroup("邮件服务")]
#endif
        public string smtpUsername = "1176892094@qq.com";
#if UNITY_EDITOR && ODIN_INSPECTOR
        [FoldoutGroup("邮件服务")]
#endif
        public string smtpPassword;
#if UNITY_EDITOR && ODIN_INSPECTOR
        [FoldoutGroup("资源加载")] [LabelText("资源加载模式")] [OnValueChanged("UpdateSceneSetting")]
#endif
        public AssetMode assetLoadMode = AssetMode.Simulate;
#if UNITY_EDITOR && ODIN_INSPECTOR
        [FoldoutGroup("资源构建")] [LabelText("资源校验文件")]
#endif
        public string assetLoadName = "AssetBundle";
#if UNITY_EDITOR && ODIN_INSPECTOR
        [FoldoutGroup("资源加载")] [LabelText("资源构建文件夹")]
#endif
        public string assetBuildPath = "AssetBundles";
#if UNITY_EDITOR && ODIN_INSPECTOR
        [FoldoutGroup("资源加载")] [LabelText("资源加载根目录")]
#endif
        public string assetSourcePath = "Assets/Template";
#if UNITY_EDITOR && ODIN_INSPECTOR
        [FoldoutGroup("资源加载")] [LabelText("资源服务器地址")]
#endif
        public string assetRemoteData = "http://192.168.0.3:8000/AssetBundles";
#if UNITY_EDITOR && ODIN_INSPECTOR
        [FoldoutGroup("其他设置")] [LabelText("密钥版本")]
#endif
        public byte secretVersion = 1;
#if UNITY_EDITOR && ODIN_INSPECTOR
        [FoldoutGroup("其他设置")] [LabelText("密钥版本")] [PropertyOrder(1)]
#endif
        public string[] secretGroup;
#if ODIN_INSPECTOR
        [ShowInInspector] [FoldoutGroup("数据表")] [LabelText("数据表程序集")] [PropertyOrder(-1)]
#endif
        public string assemblyName = "HotUpdate.Data";
#if UNITY_EDITOR && ODIN_INSPECTOR
        [ShowInInspector]
        [FoldoutGroup("资源加载")]
        [LabelText("远端资源路径")]
#endif
        public static string assetRemotePath => "{0}/{1}".Format(Instance.assetRemoteData, Instance.assetBuildPath);
#if UNITY_EDITOR && ODIN_INSPECTOR
        [ShowInInspector]
        [FoldoutGroup("资源加载")]
        [LabelText("资源存储路径")]
#endif
        public static string assetPackPath => "{0}/{1}".Format(Application.persistentDataPath, Instance.assetBuildPath);

        public static string assetPackData => "{0}.json".Format(Instance.assetLoadName);

        private static readonly Dictionary<AssetText, TextAsset> itemText = new Dictionary<AssetText, TextAsset>();

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

        public static string GetEditorPath(string assetName) => "{0}/{1}.asset".Format(Instance.assetSourcePath, GetTablePath(assetName));

        public static string GetPacketPath(string fileName) => Path.Combine(assetPackPath, fileName);

        public static string GetServerPath(string fileName) => Path.Combine(assetRemotePath, Path.Combine(Instance.assetPlatform.ToString(), fileName));

        public static string GetClientPath(string fileName) => Path.Combine(Application.streamingAssetsPath, Path.Combine(Instance.assetPlatform.ToString(), fileName));

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RuntimeInitializeOnLoad()
        {
            var source = new GameObject(nameof(GlobalManager)).AddComponent<GlobalManager>();
            source.canvas = new GameObject(nameof(PageManager)).AddComponent<Canvas>();
            source.canvas.renderMode = RenderMode.ScreenSpaceCamera;
            source.canvas.gameObject.layer = LayerMask.NameToLayer("UI");
            for (var i = UILayer.Layer1; i <= UILayer.Layer6; i++)
            {
                var format = "Pool - Canvas/{0}".Format(i);
                var parent = new GameObject(format).AddComponent<RectTransform>();
                parent.gameObject.layer = LayerMask.NameToLayer("UI");
                parent.SetParent(source.canvas.transform);
                parent.anchorMin = Vector2.zero;
                parent.anchorMax = Vector2.one;
                parent.offsetMin = Vector2.zero;
                parent.offsetMax = Vector2.zero;
                parent.localScale = Vector3.one;
                parent.localPosition = Vector3.zero;
                GlobalManager.layerData.Add(i, parent);
            }

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
                Service.Xor.Register(i, Instance.secretGroup[i]);
            }

            if (Instance.badWordFilter != BadWordFilter.Disable)
            {
                Service.Word.Register(GetTextByIndex(AssetText.BadWord));
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
        [HideInInspector] public List<string> sceneAssets = new List<string>();

#if ODIN_INSPECTOR
        [PropertyOrder(1)] [FoldoutGroup("资源加载")] [LabelText("忽略资源")]
#endif
        public List<Object> ignoreAssets = new List<Object>();

#if ODIN_INSPECTOR
        [FoldoutGroup("资源构建")] [LabelText("资源构建路径")] [PropertyOrder(-2)] [ShowInInspector]
#endif
        public BuildMode BuildPath = BuildMode.StreamingAssets;
#if ODIN_INSPECTOR
        [ShowInInspector]
        [FoldoutGroup("数据表")]
        [LabelText("脚本生成路径")]
#endif
        public static string ScriptPath
        {
            get => EditorPrefs.GetString(nameof(ScriptPath), "Assets/Scripts/DataTable");
            set => EditorPrefs.SetString(nameof(ScriptPath), value);
        }
#if ODIN_INSPECTOR
        [ShowInInspector]
        [FoldoutGroup("数据表")]
        [LabelText("数据表程序集")]
#endif
        public static string assemblyPath => "{0}/{1}.asmdef".Format(ScriptPath, Instance.assemblyName);
#if ODIN_INSPECTOR
        [ShowInInspector]
        [FoldoutGroup("数据表")]
        [LabelText("资源生成路径")]
#endif
        public static string dataTablePath => "{0}/DataTable".Format(Instance.assetSourcePath);
#if ODIN_INSPECTOR
        [ShowInInspector]
        [FoldoutGroup("资源构建")]
        [LabelText("资源构建路径")]
#endif
        public static string remoteBuildPath => Instance.BuildPath == BuildMode.BuildPath ? Instance.assetBuildPath : Application.streamingAssetsPath;
#if ODIN_INSPECTOR
        [ShowInInspector]
        [FoldoutGroup("资源构建")]
        [LabelText("资源构建平台")]
#endif
        public static string remoteAssetPath => Path.Combine(remoteBuildPath, Instance.assetPlatform.ToString());
#if ODIN_INSPECTOR
        [ShowInInspector]
        [FoldoutGroup("资源构建")]
        [LabelText("校验文件路径")]
#endif
        public static string remoteAssetData => "{0}/{1}.json".Format(remoteAssetPath, Instance.assetLoadName);

        public static string GetEnumPath(string name) => "{0}/01.枚举类/{1}.cs".Format(ScriptPath, name);

        public static string GetItemPath(string name) => "{0}/02.结构体/{1}.cs".Format(ScriptPath, name);

        public static string GetDataPath(string name) => "{0}/03.数据表/{1}DataTable.cs".Format(ScriptPath, name);

        public static string GetAssetPath(string name) => "{0}/{1}DataTable.asset".Format(dataTablePath, name);

        public static string GetDataName(string name) => "Astraia.Table.{0}Data,{1}".Format(name, Instance.assemblyName);

        public static string GetTableName(string name) => "Astraia.Table.{0}DataTable".Format(name);

        public static void UpdateSceneSetting()
        {
            var assets = EditorBuildSettings.scenes.Select(scene => scene.path).ToList();
            foreach (var scenePath in Instance.sceneAssets)
            {
                if (assets.Contains(scenePath))
                {
                    if (Instance.assetLoadMode == AssetMode.Simulate) continue;
                    var scenes = EditorBuildSettings.scenes.Where(scene => scene.path != scenePath);
                    EditorBuildSettings.scenes = scenes.ToArray();
                }
                else
                {
                    if (Instance.assetLoadMode == AssetMode.Authentic) continue;
                    var scenes = EditorBuildSettings.scenes.ToList();
                    scenes.Add(new EditorBuildSettingsScene(scenePath, true));
                    EditorBuildSettings.scenes = scenes.ToArray();
                }
            }
        }

        public static readonly List<string> agents = new List<string>();
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

            if (typeof(IAgent).IsAssignableFrom(result))
            {
                agents.Add("{0}, {1}".Format(result.FullName, result.Assembly.GetName().Name));
            }
            else if (typeof(ISystem).IsAssignableFrom(result))
            {
                systems.Add("{0}, {1}".Format(result.FullName, result.Assembly.GetName().Name));
            }
        }

        public static void LoadComplete()
        {
            agents.Sort(StringComparer.Ordinal);
            systems.Sort(StringComparer.Ordinal);
        }
    }
#endif
}