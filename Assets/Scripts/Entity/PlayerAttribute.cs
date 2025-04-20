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
        [ShowInInspector] public float jumpInput => this.GetFloat(Attribute.JumpInput);
        [ShowInInspector] public float jumpForce => this.GetFloat(Attribute.JumpForce);
        [ShowInInspector] public float jumpCount => this.GetFloat(Attribute.JumpCount);

        public bool isWalk => moveX != 0 || moveY != 0;
        public RaycastHit2D downRay => Physics2D.Raycast(transform.position, Vector3.down, 0.11f, 1 << 6);
        public RaycastHit2D leftRay => Physics2D.Raycast(transform.position, Vector3.left, 0.11f, 1 << 6);
        public RaycastHit2D rightRay => Physics2D.Raycast(transform.position, Vector3.right, 0.11f, 1 << 6);


        public override void OnShow(Component owner)
        {
            base.OnShow(owner);
            this.SetFloat(Attribute.MoveSpeed, 3);
            this.SetFloat(Attribute.JumpForce, 5);
        }

        public override void OnUpdate()
        {
            if (downRay)
            {
                state |= StateType.Ground;
                this.SetFloat(Attribute.JumpCount, 1);
            }
            else
            {
                state &= ~StateType.Ground;
            }

            if (rightRay || leftRay)
            {
                state |= StateType.Wall;
                if (moveX != 0)
                {
                    state |= StateType.Grab;
                }
                else
                {
                    state &= ~StateType.Grab;
                }

                this.SetFloat(Attribute.JumpCount, 1);
            }
            else
            {
                state &= ~StateType.Wall;
                state &= ~StateType.Grab;
            }
        }
    }
}