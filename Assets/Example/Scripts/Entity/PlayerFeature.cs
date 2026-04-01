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
        public StateType state = StateType.默认;
        public bool isWalk => InputManager.MoveX != 0 || InputManager.MoveY != 0;
        public float moveSpeed => GetFloat(Label.移动速度);
        public float jumpForce => GetFloat(Label.跳跃力度);
        public float dashSpeed => GetFloat(Label.冲刺速度);
        public float dashFrame => GetFloat(Label.冲刺时间);
        public float waitFrame => GetFloat(Label.等待时间);

        public override void Dequeue()
        {
            SetFloat(Label.移动速度, 2);
            SetFloat(Label.跳跃力度, 4);
            SetFloat(Label.冲刺速度, 5);
        }

        public void Update()
        {
            if (owner.DRHit || owner.DLHit)
            {
                state |= StateType.地面;
                SetInt(Label.跳跃次数, 1);
                SetInt(Label.冲刺次数, 1);
            }
            else
            {
                state &= ~StateType.地面;
            }

            if (owner.RDHit)
            {
                state |= StateType.墙面;
                SetInt(Label.跳跃次数, 1);
            }
            else
            {
                state &= ~StateType.墙面;
            }
        }
    }

    public class PlayerInput : Module<Player>
    {
        private PlayerFeature Feature => owner.Feature;

        public void Update()
        {
            if (InputManager.MoveX > 0)
            {
                transform.localScale = new Vector3(InputManager.MoveX, 1, 1);
            }
            else if (InputManager.MoveX < 0)
            {
                transform.localScale = new Vector3(InputManager.MoveX, 1, 1);
            }

            DashUpdate();

            if (Feature.state.HasFlag(StateType.地面))
            {
                Feature.SetFloat(Label.土狼时间, Time.time + 0.2f);
                JumpUpdate();
            }
            else if (Feature.state.HasFlag(StateType.墙面))
            {
                if (!Feature.state.HasFlag(StateType.抓墙缓冲))
                {
                    FallUpdate();
                }

                Feature.SetFloat(Label.土狼时间, Time.time + 0.2f);
                JumpUpdate();
            }
            else
            {
                FallUpdate();
            }

            if (Feature.GetFloat(Label.土狼时间) > Time.time)
            {
                JumpUpdate();
            }
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
            Feature.SetFloat(Label.冲刺输入, Time.time + 0.2f);
        }

        private void JumpButton(InputAction.CallbackContext obj)
        {
            Feature.state |= StateType.跳跃缓冲;
            Feature.SetFloat(Label.跳跃输入, Time.time + 0.2f);
        }

        private void FallButton(InputAction.CallbackContext obj)
        {
            Feature.state &= ~StateType.跳跃缓冲;
        }

        private void AttackButton(InputAction.CallbackContext obj)
        {
            Feature.state |= StateType.抓墙缓冲;
        }

        private void FinishButton(InputAction.CallbackContext obj)
        {
            Feature.state &= ~StateType.抓墙缓冲;
        }

        private void FallUpdate()
        {
            if (Feature.state.HasFlag(StateType.冲刺))
            {
                return;
            }

            if (Feature.state.HasFlag(StateType.跳跃缓冲))
            {
                owner.Machine.velocityY -= 9.81f * Time.deltaTime;
            }
            else
            {
                owner.Machine.velocityY -= 9.81f * Time.deltaTime * 2;
            }

            owner.Machine.velocityY = Mathf.Max(-5, owner.Machine.velocityY);
        }

        private void JumpUpdate()
        {
            if (Feature.GetInt(Label.跳跃次数) <= 0)
            {
                return;
            }

            if (Feature.GetFloat(Label.跳跃输入) < Time.time)
            {
                return;
            }

            if (!Feature.state.HasFlag(StateType.跳跃))
            {
                owner.Machine.Switch(StateConst.Jump);
            }

            Feature.SetFloat(Label.跳跃输入, Time.time);
        }

        private void DashUpdate()
        {
            if (Feature.GetInt(Label.冲刺次数) <= 0)
            {
                return;
            }

            if (Feature.GetFloat(Label.冲刺输入) < Time.time)
            {
                return;
            }

            if (!Feature.state.HasFlag(StateType.冲刺))
            {
                owner.Machine.Switch(StateConst.Dash);
            }

            Feature.SetFloat(Label.冲刺输入, Time.time);
        }
    }
}