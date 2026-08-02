using Astraia;
using UnityEngine;

namespace Runtime
{
    public class PlayerState : State<Player>
    {
        protected PlayerMachine Machine => owner.Machine;
        protected PlayerFeature Feature => owner.Feature;
        protected Rigidbody.Collision collision => Machine.collision;
        protected bool isWalk => GameManager.MoveX != 0;
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
        protected bool isCorner => (state & State.墙地) != 0;

        protected State state
        {
            get => Feature.State;
            set => Feature.State = value;
        }

        protected int Direction
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

        protected void InputX(int moveX, float expansion = 1, float percent = 0.16F)
        {
            if (moveX != 0)
            {
                var moveSpeed = moveX * owner.Feature.MoveSpeed;
                if (Direction != moveX || Mathf.Abs(velocityX) < Mathf.Abs(moveSpeed / 2))
                {
                    Direction = moveX;
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
                velocityY = Mathf.Max(velocityY - Feature.GrabSpeed, -Feature.GrabLimit);
            }
            else if (state.HasFlag(State.缓冲))
            {
                velocityY = Mathf.Max(velocityY - Feature.GrabSpeed, -Feature.FallLimit);
            }
            else
            {
                velocityY = Mathf.Max(velocityY - Feature.FallSpeed, -Feature.FallLimit);
            }

            state &= ~State.碰撞;
        }

        protected float Distance(Position origin, Position target)
        {
            return Vector2.Distance(origin.ToVector2(), target.ToVector2());
        }

        protected void Apply()
        {
            var position = Machine.position;
            Machine.MoveX(Feature, Machine.velocityX);
            Machine.MoveY(Feature, Machine.velocityY);
            SendPosition(position, Machine.position);
        }

        private void SendPosition(Position oldValue, Position newValue)
        {
            if (owner.isOwner && oldValue != newValue)
            {
                Machine.MoveTransform(newValue);
                SyncManager.Instance?.AddPosition(owner, newValue);
            }
        }
    }
}