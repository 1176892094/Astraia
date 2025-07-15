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
    public class PlayerOperation : Agent
    {
        public Player player => owner.GetAgent<Player>();
        private PlayerFeature feature => owner.GetAgent<PlayerFeature>();

        public void OnUpdate()
        {
            if (!player.isOwner) return;
            if (!feature.state.HasFlag(StateType.Stop))
            {
                feature.moveY = Input.GetAxisRaw("Vertical");
                feature.moveX = Input.GetAxisRaw("Horizontal");

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
            feature.state |= StateType.Jumping;
            feature.SetFloat(Attribute.JumpInput, Time.time + 0.2f);
        }

        private void FallButton()
        {
            feature.state &= ~StateType.Jumping;
        }

        private void ClimbButton()
        {
            feature.state |= StateType.Climb;
        }

        private void GrabButton()
        {
            feature.state &= ~StateType.Climb;
        }

        private void DashButton()
        {
            feature.SetFloat(Attribute.DashInput, Time.time + 0.2f);
        }
    }
}