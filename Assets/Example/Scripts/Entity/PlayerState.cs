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
using Astraia.Core;
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
                owner.Sender.Direction = moveX;
                velocityX = Mathf.Lerp(velocityX, speed * moveX, Time.deltaTime * 10);
            }
            else
            {
                velocityX = Mathf.Abs(velocityX) > 0.01F ? Mathf.Lerp(velocityX, 0, Time.deltaTime * 5) : 0;
            }
        }
    }

    public class PlayerIdle : PlayerState
    {
        public override void OnEnter()
        {
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
                owner.Sender.Direction = -owner.Sender.Direction;
                Machine.velocityX = owner.Sender.Direction * Feature.JumpForce;
            }

            Feature.JumpCount--;
            State |= StateType.跳跃;
            Machine.velocityY = Mathf.Max(Machine.velocityY + Feature.JumpForce, Feature.JumpForce);
        }

        public override void OnUpdate()
        {
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

            Machine.velocityY = 0;
        }

        public override void OnExit()
        {
            State &= ~StateType.挂墙;
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

    public class PlayerDash : PlayerState
    {
        private Vector2 direction;

        public override void OnEnter()
        {
            AudioManager.Play("30001");
            Feature.DashCount--;
            Feature.DashTimer = Time.fixedTime + 0.22F;
            owner.Sender.SyncColorServerRpc(Color.magenta);
            State |= StateType.冲刺;
            Feature.WaitTime = 0;
            direction = InputManager.Direction;
        }

        public override void OnUpdate()
        {
            if (Feature.DashTimer < Time.fixedTime)
            {
                Machine.Switch(StateConst.Idle);
                return;
            }

            if (Feature.WaitTime++ % 4 == 0)
            {
                owner.Sender.LoadEffectServerRpc(transform.position);
            }

            var moveX = direction.x * Feature.DashSpeed;
            var moveY = direction.y * Feature.DashSpeed;

            if (direction == Vector2.zero)
            {
                moveX = owner.Sender.Direction * Feature.DashSpeed;
            }
            else if (direction.y < 0)
            {
                if (State.HasFlag(StateType.地面))
                {
                    moveX = owner.Sender.Direction * Feature.DashSpeed;
                }
            }
            else if (direction.y > 0)
            {
                if (State.HasFlag(StateType.顶面))
                {
                    var bounds = Machine.collider.bounds;
                    var position = transform.position;
                    foreach (var contact in Machine.rigidbody.Contacts(LayerConst.Ground))
                    {
                        var ceiling = contact.collider.bounds;
                        if (position.x > ceiling.max.x - bounds.extents.x)
                        {
                            position.x = ceiling.max.x + bounds.extents.x + 0.02F;
                            transform.position = position;
                            break;
                        }

                        if (position.x < ceiling.min.x + bounds.extents.x)
                        {
                            position.x = ceiling.min.x - bounds.extents.x - 0.02F;
                            transform.position = position;
                            break;
                        }
                    }
                }
            }

            velocityX = moveX;
            velocityY = moveY;
        }

        public override void OnExit()
        {
            velocityY = 0;
            velocityX = 0;
            State &= ~StateType.冲刺;
        }
    }

    public class PlayerCrash : PlayerState
    {
        public override void OnEnter()
        {
            State |= StateType.冲刺跳;
            owner.Sender.SyncColorServerRpc(Color.magenta);
        }

        public override void OnUpdate()
        {
            if (isCrash)
            {
                Machine.Switch(StateConst.Idle);
                return;
            }

            if (Feature.WaitTime % 5 == 0)
            {
                owner.Sender.LoadEffectServerRpc(transform.position);
            }

            Feature.AddInt(Label.等待时间, 1);
            Machine.velocityX = transform.localScale.x * Feature.MoveSpeed * 2;
        }

        public override void OnExit()
        {
            Machine.velocityX = 0;
            State &= ~StateType.冲刺跳;
        }
    }
}