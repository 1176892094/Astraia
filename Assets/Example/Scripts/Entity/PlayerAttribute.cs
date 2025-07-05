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
using Sirenix.OdinInspector;
using UnityEngine;

namespace Runtime
{
    [Serializable]
    public class PlayerAttribute : Attribute<Player, Attribute>
    {
        public StateType state = StateType.None;
        public float moveX;
        public float moveY;
        [ShowInInspector] public bool isWalk => moveX != 0 || moveY != 0;
        [ShowInInspector] public float moveSpeed => this.GetFloat(Attribute.MoveSpeed);
        [ShowInInspector] public float jumpForce => this.GetFloat(Attribute.JumpForce);
        [ShowInInspector] public float dashSpeed => this.GetFloat(Attribute.DashSpeed);
        [ShowInInspector] public float dashFrame => this.GetFloat(Attribute.DashFrame);
        [ShowInInspector] public float waitFrame => this.GetFloat(Attribute.WaitFrame);

        public RaycastHit2D downLeftRay => Physics2D.Raycast(owner.downLeftRay.origin, owner.downLeftRay.direction, 0.12f, 1 << 6);
        public RaycastHit2D downRightRay => Physics2D.Raycast(owner.downRightRay.origin, owner.downRightRay.direction, 0.12f, 1 << 6);
        public RaycastHit2D rightUpRay => Physics2D.Raycast(owner.rightUpRay.origin, owner.rightUpRay.direction, 0.12f, 1 << 6);
        public RaycastHit2D rightDownRay => Physics2D.Raycast(owner.rightDownRay.origin, owner.rightDownRay.direction, 0.12f, 1 << 6);


        protected override void OnShow()
        {
            this.SetFloat(Attribute.MoveSpeed, 2);
            this.SetFloat(Attribute.JumpForce, 4);
            this.SetFloat(Attribute.DashSpeed, 5);
        }

        public void Update()
        {
            if (!owner.isOwner) return;
            if (downRightRay || downLeftRay)
            {
                state |= StateType.Ground;
                this.SetInt(Attribute.JumpCount, 1);
                this.SetInt(Attribute.DashCount, 1);
            }
            else
            {
                state &= ~StateType.Ground;
            }

            if (rightDownRay)
            {
                state |= StateType.Wall;
                this.SetInt(Attribute.JumpCount, 1);
            }
            else
            {
                state &= ~StateType.Wall;
            }
        }
    }
}