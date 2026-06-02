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
            get => Machine.VelocityX;
            set => Machine.VelocityX = value;
        }

        protected int velocityY
        {
            get => Machine.VelocityY;
            set => Machine.VelocityY = value;
        }

        private int positionX
        {
            get => Machine.PositionX;
            set => Machine.PositionX = value;
        }

        private int positionY
        {
            get => Machine.PositionY;
            set => Machine.PositionY = value;
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
            Contact();
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

        protected void Contact()
        {
            State &= ~State.碰撞;

            var signX = Math.Sign(velocityX);
            var signY = Math.Sign(velocityY);
            var moveX = Math.Abs(velocityX);
            var moveY = Math.Abs(velocityY);

            var collisions = Machine.GetContacts(owner.collision, moveX, moveY);
        
            foreach (var collision in collisions)
            {
                if (signX != 0)
                {
                    var stepX = Machine.BoxCast(collision, new Vector2(signX, 0), moveX);
                    if (stepX >= 0 && stepX <= moveX)
                    {
                        if (signX > 0)
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

                        velocityX = signX * stepX;
                    }
                }
            }

            positionX += velocityX;
            foreach (var collision in collisions)
            {
                if (signY != 0)
                {
                    var stepY = Machine.BoxCast(collision, new Vector2(0, signY), moveY);
                    if (stepY >= 0 && stepY <= moveY)
                    {
                        if (signY > 0)
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

                        velocityY = signY * stepY;
                    }
                }
            }

            positionY += velocityY;
            Machine.MovePosition();
        }

        protected int Dash()
        {
            // var velocity = new Vector2(velocityX, velocityY);
            // var v1 = Mathf.RoundToInt(positionX / FIX) * FIX;
            // var v2 = Mathf.RoundToInt(Machine.GetComponent<Collider>().bounds.extents.x * FIX);
            // switch (Machine.GetComponent<Collider>().DashCast(velocity.magnitude / FIX))
            // {
            //     case 1:
            //         positionX = (int)v1 + v2;
            //         positionX++;
            //         return 0;
            //     case 2:
            //         positionX = (int)v1 - v2;
            //         positionX--;
            //         return 0;
            //     default:
            //         return Direction * Feature.DashSpeed;
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