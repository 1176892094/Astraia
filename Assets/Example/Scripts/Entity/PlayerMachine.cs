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
        public Rigidbody2D rigidbody;
        public SpriteRenderer renderer;
        private PlayerFeature Feature => owner.Feature;

        public override void OnAwake()
        {
            rigidbody = owner.GetComponent<Rigidbody2D>();
            renderer = owner.GetComponent<SpriteRenderer>();
        }

        public override void OnUpdate()
        {
            if (Feature.moveX > 0)
            {
                transform.localScale = new Vector3(Feature.moveX, 1, 1);
            }
            else if (Feature.moveX < 0)
            {
                transform.localScale = new Vector3(Feature.moveX, 1, 1);
            }

            DashUpdate();

            if (Feature.state.HasFlag(StateType.Ground))
            {
                Feature.SetFloat(Attribute.JumpGrace, Time.time + 0.2f);
                JumpUpdate();
            }
            else if (Feature.state.HasFlag(StateType.Wall))
            {
                if (!Feature.state.HasFlag(StateType.Climb))
                {
                    FallUpdate();
                }

                Feature.SetFloat(Attribute.JumpGrace, Time.time + 0.2f);
                JumpUpdate();
            }
            else
            {
                FallUpdate();
            }

            if (Feature.GetFloat(Attribute.JumpGrace) > Time.time)
            {
                JumpUpdate();
            }

            base.OnUpdate();
        }

        private void FallUpdate()
        {
            if (Feature.state.HasFlag(StateType.Dash))
            {
                return;
            }

            if (Feature.state.HasFlag(StateType.Jumping))
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
            if (Feature.GetInt(Attribute.JumpCount) <= 0)
            {
                return;
            }

            if (Feature.GetFloat(Attribute.JumpInput) < Time.time)
            {
                return;
            }

            if (!Feature.state.HasFlag(StateType.Jump))
            {
                ChangeState<PlayerJump>();
            }

            Feature.SetFloat(Attribute.JumpInput, Time.time);
        }

        private void DashUpdate()
        {
            if (Feature.GetInt(Attribute.DashCount) <= 0)
            {
                return;
            }

            if (Feature.GetFloat(Attribute.DashInput) < Time.time)
            {
                return;
            }

            if (!Feature.state.HasFlag(StateType.Dash))
            {
                ChangeState<PlayerDash>();
            }

            Feature.SetFloat(Attribute.DashInput, Time.time);
        }
    }
}