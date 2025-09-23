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

namespace Runtime
{
    [Serializable]
    public class PlayerFeature : Feature<Player>
    {
        public StateType state = StateType.None;
        public float moveX;
        public float moveY;
        public bool isWalk => moveX != 0 || moveY != 0;
        public float moveSpeed => GetFloat(Attribute.MoveSpeed);
        public float jumpForce => GetFloat(Attribute.JumpForce);
        public float dashSpeed => GetFloat(Attribute.DashSpeed);
        public float dashFrame => GetFloat(Attribute.DashFrame);
        public float waitFrame => GetFloat(Attribute.WaitFrame);

        public override void Dequeue()
        {
            SetFloat(Attribute.MoveSpeed, 2);
            SetFloat(Attribute.JumpForce, 4);
            SetFloat(Attribute.DashSpeed, 5);
        }

        public void Update()
        {
            if (owner.DRHit || owner.DLHit)
            {
                state |= StateType.Ground;
                SetInt(Attribute.JumpCount, 1);
                SetInt(Attribute.DashCount, 1);
            }
            else
            {
                state &= ~StateType.Ground;
            }

            if (owner.RDHit)
            {
                state |= StateType.Wall;
                SetInt(Attribute.JumpCount, 1);
            }
            else
            {
                state &= ~StateType.Wall;
            }
        }
    }
}