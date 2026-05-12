// // *********************************************************************************
// // # Project: JFramework
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-04-20 19:04:11
// // # Recently: 2025-04-20 19:04:11
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using Astraia;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Runtime
{
    public class PlayerMachine : StateMachine<int, Player>
    {
        public Collider2D collider;
        public SpriteRenderer renderer;

        public override void Dequeue()
        {
            collider = owner.GetComponent<Collider2D>();
            renderer = owner.GetComponent<SpriteRenderer>();
        }
    }

    public class PlayerFeature : Module<Player>
    {
        public int MoveSpeed;
        public int VelocityX;
        public int VelocityY;
        public int PositionX;
        public int PositionY;

        public int JumpForce;
        public int JumpCount;
        public float JumpInput;
        public float JumpTimer;
        public float JumpCD;

        public int DashSpeed;
        public int DashCount;
        public float DashInput;
        public float DashTimer;
        public float DashCD;

        public int CrashSpeed;
        public int CrashCount;
        public Vector3 CrashPoint;

        public override void Dequeue()
        {
            MoveSpeed = 30;
            JumpForce = 60;
            DashSpeed = 120;
            CrashSpeed = 60;
        }
    }

    public class PlayerInput : Module<Player>
    {
        private PlayerFeature Feature => owner.Feature;
        private PlayerMachine Machine => owner.Machine;

        private StateType State
        {
            get => owner.State;
            set => owner.State = value;
        }

        public override void Dequeue()
        {
            InputManager.Dash.performed += DashButton;
            InputManager.Jump.started += JumpButton;
            InputManager.Jump.canceled += FallButton;
        }

        public override void Enqueue()
        {
            InputManager.Dash.performed -= DashButton;
            InputManager.Jump.started -= JumpButton;
            InputManager.Jump.canceled -= FallButton;
        }

        private void DashButton(InputAction.CallbackContext obj)
        {
            Feature.DashInput = Time.time + 0.2f;
        }

        private void JumpButton(InputAction.CallbackContext obj)
        {
            State |= StateType.缓冲;
            Feature.JumpInput = Time.time + 0.2f;
        }

        private void FallButton(InputAction.CallbackContext obj)
        {
            State &= ~StateType.缓冲;
        }


        public void Update()
        {
            DashUpdate();
            JumpUpdate();
        }

        private void DashUpdate()
        {
            if (Feature.DashCount <= 0)
            {
                return;
            }

            if (Feature.DashInput < Time.time)
            {
                return;
            }

            if (Feature.DashCD > Time.time)
            {
                return;
            }

            if (State.HasFlag(StateType.冲刺))
            {
                return;
            }

            Machine.Switch(Animations.Dash);
            Feature.DashCD = Time.time + 0.4F;
        }

        private void JumpUpdate()
        {
            if (State.HasFlag(StateType.地面))
            {
                Feature.JumpTimer = Time.time + 0.2F;
            }

            if (State.HasFlag(StateType.左墙) || State.HasFlag(StateType.右墙))
            {
                Feature.JumpTimer = Time.time + 0.2F;
            }

            if (Feature.JumpTimer < Time.time)
            {
                return;
            }

            if (Feature.JumpCount <= 0)
            {
                return;
            }

            if (Feature.JumpInput < Time.time)
            {
                return;
            }

            if (Feature.JumpCD > Time.time)
            {
                return;
            }

            if (State.HasFlag(StateType.跳跃))
            {
                return;
            }

            Machine.Switch(Animations.Jump);
            Feature.JumpCD = Time.time + 0.3F;
        }
    }
}