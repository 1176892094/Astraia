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
using Astraia.Net;
using UnityEngine;

namespace Runtime
{
    public class GameManager : Singleton<GameManager>, IEvent<ServerReady>, IEvent<ServerConnect>
    {
        protected override void Awake()
        {
            base.Awake();
            Application.targetFrameRate = 60;
            var worldCamera = FindFirstObjectByType<Camera>();
            GlobalManager.Instance.canvas.worldCamera = worldCamera;
            GlobalManager.Instance.canvas.sortingOrder = 10;
            GlobalManager.Instance.gameObject.AddComponent<DebugManager>();
        }

        private void OnEnable()
        {
            EventManager.Listen<ServerReady>(this);
            EventManager.Listen<ServerConnect>(this);
        }

        private void OnDisable()
        {
            EventManager.Remove<ServerReady>(this);
            EventManager.Remove<ServerConnect>(this);
        }

        private void Start()
        {
            UIManager.Show<LabelPanel>();
        }

        public void Execute(ServerConnect message)
        {
            if (NetworkManager.Server.connections == 1)
            {
                AssetManager.Load<GameObject>("Prefabs/SpawnManager", obj =>
                {
                    NetworkManager.Server.Spawn(obj);
                });
            }
        }

        public void Execute(ServerReady message)
        {
            AssetManager.Load<GameObject>("Prefabs/Player", obj =>
            {
                NetworkManager.Server.Spawn(obj, message.client);
            });
        }
    }
}