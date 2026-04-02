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

        public float ShadowFrame
        {
            get => GetFloat(Label.玩家阴影);
            set => SetFloat(Label.玩家阴影, value);
        }

        public float CrashSpeed => MoveSpeed * 2;

        public override void Dequeue()
        {
            MoveSpeed = 2;
            JumpForce = 4;
            DashSpeed = 5;
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
            State |= StateType.缓冲;
            Feature.JumpInput = Time.time + 0.2f;
        }

        private void FallButton(InputAction.CallbackContext obj)
        {
            State &= ~StateType.缓冲;
        }

        private void AttackButton(InputAction.CallbackContext obj)
        {
            State |= StateType.攻击;
        }

        private void FinishButton(InputAction.CallbackContext obj)
        {
            State &= ~StateType.攻击;
        }

        public void Update()
        {
            FallUpdate();
            DashUpdate();
            JumpUpdate();
        }

        private void FallUpdate()
        {
            State &= ~StateType.地面;
            State &= ~StateType.墙面;
            State &= ~StateType.顶面;
            foreach (var contact in Machine.rigidbody.Contacts(LayerConst.Ground))
            {
                if (contact.normal.y > 0.5F)
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

                if (contact.normal.y < -0.5F)
                {
                    State |= StateType.顶面;
                }

                if (contact.normal.x > 0.5F || contact.normal.x < -0.5F)
                {
                    if (!State.HasFlag(StateType.跳跃))
                    {
                        Feature.JumpCount = 1;
                    }

                    State |= StateType.墙面;
                }
            }

            if (State.HasFlag(StateType.冲刺))
            {
                return;
            }

            if (State.HasFlag(StateType.地面))
            {
                return;
            }

            velocityY = Mathf.Lerp(velocityY, Physics2D.gravity.y, Time.deltaTime * (State.HasFlag(StateType.缓冲) ? 1 : 2));
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
            Feature.DashCD = Time.time + 0.4F;
        }

        private void JumpUpdate()
        {
            if (State.HasFlag(StateType.地面))
            {
                Feature.JumpTimer = Time.time + 0.2F;
            }

            if (State.HasFlag(StateType.墙面))
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

            Machine.Switch(StateConst.Jump);
            Feature.JumpCD = Time.time + 0.3F;
        }
    }
}