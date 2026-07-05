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
using UnityEngine;

namespace Astraia.Core
{
    [DefaultExecutionOrder(-100)]
    public sealed class GlobalManager : Entity
    {
        internal static readonly Dictionary<Type, IDataTable> DataTable = new Dictionary<Type, IDataTable>();
        internal static readonly Dictionary<Type, Dictionary<int, IData>> DataTable1 = new Dictionary<Type, Dictionary<int, IData>>();
        internal static readonly Dictionary<Type, Dictionary<Enum, IData>> DataTable2 = new Dictionary<Type, Dictionary<Enum, IData>>();
        internal static readonly Dictionary<Type, Dictionary<string, IData>> DataTable3 = new Dictionary<Type, Dictionary<string, IData>>();

        public static GlobalManager Instance;
        public static Package Package;

        protected override void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            base.Awake();
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
            AudioManager.Instance.Update();
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

        protected override void OnDestroy()
        {
            Instance = null;
            base.OnDestroy();
            HeapManager.Dispose();
            EventManager.Dispose();
            GC.Collect();
        }
    }
}