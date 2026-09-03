// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-09-02 18:09:17
// # Recently: 2026-09-02 18:22:17
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using UnityEngine;

namespace Astraia
{
    [DefaultExecutionOrder(-100)]
    public class GlobalManager : Singleton<GlobalManager>, IDontDestroy
    {
        private void Start()
        {
            AssetManager.Update();
        }

        private void Update()
        {
            TimeManager.RenderUpdate(Time.time);
        }

        private void FixedUpdate()
        {
            TimeManager.PhysicUpdate(Time.fixedTime);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            HeapManager.Dispose();
            EventManager.Dispose();
            AssetManager.Dispose();
        }
#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#endif
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RuntimeInitializeOnLoad()
        {
            Log.Setup(Debug.Log, Debug.LogWarning, Debug.LogError);
            Bad.SetUp(Zip.Decompress(GlobalSetting.LoadText(TextAsset.敏感词)));
        }
    }
}