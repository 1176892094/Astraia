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
using Astraia.Core;
using Astraia.Net;
using UnityEngine;

namespace Runtime
{
    [Serializable]
    public class GameManager : Singleton<GameManager>, IEvent<ServerReady>, IEvent<OnEarlyUpdate>
    {
        private Bounds bounds;
        [SerializeField] private Vector3 smooth;
        [SerializeField] private Transform player;
        [SerializeField] private Camera mainCamera;

        public override void Dequeue()
        {
            mainCamera = Camera.main;
            Application.targetFrameRate = 60;
            UIManager.Canvas.worldCamera = mainCamera;
            UIManager.Show<LoadPanel>();

            owner.transform.Wait(0.1F).OnComplete(() =>
            {
                try
                {
                    NetworkManager.StartHost();
                }
                catch
                {
                    NetworkManager.StartClient();
                }
            });
        }

        public void SetPlayer(Transform player)
        {
            this.player = player;
        }

        public void SetBounds(Bounds bounds)
        {
            this.bounds = bounds;
        }

        public void Execute(ServerReady message)
        {
            var obj = AssetManager.Load<GameObject>("Prefabs/30001");
            NetworkManager.Server.Spawn(obj, message.client);
        }

        public void Execute(OnEarlyUpdate message)
        {
            if (player)
            {
                CameraModule.Move(mainCamera, player, ref smooth, 0.3f);
                CameraModule.Step(mainCamera, bounds);
            }
        }
    }

    public static class CameraModule
    {
        public static void Move(Camera camera, Transform target, ref Vector3 smooth, float speed)
        {
            var cam = camera.transform.parent;
            var pixelate = camera.orthographicSize * 2 / (480 / camera.aspect);
            var targetPos = new Vector3(target.position.x, target.position.y, cam.position.z);
            var smoothPos = Vector3.Distance(target.position, cam.position) > pixelate ? Vector3.SmoothDamp(cam.position, targetPos, ref smooth, speed) : target.position;
            smoothPos.x = Mathf.Round(smoothPos.x / pixelate) * pixelate;
            smoothPos.y = Mathf.Round(smoothPos.y / pixelate) * pixelate;
            cam.position = new Vector3(smoothPos.x, smoothPos.y, cam.position.z);
        }

        public static void Step(Camera camera, Bounds bounds)
        {
            var cam = camera.transform.parent;
            var pos = cam.position;

            var min = bounds.min;
            var max = bounds.max;

            var w = max.x - min.x;
            var h = max.y - min.y;

            var x = camera.orthographicSize;
            var y = camera.orthographicSize * camera.aspect;

            pos.x = w <= y * 2 ? bounds.center.x : Mathf.Clamp(pos.x, min.x + y, max.x - y);
            pos.y = h <= x * 2 ? bounds.center.y : Mathf.Clamp(pos.y, min.y + x, max.y - x);

            cam.position = pos;
        }
    }
}