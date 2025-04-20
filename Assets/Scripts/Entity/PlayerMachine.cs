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

        public Rigidbody2D rigidbody;
        public SpriteRenderer renderer;

        public override void OnShow(Component owner)
        {
            base.OnShow(owner);
            rigidbody = owner.GetComponent<Rigidbody2D>();
            renderer = owner.GetComponent<SpriteRenderer>();
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

            if (rigidbody.linearVelocityY < 0)
            {
                attribute.state |= StateType.Fall;
            }
            else
            {
                attribute.state &= ~StateType.Fall;
            }

            if (attribute.state.HasFlag(StateType.Wall))
            {
                if (attribute.jumpCount > 0 && attribute.jumpInput > Time.time)
                {
                    if (!attribute.state.HasFlag(StateType.Jump))
                    {
                        ChangeState<PlayerJump>();
                    }
                }
            }

            if (attribute.state.HasFlag(StateType.Ground))
            {
                rigidbody.linearVelocityY = 0;
                if (attribute.jumpCount > 0 && attribute.jumpInput > Time.time)
                {
                    if (!attribute.state.HasFlag(StateType.Jump))
                    {
                        ChangeState<PlayerJump>();
                    }
                }
            }
            else
            {
                rigidbody.linearVelocityY = Mathf.Max(-10, rigidbody.linearVelocityY);
            }

            rigidbody.linearVelocityX = attribute.moveX * attribute.moveSpeed;
            base.OnUpdate();
        }
    }

    public abstract class PlayerState : State<Player>
    {
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
            renderer.color = Color.cyan;
            attribute.state |= StateType.Jump;
            attribute.SubFloat(Attribute.JumpCount, 1);
            rigidbody.linearVelocityY = attribute.jumpForce;
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
            attribute.state &= ~StateType.Jump;
        }
    }
}