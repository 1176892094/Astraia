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
using AssetData = System.Collections.Generic.KeyValuePair<string, string>;
using EnumTable = System.Collections.Generic.Dictionary<System.Enum, Astraia.Common.IData>;
using ItemTable = System.Collections.Generic.Dictionary<int, Astraia.Common.IData>;
using NameTable = System.Collections.Generic.Dictionary<string, Astraia.Common.IData>;
using AgentData = System.Collections.Generic.Dictionary<System.Type, Astraia.Common.IAgent>;

namespace Astraia.Common
{
    public class GlobalManager : MonoBehaviour
    {
        public static GlobalManager Instance;

        public Canvas canvas;

        public AudioSource sounds;

        internal static AudioSetting settings;
#if UNITY_EDITOR && ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        internal static AssetBundleManifest manifest;
#if UNITY_EDITOR && ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        internal static readonly List<ITimer> timerData = new List<ITimer>();
#if UNITY_EDITOR && ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        internal static readonly List<AudioSource> audioData = new List<AudioSource>();
#if UNITY_EDITOR && ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        internal static readonly Dictionary<string, PackData> clientPacks = new Dictionary<string, PackData>();
#if UNITY_EDITOR && ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        internal static readonly Dictionary<string, PackData> serverPacks = new Dictionary<string, PackData>();
#if UNITY_EDITOR && ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        internal static readonly Dictionary<string, AssetData> assetData = new Dictionary<string, AssetData>();
#if UNITY_EDITOR && ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        internal static readonly Dictionary<string, AssetBundle> assetPack = new Dictionary<string, AssetBundle>();
#if UNITY_EDITOR && ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        internal static readonly Dictionary<string, Task<AssetBundle>> assetTask = new Dictionary<string, Task<AssetBundle>>();
#if UNITY_EDITOR && ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        internal static readonly Dictionary<string, string> assetPath = new Dictionary<string, string>();
#if UNITY_EDITOR && ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        internal static readonly LiDictionary<Type, ItemTable> itemTable = new LiDictionary<Type, ItemTable>();
#if UNITY_EDITOR && ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        internal static readonly LiDictionary<Type, NameTable> nameTable = new LiDictionary<Type, NameTable>();
#if UNITY_EDITOR && ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        internal static readonly LiDictionary<Type, EnumTable> enumTable = new LiDictionary<Type, EnumTable>();
#if UNITY_EDITOR && ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        internal static readonly LiDictionary<string, IPool> poolData = new LiDictionary<string, IPool>();
#if UNITY_EDITOR && ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        internal static readonly Dictionary<string, GameObject> poolGroup = new Dictionary<string, GameObject>();
#if UNITY_EDITOR && ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        internal static readonly LiDictionary<Component, AgentData> agentData = new LiDictionary<Component, AgentData>();
#if UNITY_EDITOR && ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        internal static readonly Dictionary<Type, UIPanel> panelData = new Dictionary<Type, UIPanel>();
#if UNITY_EDITOR && ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        internal static readonly Dictionary<int, RectTransform> layerData = new Dictionary<int, RectTransform>();
#if UNITY_EDITOR && ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        internal static readonly LiDictionary<string, HashSet<UIPanel>> groupData = new LiDictionary<string, HashSet<UIPanel>>();
#if UNITY_EDITOR && ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        internal static readonly Dictionary<ulong, GameObject> objectData = new Dictionary<ulong, GameObject>();


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
            AgentManager.Update();
            TimerManager.Update();
        }

        private void OnDestroy()
        {
            manifest = null;
            Instance = null;
            UIManager.Dispose();
            PackManager.Dispose();
            DataManager.Dispose();
            PoolManager.Dispose();
            AudioManager.Dispose();
            AssetManager.Dispose();
            AgentManager.Dispose();
            TimerManager.Dispose();
            EventManager.Dispose();
            HeapManager.Dispose();
            GC.Collect();
        }
    }
}