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
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Astraia
{
    internal class GlobalSetting : ScriptableObject
    {
        private static GlobalSetting instance;

        public AssetPlatform assetPlatform = AssetPlatform.StandaloneWindows;

        public string smtpServer = "smtp.qq.com";

        public int smtpPort = 587;

        public string smtpUsername = "1176892094@qq.com";

        public string smtpPassword;

        [Range(1, 255)] public int jsonVersion = 1;

#if UNITY_EDITOR && ODIN_INSPECTOR
        [Sirenix.OdinInspector.OnValueChanged("UpdateSceneSetting")]
#endif
        public AssetMode assetLoadMode = AssetMode.Simulate;

        public string assetLoadName = "AssetBundle";

        public string assetBuildPath = "AssetBundles";

        public string assetSourcePath = "Assets/Template";

        public string assetRemotePath = "http://192.168.0.3:8000/AssetBundles";

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
                    AssetDatabase.CreateAsset(instance, assetPath);
                    AssetDatabase.SaveAssets();
                }
#endif
                return instance;
            }
        }

        public static string assetPackData => Service.Text.Format("{0}.json", Instance.assetLoadName);
        public static string assetPackPath => Service.Text.Format("{0}/{1}", Application.persistentDataPath, Instance.assetBuildPath);

#if UNITY_EDITOR && ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        public static string assemblyName => JsonUtility.FromJson<Name>(assemblyData.text).name;

        public static TextAsset assemblyData => Resources.Load<TextAsset>(nameof(GlobalSetting));

#if UNITY_EDITOR
        [HideInInspector] public List<string> sceneAssets = new List<string>();

#if UNITY_EDITOR && ODIN_INSPECTOR
        [Sirenix.OdinInspector.PropertyOrder(1)]
#endif
        public List<Object> ignoreAssets = new List<Object>();

        public static TextAsset[] templateData => Resources.LoadAll<TextAsset>(nameof(GlobalSetting));

#if UNITY_EDITOR && ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        private static BuildMode BuildPath
        {
            get => (BuildMode)EditorPrefs.GetInt(nameof(BuildPath), (int)BuildMode.StreamingAssets);
            set => EditorPrefs.SetInt(nameof(BuildPath), (int)value);
        }

#if UNITY_EDITOR && ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        public static string EditorPath
        {
            get => EditorPrefs.GetString(nameof(EditorPath), "Assets/Editor/Resources");
            set => EditorPrefs.SetString(nameof(EditorPath), value);
        }
#if UNITY_EDITOR && ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        public static string ScriptPath
        {
            get => EditorPrefs.GetString(nameof(ScriptPath), "Assets/Scripts/DataTable");
            set => EditorPrefs.SetString(nameof(ScriptPath), value);
        }

#if UNITY_EDITOR && ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        public static string assemblyPath => Service.Text.Format("{0}/{1}.asmdef", ScriptPath, assemblyName);

#if UNITY_EDITOR && ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        public static string remoteBuildPath => BuildPath == BuildMode.BuildPath ? Instance.assetBuildPath : Application.streamingAssetsPath;
#if UNITY_EDITOR && ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        public static string remoteAssetPath => Path.Combine(remoteBuildPath, Instance.assetPlatform.ToString());
#if UNITY_EDITOR && ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        public static string remoteAssetData => Service.Text.Format("{0}/{1}.json", remoteAssetPath, Instance.assetLoadName);

        public static string GetEnumPath(string name) => Service.Text.Format("{0}/01.枚举类/{1}.cs", ScriptPath, name);

        public static string GetItemPath(string name) => Service.Text.Format("{0}/02.结构体/{1}.cs", ScriptPath, name);

        public static string GetDataPath(string name) => Service.Text.Format("{0}/03.数据表/{1}DataTable.cs", ScriptPath, name);

        public static string GetAssetPath(string name) => Service.Text.Format("{0}/DataTable/{1}DataTable.asset", Instance.assetSourcePath, name);

        public static string GetDataName(string name) => Service.Text.Format("Astraia.Table.{0}Data,{1}", name, assemblyName);

        public static string GetTableName(string name) => Service.Text.Format("Astraia.Table.{0}DataTable", name);

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
#endif
        public static string GetScenePath(string assetName) => Service.Text.Format("Scenes/{0}", assetName);

        public static string GetAudioPath(string assetName) => Service.Text.Format("Audios/{0}", assetName);

        public static string GetPanelPath(string assetName) => Service.Text.Format("Prefabs/{0}", assetName);

        public static string GetTablePath(string assetName) => Service.Text.Format("DataTable/{0}", assetName);

        private static string GetPlatform(string fileName) => Path.Combine(Instance.assetPlatform.ToString(), fileName);

        public static string GetPacketPath(string fileName) => Path.Combine(assetPackPath, fileName);

        public static string GetServerPath(string fileName) => Path.Combine(Instance.assetRemotePath, GetPlatform(fileName));

        public static string GetClientPath(string fileName) => Path.Combine(Application.streamingAssetsPath, GetPlatform(fileName));

        public MailData MailData(string mailBody)
        {
            return new MailData
            {
                smtpServer = smtpServer,
                smtpPort = smtpPort,
                senderName = "Astraia",
                senderAddress = smtpUsername,
                senderPassword = smtpPassword,
                targetAddress = smtpUsername,
                mailName = "来自《Astraia》的调试日志:",
                mailBody = mailBody
            };
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RuntimeInitializeOnLoad()
        {
            var canvas = new GameObject(nameof(UIManager)).AddComponent<Canvas>();
            canvas.gameObject.layer = LayerMask.NameToLayer("UI");
            for (var i = UILayer.Layer1; i <= UILayer.Layer6; i++)
            {
                var name = Service.Text.Format("Pool - Canvas/{0}", i);
                var item = new GameObject(name, typeof(RectTransform))
                {
                    layer = canvas.gameObject.layer
                };
                var rect = item.GetComponent<RectTransform>();
                rect.SetParent(canvas.transform);
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                rect.localScale = Vector3.one;
                rect.localPosition = Vector3.zero;
                GlobalManager.layerData.Add(i, rect);
            }

            canvas.gameObject.AddComponent<GraphicRaycaster>();
            var scaler = canvas.gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.referencePixelsPerUnit = 64;
            DontDestroyOnLoad(canvas);


            var manager = new GameObject(nameof(PoolManager)).AddComponent<GlobalManager>();
            manager.canvas = canvas;
            manager.canvas.renderMode = RenderMode.ScreenSpaceCamera;
        }


        [Serializable]
        private struct Name
        {
            public string name;
        }
    }
}