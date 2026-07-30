using System;
using Astraia;
using Astraia.Net;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Runtime
{
    [Serializable]
    public class GameManager : Export, IEvent<ServerReady>
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

        protected override void Awake()
        {
            Application.targetFrameRate = 60;
            inputAsset = AssetManager.Load<InputActionAsset>("Settings/InputManager");
            inputAsset.Enable();
        }

        protected override void OnDestroy()
        {
            inputAsset.Disable();
        }

        private void Start()
        {
            transform.Wait(0.1F).OnComplete(() =>
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

        public void Execute(ServerReady message)
        {
            if (NetworkManager.Server.connections == 1)
            {
                NetworkManager.Server.Spawn(AssetManager.Load<GameObject>("Prefabs/10004"));
            }

            NetworkManager.Server.Spawn(AssetManager.Load<GameObject>("Prefabs/10001"), message.client);
        }
    }
}