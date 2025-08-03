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
using UnityEngine;

namespace Runtime
{
    public class PlayerOperation : Agent<Player>
    {
        private PlayerFeature Feature => owner.Feature;
        
        public void OnUpdate()
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
    }
}