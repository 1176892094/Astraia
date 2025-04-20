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

namespace Runtime
{
    public class PlayerMachine : StateMachine<Player>
    {
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
            if (!attribute.state.HasFlag(StateType.Ground))
            {
                rigidbody.linearVelocityY = Mathf.Max(-10, rigidbody.linearVelocityY);
            }
            else
            {
                rigidbody.linearVelocityY = 0;
            }

            rigidbody.linearVelocityX = attribute.moveX * attribute.moveSpeed;
            base.OnUpdate();
        }
    }

    public abstract class PlayerState : State<Player>
    {
        public Transform transform => owner.transform;
        public SpriteRenderer renderer => machine.renderer;
        public PlayerMachine machine => owner.machine;
        public PlayerAttribute attribute => owner.attribute;
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
            if (attribute.moveX > 0)
            {
                transform.localScale = new Vector3(attribute.moveX, 1, 1);
            }
            else if (attribute.moveX < 0)
            {
                transform.localScale = new Vector3(attribute.moveX, 1, 1);
            }

            if (!attribute.isWalk)
            {
                machine.ChangeState<PlayerIdle>();
            }
        }

        protected override void OnExit()
        {
        }
    }
}