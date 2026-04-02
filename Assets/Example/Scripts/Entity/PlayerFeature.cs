// // *********************************************************************************
// // # Project: JFramework
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-04-20 19:04:06
// // # Recently: 2025-04-20 19:04:06
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using Astraia;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Runtime
{
    [Serializable]
    public class PlayerFeature : Blackboard<Label, Player>
    {
        public float MoveSpeed
        {
            get => GetFloat(Label.移动速度);
            set => SetFloat(Label.移动速度, value);
        }

        public float JumpForce
        {
            get => GetFloat(Label.跳跃力度);
            set => SetFloat(Label.跳跃力度, value);
        }

        public float JumpTimer
        {
            get => GetFloat(Label.土狼时间);
            set => SetFloat(Label.土狼时间, value);
        }

        public float JumpInput
        {
            get => GetFloat(Label.跳跃输入);
            set => SetFloat(Label.跳跃输入, value);
        }

        public float JumpCount
        {
            get => GetFloat(Label.跳跃次数);
            set => SetFloat(Label.跳跃次数, value);
        }

        public float JumpCD
        {
            get => GetFloat(Label.跳跃冷却);
            set => SetFloat(Label.跳跃冷却, value);
        }

        public float DashInput
        {
            get => GetFloat(Label.冲刺输入);
            set => SetFloat(Label.冲刺输入, value);
        }

        public float DashCount
        {
            get => GetFloat(Label.冲刺次数);
            set => SetFloat(Label.冲刺次数, value);
        }

        public float DashSpeed
        {
            get => GetFloat(Label.冲刺速度);
            set => SetFloat(Label.冲刺速度, value);
        }

        public float DashTimer
        {
            get => GetFloat(Label.冲刺时间);
            set => SetFloat(Label.冲刺时间, value);
        }

        public float DashCD
        {
            get => GetFloat(Label.冲刺冷却);
            set => SetFloat(Label.冲刺冷却, value);
        }

        public float WaitTime
        {
            get => GetFloat(Label.等待时间);
            set => SetFloat(Label.等待时间, value);
        }

        public override void Dequeue()
        {
            MoveSpeed = 2;
            JumpForce = 5;
            DashSpeed = 5;
        }
    }

    public class PlayerInput : Module<Player>
    {
        private PlayerFeature Feature => owner.Feature;
        private PlayerMachine Machine => owner.Machine;
        private Bounds Bounds => Machine.collider.bounds;
        public float velocity;

        private StateType State
        {
            get => owner.State;
            set => owner.State = value;
        }

        private float velocityX
        {
            get => Machine.velocityX;
            set => Machine.velocityX = value;
        }

        private float velocityY
        {
            get => Machine.velocityY;
            set => Machine.velocityY = value;
        }

        public override void Dequeue()
        {
            InputManager.Dash.performed += DashButton;
            InputManager.Jump.started += JumpButton;
            InputManager.Jump.canceled += FallButton;
            InputManager.Attack.started += AttackButton;
            InputManager.Attack.canceled += FinishButton;
        }

        public override void Enqueue()
        {
            InputManager.Dash.performed -= DashButton;
            InputManager.Jump.started -= JumpButton;
            InputManager.Jump.canceled -= FallButton;
            InputManager.Attack.started -= AttackButton;
            InputManager.Attack.canceled -= FinishButton;
        }

        private void DashButton(InputAction.CallbackContext obj)
        {
            Feature.DashInput = Time.time + 0.2f;
        }

        private void JumpButton(InputAction.CallbackContext obj)
        {
            State |= StateType.跳跃缓冲;
            Feature.JumpInput = Time.time + 0.2f;
        }

        private void FallButton(InputAction.CallbackContext obj)
        {
            State &= ~StateType.跳跃缓冲;
        }

        private void AttackButton(InputAction.CallbackContext obj)
        {
            State |= StateType.抓墙缓冲;
        }

        private void FinishButton(InputAction.CallbackContext obj)
        {
            State &= ~StateType.抓墙缓冲;
        }

        public void Update()
        {
            MoveUpdate();
            DashUpdate();
            GroundUpdate();
            FallUpdate();
        }

        private void MoveUpdate()
        {
            if (State.HasFlag(StateType.冲刺))
            {
                return;
            }

            var moveX = InputManager.MoveX;
            owner.Sender.Direction = moveX;

            if (State.HasFlag(StateType.冲刺跳))
            {
                return;
            }

            if (State.HasFlag(StateType.跳跃))
            {
                return;
            }

            if (State.HasFlag(StateType.下落))
            {
                return;
            }

            if (moveX != 0)
            {
                velocityX = Mathf.Lerp(velocityX, Feature.MoveSpeed * moveX, 0.2F);
            }
            else
            {
                velocityX = Mathf.Abs(velocityX) > 0.01F ? Mathf.Lerp(velocityX, 0, 0.1F) : 0;
            }
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

            Machine.Switch(StateConst.Dash);
            Feature.DashInput = Time.time;
            Feature.DashCD = Time.time + 0.4F;
        }

        private void JumpUpdate()
        {
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

            Machine.Switch(StateConst.Jump);
            Feature.JumpInput = Time.time;
            Feature.JumpCD = Time.time + 0.3F;
        }

        private void GroundUpdate()
        {
            if (State.HasFlag(StateType.地面))
            {
                Feature.JumpTimer = Time.time + 0.2F;
                JumpUpdate();
            }
            else if (State.HasFlag(StateType.墙面))
            {
                Feature.JumpTimer = Time.time + 0.2F;
                JumpUpdate();
            }

            if (Feature.JumpTimer > Time.time)
            {
                JumpUpdate();
            }
        }

        private void FallUpdate()
        {
            var collider = Physics2D.BoxCast(Bounds.center, Bounds.size, 0, Vector2.down, 0.01F, 1 << 6);
            if (collider)
            {
                if (!State.HasFlag(StateType.跳跃))
                {
                    Feature.JumpCount = 1;
                }

                if (!State.HasFlag(StateType.冲刺))
                {
                    Feature.DashCount = 1;
                }

                State |= StateType.地面;
            }
            else
            {
                State &= ~StateType.地面;
            }

            if (State.HasFlag(StateType.冲刺))
            {
                return;
            }

            if (State.HasFlag(StateType.地面))
            {
                return;
            }

            velocityY = Mathf.SmoothDamp(velocityY, -9.81F, ref velocity, State.HasFlag(StateType.跳跃缓冲) ? 0.8F : 0.4F);
        }
    }
}