using System;
using Astraia;
using UnityEngine;

namespace Runtime
{
    public abstract class PlayerState : State<Player>
    {
        protected bool isWalk => InputManager.MoveX != 0;
        protected bool isWall => State.HasFlag(State.左墙) || State.HasFlag(State.右墙);
        protected bool isGround => State.HasFlag(State.地面);
        protected bool isRoad => isWall || isGround;
        protected bool isGrab => isWall && isFall;
        protected bool isFall => !isGround && velocityY < 0;
        protected Transform transform => owner.transform;
        protected PlayerMachine Machine => owner.Machine;
        protected PlayerFeature Feature => owner.Feature;

        protected int Direction
        {
            get => owner.Sender.Direction;
            set => owner.Sender.Direction = value;
        }

        protected State State
        {
            get => Feature.State;
            set => Feature.State = value;
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

        protected void Move(int moveSpeed, int percent = 0)
        {
            var moveX = InputManager.MoveX;
            if (moveX != 0)
            {
                moveSpeed = moveX * moveSpeed;
                if (Direction != moveX || Mathf.Abs(velocityX) < Mathf.Abs(moveSpeed / 2))
                {
                    Direction = moveX;
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
            if (State.HasFlag(State.攀爬))
            {
                velocityY = Mathf.Max(velocityY - 1, -10);
            }
            else if (State.HasFlag(State.缓冲))
            {
                velocityY = Mathf.Max(velocityY - 3, -60);
            }
            else
            {
                velocityY = Mathf.Max(velocityY - 6, -60);
            }
        }

        protected void Collision()
        {
            State &= ~State.碰撞;
            MoveX(Math.Sign(velocityX), Math.Abs(velocityX));
            MoveY(Math.Sign(velocityY), Math.Abs(velocityY));
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
                            if (!State.HasFlag(State.跳跃))
                            {
                                Feature.JumpCount = 1;
                            }

                            State |= State.右墙;
                        }
                        else
                        {
                            if (!State.HasFlag(State.跳跃))
                            {
                                Feature.JumpCount = 1;
                            }

                            State |= State.左墙;
                        }

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
                            State |= State.头顶;
                        }
                        else
                        {
                            if (!State.HasFlag(State.跳跃))
                            {
                                Feature.JumpCount = 1;
                            }

                            if (!State.HasFlag(State.冲刺))
                            {
                                Feature.DashCount = 1;
                            }

                            State |= State.地面;
                        }

                        velocityX = moveY * stepY;
                    }
                }
            }

            positionY += velocityY;
        }

        protected int Dash()
        {
            // var collisions = Machine.GetContacts(owner.collision, velocityX, velocityY);
            // var state = 0;
            // foreach (var collision in collisions)
            // {
            //     if (!collision.Contains(Machine.Bounds.TopLeft))
            //     {
            //         state |= 1 << 0;
            //         break;
            //     }
            // }
            //
            // foreach (var collision in collisions)
            // {
            //     if (!collision.Contains(Machine.Bounds.TopRight))
            //     {
            //         state |= 1 << 1;
            //         break;
            //     }
            // }
            //
            // switch (state)
            // {
            //     case 1:
            //         Debug.Log(1);
            //         return -Feature.MoveSpeed;
            //     case 2:
            //         Debug.Log(2);
            //         return Feature.MoveSpeed;
            //
            // }

            return Direction * Feature.DashSpeed;
        }

        protected bool Hold()
        {
            // var velocity = new Vector2(velocityX, velocityY);
            // switch (Machine.GetComponent<Collider>().HoldCast(Direction, velocity.magnitude / FIX))
            // {
            //     case 1:
            //         velocityY = Feature.MoveSpeed;
            //         return true;
            //     default:
            //         return false;
            // }
            return false;
        }
    }
}