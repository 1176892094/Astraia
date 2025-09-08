// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-04-09 21:04:41
// // # Recently: 2025-04-09 21:04:41
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
#if UNITY_EDITOR && ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Astraia.Common
{
    public class GlobalManager : MonoBehaviour
    {
        public static GlobalManager Instance;

        public Canvas canvas;

        public AudioSource source;

        internal static float musicVolume;

        internal static float audioVolume;

        internal static AssetBundleManifest manifest;
#if UNITY_EDITOR && ODIN_INSPECTOR
        [ShowInInspector]
#endif
        internal static readonly List<IAsync> asyncData = new List<IAsync>();
#if UNITY_EDITOR && ODIN_INSPECTOR
        [ShowInInspector]
#endif
        internal static readonly List<AudioSource> audioData = new List<AudioSource>();
#if UNITY_EDITOR && ODIN_INSPECTOR
        [ShowInInspector]
#endif
        internal static readonly Dictionary<string, PackData> clientData = new Dictionary<string, PackData>();
#if UNITY_EDITOR && ODIN_INSPECTOR
        [ShowInInspector]
#endif
        internal static readonly Dictionary<string, PackData> serverData = new Dictionary<string, PackData>();
#if UNITY_EDITOR && ODIN_INSPECTOR
        [ShowInInspector]
#endif
        internal static readonly Dictionary<string, (string, string)> assetData = new Dictionary<string, (string, string)>();
#if UNITY_EDITOR && ODIN_INSPECTOR
        [ShowInInspector]
#endif
        internal static readonly Dictionary<string, AssetBundle> assetPack = new Dictionary<string, AssetBundle>();
#if UNITY_EDITOR && ODIN_INSPECTOR
        [ShowInInspector]
#endif
        internal static readonly Dictionary<string, Task<AssetBundle>> assetTask = new Dictionary<string, Task<AssetBundle>>();
#if UNITY_EDITOR && ODIN_INSPECTOR
        [ShowInInspector]
#endif
        internal static readonly Dictionary<string, string> assetPath = new Dictionary<string, string>();
#if UNITY_EDITOR && ODIN_INSPECTOR
        [ShowInInspector]
#endif
        internal static readonly Dictionary<string, IPool> poolData = new Dictionary<string, IPool>();
#if UNITY_EDITOR && ODIN_INSPECTOR
        [ShowInInspector]
#endif
        internal static readonly Dictionary<string, GameObject> rootData = new Dictionary<string, GameObject>();
#if UNITY_EDITOR && ODIN_INSPECTOR
        [ShowInInspector]
#endif
        internal static readonly Dictionary<Type, List<Entity>> queryData = new Dictionary<Type, List<Entity>>();
#if UNITY_EDITOR && ODIN_INSPECTOR
        [ShowInInspector]
#endif
        internal static readonly Dictionary<Type, ISystem> systemData = new Dictionary<Type, ISystem>();
#if UNITY_EDITOR && ODIN_INSPECTOR
        [ShowInInspector]
#endif
        internal static readonly Dictionary<Type, UIPanel> panelData = new Dictionary<Type, UIPanel>();
#if UNITY_EDITOR && ODIN_INSPECTOR
        [ShowInInspector]
#endif
        internal static readonly Dictionary<int, HashSet<UIPanel>> groupData = new Dictionary<int, HashSet<UIPanel>>();
#if UNITY_EDITOR && ODIN_INSPECTOR
        [ShowInInspector]
#endif
        internal static readonly Dictionary<UILayer, RectTransform> layerData = new Dictionary<UILayer, RectTransform>();
#if UNITY_EDITOR && ODIN_INSPECTOR
        [ShowInInspector]
#endif
        internal static readonly Dictionary<uint, GameObject> sceneData = new Dictionary<uint, GameObject>();

        private void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        private void Start()
        {
            PackManager.LoadAssetData();
        }
        
        private void Update()
        {
            SystemManager.Update();
        }

        private async void OnDestroy()
        {
            Instance = null;
            await Task.Yield();
            PageManager.Dispose();
            PackManager.Dispose();
            PoolManager.Dispose();
            AudioManager.Dispose();
            AssetManager.Dispose();
            EventManager.Dispose();
            SystemManager.Dispose();
            EntityManager.Dispose();
            // HeapManager.Dispose();
            GC.Collect();
        }
    }
}