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
        protected Rigidbody2D rigidbody => machine.rigidbody;
        protected SpriteRenderer renderer => machine.renderer;
        protected PlayerMachine machine => owner.machine;
        protected PlayerAttribute attribute => owner.attribute;
        protected abstract override void OnEnter();
        protected abstract override void OnExit();
        protected abstract override void OnUpdate();
    }

    public class PlayerIdle : PlayerState
    {
        protected override void OnEnter()
        {
            renderer.color = Color.white;
        }

        protected override void OnUpdate()
        {
            if (attribute.state.HasFlag(StateType.Wall) && attribute.state.HasFlag(StateType.Climb))
            {
                machine.ChangeState<PlayerGrab>();
            }
            else if (attribute.isWalk)
            {
                machine.ChangeState<PlayerWalk>();
            }
        }

        protected override void OnExit()
        {
        }
    }

    public class PlayerWalk : PlayerState
    {
        protected override void OnEnter()
        {
            renderer.color = Color.green;
        }

        protected override void OnUpdate()
        {
            if (attribute.state.HasFlag(StateType.Wall) && attribute.state.HasFlag(StateType.Climb))
            {
                machine.ChangeState<PlayerGrab>();
            }
            else if (!attribute.isWalk)
            {
                machine.ChangeState<PlayerIdle>();
            }

            rigidbody.linearVelocityX = attribute.moveX * attribute.moveSpeed;
        }

        protected override void OnExit()
        {
        }
    }

    public class PlayerJump : PlayerState
    {
        private int frameCount;

        protected override void OnEnter()
        {
            frameCount = Time.frameCount + 10;
            renderer.color = Color.red;
            attribute.state |= StateType.Jump;
            attribute.SubInt(Attribute.JumpCount, 1);
            if (attribute.state.HasFlag(StateType.Ground))
            {
                rigidbody.linearVelocityY = attribute.jumpForce;
            }
            else if (attribute.state.HasFlag(StateType.Wall))
            {
                attribute.state |= StateType.Jumped;
                rigidbody.linearVelocityY = attribute.jumpForce;
                rigidbody.linearVelocityX = -transform.localScale.x * attribute.jumpForce * 0.7f;
                transform.localScale = new Vector3(-transform.localScale.x, 1, 1);
            }
            else
            {
                rigidbody.linearVelocityY = attribute.jumpForce;
            }
        }

        protected override void OnUpdate()
        {
            if (frameCount < Time.frameCount)
            {
                machine.ChangeState<PlayerIdle>();
            }
        }

        protected override void OnExit()
        {
            attribute.state &= ~StateType.Jump;
            attribute.state &= ~StateType.Jumped;
        }
    }

    public class PlayerGrab : PlayerState
    {
        private int frameCount;


        protected override void OnEnter()
        {
            frameCount = Time.frameCount + 5;
            renderer.color = Color.cyan;
            attribute.state |= StateType.Grab;
            rigidbody.linearVelocityY = 0;
        }

        protected override void OnUpdate()
        {
            if (attribute.rightRay && !attribute.rightUpRay)
            {
                machine.ChangeState<PlayerHop>();
                return;
            }

            if (frameCount < Time.frameCount)
            {
                if (!attribute.state.HasFlag(StateType.Wall) || !attribute.state.HasFlag(StateType.Climb))
                {
                    machine.ChangeState<PlayerIdle>();
                }
            }
            else if (attribute.moveX != 0)
            {
                transform.position += Vector3.up * 0.02f;
            }

            rigidbody.linearVelocityY = attribute.moveY * attribute.moveSpeed / 2;
        }

        protected override void OnExit()
        {
            attribute.state &= ~StateType.Grab;
        }
    }

    public class PlayerHop : PlayerState
    {
        private int frameCount;
        private Vector3 point;


        protected override void OnEnter()
        {
            frameCount = Time.frameCount + 10;
            renderer.color = Color.red;
            attribute.state |= StateType.Grab;
            point = transform.position;
        }

        protected override void OnUpdate()
        {
            if (frameCount < Time.frameCount)
            {
                machine.ChangeState<PlayerIdle>();
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

        protected override void OnExit()
        {
            attribute.state &= ~StateType.Grab;
        }
    }

    public class PlayerDash : PlayerState
    {
        private int frameAwait;
        private int frameCount;
        private Vector3 direction;

        protected override void OnEnter()
        {
            frameCount = Time.frameCount + 10;
            renderer.color = Color.magenta;
            attribute.state |= StateType.Dash;
            attribute.SubInt(Attribute.DashCount, 1);
            direction = new Vector3(attribute.moveX, attribute.moveY).normalized;
            frameAwait = 0;
        }

        protected override void OnUpdate()
        {
            if (frameCount < Time.frameCount)
            {
                machine.ChangeState<PlayerIdle>();
            }

            if (frameAwait % 4 == 0)
            {
                owner.LoadEffectServerRpc(transform.position);
            }

            frameAwait++;
            var position = transform.position;
            if (direction == Vector3.zero)
            {
                position += Vector3.right * (transform.localScale.x * attribute.dashSpeed * Time.fixedDeltaTime);
            }
            else
            {
                position += direction * (attribute.dashSpeed * Time.fixedDeltaTime);
            }

            rigidbody.MovePosition(position);
        }

        protected override void OnExit()
        {
            rigidbody.linearVelocityY = 0;
            attribute.state &= ~StateType.Dash;
        }
    }
}