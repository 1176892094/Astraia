// // *********************************************************************************
// // # Project: JFramework
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-04-20 19:04:18
// // # Recently: 2025-04-20 19:04:18
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using Astraia;
using Astraia.Common;
using UnityEngine;

namespace Runtime
{
    public class GameManager : Singleton<GameManager>
    {
        protected override void Awake()
        {
            base.Awake();
            var worldCamera = FindFirstObjectByType<Camera>();
            GlobalManager.Instance.canvas.worldCamera = worldCamera;
            GlobalManager.Instance.canvas.sortingOrder = 10;
            GlobalManager.Instance.gameObject.AddComponent<DebugManager>();
        }

        private void Start()
        {
            AssetManager.Load<GameObject>("Prefabs/Player", player =>
            {
                
            });
        }
    }
}