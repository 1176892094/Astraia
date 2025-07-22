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

namespace Astraia.Common
{
    public class GlobalManager : MonoBehaviour
    {
        public static GlobalManager Instance;

        public Canvas canvas;

        public AudioSource sounds;

        internal static AudioSetting settings;
        
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
        internal static readonly Dictionary<string, (string, string)> assetData = new Dictionary<string, (string, string)>();
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
        internal static readonly Dictionary<Type, ItemTable> itemTable = new Dictionary<Type, ItemTable>();
#if UNITY_EDITOR && ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        internal static readonly Dictionary<Type, NameTable> nameTable = new Dictionary<Type, NameTable>();
#if UNITY_EDITOR && ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        internal static readonly Dictionary<Type, EnumTable> enumTable = new Dictionary<Type, EnumTable>();
#if UNITY_EDITOR && ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        internal static readonly Dictionary<string, IPool> poolData = new Dictionary<string, IPool>();
#if UNITY_EDITOR && ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        internal static readonly Dictionary<string, GameObject> poolGroup = new Dictionary<string, GameObject>();
#if UNITY_EDITOR && ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        internal static readonly Dictionary<int, Entity> entityData = new Dictionary<int, Entity>();
#if UNITY_EDITOR && ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        internal static readonly Dictionary<Type, UIPanel> panelData = new Dictionary<Type, UIPanel>();
#if UNITY_EDITOR && ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        internal static readonly Dictionary<UILayer, RectTransform> layerData = new Dictionary<UILayer, RectTransform>();
#if UNITY_EDITOR && ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        internal static readonly Dictionary<int, List<UIPanel>> groupData = new Dictionary<int, List<UIPanel>>();
#if UNITY_EDITOR && ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        internal static readonly Dictionary<uint, GameObject> objectData = new Dictionary<uint, GameObject>();

        public static event Action OnUpdate;
        public static event Action OnLateUpdate;
        public static event Action OnFixedUpdate;

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
            try
            {
                OnUpdate?.Invoke();
                TimerManager.Update();
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }
        }

        private void LateUpdate()
        {
            try
            {
                OnLateUpdate?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }
        }

        private void FixedUpdate()
        {
            try
            {
                OnFixedUpdate?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }
        }

        private async void OnDestroy()
        {
            manifest = null;
            Instance = null;
            OnUpdate = null;
            OnLateUpdate = null;
            OnFixedUpdate = null;
            await Task.Yield();
            UIManager.Dispose();
            PackManager.Dispose();
            DataManager.Dispose();
            PoolManager.Dispose();
            AudioManager.Dispose();
            AssetManager.Dispose();
            TimerManager.Dispose();
            EventManager.Dispose();
            EntityManager.Dispose();
            // HeapManager.Dispose();
            GC.Collect();
        }
    }
}