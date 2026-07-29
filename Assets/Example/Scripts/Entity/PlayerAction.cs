using System;
using Astraia;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Runtime
{
    [Serializable]
    public class PlayerAction : Module<Player>
    {
        private PlayerFeature Feature => owner.Feature;
        private bool CanDash => Feature.DashCount > 0 && Feature.DashInput > Time.time && Feature.DashCD < Time.time && (State & State.冲刺) == 0;
        private bool CanJump => Feature.JumpCount > 0 && Feature.JumpInput > Time.time && Feature.JumpCD < Time.time && (State & State.跳跃) == 0;

        private State State
        {
            get => Feature.State;
            set => Feature.State = value;
        }

        public new void Dequeue()
        {
            InputManager.Dash.performed += DashButton;
            InputManager.Jump.started += JumpButton;
            InputManager.Jump.canceled += FallButton;
        }

        protected override void Enqueue()
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
            State |= State.缓冲;
            Feature.JumpInput = Time.time + 0.2f;
        }

        private void FallButton(InputAction.CallbackContext obj)
        {
            State &= ~State.缓冲;
        }

        public void Tick()
        {
            if (CanDash)
            {
                State |= State.冲刺;
                Feature.DashCD = Time.time + 0.4F;
            }

            if (State.HasFlag(State.地面) || State.HasFlag(State.左墙) || State.HasFlag(State.右墙))
            {
                Feature.JumpTimer = Time.time + 0.2F;
            }

            if (Feature.JumpTimer > Time.time)
            {
                if (CanJump)
                {
                    State |= State.跳跃;
                    Feature.JumpCD = Time.time + 0.3F;
                }
            }
        }
    }
}