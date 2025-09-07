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

using System;
using Astraia;
using Astraia.Common;
using Astraia.Net;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Runtime
{
    public struct GameSystem : ISystem
    {
        public void Update()
        {
            GameManager.Instance.Update();
        }
    }

    [Serializable]
    public class GameManager : Singleton<GameManager, Entity>, IEvent<ServerReady>, IEvent<ServerConnect>
    {
        [SerializeField] private Vector2 center;
        [SerializeField] private Vector2 content;
        [SerializeField] private Player player;
        [SerializeField] private Camera mainCamera;

        public override void Dequeue()
        {
            mainCamera = Object.FindFirstObjectByType<Camera>();
            GlobalManager.Instance.canvas.worldCamera = mainCamera;
            GlobalManager.Instance.canvas.sortingOrder = 10;
            GlobalManager.Instance.gameObject.AddComponent<DebugManager>();
            Application.targetFrameRate = 60;
            PageManager.Show<LabelPanel>();
        }

        public override void OnShow()
        {
            EventManager.Listen<ServerReady>(this);
            EventManager.Listen<ServerConnect>(this);
        }

        public override void OnHide()
        {
            EventManager.Remove<ServerReady>(this);
            EventManager.Remove<ServerConnect>(this);
        }

        public void Update()
        {
            if (!player || !player.transform)
            {
                return;
            }

            var position = player.transform.position;
            var x = position.x;
            if (x < center.x - content.x)
            {
                x = center.x - content.x;
            }
            else if (x > center.x + content.x)
            {
                x = center.x + content.x;
            }

            var y = position.y;
            if (y < center.y - content.y)
            {
                y = center.y - content.y;
            }
            else if (y > center.y + content.y)
            {
                y = center.y + content.y;
            }

            position = new Vector3(x, y, 0);
            var distance = Vector3.Distance(transform.position, position);
            transform.position = Vector3.Lerp(transform.position, position, distance < 0.1f ? 0.5f - distance : 0.1f);
        }

        public void SetCamera(Player player, Vector3 center, Vector2 sizeData)
        {
            this.player = player;
            this.center = center;
            var height = sizeData.y / 2 - mainCamera.orthographicSize;
            var width = sizeData.x / 2 - mainCamera.orthographicSize * Screen.width / Screen.height;
            content = new Vector2(Math.Max(0, width), Math.Max(0, height));
        }

        public void Execute(ServerConnect message)
        {
            if (NetworkManager.Server.connections == 1)
            {
                AssetManager.Load<GameObject>("Prefabs/10001", obj => { NetworkManager.Server.Spawn(obj); });
            }
        }

        public void Execute(ServerReady message)
        {
            AssetManager.Load<GameObject>("Prefabs/30001", obj => { NetworkManager.Server.Spawn(obj, message.client); });
        }
    }
}