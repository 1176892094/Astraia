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
        private bool CanGround => (State & State.左墙) != 0 || (State & State.右墙) != 0 || (State & State.地面) != 0 || (State & State.平台) != 0;
        private bool CanPlatform => (State & State.地面) == 0 && (State & State.冲刺) == 0 && (State & State.平台) != 0;

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
            if (InputManager.Direction != Vector2.zero)
            {
                var input = new Vector2(InputManager.MoveX, InputManager.MoveY);
                var success = true;
                Collider2D collider = null;
                foreach (var hit in owner.Machine.collision.Boxcast(input.normalized, input.magnitude, LayerConst.GroundAndCollision))
                {
                    if (hit.collider.CompareTag("DashQuad"))
                    {
                        collider = hit.collider;
                    }
                    else
                    {
                        success = false;
                    }
                }

                if (collider && success)
                {
                    State |= State.穿梭;
                    collider.tag = "Untagged";
                    Feature.DashQuad = input.normalized;
                    return;
                }
            }

            Feature.DashInput = Time.time + 0.2f;
        }

        private void JumpButton(InputAction.CallbackContext obj)
        {
            if (InputManager.MoveY < 0)
            {
                var success = true;
                Collider2D collider = null;
                foreach (var hit in owner.Machine.collision.Boxcast(0.1F, LayerConst.GroundAndCollision))
                {
                    if (hit.collider.CompareTag("Platform") && CanPlatform)
                    {
                        collider = hit.collider;
                    }
                    else
                    {
                        success = false;
                    }
                }

                if (collider && success)
                {
                    Feature.Platform = Time.fixedTime + 0.1F;
                    return;
                }
            }

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
            }

            if (CanGround)
            {
                Feature.JumpTimer = Time.time + 0.2F;
            }

            if (Feature.JumpTimer > Time.time)
            {
                if (CanJump)
                {
                    State |= State.跳跃;
                }
            }
        }
    }
}