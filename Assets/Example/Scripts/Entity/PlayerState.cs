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
            if (Feature.state.HasFlag(StateType.墙面) && Feature.state.HasFlag(StateType.抓墙缓冲))
            {
                Machine.Switch(StateConst.Grab);
                return;
            }

            if (Feature.isWalk)
            {
                Machine.Switch(StateConst.Walk);
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
            if (Feature.state.HasFlag(StateType.墙面) && Feature.state.HasFlag(StateType.抓墙缓冲))
            {
                Machine.Switch(StateConst.Grab);
                return;
            }

            if (!Feature.isWalk)
            {
                Machine.Switch(StateConst.Idle);
                return;
            }

            Machine.velocityX = InputManager.MoveX * Feature.moveSpeed;
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
            Feature.state |= StateType.跳跃;
            Feature.SubInt(Label.跳跃次数, 1);
            if (Feature.state.HasFlag(StateType.地面))
            {
                Machine.velocityY = Feature.jumpForce;
            }
            else if (Feature.state.HasFlag(StateType.墙面))
            {
                Feature.state |= StateType.墙蹬跳;
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
                Machine.Switch(StateConst.Idle);
                return;
            }

            if (Feature.dashFrame > Time.frameCount)
            {
                if (InputManager.MoveX != 0)
                {
                    Machine.Switch(StateConst.Crash);
                    return;
                }
            }

            if (!Feature.state.HasFlag(StateType.墙蹬跳))
            {
                Machine.velocityX =InputManager.MoveX * Feature.moveSpeed;
            }
        }

        public override void OnExit()
        {
            Feature.state &= ~StateType.跳跃;
            Feature.state &= ~StateType.墙蹬跳;
        }
    }

    public class PlayerGrab : PlayerState
    {
        private int frameCount;

        public override void OnEnter()
        {
            frameCount = Time.frameCount + 5;
            owner.Sender.SyncColorServerRpc(Color.cyan);
            Feature.state |= StateType.抓墙;
            Machine.velocityY = 0;
        }

        public override void OnUpdate()
        {
            if (owner.RDHit && !owner.RUHit)
            {
                Machine.Switch(StateConst.Hop);
                return;
            }

            if (frameCount < Time.frameCount)
            {
                if (!Feature.state.HasFlag(StateType.墙面) || !Feature.state.HasFlag(StateType.抓墙缓冲))
                {
                    Machine.Switch(StateConst.Idle);
                    return;
                }
            }
            else if (InputManager.MoveX != 0)
            {
                transform.position += Vector3.up * 0.02f;
            }

            Machine.velocityY = InputManager.MoveX * Feature.moveSpeed / 2;
        }

        public override void OnExit()
        {
            Feature.state &= ~StateType.抓墙;
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
            Feature.state |= StateType.抓墙;
            point = transform.position;
        }

        public override void OnUpdate()
        {
            if (frameCount < Time.frameCount)
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
            Feature.state &= ~StateType.抓墙;
        }
    }

    public class PlayerDash : PlayerState
    {
        private Vector3 direction;

        public override void OnEnter()
        {
            AudioManager.Play("30001");
            Feature.SetInt(Label.冲刺时间, Time.frameCount + 10);
            owner.Sender.SyncColorServerRpc(Color.magenta);
            Feature.state |= StateType.冲刺;
            Feature.SubInt(Label.冲刺次数, 1);
            direction = new Vector3(InputManager.MoveX, InputManager.MoveY).normalized;
            Feature.SetInt(Label.等待时间, 0);
        }

        public override void OnUpdate()
        {
            if (Feature.dashFrame < Time.frameCount)
            {
                Machine.Switch(StateConst.Idle);
                return;
            }

            if (Feature.waitFrame % 4 == 0)
            {
                owner.Sender.LoadEffectServerRpc(transform.position);
            }

            Feature.AddInt(Label.等待时间, 1);
            var position = transform.position;
            if (direction == Vector3.zero)
            {
                position += Vector3.right * (transform.localScale.x * Feature.dashSpeed * Time.fixedDeltaTime);
            }
            else if (InputManager.MoveY < 0 && Feature.state.HasFlag(StateType.地面))
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
            Feature.state &= ~StateType.冲刺;
        }
    }

    public class PlayerCrash : PlayerState
    {
        public override void OnEnter()
        {
            Feature.state |= StateType.冲刺跳;
            owner.Sender.SyncColorServerRpc(Color.magenta);
        }

        public override void OnUpdate()
        {
            if (Feature.state.HasFlag(StateType.墙面) || Feature.state.HasFlag(StateType.地面))
            {
                Machine.Switch(StateConst.Idle);
                return;
            }

            if (Feature.waitFrame % 5 == 0)
            {
                owner.Sender.LoadEffectServerRpc(transform.position);
            }

            Feature.AddInt(Label.等待时间, 1);
            Machine.velocityX = transform.localScale.x * Feature.moveSpeed * 2;
        }

        public override void OnExit()
        {
            Machine.velocityX = 0;
            Feature.state &= ~StateType.冲刺跳;
        }
    }
}