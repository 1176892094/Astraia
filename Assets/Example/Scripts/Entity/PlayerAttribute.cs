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
using Runtime;
using Sirenix.OdinInspector;
using UnityEngine;
using Attribute = Runtime.Attribute;

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
        public RaycastHit2D rightRay => Physics2D.Raycast(owner.rightRay.origin, owner.rightRay.direction, 0.12f, 1 << 6);
        public RaycastHit2D rightUpRay => Physics2D.Raycast(owner.rightUpRay.origin, owner.rightUpRay.direction, 0.12f, 1 << 6);
        public RaycastHit2D leftDownRay => Physics2D.Raycast(owner.leftDownRay.origin, owner.leftDownRay.direction, 0.12f, 1 << 6);
        public RaycastHit2D rightDownRay => Physics2D.Raycast(owner.rightDownRay.origin, owner.rightDownRay.direction, 0.12f, 1 << 6);


        public override void OnShow(Component owner)
        {
            base.OnShow(owner);
            this.SetFloat(Attribute.MoveSpeed, 2);
            this.SetFloat(Attribute.JumpForce, 4);
            this.SetFloat(Attribute.DashSpeed, 5);
        }

        public override void OnUpdate()
        {
            if (rightDownRay || leftDownRay)
            {
                state |= StateType.Ground;
                this.SetInt(Attribute.JumpCount, 1);
                this.SetInt(Attribute.DashCount, 1);
            }
            else
            {
                state &= ~StateType.Ground;
            }

            if (rightRay)
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