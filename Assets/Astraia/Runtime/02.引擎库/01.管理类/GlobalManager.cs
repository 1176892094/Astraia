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
using EnumTable = System.Collections.Generic.Dictionary<System.Enum, Astraia.Common.IData>;
using ItemTable = System.Collections.Generic.Dictionary<int, Astraia.Common.IData>;
using NameTable = System.Collections.Generic.Dictionary<string, Astraia.Common.IData>;
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
        internal static readonly Dictionary<Type, ItemTable> itemTable = new Dictionary<Type, ItemTable>();
#if UNITY_EDITOR && ODIN_INSPECTOR
        [ShowInInspector]
#endif
        internal static readonly Dictionary<Type, NameTable> nameTable = new Dictionary<Type, NameTable>();
#if UNITY_EDITOR && ODIN_INSPECTOR
        [ShowInInspector]
#endif
        internal static readonly Dictionary<Type, EnumTable> enumTable = new Dictionary<Type, EnumTable>();
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
        internal static readonly Dictionary<Entity, List<Type, IAgent>> agentData = new Dictionary<Entity, List<Type, IAgent>>();
#if UNITY_EDITOR && ODIN_INSPECTOR
        [ShowInInspector]
#endif
        internal static readonly Dictionary<Type, List<Entity, IAgent>> queryData = new Dictionary<Type, List<Entity, IAgent>>();
#if UNITY_EDITOR && ODIN_INSPECTOR
        [ShowInInspector]
#endif
        internal static readonly List<Type, ISystem> systemData = new List<Type, ISystem>();
#if UNITY_EDITOR && ODIN_INSPECTOR
        [ShowInInspector]
#endif
        internal static readonly List<Component, IAsync> asyncData = new List<Component, IAsync>();
#if UNITY_EDITOR && ODIN_INSPECTOR
        [ShowInInspector]
#endif
        internal static readonly List<Type, UIPanel> panelType = new List<Type, UIPanel>();
#if UNITY_EDITOR && ODIN_INSPECTOR
        [ShowInInspector]
#endif
        internal static readonly Dictionary<int, HashSet<UIPanel>> groupData = new Dictionary<int, HashSet<UIPanel>>();
#if UNITY_EDITOR && ODIN_INSPECTOR
        [ShowInInspector]
#endif
        internal static readonly Dictionary<UIPanel, HashSet<int>> panelData = new Dictionary<UIPanel, HashSet<int>>();
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

        private void OnEnable()
        {
            SystemManager.Listen<UISystem>();
            SystemManager.Listen<PageSystem>();
        }

        private void Start()
        {
            PackManager.LoadAssetData();
        }

        private void OnDisable()
        {
            SystemManager.Remove<UISystem>();
            SystemManager.Remove<PageSystem>();
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
            DataManager.Dispose();
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