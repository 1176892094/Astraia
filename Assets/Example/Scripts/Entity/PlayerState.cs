// // *********************************************************************************
// // # Project: JFramework
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-04-21 02:04:44
// // # Recently: 2025-04-21 02:04:44
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using Astraia;
using UnityEngine;

namespace Runtime
{
    public abstract class PlayerState : State<Player>
    {
        protected bool isWalk => InputManager.MoveX != 0 || InputManager.MoveY != 0;
        protected bool isWall => State.HasFlag(State.左墙) || State.HasFlag(State.右墙);
        protected bool isGround => isWall || State.HasFlag(State.地面);
        protected bool isGrab => isWall && State.HasFlag(State.攻击);
        protected bool isFall => !isWall && !State.HasFlag(State.地面) && velocityY < 0;
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

        protected float velocityX
        {
            get => Machine.velocityX;
            set => Machine.velocityX = value;
        }

        protected float velocityY
        {
            get => Machine.velocityY;
            set => Machine.velocityY = value;
        }

        protected void Move(int moveX, float speed)
        {
            if (moveX != 0)
            {
                Direction = moveX;
                velocityX = Mathf.Lerp(velocityX, speed * moveX, Time.fixedDeltaTime * 10);
            }
            else
            {
                velocityX = Mathf.Abs(velocityX) > 0.01F ? Mathf.Lerp(velocityX, 0, Time.fixedDeltaTime * 5) : 0;
            }

            Contact();
            Gravity();
        }

        protected void Grab(int moveY, float speed)
        {
            if (moveY != 0)
            {
                velocityY = Mathf.Lerp(velocityY, speed * moveY, Time.fixedDeltaTime * 10);
            }
            else
            {
                velocityY = 0;
            }

            Contact();
        }

        protected void Gravity()
        {
            if (State.HasFlag(State.地面))
            {
                return;
            }

            if (State.HasFlag(State.缓冲))
            {
                velocityY = Mathf.Lerp(velocityY, Physics2D.gravity.y, Time.fixedDeltaTime);
                return;
            }

            velocityY = Mathf.Lerp(velocityY, Physics2D.gravity.y, Time.fixedDeltaTime * 2);
        }

        protected void Contact()
        {
            State &= ~State.地面;
            State &= ~State.左墙;
            State &= ~State.右墙;
            State &= ~State.头顶;
            foreach (var contact in Machine.rigidbody.Contacts(LayerConst.Ground))
            {
                var normal = contact.normal;
                if (Mathf.Abs(normal.y) > Mathf.Abs(normal.x))
                {
                    if (normal.y > 0)
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
                    else
                    {
                        State |= State.头顶;
                    }
                }
                else
                {
                    if (!State.HasFlag(State.跳跃))
                    {
                        Feature.JumpCount = 1;
                    }

                    if (normal.x > 0)
                    {
                        State |= State.左墙;
                    }
                    else
                    {
                        State |= State.右墙;
                    }
                }
            }
        }

        protected float Dash()
        {
            var moveX = Direction * Feature.DashSpeed;
            var extents = Machine.collider.bounds.extents;
            var position = transform.position;
            foreach (var contact in Machine.rigidbody.Contacts(LayerConst.Ground))
            {
                var bounds = contact.collider.bounds;
                if (position.x > bounds.max.x - extents.x)
                {
                    position.x = bounds.max.x + extents.x;
                    transform.position = position;
                    return 0;
                }

                if (position.x < bounds.min.x + extents.x)
                {
                    position.x = bounds.min.x - extents.x;
                    transform.position = position;
                    return 0;
                }
            }

            return moveX;
        }
    }

    public class PlayerIdle : PlayerState
    {
        public override void OnEnter()
        {
            Feature.DashStack = 0;
            owner.Sender.SyncColorServerRpc(Color.white);
        }

        public override void OnUpdate()
        {
            if (isGrab)
            {
                Machine.Switch(Animations.Grab);
                return;
            }

            if (isFall)
            {
                Machine.Switch(Animations.Fall);
                return;
            }

            if (isWalk)
            {
                Machine.Switch(Animations.Walk);
            }

            Move(InputManager.MoveX, Feature.MoveSpeed);
        }

        public override void OnExit()
        {
        }
    }

    public class PlayerWalk : PlayerState
    {
        public override void OnEnter()
        {
            owner.Sender.SyncColorServerRpc(Color.green);
        }

        public override void OnUpdate()
        {
            if (isGrab)
            {
                Machine.Switch(Animations.Grab);
                return;
            }

            if (isFall)
            {
                Machine.Switch(Animations.Fall);
                return;
            }

            if (!isWalk)
            {
                Machine.Switch(Animations.Idle);
            }


            Move(InputManager.MoveX, Feature.MoveSpeed);
        }

        public override void OnExit()
        {
        }
    }

    public class PlayerJump : PlayerState
    {
        private float waitTime;

        public override void OnEnter()
        {
            waitTime = Time.fixedTime + 0.1F;
            owner.Sender.SyncColorServerRpc(Color.yellow);

            if (isWall && !State.HasFlag(State.地面))
            {
                State |= State.墙蹬跳;
                Direction = -owner.Sender.Direction;
                Machine.velocityX = Direction * Feature.JumpForce / 2;
            }

            Feature.JumpCount--;
            State |= State.跳跃;
            Machine.velocityY = Mathf.Max(Machine.velocityY + Feature.JumpForce, Feature.JumpForce);
        }

        public override void OnUpdate()
        {
            if (Feature.DashTimer > Time.fixedTime)
            {
                if (InputManager.MoveY != 1)
                {
                    if (State.HasFlag(State.地面) && isWalk)
                    {
                        Machine.Switch(Animations.Crash);
                        return;
                    }
                }

                if (isWall && State.HasFlag(State.墙蹬跳))
                {
                    State |= State.超级跳;
                    State &= ~State.墙蹬跳;

                    if (State.HasFlag(State.左墙))
                    {
                        Machine.velocityX = Feature.JumpForce * 0.8f;
                    }
                    else
                    {
                        Machine.velocityX = -Feature.JumpForce * 0.8f;
                    }

                    Machine.velocityY *= InputManager.MoveY >= 0 ? 1.2f : -1.2f;
                }
            }

            if (isGrab)
            {
                Machine.Switch(Animations.Grab);
                return;
            }

            if (isFall)
            {
                Machine.Switch(Animations.Fall);
            }

            if (waitTime < Time.fixedTime)
            {
                if (isGround)
                {
                    Machine.Switch(Animations.Idle);
                    return;
                }
            }
            else
            {
                if (State.HasFlag(State.超级跳))
                {
                    if (Feature.ShadowIndex++ % 4 == 0)
                    {
                        owner.Sender.LoadEffectServerRpc(transform.position);
                    }

                    return;
                }

                if (State.HasFlag(State.墙蹬跳))
                {
                    return;
                }
            }


            Move(InputManager.MoveX, Feature.MoveSpeed * 1.2F);
        }

        public override void OnExit()
        {
            State &= ~State.跳跃;
            State &= ~State.超级跳;
            State &= ~State.墙蹬跳;
        }
    }

    public class PlayerFall : PlayerState
    {
        public override void OnEnter()
        {
            State |= State.下落;
            owner.Sender.SyncColorServerRpc(Color.red);
        }

        public override void OnUpdate()
        {
            if (isGrab)
            {
                Machine.Switch(Animations.Grab);
                return;
            }

            if (!isFall)
            {
                Machine.Switch(Animations.Idle);
            }

            Move(InputManager.MoveX, Feature.MoveSpeed * 1.2F);
        }

        public override void OnExit()
        {
            State &= ~State.下落;
        }
    }

    public class PlayerGrab : PlayerState
    {
        public override void OnEnter()
        {
            State |= State.挂墙;
            owner.Sender.SyncColorServerRpc(Color.cyan);
        }

        public override void OnUpdate()
        {
            if (!isGrab)
            {
                Machine.Switch(Animations.Idle);
                return;
            }

            if (isFall)
            {
                Machine.Switch(Animations.Fall);
                return;
            }

            Grab(InputManager.MoveY, Feature.MoveSpeed);
        }

        public override void OnExit()
        {
            State &= ~State.挂墙;
        }
    }

    public class PlayerDash : PlayerState
    {
        private Vector2 velocity;

        public override void OnEnter()
        {
            Feature.DashCount--;
            Feature.DashTimer = Time.fixedTime + 0.24F;
            owner.Sender.SyncColorServerRpc(Color.magenta);
            State |= State.冲刺;
            Feature.ShadowIndex = 0;
            velocity = InputManager.Direction;
        }

        public override void OnUpdate()
        {
            if (Feature.DashTimer < Time.fixedTime)
            {
                Machine.Switch(Animations.Idle);
                return;
            }

            if (Feature.ShadowIndex++ % 4 == 0)
            {
                owner.Sender.LoadEffectServerRpc(transform.position);
            }

            var moveX = velocity.x * Feature.DashSpeed;
            var moveY = velocity.y * Feature.DashSpeed;

            if (velocity == Vector2.zero)
            {
                moveX = Direction * Feature.DashSpeed;
            }
            else if (velocity.y < 0)
            {
                Contact();
                if (State.HasFlag(State.地面))
                {
                    moveX = Dash();
                }
            }
            else if (velocity.y > 0)
            {
                Contact();
                if (State.HasFlag(State.头顶))
                {
                    Dash();
                }
            }

            velocityX = moveX;
            velocityY = moveY;
        }

        public override void OnExit()
        {
            velocityX = 0;
            velocityY = 0;
            State &= ~State.冲刺;
        }
    }

    public class PlayerCrash : PlayerState
    {
        private float waitTime;

        public override void OnEnter()
        {
            State |= State.冲刺跳;
            waitTime = Time.fixedTime + 0.1F;
            Feature.DashStack += Feature.CrashSpeed / 4;
            velocityX = Direction * (Feature.CrashSpeed + Feature.DashStack);
        }

        public override void OnUpdate()
        {
            if (waitTime < Time.fixedTime)
            {
                if (isGround)
                {
                    Machine.Switch(Animations.Idle);
                    return;
                }
            }

            if (Feature.ShadowIndex++ % 4 == 0)
            {
                owner.Sender.LoadEffectServerRpc(transform.position);
            }

            Contact();
            Gravity();
        }

        public override void OnExit()
        {
            State &= ~State.冲刺跳;
        }
    }

    public class PlayerHold : PlayerState
    {
        private float waitTime;
        private Vector3 point;

        public override void OnEnter()
        {
            waitTime = Time.fixedTime + 0.3f;
            owner.Sender.SyncColorServerRpc(Color.red);
            State |= State.挂墙;
            point = transform.position;
        }

        public override void OnUpdate()
        {
            if (waitTime < Time.fixedTime)
            {
                Machine.Switch(Animations.Idle);
                return;
            }

            var position = transform.position;
            if (position.y < point.y + 0.2f)
            {
                position += Vector3.up * (2 * Time.fixedDeltaTime);
            }

            if (position.x < point.x + 0.2f || position.x > point.x - 0.2f)
            {
                position += Vector3.right * (transform.localScale.x * Time.fixedDeltaTime);
            }

            Machine.rigidbody.MovePosition(position);
        }

        public override void OnExit()
        {
            State &= ~State.挂墙;
        }
    }
}