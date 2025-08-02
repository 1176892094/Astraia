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
using UnityEngine;

namespace Runtime
{
    [Serializable]
    public class PlayerFeature : Feature<Player, Attribute>
    {
        public StateType state = StateType.None;
        public float moveX;
        public float moveY;
        public int nextFrame;
        public int lastFrame;
        public bool isWalk => moveX != 0 || moveY != 0;
        public float moveSpeed => GetFloat(Attribute.MoveSpeed);
        public float jumpForce => GetFloat(Attribute.JumpForce);
        public float dashSpeed => GetFloat(Attribute.DashSpeed);
        public float dashFrame => GetFloat(Attribute.DashFrame);
        public float waitFrame => GetFloat(Attribute.WaitFrame);

        public RaycastHit2D downLeftRay => Physics2D.Raycast(owner.downLeftRay.origin, owner.downLeftRay.direction, 0.12f, 1 << 6);
        public RaycastHit2D downRightRay => Physics2D.Raycast(owner.downRightRay.origin, owner.downRightRay.direction, 0.12f, 1 << 6);
        public RaycastHit2D rightUpRay => Physics2D.Raycast(owner.rightUpRay.origin, owner.rightUpRay.direction, 0.12f, 1 << 6);
        public RaycastHit2D rightDownRay => Physics2D.Raycast(owner.rightDownRay.origin, owner.rightDownRay.direction, 0.12f, 1 << 6);

        public override void OnAwake()
        {
            SetFloat(Attribute.MoveSpeed, 2);
            SetFloat(Attribute.JumpForce, 4);
            SetFloat(Attribute.DashSpeed, 5);
        }

        public void OnUpdate()
        {
            if (downRightRay || downLeftRay)
            {
                state |= StateType.Ground;
                SetInt(Attribute.JumpCount, 1);
                SetInt(Attribute.DashCount, 1);
            }
            else
            {
                state &= ~StateType.Ground;
            }

            if (rightDownRay)
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