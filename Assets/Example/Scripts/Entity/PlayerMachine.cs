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
    public class PlayerMachine : StateMachine
    {
        private Player player => owner.GetAgent<Player>();
        private PlayerFeature feature => owner.GetAgent<PlayerFeature>();

        public Rigidbody2D rigidbody;
        public SpriteRenderer renderer;

        public override void OnLoad()
        {
            rigidbody = owner.GetComponent<Rigidbody2D>();
            renderer = owner.GetComponent<SpriteRenderer>();
        }

        public override void OnUpdate()
        {
            if (!player.isOwner) return;
            if (feature.moveX > 0)
            {
                transform.localScale = new Vector3(feature.moveX, 1, 1);
            }
            else if (feature.moveX < 0)
            {
                transform.localScale = new Vector3(feature.moveX, 1, 1);
            }

            DashUpdate();

            if (feature.state.HasFlag(StateType.Ground))
            {
                feature.SetFloat(Attribute.JumpGrace, Time.time + 0.2f);
                JumpUpdate();
            }
            else if (feature.state.HasFlag(StateType.Wall))
            {
                if (!feature.state.HasFlag(StateType.Climb))
                {
                    FallUpdate();
                }

                feature.SetFloat(Attribute.JumpGrace, Time.time + 0.2f);
                JumpUpdate();
            }
            else
            {
                FallUpdate();
            }

            if (feature.GetFloat(Attribute.JumpGrace) > Time.time)
            {
                JumpUpdate();
            }

            base.OnUpdate();
        }

        private void FallUpdate()
        {
            if (feature.state.HasFlag(StateType.Dash))
            {
                return;
            }

            if (feature.state.HasFlag(StateType.Jumping))
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
            if (feature.GetInt(Attribute.JumpCount) <= 0)
            {
                return;
            }

            if (feature.GetFloat(Attribute.JumpInput) < Time.time)
            {
                return;
            }

            if (!feature.state.HasFlag(StateType.Jump))
            {
                ChangeState<PlayerJump>();
            }

            feature.SetFloat(Attribute.JumpInput, Time.time);
        }

        private void DashUpdate()
        {
            if (feature.GetInt(Attribute.DashCount) <= 0)
            {
                return;
            }

            if (feature.GetFloat(Attribute.DashInput) < Time.time)
            {
                return;
            }

            if (!feature.state.HasFlag(StateType.Dash))
            {
                ChangeState<PlayerDash>();
            }

            feature.SetFloat(Attribute.DashInput, Time.time);
        }
    }
}