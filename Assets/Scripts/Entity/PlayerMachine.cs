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

            if (attribute.state.HasFlag(StateType.Ground))
            {
                rigidbody.linearVelocityY = 0;
                attribute.SetFloat(Attribute.JumpGrace, Time.time + 0.2f);
                JumpUpdate();
            }
            else if (attribute.state.HasFlag(StateType.Wall))
            {
                if (attribute.moveX == 0)
                {
                    FallUpdate();
                }

                rigidbody.linearVelocityY = Mathf.Max(-1, rigidbody.linearVelocityY);
                attribute.SetFloat(Attribute.JumpGrace, Time.time + 0.2f);
                JumpUpdate();
            }
            else
            {
                FallUpdate();
                rigidbody.linearVelocityY = Mathf.Max(-5, rigidbody.linearVelocityY);
            }

            if (attribute.GetFloat(Attribute.JumpGrace) > Time.time)
            {
                JumpUpdate();
            }

            MoveUpdate();
            base.OnUpdate();
        }

        private void MoveUpdate()
        {
            if (attribute.state.HasFlag(StateType.Jumped))
            {
                return;
            }

            if (attribute.state.HasFlag(StateType.Grabbing))
            {
                return;
            }

            rigidbody.linearVelocityX = attribute.moveX * attribute.moveSpeed;
        }

        private void FallUpdate()
        {
            if (attribute.state.HasFlag(StateType.Jumping))
            {
                rigidbody.linearVelocityY -= 9.81f * Time.deltaTime;
            }
            else
            {
                rigidbody.linearVelocityY -= 9.81f * Time.deltaTime * 2;
            }
        }

        private void JumpUpdate()
        {
            if (attribute.GetInt(Attribute.JumpCount) <= 0)
            {
                return;
            }

            if (attribute.GetFloat(Attribute.JumpInput) < Time.time)
            {
                return;
            }

            if (!attribute.state.HasFlag(StateType.Jump))
            {
                ChangeState<PlayerJump>();
            }

            attribute.SetFloat(Attribute.JumpInput, Time.time);
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
            if (attribute.state.HasFlag(StateType.Grab))
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
            if (attribute.state.HasFlag(StateType.Grab))
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
            if (attribute.state.HasFlag(StateType.Grab))
            {
                attribute.state |= StateType.Jumped;
                rigidbody.linearVelocityY = attribute.jumpForce;
                rigidbody.linearVelocityX = -transform.localScale.x * attribute.jumpForce * 0.7f;
                transform.localScale = new Vector3(-transform.localScale.x, 1, 1);
            }
            else if (attribute.state.HasFlag(StateType.Ground))
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
                if (!attribute.state.HasFlag(StateType.Grab))
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
            attribute.state &= ~StateType.Grabbing;
        }
    }
}