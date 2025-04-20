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
                attribute.SetFloat(Attribute.Horizontal, Input.GetAxis("Horizontal"));
                attribute.SetFloat(Attribute.Vertical, Input.GetAxis("Vertical"));

                if (Input.GetKeyDown(KeyCode.C))
                {
                }

                if (Input.GetKeyDown(KeyCode.X))
                {
                }

                if (Input.GetKeyDown(KeyCode.Z))
                {
                }
            }
        }
    }
}