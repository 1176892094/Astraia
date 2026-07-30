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
        private bool CanDash => Feature.DashCount > 0 && Feature.DashInput > Time.fixedTime && Feature.DashCD < Time.fixedTime;
        private bool CanJump => Feature.JumpCount > 0 && Feature.JumpInput > Time.fixedTime && Feature.JumpCD < Time.fixedTime && (State & State.跳跃) == 0;
        private bool CanGround => (State & State.左墙) != 0 || (State & State.右墙) != 0 || (State & State.地面) != 0 || (State & State.平台) != 0;
        private bool CanPlatform => (State & State.地面) == 0 && (State & State.冲刺) == 0 && (State & State.平台) != 0;

        private State State
        {
            get => Feature.State;
            set => Feature.State = value;
        }

        public new void Dequeue()
        {
            GameManager.Dash.performed += DashButton;
            GameManager.Jump.started += JumpButton;
            GameManager.Jump.canceled += FallButton;
        }

        protected override void Enqueue()
        {
            GameManager.Dash.performed -= DashButton;
            GameManager.Jump.started -= JumpButton;
            GameManager.Jump.canceled -= FallButton;
        }

        private void DashButton(InputAction.CallbackContext obj)
        {
            if (GameManager.Direction != Vector2.zero)
            {
                var input = new Vector2(GameManager.MoveX, GameManager.MoveY);
                var success = true;
                Collider2D collider = null;
                foreach (var hit in owner.Machine.collision.Boxcast(input.normalized, input.magnitude, LayerConst.GroundAndCollision))
                {
                    if (hit.collider.CompareTag("Collision"))
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

            Feature.DashInput = Time.fixedTime + 0.2f;
        }

        private void JumpButton(InputAction.CallbackContext obj)
        {
            if (GameManager.MoveY < 0)
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
            Feature.JumpInput = Time.fixedTime + 0.2f;
        }

        private void FallButton(InputAction.CallbackContext obj)
        {
            State &= ~State.缓冲;
        }

        public void Tick()
        {
            if (CanDash)
            {
                if ((State & State.冲刺) == 0)
                {
                    State |= State.冲刺;
                }
                else
                {
                    Feature.DashTimer += Feature.DashTime;
                    Feature.DashDirection = GameManager.Direction;
                    Feature.DashCount--;
                }

                Feature.DashInput = 0;
            }

            if (CanGround)
            {
                Feature.JumpTimer = Time.fixedTime + 0.2F;
            }

            if (Feature.JumpTimer > Time.fixedTime)
            {
                if (CanJump)
                {
                    State |= State.跳跃;
                }
            }
        }
    }
}