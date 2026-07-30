using Astraia;
using UnityEngine;

namespace Runtime
{
    public class PlayerState : State<Player>
    {
        protected PlayerMachine Machine => owner.Machine;
        protected PlayerFeature Feature => owner.Feature;
        protected Rigidbody.Collision collision => Machine.collision;
        protected bool isWalk => InputManager.MoveX != 0;
        protected bool isGrab => isWall && velocityY < 0;
        protected bool isDash => (state & State.冲刺) != 0;
        protected bool isJump => (state & State.跳跃) != 0;
        protected bool isHold => (state & State.悬挂) != 0;
        protected bool isRush => (state & State.横冲) != 0;
        protected bool isWall => (state & State.墙面) != 0;
        protected bool isHead => (state & State.头顶) != 0;
        protected bool isPlane => (state & State.平面) != 0;
        protected bool isGround => (state & State.地面) != 0;
        protected bool isShuttle => (state & State.穿梭) != 0;
        protected bool isPlatform => (state & State.平台) != 0;
        protected bool isCorner => (state & State.碰撞) != 0;

        protected State state
        {
            get => Feature.State;
            set => Feature.State = value;
        }

        protected int direction
        {
            get => owner.Direction;
            set => owner.Direction = value;
        }

        protected Fixation velocityX
        {
            get => Machine.velocityX;
            set => Machine.velocityX = value;
        }

        protected Fixation velocityY
        {
            get => Machine.velocityY;
            set => Machine.velocityY = value;
        }

        private Fixation positionX
        {
            get => Machine.positionX;
            set => Machine.positionX = value;
        }

        private Fixation positionY
        {
            get => Machine.positionY;
            set => Machine.positionY = value;
        }

        protected void InputX(int moveX, float expansion = 1, float percent = 0.16F)
        {
            if (moveX != 0)
            {
                var moveSpeed = moveX * owner.Feature.MoveSpeed;
                if (direction != moveX || Mathf.Abs(velocityX) < Mathf.Abs(moveSpeed / 2))
                {
                    direction = moveX;
                    velocityX = moveSpeed / 2;
                }
                else
                {
                    velocityX = Mathf.Lerp(velocityX, moveSpeed * expansion, percent);
                }
            }
            else if (Mathf.Abs(velocityX) > 0.01F)
            {
                velocityX = Mathf.Lerp(velocityX, 0, percent);
            }
            else
            {
                velocityX = 0;
            }
        }

        protected void InputY(int moveY)
        {
            velocityY = moveY * owner.Feature.MoveSpeed;
            state &= ~State.碰撞;
        }

        protected void InputY()
        {
            if (state.HasFlag(State.攀爬))
            {
                velocityY = Mathf.Max(velocityY - 0.012F, -0.04F);
            }
            else if (state.HasFlag(State.缓冲))
            {
                velocityY = Mathf.Max(velocityY - 0.012F, -0.24F);
            }
            else
            {
                velocityY = Mathf.Max(velocityY - 0.024F, -0.24F);
            }

            state &= ~State.碰撞;
        }

        protected void Apply()
        {
            var position = Machine.position;
            MoveX(Fixation.Sign(velocityX), Mathf.Abs(velocityX));
            MoveY(Fixation.Sign(velocityY), Mathf.Abs(velocityY));
            SendPosition(position, Machine.position);
        }

        private void SendPosition(Position oldValue, Position newValue)
        {
            if (owner.isOwner && oldValue != newValue)
            {
                Machine.MovePosition(newValue);
                SyncManager.Instance?.AddPosition(owner, newValue);
            }
        }

        protected float Distance(Position origin, Position target)
        {
            return Vector2.Distance(origin.ToVector2(), target.ToVector2());
        }

        private void MoveX(int moveX, float value)
        {
            if (moveX != 0)
            {
                foreach (var hit in collision.Boxcast(new Vector2(moveX, 0), value, LayerConst.GroundAndCollision))
                {
                    if (!hit.collider.CompareTag("Platform") && !hit.collider.CompareTag("Untagged"))
                    {
                        if (hit.distance >= 0)
                        {
                            if (moveX > 0)
                            {
                                state |= State.右墙;
                            }
                            else
                            {
                                state |= State.左墙;
                            }

                            Feature.JumpCount = 1;
                            Feature.GrabInput = -moveX;
                            Feature.GrabTimer = Time.fixedTime + 0.1F;
                            velocityX = moveX * hit.distance;
                        }
                    }
                }
            }

            positionX += velocityX;
        }

        private void MoveY(int moveY, float value)
        {
            if (moveY != 0)
            {
                foreach (var hit in collision.Boxcast(new Vector2(0, moveY), value, LayerConst.GroundAndCollision))
                {
                    if (!hit.collider.CompareTag("Platform") && !hit.collider.CompareTag("Untagged"))
                    {
                        if (hit.distance >= 0)
                        {
                            if (moveY > 0)
                            {
                                state |= State.头顶;
                            }
                            else
                            {
                                Feature.JumpCount = 1;
                                Feature.DashCount = 1;
                                state |= State.地面;
                            }

                            velocityY = moveY * hit.distance;
                        }
                    }
                }
            }

            if (moveY < 0 && Feature.Platform < Time.fixedTime)
            {
                foreach (var hit in collision.Boxcast(value, LayerConst.Collision))
                {
                    if (hit.collider.CompareTag("Platform"))
                    {
                        if (hit.distance >= 0)
                        {
                            Feature.JumpCount = 1;
                            Feature.DashCount = 1;
                            state |= State.平台;
                            velocityY = moveY * hit.distance;
                        }
                    }
                }
            }

            positionY += velocityY;
        }
    }
}