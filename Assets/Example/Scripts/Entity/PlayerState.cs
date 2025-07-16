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
        protected new Player owner => base.owner;
        protected Transform transform => owner.transform;
        protected Rigidbody2D rigidbody => machine.rigidbody;
        protected PlayerMachine machine => base.owner.GetAgent<PlayerMachine>();
        protected PlayerFeature feature => base.owner.GetAgent<PlayerFeature>();
      
    }

    public class PlayerIdle : PlayerState
    {
        public override void OnEnter()
        {
            rigidbody.linearVelocityX = 0;
            owner.Sender.SyncColorServerRpc(Color.white);
        }

        public override void OnUpdate()
        {
            if (feature.state.HasFlag(StateType.Wall) && feature.state.HasFlag(StateType.Climb))
            {
                machine.ChangeState<PlayerGrab>();
                return;
            }

            if (feature.isWalk)
            {
                machine.ChangeState<PlayerWalk>();
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
            if (feature.state.HasFlag(StateType.Wall) && feature.state.HasFlag(StateType.Climb))
            {
                machine.ChangeState<PlayerGrab>();
                return;
            }

            if (!feature.isWalk)
            {
                machine.ChangeState<PlayerIdle>();
                return;
            }

            rigidbody.linearVelocityX = feature.moveX * feature.moveSpeed;
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
            feature.state |= StateType.Jump;
            feature.SubInt(Attribute.JumpCount, 1);
            if (feature.state.HasFlag(StateType.Ground))
            {
                rigidbody.linearVelocityY = feature.jumpForce;
            }
            else if (feature.state.HasFlag(StateType.Wall))
            {
                feature.state |= StateType.Jumped;
                rigidbody.linearVelocityY = feature.jumpForce;
                rigidbody.linearVelocityX = -transform.localScale.x * feature.jumpForce;
                transform.localScale = new Vector3(-transform.localScale.x, 1, 1);
            }
            else
            {
                rigidbody.linearVelocityY = feature.jumpForce;
            }
        }

        public override void OnUpdate()
        {
            if (frameCount < Time.frameCount)
            {
                machine.ChangeState<PlayerIdle>();
                return;
            }

            if (feature.dashFrame > Time.frameCount)
            {
                if (feature.moveX != 0)
                {
                    machine.ChangeState<PlayerCrash>();
                    return;
                }
            }

            if (!feature.state.HasFlag(StateType.Jumped))
            {
                rigidbody.linearVelocityX = feature.moveX * feature.moveSpeed;
            }
        }

        public override void OnExit()
        {
            feature.state &= ~StateType.Jump;
            feature.state &= ~StateType.Jumped;
        }
    }

    public class PlayerGrab : PlayerState
    {
        private int frameCount;

        public override void OnEnter()
        {
            frameCount = Time.frameCount + 5;
            owner.Sender.SyncColorServerRpc(Color.cyan);
            feature.state |= StateType.Grab;
            rigidbody.linearVelocityY = 0;
        }

        public override void OnUpdate()
        {
            if (feature.rightDownRay && !feature.rightUpRay)
            {
                machine.ChangeState<PlayerHop>();
                return;
            }

            if (frameCount < Time.frameCount)
            {
                if (!feature.state.HasFlag(StateType.Wall) || !feature.state.HasFlag(StateType.Climb))
                {
                    machine.ChangeState<PlayerIdle>();
                    return;
                }
            }
            else if (feature.moveX != 0)
            {
                transform.position += Vector3.up * 0.02f;
            }

            rigidbody.linearVelocityY = feature.moveY * feature.moveSpeed / 2;
        }

        public override void OnExit()
        {
            feature.state &= ~StateType.Grab;
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
            feature.state |= StateType.Grab;
            point = transform.position;
        }

        public override void OnUpdate()
        {
            if (frameCount < Time.frameCount)
            {
                machine.ChangeState<PlayerIdle>();
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

            rigidbody.MovePosition(position);
        }

        public override void OnExit()
        {
            feature.state &= ~StateType.Grab;
        }
    }

    public class PlayerDash : PlayerState
    {
        private Vector3 direction;

        public override void OnEnter()
        {
            feature.SetInt(Attribute.DashFrame, Time.frameCount + 10);
            owner.Sender.SyncColorServerRpc(Color.magenta);
            feature.state |= StateType.Dash;
            feature.SubInt(Attribute.DashCount, 1);
            direction = new Vector3(feature.moveX, feature.moveY).normalized;
            feature.SetInt(Attribute.WaitFrame, 0);
        }

        public override void OnUpdate()
        {
            if (feature.dashFrame < Time.frameCount)
            {
                machine.ChangeState<PlayerIdle>();
                return;
            }

            if (feature.waitFrame % 4 == 0)
            {
                owner.Sender.LoadEffectServerRpc(transform.position);
            }

            feature.AddInt(Attribute.WaitFrame, 1);
            var position = transform.position;
            if (direction == Vector3.zero)
            {
                position += Vector3.right * (transform.localScale.x * feature.dashSpeed * Time.fixedDeltaTime);
            }
            else if (feature.moveY < 0 && feature.state.HasFlag(StateType.Ground))
            {
                position += Vector3.right * (transform.localScale.x * feature.dashSpeed * Time.fixedDeltaTime);
            }
            else
            {
                position += direction * (feature.dashSpeed * Time.fixedDeltaTime);
            }

            rigidbody.MovePosition(position);
        }

        public override void OnExit()
        {
            rigidbody.linearVelocityY = 0;
            feature.state &= ~StateType.Dash;
        }
    }

    public class PlayerCrash : PlayerState
    {
        public override void OnEnter()
        {
            feature.state |= StateType.Crash;
            owner.Sender.SyncColorServerRpc(Color.magenta);
        }

        public override void OnUpdate()
        {
            if (feature.state.HasFlag(StateType.Wall) || feature.state.HasFlag(StateType.Ground))
            {
                machine.ChangeState<PlayerIdle>();
                return;
            }

            if (feature.waitFrame % 5 == 0)
            {
                owner.Sender.LoadEffectServerRpc(transform.position);
            }

            feature.AddInt(Attribute.WaitFrame, 1);
            rigidbody.linearVelocityX = transform.localScale.x * feature.moveSpeed * 2;
        }

        public override void OnExit()
        {
            rigidbody.linearVelocityX = 0;
            feature.state &= ~StateType.Crash;
        }
    }
}