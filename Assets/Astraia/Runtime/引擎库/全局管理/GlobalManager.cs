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
    public sealed class GlobalManager : Entity
    {
        internal static readonly List<AudioSource> AudioData = new List<AudioSource>();
        internal static readonly Dictionary<string, AssetData> AssetPath = new Dictionary<string, AssetData>();
        internal static readonly Dictionary<string, AssetBundle> AssetPack = new Dictionary<string, AssetBundle>();
        internal static readonly Dictionary<string, Task<AssetBundle>> AssetTask = new Dictionary<string, Task<AssetBundle>>();
        internal static readonly Dictionary<string, IPool> PoolData = new Dictionary<string, IPool>();
        internal static readonly Dictionary<string, Transform> PoolRoot = new Dictionary<string, Transform>();
        internal static readonly Dictionary<Type, IDataTable> DataTable = new Dictionary<Type, IDataTable>();
        internal static readonly Dictionary<Type, Dictionary<int, IData>> DataTable1 = new Dictionary<Type, Dictionary<int, IData>>();
        internal static readonly Dictionary<Type, Dictionary<Enum, IData>> DataTable2 = new Dictionary<Type, Dictionary<Enum, IData>>();
        internal static readonly Dictionary<Type, Dictionary<string, IData>> DataTable3 = new Dictionary<Type, Dictionary<string, IData>>();

        internal static int MusicVolume;
        internal static int AudioVolume;
        internal static AudioState AudioState;
        internal static AssetBundleManifest Manifest;

        public static GlobalManager Instance;
        public static Package Package;

        [SerializeField] internal Canvas canvas;
        [SerializeField] internal AudioSource source;

        public static AudioSource Source => Instance.source;

        protected override void Awake()
        {
            base.Awake();
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            Async.Time = 0;
            LoadManager.Update();
        }

        private void Update()
        {
            Async.Time = Time.time;
            EventManager.Invoke(new OnEarlyUpdate());
        }

        private void LateUpdate()
        {
            AudioManager.Update();
            EventManager.Invoke(new OnAfterUpdate());
        }

        private void FixedUpdate()
        {
            EventManager.Invoke(new OnFixedUpdate());
        }

        private void OnDrawGizmos()
        {
            EventManager.Invoke(new OnGizmoUpdate());
        }

        protected override async void OnDestroy()
        {
            Manifest = null;
            Instance = null;
            await Task.Yield();
            PoolManager.Dispose();
            HeapManager.Dispose();
            AssetManager.Dispose();
            EventManager.Dispose();
            AudioManager.Dispose();
            base.OnDestroy();
            GC.Collect();
        }

        public struct AssetData
        {
            public string Name;
            public string Path;
        }
    }
}