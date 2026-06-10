using System;
using Astraia;
using Astraia.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Runtime
{
    [Serializable]
    public class InputManager : Singleton<InputManager>
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
        }

        public override void Enqueue()
        {
            inputAsset.Disable();
        }
    }
}