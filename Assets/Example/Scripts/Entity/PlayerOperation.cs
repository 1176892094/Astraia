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
        private PlayerAttribute attribute => owner.attribute;
        
        public void Update()
        {
            if (!owner.isOwner) return;
            if (!attribute.state.HasFlag(StateType.Stop))
            {
                attribute.moveY = Input.GetAxisRaw("Vertical");
                attribute.moveX = Input.GetAxisRaw("Horizontal");

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
            attribute.state |= StateType.Jumping;
            attribute.SetFloat(Attribute.JumpInput, Time.time + 0.2f);
        }

        private void FallButton()
        {
            attribute.state &= ~StateType.Jumping;
        }

        private void ClimbButton()
        {
            attribute.state |= StateType.Climb;
        }

        private void GrabButton()
        {
            attribute.state &= ~StateType.Climb;
        }

        private void DashButton()
        {
            attribute.SetFloat(Attribute.DashInput, Time.time + 0.2f);
        }
    }
}