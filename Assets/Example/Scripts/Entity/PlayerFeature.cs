// // *********************************************************************************
// // # Project: JFramework
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-04-20 19:04:06
// // # Recently: 2025-04-20 19:04:06
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using Astraia;
using Astraia.Core;

namespace Runtime
{
    [Serializable]
    public class PlayerFeature : Blackboard<Label, Player>
    {
        public StateType state = StateType.默认;
        public bool isWalk => InputManager.MoveX != 0 || InputManager.MoveY != 0;
        public float moveSpeed => GetFloat(Label.移动速度);
        public float jumpForce => GetFloat(Label.跳跃力度);
        public float dashSpeed => GetFloat(Label.冲刺速度);
        public float dashFrame => GetFloat(Label.冲刺时间);
        public float waitFrame => GetFloat(Label.等待时间);

        public override void Dequeue()
        {
            SetFloat(Label.移动速度, 2);
            SetFloat(Label.跳跃力度, 4);
            SetFloat(Label.冲刺速度, 5);
        }

        public void Update()
        {
            if (owner.DRHit || owner.DLHit)
            {
                state |= StateType.地面;
                SetInt(Label.跳跃次数, 1);
                SetInt(Label.冲刺次数, 1);
            }
            else
            {
                state &= ~StateType.地面;
            }

            if (owner.RDHit)
            {
                state |= StateType.墙面;
                SetInt(Label.跳跃次数, 1);
            }
            else
            {
                state &= ~StateType.墙面;
            }
        }
    }
}