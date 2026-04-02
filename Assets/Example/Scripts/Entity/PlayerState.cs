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
        protected bool isCrash => State.HasFlag(StateType.墙面) || State.HasFlag(StateType.地面);
        protected bool isGrab => State.HasFlag(StateType.墙面) && State.HasFlag(StateType.攻击);
        protected bool isFall => !State.HasFlag(StateType.墙面) && !State.HasFlag(StateType.地面) && velocityY < 0;
        protected Transform transform => owner.transform;
        protected PlayerMachine Machine => owner.Machine;
        protected PlayerFeature Feature => owner.Feature;

        protected int Direction
        {
            get => owner.Sender.Direction;
            set => owner.Sender.Direction = value;
        }

        protected StateType State
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
            if (State.HasFlag(StateType.地面))
            {
                return;
            }

            if (State.HasFlag(StateType.缓冲))
            {
                velocityY = Mathf.Lerp(velocityY, Physics2D.gravity.y, Time.fixedDeltaTime);
                return;
            }

            velocityY = Mathf.Lerp(velocityY, Physics2D.gravity.y, Time.fixedDeltaTime * 2);
        }

        protected void Contact()
        {
            State &= ~StateType.地面;
            State &= ~StateType.墙面;
            State &= ~StateType.头顶;
            foreach (var contact in Machine.rigidbody.Contacts(LayerConst.Ground))
            {
                var normal = contact.normal;
                if (Mathf.Abs(normal.y) > Mathf.Abs(normal.x))
                {
                    if (normal.y > 0)
                    {
                        if (!State.HasFlag(StateType.跳跃))
                        {
                            Feature.JumpCount = 1;
                        }

                        if (!State.HasFlag(StateType.冲刺))
                        {
                            Feature.DashCount = 1;
                        }

                        State |= StateType.地面;
                    }
                    else
                    {
                        State |= StateType.头顶;
                    }
                }
                else
                {
                    if (!State.HasFlag(StateType.跳跃))
                    {
                        Feature.JumpCount = 1;
                    }

                    State |= StateType.墙面;
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
                    position.x = bounds.max.x + extents.x + 0.02F;
                    transform.position = position;
                    return 0;
                }

                if (position.x < bounds.min.x + extents.x)
                {
                    position.x = bounds.min.x - extents.x - 0.02F;
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
                Machine.Switch(StateConst.Grab);
                return;
            }

            if (isFall)
            {
                Machine.Switch(StateConst.Fall);
                return;
            }

            if (isWalk)
            {
                Machine.Switch(StateConst.Walk);
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
                Machine.Switch(StateConst.Grab);
                return;
            }

            if (isFall)
            {
                Machine.Switch(StateConst.Fall);
                return;
            }

            if (!isWalk)
            {
                Machine.Switch(StateConst.Idle);
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
            waitTime = Time.fixedTime + 0.2F;
            owner.Sender.SyncColorServerRpc(Color.yellow);

            if (State.HasFlag(StateType.墙面) && !State.HasFlag(StateType.地面))
            {
                State |= StateType.墙蹬跳;
                Direction = -owner.Sender.Direction;
                Machine.velocityX = Direction * Feature.JumpForce;
            }

            Feature.JumpCount--;
            State |= StateType.跳跃;
            Machine.velocityY = Mathf.Max(Machine.velocityY + Feature.JumpForce, Feature.JumpForce);
        }

        public override void OnUpdate()
        {
            if (Feature.DashTimer > Time.fixedTime)
            {
                if (isWalk && InputManager.MoveY != 1)
                {
                    if (State.HasFlag(StateType.地面))
                    {
                        Machine.Switch(StateConst.Crash);
                        return;
                    }
                }
            }

            if (waitTime < Time.fixedTime)
            {
                Machine.Switch(StateConst.Idle);
                return;
            }

            if (isGrab)
            {
                Machine.Switch(StateConst.Grab);
                return;
            }

            if (isFall)
            {
                Machine.Switch(StateConst.Fall);
            }

            Move(InputManager.MoveX, Feature.MoveSpeed * 1.2F);
        }

        public override void OnExit()
        {
            State &= ~StateType.跳跃;
            State &= ~StateType.墙蹬跳;
        }
    }

    public class PlayerFall : PlayerState
    {
        public override void OnEnter()
        {
            State |= StateType.下落;
            owner.Sender.SyncColorServerRpc(Color.red);
        }

        public override void OnUpdate()
        {
            if (isGrab)
            {
                Machine.Switch(StateConst.Grab);
                return;
            }

            if (!isFall)
            {
                Machine.Switch(StateConst.Idle);
            }

            Move(InputManager.MoveX, Feature.MoveSpeed * 1.2F);
        }

        public override void OnExit()
        {
            State &= ~StateType.下落;
        }
    }

    public class PlayerGrab : PlayerState
    {
        public override void OnEnter()
        {
            State |= StateType.挂墙;
            owner.Sender.SyncColorServerRpc(Color.cyan);
        }

        public override void OnUpdate()
        {
            if (!isGrab)
            {
                Machine.Switch(StateConst.Idle);
                return;
            }

            if (isFall)
            {
                Machine.Switch(StateConst.Fall);
                return;
            }

            Grab(InputManager.MoveY, Feature.MoveSpeed);
        }

        public override void OnExit()
        {
            State &= ~StateType.挂墙;
        }
    }

    public class PlayerDash : PlayerState
    {
        private Vector2 direction;

        public override void OnEnter()
        {
            Feature.DashCount--;
            Feature.DashTimer = Time.fixedTime + 0.22F;
            owner.Sender.SyncColorServerRpc(Color.magenta);
            State |= StateType.冲刺;
            Feature.ShadowFrame = 0;
            direction = InputManager.Direction;
        }

        public override void OnUpdate()
        {
            if (Feature.DashTimer < Time.fixedTime)
            {
                Machine.Switch(StateConst.Idle);
                return;
            }

            if (Feature.ShadowFrame++ % 4 == 0)
            {
                owner.Sender.LoadEffectServerRpc(transform.position);
            }

            var moveX = direction.x * Feature.DashSpeed;
            var moveY = direction.y * Feature.DashSpeed;

            if (direction == Vector2.zero)
            {
                moveX = Direction * Feature.DashSpeed;
            }
            else if (direction.y < 0)
            {
                Contact();
                if (State.HasFlag(StateType.地面))
                {
                    moveX = Dash();
                }
            }
            else if (direction.y > 0)
            {
                Contact();
                if (State.HasFlag(StateType.头顶))
                {
                    moveX = Dash();
                }
            }

            velocityX = moveX;
            velocityY = moveY;
        }

        public override void OnExit()
        {
            velocityX = 0;
            velocityY = 0;
            State &= ~StateType.冲刺;
        }
    }

    public class PlayerCrash : PlayerState
    {
        private float waitTime;

        public override void OnEnter()
        {
            State |= StateType.冲刺跳;
            waitTime = Time.fixedTime + 0.1F;
            Feature.DashStack += Feature.CrashSpeed / 4;
            velocityX = Direction * (Feature.CrashSpeed + Feature.DashStack);
        }

        public override void OnUpdate()
        {
            if (waitTime < Time.fixedTime)
            {
                if (isCrash)
                {
                    Machine.Switch(StateConst.Idle);
                    return;
                }
            }

            if (Feature.ShadowFrame++ % 4 == 0)
            {
                owner.Sender.LoadEffectServerRpc(transform.position);
            }

            Contact();
            Gravity();
        }

        public override void OnExit()
        {
            State &= ~StateType.冲刺跳;
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
            State |= StateType.挂墙;
            point = transform.position;
        }

        public override void OnUpdate()
        {
            if (waitTime < Time.fixedTime)
            {
                Machine.Switch(StateConst.Idle);
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
            State &= ~StateType.挂墙;
        }
    }
}