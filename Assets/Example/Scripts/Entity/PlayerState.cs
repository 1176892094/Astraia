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
using Const;
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
            if (attribute.state.HasFlag(StateType.Wall) && attribute.state.HasFlag(StateType.Grab))
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
            if (attribute.state.HasFlag(StateType.Wall) && attribute.state.HasFlag(StateType.Grab))
            {
                machine.ChangeState<PlayerGrab>();
            }
            else if (!attribute.isWalk)
            {
                machine.ChangeState<PlayerIdle>();
            }
        }

        protected override void OnExit()
        {
        }
    }

    public class PlayerJump : PlayerState
    {
        private float waitTime;

        protected override void OnEnter()
        {
            waitTime = Time.time + 0.2f;
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
            if (waitTime < Time.time)
            {
                machine.ChangeState<PlayerIdle>();
            }
        }

        protected override void OnExit()
        {
            attribute.state &= ~StateType.Jumped;
            attribute.state &= ~StateType.Jump;
        }
    }

    public class PlayerGrab : PlayerState
    {
        private float waitTime;

        protected override void OnEnter()
        {
            waitTime = Time.time + 0.1f;
            rigidbody.linearVelocityY = 0;
            attribute.state |= StateType.Grabbing;
        }

        protected override void OnUpdate()
        {
            if (waitTime < Time.time)
            {
                if (!attribute.state.HasFlag(StateType.Wall) || !attribute.state.HasFlag(StateType.Grab))
                {
                    machine.ChangeState<PlayerIdle>();
                }
            }
            else if (attribute.moveX != 0)
            {
                transform.position += Vector3.up * 0.02f;
            }

            rigidbody.linearVelocityY = attribute.moveY * attribute.moveSpeed / 2;

            if (!attribute.heightRay)
            {
                rigidbody.linearVelocityY = attribute.moveY * attribute.moveSpeed;
                rigidbody.linearVelocityX = transform.localScale.x * attribute.moveSpeed;
            }
        }

        protected override void OnExit()
        {
            attribute.state &= ~StateType.Grabbing;
        }
    }

    public class PlayerDash : PlayerState
    {
        private int frameWait;
        private int frameCount;
        private Vector3 direction;

        protected override void OnEnter()
        {
            frameWait = 0;
            frameCount = Time.frameCount + 10;
            renderer.color = Color.magenta;
            attribute.state |= StateType.Dash;
            attribute.SubInt(Attribute.DashCount, 1);
            direction = new Vector3(attribute.moveX, attribute.moveY).normalized;
        }

        protected override void OnUpdate()
        {
            if (frameWait % 4 == 0)
            {
                PoolManager.Show("Prefabs/Effect", obj =>
                {
                    obj.transform.position = transform.position;
                    var component = obj.GetComponent<SpriteRenderer>();
                    component.color = new Color(0, 0, 0, 1);
                    component.DOFade(0, 0.5f).OnComplete(() => PoolManager.Hide(obj));
                });
            }

            frameWait++;
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
            if (frameCount < Time.frameCount)
            {
                machine.ChangeState<PlayerIdle>();
            }
        }

        protected override void OnExit()
        {
            rigidbody.linearVelocityY = 0;
            attribute.state &= ~StateType.Dash;
        }
    }
}