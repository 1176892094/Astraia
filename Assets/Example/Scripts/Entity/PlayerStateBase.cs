using Astraia;
using UnityEngine;

namespace Runtime
{
    public abstract class PlayerState : State<Player>
    {
        protected const float FIX = 100;
        private bool isLeft => State.HasFlag(State.左墙) && InputManager.MoveX < 0;
        private bool isRight => State.HasFlag(State.右墙) && InputManager.MoveX > 0;
        protected bool isWalk => InputManager.MoveX != 0;
        protected bool isWall => State.HasFlag(State.左墙) || State.HasFlag(State.右墙);
        protected bool isGround => State.HasFlag(State.地面);
        protected bool isRoad => isWall || isGround;
        protected bool isGrab => (isLeft || isRight) && isFall;
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
            get => owner.State;
            set => owner.State = value;
        }

        protected int velocityX
        {
            get => Feature.VelocityX;
            set => Feature.VelocityX = value;
        }

        protected int velocityY
        {
            get => Feature.VelocityY;
            set => Feature.VelocityY = value;
        }

        protected int positionX
        {
            get => Feature.PositionX;
            set => Feature.PositionX = value;
        }

        protected int positionY
        {
            get => Feature.PositionY;
            set => Feature.PositionY = value;
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
            if (State.HasFlag(State.地面))
            {
                return;
            }

            if (State.HasFlag(State.攀爬))
            {
                velocityY = Mathf.Max(velocityY - 1, -10);
                return;
            }

            if (State.HasFlag(State.缓冲))
            {
                velocityY = Mathf.Max(velocityY - 1, -20);
                return;
            }

            velocityY = Mathf.Max(velocityY - 2, -40);
        }

        protected void Contact()
        {
            State &= ~State.碰撞;
            var extents = Machine.collider.bounds.extents;
            var velocity = new Vector2(velocityX, velocityY);
            var distance = velocity.magnitude / FIX;
            var direction = velocity.normalized;
            foreach (var hit in Machine.collider.Cast(direction, distance))
            {
                var point = hit.point;
                var normal = hit.normal;

                if (normal.x > 0.5F)
                {
                    if (!State.HasFlag(State.跳跃))
                    {
                        Feature.JumpCount = 1;
                    }

                    State |= State.左墙;
                    velocityX = Mathf.Max(velocityX, 0);
                    positionX = Mathf.RoundToInt((point.x + extents.x) * FIX);
                }

                if (normal.x < -0.5F)
                {
                    State |= State.右墙;
                    velocityX = Mathf.Min(velocityX, 0);
                    positionX = Mathf.RoundToInt((point.x - extents.x) * FIX);
                }

                if (normal.y > 0.5F)
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
                    velocityY = Mathf.Max(velocityY, 0);
                    positionY = Mathf.RoundToInt((point.y + extents.y) * FIX);
                }

                if (normal.y < -0.5F)
                {
                    State |= State.头顶;
                    velocityY = Mathf.Min(velocityY, 0);
                    positionY = Mathf.RoundToInt((point.y - extents.y) * FIX);
                }
            }

            positionX += velocityX;
            positionY += velocityY;
            transform.position = new Vector3(positionX, positionY) / FIX;
        }

        protected int Dash()
        {
            var extents = Machine.collider.bounds.extents;
            var velocity = new Vector2(velocityX, velocityY);
            var distance = velocity.magnitude / FIX;
            var direction = velocity.normalized;
            foreach (var hit in Machine.collider.Cast(direction, distance))
            {
                var bounds = hit.collider.bounds;
                if (positionX > (int)((bounds.max.x - extents.x) * FIX))
                {
                    positionX = (int)((bounds.max.x + extents.x) * FIX);
                    return 0;
                }

                if (positionX < (int)((bounds.min.x + extents.x) * FIX))
                {
                    positionX = (int)((bounds.min.x - extents.x) * FIX);
                    return 0;
                }
            }

            return Direction * Feature.DashSpeed;
        }

        protected bool Hold()
        {
            var extents = Machine.collider.bounds.extents;
            var velocity = new Vector2(velocityX, velocityY);
            var distance = velocity.magnitude / FIX;
            var direction = velocity.normalized;
            foreach (var hit in Machine.collider.Cast(direction, distance))
            {
                var normal = hit.normal;
                var bounds = hit.collider.bounds;
                if (Mathf.Abs(normal.y) < Mathf.Abs(normal.x))
                {
                    var min = (int)((bounds.max.y - extents.y) * FIX);
                    var max = (int)((bounds.max.y + extents.y) * FIX);
                    if (positionY > min && positionY < max)
                    {
                        positionY += Feature.JumpForce;
                        transform.position = new Vector3(positionX, positionY) / FIX;
                        return true;
                    }
                }
            }

            return false;
        }
    }
}