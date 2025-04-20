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
using Const;
using UnityEngine;

namespace Runtime
{
    public class PlayerOperation : Agent<Player>
    {
        private PlayerMachine machine => owner.machine;
        private PlayerAttribute attribute => owner.attribute;

        public override void OnUpdate()
        {
            if (!attribute.state.HasFlag(StateType.Stop))
            {
                attribute.SetFloat(Attribute.Horizontal, Input.GetAxisRaw("Horizontal"));
                attribute.SetFloat(Attribute.Vertical, Input.GetAxisRaw("Vertical"));

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
                    JumpedButton();
                }

                if (Input.GetKeyDown(KeyCode.Z))
                {
                    GrabButton();
                }

                if (Input.GetKeyUp(KeyCode.Z))
                {
                    GrabbedButton();
                }
            }
        }

        private void JumpButton()
        {
            attribute.state |= StateType.Jumping;
            attribute.SetFloat(Attribute.JumpInput, Time.time + 0.2f);
        }

        private void JumpedButton()
        {
            attribute.state &= ~StateType.Jumping;
        }

        private void GrabButton()
        {
            attribute.state |= StateType.Grab;
        }

        private void GrabbedButton()
        {
            attribute.state &= ~StateType.Grab;
        }

        private void DashButton()
        {
            attribute.SetFloat(Attribute.DashInput, Time.time + 0.2f);
        }
    }
}