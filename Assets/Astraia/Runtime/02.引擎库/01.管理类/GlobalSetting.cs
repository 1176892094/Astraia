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
#endif

namespace Astraia
{
    internal partial class GlobalSetting : ScriptableObject
    {
        private static GlobalSetting instance;

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

        public static GlobalSetting Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load<GlobalSetting>(nameof(GlobalSetting));
                }

#if UNITY_EDITOR
                if (instance == null)
                {
                    var assetPath = Service.Text.Format("Assets/{0}", nameof(Resources));
                    instance = CreateInstance<GlobalSetting>();
                    if (!Directory.Exists(assetPath))
                    {
                        Directory.CreateDirectory(assetPath);
                    }

                    assetPath = Service.Text.Format("{0}/{1}.asset", assetPath, nameof(GlobalSetting));
                    UnityEditor.AssetDatabase.CreateAsset(instance, assetPath);
                    UnityEditor.AssetDatabase.SaveAssets();
                }
#endif
                return instance;
            }
        }
#if UNITY_EDITOR && ODIN_INSPECTOR
        [ShowInInspector]
        [FoldoutGroup("资源加载")]
        [LabelText("远端资源路径")]
#endif
        public static string assetRemotePath => Service.Text.Format("{0}/{1}", Instance.assetRemoteData, Instance.assetBuildPath);
#if UNITY_EDITOR && ODIN_INSPECTOR
        [ShowInInspector]
        [FoldoutGroup("资源加载")]
        [LabelText("资源存储路径")]
#endif
        public static string assetPackPath => Service.Text.Format("{0}/{1}", Application.persistentDataPath, Instance.assetBuildPath);
        
        public static string assetPackData => Service.Text.Format("{0}.json", Instance.assetLoadName);

        public static string assemblyName => JsonUtility.FromJson<Name>(GetTextByIndex(AssetText.Assembly)).name;

        public static string GetScenePath(string assetName) => Service.Text.Format("Scenes/{0}", assetName);

        public static string GetAudioPath(string assetName) => Service.Text.Format("Audios/{0}", assetName);

        public static string GetPanelPath(string assetName) => Service.Text.Format("Prefabs/{0}", assetName);

        public static string GetTablePath(string assetName) => Service.Text.Format("DataTable/{0}", assetName);

        public static string GetPacketPath(string fileName) => Path.Combine(assetPackPath, fileName);

        public static string GetServerPath(string fileName) => Path.Combine(assetRemotePath, Path.Combine(Instance.assetPlatform.ToString(), fileName));

        public static string GetClientPath(string fileName) => Path.Combine(Application.streamingAssetsPath, Path.Combine(Instance.assetPlatform.ToString(), fileName));

        private static TextAsset[] assetTextArray;

        public static string GetTextByIndex(AssetText option)
        {
            if (assetTextArray != null)
            {
                return assetTextArray[(int)option].text;
            }

            assetTextArray = Resources.LoadAll<TextAsset>(nameof(GlobalSetting));
            return assetTextArray[(int)option].text;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RuntimeInitializeOnLoad()
        {
            var source = new GameObject(nameof(GlobalManager)).AddComponent<GlobalManager>();
            source.canvas = new GameObject(nameof(PageManager)).AddComponent<Canvas>();
            source.canvas.renderMode = RenderMode.ScreenSpaceCamera;
            source.canvas.gameObject.layer = LayerMask.NameToLayer("UI");
            for (var layer = UILayer.Layer1; layer <= UILayer.Layer6; layer++)
            {
                var format = Service.Text.Format("Pool - Canvas/{0}", layer);
                var parent = new GameObject(format).AddComponent<RectTransform>();
                parent.gameObject.layer = LayerMask.NameToLayer("UI");
                parent.SetParent(source.canvas.transform);
                parent.anchorMin = Vector2.zero;
                parent.anchorMax = Vector2.one;
                parent.offsetMin = Vector2.zero;
                parent.offsetMax = Vector2.zero;
                parent.localScale = Vector3.one;
                parent.localPosition = Vector3.zero;
                GlobalManager.LayerData.Add(layer, parent);
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

        [Serializable]
        private struct Name
        {
            public string name;
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
        [FoldoutGroup("资源构建")]
        [LabelText("资源构建路径")]
        [PropertyOrder(-2)]
        [ShowInInspector]
#endif
        private static BuildMode BuildPath
        {
            get => (BuildMode)UnityEditor.EditorPrefs.GetInt(nameof(BuildPath), (int)BuildMode.StreamingAssets);
            set => UnityEditor.EditorPrefs.SetInt(nameof(BuildPath), (int)value);
        }

#if ODIN_INSPECTOR
        [ShowInInspector]
        [FoldoutGroup("资源构建")]
        [LabelText("编辑资源路径")]
#endif
        public static string EditorPath
        {
            get => UnityEditor.EditorPrefs.GetString(nameof(EditorPath), "Assets/Editor/Resources");
            set => UnityEditor.EditorPrefs.SetString(nameof(EditorPath), value);
        }
#if ODIN_INSPECTOR
        [ShowInInspector]
        [FoldoutGroup("数据表")]
        [LabelText("脚本生成路径")]
        [PropertyOrder(-1)]
#endif
        public static string ScriptPath
        {
            get => UnityEditor.EditorPrefs.GetString(nameof(ScriptPath), "Assets/Scripts/DataTable");
            set => UnityEditor.EditorPrefs.SetString(nameof(ScriptPath), value);
        }
#if ODIN_INSPECTOR
        [ShowInInspector]
        [FoldoutGroup("数据表")]
        [LabelText("资源生成路径")]
#endif
        public static string dataTablePath => Service.Text.Format("{0}/DataTable", Instance.assetSourcePath);
#if ODIN_INSPECTOR
        [ShowInInspector]
        [FoldoutGroup("数据表")]
        [LabelText("数据表程序集")]
#endif
        public static string assemblyPath => Service.Text.Format("{0}/{1}.asmdef", ScriptPath, assemblyName);
#if ODIN_INSPECTOR
        [ShowInInspector]
        [FoldoutGroup("资源构建")]
        [LabelText("资源构建路径")]
#endif
        public static string remoteBuildPath => BuildPath == BuildMode.BuildPath ? Instance.assetBuildPath : Application.streamingAssetsPath;
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
        public static string remoteAssetData => Service.Text.Format("{0}/{1}.json", remoteAssetPath, Instance.assetLoadName);

        public static string GetEnumPath(string name) => Service.Text.Format("{0}/01.枚举类/{1}.cs", ScriptPath, name);

        public static string GetItemPath(string name) => Service.Text.Format("{0}/02.结构体/{1}.cs", ScriptPath, name);

        public static string GetDataPath(string name) => Service.Text.Format("{0}/03.数据表/{1}DataTable.cs", ScriptPath, name);

        public static string GetAssetPath(string name) => Service.Text.Format("{0}/{1}DataTable.asset", dataTablePath, name);

        public static string GetDataName(string name) => Service.Text.Format("Astraia.Table.{0}Data,{1}", name, assemblyName);

        public static string GetTableName(string name) => Service.Text.Format("Astraia.Table.{0}DataTable", name);

        public static void UpdateSceneSetting()
        {
            var assets = UnityEditor.EditorBuildSettings.scenes.Select(scene => scene.path).ToList();
            foreach (var scenePath in Instance.sceneAssets)
            {
                if (assets.Contains(scenePath))
                {
                    if (Instance.assetLoadMode == AssetMode.Simulate) continue;
                    var scenes = UnityEditor.EditorBuildSettings.scenes.Where(scene => scene.path != scenePath);
                    UnityEditor.EditorBuildSettings.scenes = scenes.ToArray();
                }
                else
                {
                    if (Instance.assetLoadMode == AssetMode.Authentic) continue;
                    var scenes = UnityEditor.EditorBuildSettings.scenes.ToList();
                    scenes.Add(new UnityEditor.EditorBuildSettingsScene(scenePath, true));
                    UnityEditor.EditorBuildSettings.scenes = scenes.ToArray();
                }
            }
        }

        private static List<string> agents;

        public static List<string> GetAgents()
        {
            if (agents != null)
            {
                return agents;
            }

            agents = new List<string>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes();

                foreach (var type in types)
                {
                    if (type.IsAbstract)
                    {
                        continue;
                    }

                    if (type.IsGenericType)
                    {
                        continue;
                    }

                    if (!typeof(IAgent).IsAssignableFrom(type))
                    {
                        continue;
                    }

                    agents.Add(Service.Text.Format("{0}, {1}", type.FullName, type.Assembly.GetName().Name));
                }
            }

            agents.Sort(StringComparer.Ordinal);
            return agents;
        }
    }
#endif
}