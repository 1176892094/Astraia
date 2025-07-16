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

namespace Astraia
{
    internal partial class GlobalSetting : ScriptableObject
    {
        private static GlobalSetting instance;

        public AssetPlatform assetPlatform = AssetPlatform.StandaloneWindows;

        public int assetVersion = 1;

        public string smtpServer = "smtp.qq.com";

        public int smtpPort = 587;

        public string smtpUsername = "1176892094@qq.com";

        public string smtpPassword;

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
                    UnityEditor.AssetDatabase.CreateAsset(instance, assetPath);
                    UnityEditor.AssetDatabase.SaveAssets();
                }
#endif
                return instance;
            }
        }

        public static string assetPackData => Service.Text.Format("{0}.json", Instance.assetLoadName);

        public static string assetPackPath => Service.Text.Format("{0}/{1}", Application.persistentDataPath, Instance.assetBuildPath);

        public static string assemblyName => JsonUtility.FromJson<Name>(assemblyData.text).name;

        public static TextAsset assemblyData => Resources.Load<TextAsset>(nameof(GlobalSetting));

        public static string GetScenePath(string assetName) => Service.Text.Format("Scenes/{0}", assetName);

        public static string GetAudioPath(string assetName) => Service.Text.Format("Audios/{0}", assetName);

        public static string GetPanelPath(string assetName) => Service.Text.Format("Prefabs/{0}", assetName);

        public static string GetTablePath(string assetName) => Service.Text.Format("DataTable/{0}", assetName);

        public static string GetPacketPath(string fileName) => Path.Combine(assetPackPath, fileName);

        public static string GetServerPath(string fileName) => Path.Combine(Instance.assetRemotePath, Path.Combine(Instance.assetPlatform.ToString(), fileName));

        public static string GetClientPath(string fileName) => Path.Combine(Application.streamingAssetsPath, Path.Combine(Instance.assetPlatform.ToString(), fileName));

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RuntimeInitializeOnLoad()
        {
            var source = new GameObject(nameof(GlobalManager)).AddComponent<GlobalManager>();
            source.canvas = new GameObject(nameof(UIManager)).AddComponent<Canvas>();
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
                GlobalManager.layerData.Add(layer, parent);
            }

            source.canvas.gameObject.AddComponent<GraphicRaycaster>();
            var canvas = source.canvas.gameObject.AddComponent<CanvasScaler>();
            canvas.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvas.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvas.matchWidthOrHeight = 0.5f;
            canvas.referenceResolution = new Vector2(1920, 1080);
            canvas.referencePixelsPerUnit = 64;
            DontDestroyOnLoad(source.canvas);
        }


        [Serializable]
        private struct Name
        {
            public string name;
        }
    }
#if UNITY_EDITOR
    internal partial class GlobalSetting
    {
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
            get => (BuildMode)UnityEditor.EditorPrefs.GetInt(nameof(BuildPath), (int)BuildMode.StreamingAssets);
            set => UnityEditor.EditorPrefs.SetInt(nameof(BuildPath), (int)value);
        }

#if UNITY_EDITOR && ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        public static string EditorPath
        {
            get => UnityEditor.EditorPrefs.GetString(nameof(EditorPath), "Assets/Editor/Resources");
            set => UnityEditor.EditorPrefs.SetString(nameof(EditorPath), value);
        }
#if UNITY_EDITOR && ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        public static string ScriptPath
        {
            get => UnityEditor.EditorPrefs.GetString(nameof(ScriptPath), "Assets/Scripts/DataTable");
            set => UnityEditor.EditorPrefs.SetString(nameof(ScriptPath), value);
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
    }
#endif
}