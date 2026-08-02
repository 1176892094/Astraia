using System;
using Astraia;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Runtime
{
    public struct OnPlayerUpdate : IEvent { }

    public struct OnPlatformUpdate : IEvent { }

    [Serializable]
    public class GameManager : Export
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

        public void FixedUpdate()
        {
            if (SyncManager.Instance != null && SyncManager.Instance.syncTime != 0)
            {
                EventManager.Invoke(new OnPlatformUpdate());
                Physics2D.SyncTransforms();
                EventManager.Invoke(new OnPlayerUpdate());
            }
        }

        protected override void OnDestroy()
        {
            inputAsset.Disable();
        }
    }
}