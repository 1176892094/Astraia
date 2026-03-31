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
using UnityEngine.UI;
using Text = UnityEngine.UI.Text;

namespace Runtime
{
    [Serializable]
    public class GameManager : Singleton<GameManager, Entity>, ISystem, IEvent<ServerReady>, IEvent<ServerConnect>
    {
        [SerializeField] private Bounds bounds;
        [SerializeField] private Vector3 smooth;
        [SerializeField] private Transform player;
        [SerializeField] private Camera mainCamera;

        public override void Dequeue()
        {
            mainCamera = Camera.main;
            Application.targetFrameRate = 60;
            UIManager.SetCamera(mainCamera);
            UIManager.Show<LoadPanel>();
        }

        public void Update()
        {
            if (player)
            {
                CameraUtils.Move(mainCamera, player, ref smooth, 0.3f);
                CameraUtils.Clamp(mainCamera, bounds);
            }
        }

        public void SetPlayer(Transform player)
        {
            this.player = player;
        }

        public void SetBounds(Bounds bounds)
        {
            this.bounds = bounds;
        }

        public void Execute(ServerConnect message)
        {
            if (NetworkManager.Server.connections == 1)
            {
                var obj = AssetManager.Load<GameObject>("Prefabs/10001");
                NetworkManager.Server.Spawn(obj);
            }
        }

        public void Execute(ServerReady message)
        {
            var obj = AssetManager.Load<GameObject>("Prefabs/30001");
            NetworkManager.Server.Spawn(obj, message.client);
        }
    }

    public static class CameraUtils
    {
        public static void Clamp(Camera camera, Bounds bounds)
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

        public static void Move(Camera camera, Transform target, ref Vector3 smooth, float speed)
        {
            var cam = camera.transform.parent;
            var targetPos = new Vector3(target.position.x, target.position.y, cam.position.z);
            Vector3 smoothPos;
            if (camera.targetTexture)
            {
                var pixelate = camera.orthographicSize * 2 / camera.targetTexture.height;
                smoothPos = Vector3.Distance(target.position, cam.position) > pixelate ? Vector3.SmoothDamp(cam.position, targetPos, ref smooth, speed) : target.position;
                smoothPos.x = Mathf.Round(smoothPos.x / pixelate) * pixelate;
                smoothPos.y = Mathf.Round(smoothPos.y / pixelate) * pixelate;
            }
            else
            {
                smoothPos = Vector3.SmoothDamp(cam.position, targetPos, ref smooth, speed);
            }

            cam.position = new Vector3(smoothPos.x, smoothPos.y, cam.position.z);
        }
    }

    [UIMask(2)]
    public class LoadPanel : UIPanel, ITween
    {
        [Inject] private Image panel;

        public override async void OnShow()
        {
            await panel.DOFade(1, 0.25f);
            UIManager.Hide<LoadPanel>();
        }


        public override async void OnHide()
        {
            await panel.DOFade(0, 0.5f);
            gameObject.SetActive(false);
            UIManager.Show<LabelPanel>();
        }
    }

    [UIMask(1)]
    public class LabelPanel : UIPanel
    {
        [Inject] private Text message;
        [Inject] private Button prevButton;
        [Inject] private Button nextButton;

        public override void OnShow()
        {
            message.text = "1.点击调试器\n2.找到Network面板\n3.房主启动 Host 模式\n4.房员启动 Client 模式";
        }

        private void PrevButton()
        {
            message.text = "1.点击调试器\n2.找到Network面板\n3.房主启动 Host 模式\n4.房员启动 Client 模式";
        }

        private void NextButton()
        {
            message.text = "1.使用方向键进行移动\n2.使用Z键抓取墙壁\n3.使用X键进行跳跃\n4.使用C键进行冲刺";
        }
    }
}