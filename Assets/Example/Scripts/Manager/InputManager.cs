using System;
using Astraia;
using Astraia.Net;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Runtime
{
    [Serializable]
    public class InputManager : MonoBehaviour
    {
        private static int moveX;
        private static int moveY;
        private static InputActionAsset inputAsset;
        public static InputAction Bag => inputAsset.FindAction(nameof(Bag));
        public static InputAction Move => inputAsset.FindAction(nameof(Move));
        public static InputAction Jump => inputAsset.FindAction(nameof(Jump));
        public static InputAction Dash => inputAsset.FindAction(nameof(Dash));
        public static InputAction Attack => inputAsset.FindAction(nameof(Attack));
        public static InputAction Escape => inputAsset.FindAction(nameof(Escape));
        public static int MoveX => moveX != 0 ? moveX : Move.ReadValue<Vector2>().x > 0 ? 1 : Move.ReadValue<Vector2>().x < 0 ? -1 : 0;
        public static int MoveY => moveY != 0 ? moveY : Move.ReadValue<Vector2>().y > 0 ? 1 : Move.ReadValue<Vector2>().y < 0 ? -1 : 0;
        public static Vector2 Direction => new Vector2(MoveX, MoveY).normalized;

        private void Awake()
        {
            inputAsset = AssetManager.Load<InputActionAsset>("Settings/InputManager");
            inputAsset.Enable();
        }

        private void OnDestroy()
        {
            inputAsset.Disable();
        }
    }

    [Serializable]
    public class CameraManager : Singleton<CameraManager>, IEvent<ServerReady>, IEvent<OnEarlyUpdate>
    {
        private Bounds bounds;
        private Vector3 velocity;
        private Vector3 position;

        private Camera camera;
        private Transform player;

        protected override void Dequeue()
        {
            camera = Camera.main;
            Application.targetFrameRate = 60;
            owner.Wait(0.1F).OnComplete(() =>
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

        public void SetBounds(Bounds bounds)
        {
            this.bounds = bounds;
        }

        public void SetPlayer(Transform player)
        {
            this.player = player;
        }

        public void Execute(ServerReady message)
        {
            if (NetworkManager.Server.connections == 1)
            {
                NetworkManager.Server.Spawn(AssetManager.Load<GameObject>("Prefabs/10004"));
            }

            NetworkManager.Server.Spawn(AssetManager.Load<GameObject>("Prefabs/10001"), message.client);
        }

        public void Execute(OnEarlyUpdate message)
        {
            Move(camera, player, bounds, ref position, ref velocity, Time.deltaTime * 10);
        }

        public static void Move(Camera camera, Transform target, Bounds bounds, ref Vector3 velocity, ref Vector3 position, float smoothTime)
        {
            if (target)
            {
                var cam = camera.transform;

                var min = bounds.min;
                var max = bounds.max;

                var ps = camera.orthographicSize * 2f / (480f / camera.aspect);

                var eh = camera.orthographicSize;
                var ew = eh * camera.aspect;

                var sw = max.x - min.x;
                var sh = max.y - min.y;

                // 目标位置
                var targetPos = new Vector2(target.position.x, target.position.y);

                // 提前限制目标点
                targetPos.x = sw <= ew * 2 ? bounds.center.x : Mathf.Clamp(targetPos.x, min.x + ew, max.x - ew);
                targetPos.y = sh <= eh * 2 ? bounds.center.y : Mathf.Clamp(targetPos.y, min.y + eh, max.y - eh);

                // 平滑移动
                position = Vector3.SmoothDamp(position, targetPos, ref velocity, smoothTime);

                // 像素对齐
                var smoothPos = position;
                smoothPos.x = Mathf.Round(smoothPos.x / ps) * ps;
                smoothPos.y = Mathf.Round(smoothPos.y / ps) * ps;
                cam.parent.position = smoothPos;
            }
        }
    }
}