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
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;


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

    [Serializable]
    public class InputManager : Singleton<InputManager, Entity>
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

        public override void Dequeue()
        {
            inputAsset = AssetManager.Load<InputActionAsset>("Settings/InputManager");
            inputAsset.Enable();
            inputAsset.LoadBindingOverridesFromJson(JsonManager.Load<string>(nameof(InputManager)));
        }

        public override void Enqueue()
        {
            inputAsset.Disable();
        }
    }

    public class SpawnManager : NetworkModule
    {
        public static SpawnManager Instance;

        public override void Dequeue()
        {
            Instance = this;
            Object.DontDestroyOnLoad(gameObject);
        }

        [ClientRpc]
        public async void LoadEffectClientRpc(Vector3 position)
        {
            var sprite = PoolManager.Show<SpriteRenderer>("Prefabs/Effect", position);
            sprite.color = new Color(0, 0, 0, 1);
            await sprite.DOFade(0, 0.5f);
            PoolManager.Hide(sprite);
        }
    }
}