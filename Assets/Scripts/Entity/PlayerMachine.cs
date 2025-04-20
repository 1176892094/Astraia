// // *********************************************************************************
// // # Project: JFramework
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-04-20 19:04:11
// // # Recently: 2025-04-20 19:04:11
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using Astraia;
using Const;
using UnityEngine;
using Attribute = Const.Attribute;

namespace Runtime
{
    public class PlayerMachine : StateMachine<Player>
    {
        private Transform transform => owner.transform;
        private PlayerAttribute attribute => owner.attribute;

        private float gravity;
        public Rigidbody2D rigidbody;
        public SpriteRenderer renderer;

        public override void OnShow(Component owner)
        {
            base.OnShow(owner);
            rigidbody = owner.GetComponent<Rigidbody2D>();
            renderer = owner.GetComponent<SpriteRenderer>();
            gravity = rigidbody.gravityScale;
        }

        public override void OnUpdate()
        {
            if (attribute.moveX > 0)
            {
                transform.localScale = new Vector3(attribute.moveX, 1, 1);
            }
            else if (attribute.moveX < 0)
            {
                transform.localScale = new Vector3(attribute.moveX, 1, 1);
            }

            if (attribute.state.HasFlag(StateType.Grab))
            {
                rigidbody.gravityScale = 0;
                rigidbody.linearVelocityY = 0;
            }
            else
            {
                rigidbody.gravityScale = gravity;
            }

            if (rigidbody.linearVelocityY < 0)
            {
                attribute.state |= StateType.Fall;
            }
            else
            {
                attribute.state &= ~StateType.Fall;
            }

            if (attribute.state.HasFlag(StateType.Ground))
            {
                rigidbody.linearVelocityY = 0;
                JumpUpdate();
            }
            else
            {
                rigidbody.linearVelocityY = Mathf.Max(-10, rigidbody.linearVelocityY);
            }

            if (attribute.state.HasFlag(StateType.Wall))
            {
                rigidbody.linearVelocityY = Mathf.Max(-1, rigidbody.linearVelocityY);
                JumpUpdate();
            }

            if (!attribute.state.HasFlag(StateType.Fly))
            {
                rigidbody.linearVelocityX = attribute.moveX * attribute.moveSpeed;
            }


            base.OnUpdate();
        }

        private void JumpUpdate()
        {
            if (attribute.jumpCount > 0 && attribute.jumpInput > Time.time)
            {
                if (!attribute.state.HasFlag(StateType.Jump))
                {
                    ChangeState<PlayerJump>();
                }

                attribute.SetFloat(Attribute.JumpInput, Time.time);
            }
        }
    }

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
            if (attribute.isWalk)
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
            if (!attribute.isWalk)
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
            attribute.SubFloat(Attribute.JumpCount, 1);
            if (attribute.state.HasFlag(StateType.Ground))
            {
                rigidbody.linearVelocityY = attribute.jumpForce;
            }
            else
            {
                attribute.state |= StateType.Fly;
                rigidbody.linearVelocityY = attribute.jumpForce;
                rigidbody.linearVelocityX = -transform.localScale.x * attribute.jumpForce;
                transform.localScale = new Vector3(-transform.localScale.x, 1, 1);
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
            attribute.state &= ~StateType.Fly;
            attribute.state &= ~StateType.Jump;
        }
    }
}