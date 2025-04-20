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
using Const;
using Sirenix.OdinInspector;
using UnityEngine;
using Attribute = Const.Attribute;

namespace Runtime
{
    [Serializable]
    public class PlayerAttribute : Attribute<Player, Attribute>
    {
        private Transform transform => owner.transform;
        public StateType state;

        [ShowInInspector] public float moveX => this.GetFloat(Attribute.Horizontal);
        [ShowInInspector] public float moveY => this.GetFloat(Attribute.Vertical);
        [ShowInInspector] public float moveSpeed => this.GetFloat(Attribute.MoveSpeed);
        [ShowInInspector] public float jumpForce => this.GetFloat(Attribute.JumpForce);
        [ShowInInspector] public float dashSpeed => this.GetFloat(Attribute.DashSpeed);

        public bool isWalk => moveX != 0 || moveY != 0;
        public RaycastHit2D groundRay => Physics2D.Raycast(transform.position, Vector3.down, 0.11f, 1 << 6);
        public RaycastHit2D directRay => Physics2D.Raycast(transform.position, transform.localScale.x * Vector3.right, 0.11f, 1 << 6);


        public override void OnShow(Component owner)
        {
            base.OnShow(owner);
            this.SetFloat(Attribute.MoveSpeed, 2);
            this.SetFloat(Attribute.JumpForce, 4);
            this.SetFloat(Attribute.DashSpeed, 6);
        }

        public override void OnUpdate()
        {
            if (groundRay)
            {
                state |= StateType.Ground;
                this.SetInt(Attribute.JumpCount, 1);
                this.SetInt(Attribute.DashCount, 1);
            }
            else
            {
                state &= ~StateType.Ground;
            }

            if (directRay)
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