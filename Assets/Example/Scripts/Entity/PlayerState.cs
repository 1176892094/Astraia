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
using Astraia.Common;
using UnityEngine;

namespace Runtime
{
    public abstract class PlayerState : State<Player>
    {
        protected Transform transform => owner.transform;
        protected PlayerMachine Machine => owner.Machine;
        protected PlayerFeature Feature => owner.Feature;
    }

    public class PlayerIdle : PlayerState
    {
        public override void OnEnter()
        {
            Machine.velocityX = 0;
            owner.Sender.SyncColorServerRpc(Color.white);
        }

        public override void OnUpdate()
        {
            if (Feature.state.HasFlag(StateType.Wall) && Feature.state.HasFlag(StateType.Climb))
            {
                Machine.ChangeState(StateConst.Grab);
                return;
            }

            if (Feature.isWalk)
            {
                Machine.ChangeState(StateConst.Walk);
            }
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
            if (Feature.state.HasFlag(StateType.Wall) && Feature.state.HasFlag(StateType.Climb))
            {
                Machine.ChangeState(StateConst.Grab);
                return;
            }

            if (!Feature.isWalk)
            {
                Machine.ChangeState(StateConst.Idle);
                return;
            }

            Machine.velocityX = Feature.moveX * Feature.moveSpeed;
        }

        public override void OnExit()
        {
        }
    }

    public class PlayerJump : PlayerState
    {
        private int frameCount;

        public override void OnEnter()
        {
            frameCount = Time.frameCount + 10;
            owner.Sender.SyncColorServerRpc(Color.red);
            Feature.state |= StateType.Jump;
            Feature.SubInt(Attribute.JumpCount, 1);
            if (Feature.state.HasFlag(StateType.Ground))
            {
                Machine.velocityY = Feature.jumpForce;
            }
            else if (Feature.state.HasFlag(StateType.Wall))
            {
                Feature.state |= StateType.Jumped;
                Machine.velocityY = Feature.jumpForce;
                Machine.velocityX = -transform.localScale.x * Feature.jumpForce;
                transform.localScale = new Vector3(-transform.localScale.x, 1, 1);
            }
            else
            {
                Machine.velocityY = Feature.jumpForce;
            }
        }

        public override void OnUpdate()
        {
            if (frameCount < Time.frameCount)
            {
                Machine.ChangeState(StateConst.Idle);
                return;
            }

            if (Feature.dashFrame > Time.frameCount)
            {
                if (Feature.moveX != 0)
                {
                    Machine.ChangeState(StateConst.Crash);
                    return;
                }
            }

            if (!Feature.state.HasFlag(StateType.Jumped))
            {
                Machine.velocityX = Feature.moveX * Feature.moveSpeed;
            }
        }

        public override void OnExit()
        {
            Feature.state &= ~StateType.Jump;
            Feature.state &= ~StateType.Jumped;
        }
    }

    public class PlayerGrab : PlayerState
    {
        private int frameCount;

        public override void OnEnter()
        {
            frameCount = Time.frameCount + 5;
            owner.Sender.SyncColorServerRpc(Color.cyan);
            Feature.state |= StateType.Grab;
            Machine.velocityY = 0;
        }

        public override void OnUpdate()
        {
            if (owner.RDHit && !owner.RUHit)
            {
                Machine.ChangeState(StateConst.Hop);
                return;
            }

            if (frameCount < Time.frameCount)
            {
                if (!Feature.state.HasFlag(StateType.Wall) || !Feature.state.HasFlag(StateType.Climb))
                {
                    Machine.ChangeState(StateConst.Idle);
                    return;
                }
            }
            else if (Feature.moveX != 0)
            {
                transform.position += Vector3.up * 0.02f;
            }

            Machine.velocityY = Feature.moveY * Feature.moveSpeed / 2;
        }

        public override void OnExit()
        {
            Feature.state &= ~StateType.Grab;
        }
    }

    public class PlayerHop : PlayerState
    {
        private int frameCount;
        private Vector3 point;


        public override void OnEnter()
        {
            frameCount = Time.frameCount + 10;
            owner.Sender.SyncColorServerRpc(Color.red);
            Feature.state |= StateType.Grab;
            point = transform.position;
        }

        public override void OnUpdate()
        {
            if (frameCount < Time.frameCount)
            {
                Machine.ChangeState(StateConst.Idle);
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
            Feature.state &= ~StateType.Grab;
        }
    }

    public class PlayerDash : PlayerState
    {
        private Vector3 direction;

        public override void OnEnter()
        {
            AudioManager.PlayOnce("30001");
            Feature.SetInt(Attribute.DashFrame, Time.frameCount + 10);
            owner.Sender.SyncColorServerRpc(Color.magenta);
            Feature.state |= StateType.Dash;
            Feature.SubInt(Attribute.DashCount, 1);
            direction = new Vector3(Feature.moveX, Feature.moveY).normalized;
            Feature.SetInt(Attribute.WaitFrame, 0);
        }

        public override void OnUpdate()
        {
            if (Feature.dashFrame < Time.frameCount)
            {
                Machine.ChangeState(StateConst.Idle);
                return;
            }

            if (Feature.waitFrame % 4 == 0)
            {
                owner.Sender.LoadEffectServerRpc(transform.position);
            }

            Feature.AddInt(Attribute.WaitFrame, 1);
            var position = transform.position;
            if (direction == Vector3.zero)
            {
                position += Vector3.right * (transform.localScale.x * Feature.dashSpeed * Time.fixedDeltaTime);
            }
            else if (Feature.moveY < 0 && Feature.state.HasFlag(StateType.Ground))
            {
                position += Vector3.right * (transform.localScale.x * Feature.dashSpeed * Time.fixedDeltaTime);
            }
            else
            {
                position += direction * (Feature.dashSpeed * Time.fixedDeltaTime);
            }

            Machine.rigidbody.MovePosition(position);
        }

        public override void OnExit()
        {
            Machine.velocityY = 0;
            Feature.state &= ~StateType.Dash;
        }
    }

    public class PlayerCrash : PlayerState
    {
        public override void OnEnter()
        {
            Feature.state |= StateType.Crash;
            owner.Sender.SyncColorServerRpc(Color.magenta);
        }

        public override void OnUpdate()
        {
            if (Feature.state.HasFlag(StateType.Wall) || Feature.state.HasFlag(StateType.Ground))
            {
                Machine.ChangeState(StateConst.Idle);
                return;
            }

            if (Feature.waitFrame % 5 == 0)
            {
                owner.Sender.LoadEffectServerRpc(transform.position);
            }

            Feature.AddInt(Attribute.WaitFrame, 1);
            Machine.velocityX = transform.localScale.x * Feature.moveSpeed * 2;
        }

        public override void OnExit()
        {
            Machine.velocityX = 0;
            Feature.state &= ~StateType.Crash;
        }
    }
}