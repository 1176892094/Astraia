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

using Astraia;
using UnityEngine;

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
            if (!owner.isOwner) return;
            if (attribute.moveX > 0)
            {
                transform.localScale = new Vector3(attribute.moveX, 1, 1);
            }
            else if (attribute.moveX < 0)
            {
                transform.localScale = new Vector3(attribute.moveX, 1, 1);
            }
            
            DashUpdate();

            if (attribute.state.HasFlag(StateType.Ground))
            {
                attribute.SetFloat(Attribute.JumpGrace, Time.time + 0.2f);
                JumpUpdate();
            }
            else if (attribute.state.HasFlag(StateType.Wall))
            {
                if (!attribute.state.HasFlag(StateType.Climb))
                {
                    FallUpdate();
                }

                attribute.SetFloat(Attribute.JumpGrace, Time.time + 0.2f);
                JumpUpdate();
            }
            else
            {
                FallUpdate();
            }

            if (attribute.GetFloat(Attribute.JumpGrace) > Time.time)
            {
                JumpUpdate();
            }
            
            base.OnUpdate();
        }

        private void FallUpdate()
        {
            if (attribute.state.HasFlag(StateType.Dash))
            {
                return;
            }

            if (attribute.state.HasFlag(StateType.Jumping))
            {
                rigidbody.linearVelocityY -= 9.81f * Time.deltaTime;
            }
            else
            {
                rigidbody.linearVelocityY -= 9.81f * Time.deltaTime * 2;
            }

            rigidbody.linearVelocityY = Mathf.Max(-5, rigidbody.linearVelocityY);
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

        private void DashUpdate()
        {
            if (attribute.GetInt(Attribute.DashCount) <= 0)
            {
                return;
            }

            if (attribute.GetFloat(Attribute.DashInput) < Time.time)
            {
                return;
            }

            if (!attribute.state.HasFlag(StateType.Dash))
            {
                ChangeState<PlayerDash>();
            }

            attribute.SetFloat(Attribute.DashInput, Time.time);
        }
    }
}