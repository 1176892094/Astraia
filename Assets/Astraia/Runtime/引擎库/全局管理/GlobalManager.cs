// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2025-04-09 21:04:41
// # Recently: 2025-04-09 21:04:41
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Astraia.Core
{
    [DefaultExecutionOrder(-100)]
    public sealed class GlobalManager : MonoBehaviour
    {
        public static GlobalManager Instance;

        public static Verify verify;

        public static Canvas canvas;

        public static AudioSource source;

        internal static int musicVolume;

        internal static int audioVolume;

        internal static AudioState audioState;

        internal static AssetBundleManifest manifest;

#if UNITY_EDITOR && ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        internal static readonly IList<AudioSource> audioLoop = new List<AudioSource>();
#if UNITY_EDITOR && ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        internal static readonly IDictionary<string, (string, string)> assetData = new Dictionary<string, (string, string)>();
#if UNITY_EDITOR && ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        internal static readonly IDictionary<string, string> assetPath = new Dictionary<string, string>();
#if UNITY_EDITOR && ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        internal static readonly IDictionary<string, AssetBundle> assetPack = new Dictionary<string, AssetBundle>();
#if UNITY_EDITOR && ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        internal static readonly IDictionary<string, Task<AssetBundle>> assetTask = new Dictionary<string, Task<AssetBundle>>();
#if UNITY_EDITOR && ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        internal static readonly IDictionary<Type, IDataTable> dataTable = new Dictionary<Type, IDataTable>();
#if UNITY_EDITOR && ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        internal static readonly IDictionary<Type, Dictionary<int, IData>> dataTable1 = new Dictionary<Type, Dictionary<int, IData>>();
#if UNITY_EDITOR && ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        internal static readonly IDictionary<Type, Dictionary<Enum, IData>> dataTable2 = new Dictionary<Type, Dictionary<Enum, IData>>();
#if UNITY_EDITOR && ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        internal static readonly IDictionary<Type, Dictionary<string, IData>> dataTable3 = new Dictionary<Type, Dictionary<string, IData>>();
#if UNITY_EDITOR && ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        internal static readonly IDictionary<string, IPool> poolData = new Dictionary<string, IPool>();
#if UNITY_EDITOR && ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        internal static readonly IDictionary<string, GameObject> poolRoot = new Dictionary<string, GameObject>();
#if UNITY_EDITOR && ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        internal static readonly IDictionary<Type, UIPanel> panelData = new Dictionary<Type, UIPanel>();
#if UNITY_EDITOR && ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        internal static readonly IDictionary<int, UIStack> stackData = new Dictionary<int, UIStack>();
#if UNITY_EDITOR && ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        internal static readonly IDictionary<int, RectTransform> layerData = new Dictionary<int, RectTransform>();

        private void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            LoadManager.Update();
        }

        private void Update()
        {
            TimeManager.Update(Time.time);
        }

        private void LateUpdate()
        {
            AudioManager.Update();
        }

        private async void OnDestroy()
        {
            manifest = null;
            Instance = null;
            await Task.Yield();
            UIManager.Dispose();
            TimeManager.Dispose();
            PoolManager.Dispose();
            HeapManager.Dispose();
            AssetManager.Dispose();
            EventManager.Dispose();
            AudioManager.Dispose();
            GC.Collect();
        }
    }
}