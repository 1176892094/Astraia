using System;
using Astraia;
using UnityEngine;

namespace Runtime
{
    public abstract class PlayerState : State<Player>
    {
        protected bool isCrash => InputManager.MoveY != 1 && InputManager.MoveX != 0;
        protected bool isGround => (state & State.地面) != 0;
        protected bool isHead => (state & State.头顶) != 0;
        protected bool isWall => (state & State.墙面) != 0;
        protected bool isCorner => isWall || isGround;
        protected bool isFall => !isGround && velocityY < 0;
        protected bool isGrab => isWall && isFall;
        protected Transform transform => owner.transform;
        protected PlayerMachine Machine => owner.Machine;
        protected PlayerFeature Feature => owner.Feature;

        protected State state
        {
            get => Feature.State;
            set => Feature.State = value;
        }

        protected int direction
        {
            get => owner.Sender.Direction;
            set => owner.Sender.Direction = value;
        }

        protected int velocityX
        {
            get => Machine.velocityX;
            set => Machine.velocityX = value;
        }

        protected int velocityY
        {
            get => Machine.velocityY;
            set => Machine.velocityY = value;
        }

        private int positionX
        {
            get => Machine.positionX;
            set => Machine.positionX = value;
        }

        private int positionY
        {
            get => Machine.positionY;
            set => Machine.positionY = value;
        }

        protected void Move(int percent = 0)
        {
            var moveX = InputManager.MoveX;
            if (moveX != 0)
            {
                var moveSpeed = moveX * Feature.MoveSpeed;
                if (direction != moveX || Mathf.Abs(velocityX) < Mathf.Abs(moveSpeed / 2))
                {
                    direction = moveX;
                    velocityX = moveSpeed / 2;
                }
                else
                {
                    moveSpeed += moveSpeed * percent / 10;
                    switch (velocityX)
                    {
                        case > 0 when velocityX < moveSpeed:
                            velocityX++;
                            break;
                        case < 0 when velocityX > moveSpeed:
                            velocityX--;
                            break;
                        case < 0 when velocityX < moveSpeed:
                            velocityX++;
                            break;
                        case > 0 when velocityX > moveSpeed:
                            velocityX--;
                            break;
                    }
                }
            }
            else if (velocityX != 0)
            {
                switch (velocityX)
                {
                    case > 0:
                        velocityX = Mathf.Max(velocityX - 2, 0);
                        break;
                    case < 0:
                        velocityX = Mathf.Min(velocityX + 2, 0);
                        break;
                }
            }
            else
            {
                velocityX = 0;
            }

            Gravity();
            Collision();
        }

        protected void Gravity()
        {
            if (state.HasFlag(State.攀爬))
            {
                velocityY = Mathf.Max(velocityY - 1, -10);
            }
            else if (state.HasFlag(State.缓冲))
            {
                velocityY = Mathf.Max(velocityY - 3, -60);
            }
            else
            {
                velocityY = Mathf.Max(velocityY - 6, -60);
            }

            state &= ~State.碰撞;
        }

        protected void Collision()
        {
            MoveY(Math.Sign(velocityY), Math.Abs(velocityY));
            MoveX(Math.Sign(velocityX), Math.Abs(velocityX));
            Machine.MovePosition();
        }

        private void MoveX(int moveX, int distance)
        {
            if (moveX != 0)
            {
                foreach (var hit in Machine.Boxcast(new Vector2(moveX, 0), distance, LayerConst.Ground))
                {
                    var stepX = Mathf.RoundToInt(hit.distance * Rigidbody.FIX);
                    if (stepX >= 0)
                    {
                        if (moveX > 0)
                        {
                            if (!state.HasFlag(State.跳跃))
                            {
                                Feature.JumpCount = 1;
                            }

                            state |= State.右墙;
                        }
                        else
                        {
                            if (!state.HasFlag(State.跳跃))
                            {
                                Feature.JumpCount = 1;
                            }

                            state |= State.左墙;
                        }

                        Feature.WallTimer = Time.fixedTime + 0.1F;
                        Feature.WallInput = -moveX;
                        velocityX = moveX * stepX;
                    }
                }
            }

            positionX += velocityX;
        }

        private void MoveY(int moveY, int distance)
        {
            if (moveY != 0)
            {
                foreach (var hit in Machine.Boxcast(new Vector2(0, moveY), distance, LayerConst.Ground))
                {
                    var stepY = Mathf.RoundToInt(hit.distance * Rigidbody.FIX);
                    if (stepY >= 0)
                    {
                        if (moveY > 0)
                        {
                            state |= State.头顶;
                        }
                        else
                        {
                            if (!state.HasFlag(State.跳跃))
                            {
                                Feature.JumpCount = 1;
                            }

                            if (!state.HasFlag(State.冲刺))
                            {
                                Feature.DashCount = 1;
                            }

                            state |= State.地面;
                        }

                        velocityY = moveY * stepY;
                    }
                }
            }

            positionY += velocityY;
        }

    }
}