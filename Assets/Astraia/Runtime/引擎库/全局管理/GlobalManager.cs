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

using UnityEngine;

namespace Astraia.Core
{
    [DefaultExecutionOrder(-100)]
    public sealed class GlobalManager : Entity
    {
        protected override void Awake()
        {
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
            base.OnDestroy();
            HeapManager.Dispose();
            EventManager.Dispose();
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#endif
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RuntimeInitializeOnLoad()
        {
            Xor.SetUp(GlobalSetting.Instance.EncryptGroup);
            Bad.SetUp(GlobalSetting.LoadAsset(AssetData.BadWord));
            Log.Setup(Debug.Log, Debug.LogWarning, Debug.LogError);
        }
    }
}