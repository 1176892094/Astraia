// // *********************************************************************************
// // # Project: JFramework
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-04-20 19:04:23
// // # Recently: 2025-04-20 19:04:23
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using Astraia;
using Astraia.Core;
using UnityEngine;

namespace Runtime
{
    public class PlayerInput : Module<Player>
    {
        private PlayerFeature Feature => owner.Feature;

        public void Update()
        {
            if (!Feature.state.HasFlag(StateType.Stop))
            {
                var x = Input.GetAxisRaw("Horizontal");
                var y = Input.GetAxisRaw("Vertical");
                Feature.moveX = x;
                Feature.moveY = y;

                if (Input.GetKeyDown(KeyCode.C))
                {
                    DashButton();
                }

                if (Input.GetKeyDown(KeyCode.X))
                {
                    JumpButton();
                }

                if (Input.GetKeyUp(KeyCode.X))
                {
                    FallButton();
                }

                if (Input.GetKeyDown(KeyCode.Z))
                {
                    ClimbButton();
                }

                if (Input.GetKeyUp(KeyCode.Z))
                {
                    GrabButton();
                }
            }

            MoveUpdate();
        }

        private void JumpButton()
        {
            Feature.state |= StateType.Jumping;
            Feature.SetFloat(Attribute.JumpInput, Time.time + 0.2f);
        }

        private void FallButton()
        {
            Feature.state &= ~StateType.Jumping;
        }

        private void ClimbButton()
        {
            Feature.state |= StateType.Climb;
        }

        private void GrabButton()
        {
            Feature.state &= ~StateType.Climb;
        }

        private void DashButton()
        {
            Feature.SetFloat(Attribute.DashInput, Time.time + 0.2f);
        }

        private void FallUpdate()
        {
            if (Feature.state.HasFlag(StateType.Dash))
            {
                return;
            }

            if (Feature.state.HasFlag(StateType.Jumping))
            {
                owner.Machine.velocityY -= 9.81f * Time.deltaTime;
            }
            else
            {
                owner.Machine.velocityY -= 9.81f * Time.deltaTime * 2;
            }

            owner.Machine.velocityY = Mathf.Max(-5, owner.Machine.velocityY);
        }

        private void MoveUpdate()
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
                owner.Machine.Switch(StateConst.Jump);
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
                owner.Machine.Switch(StateConst.Dash);
            }

            Feature.SetFloat(Attribute.DashInput, Time.time);
        }
    }
}