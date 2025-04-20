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

        public bool isWalk => moveX != 0 || moveY != 0;
        public RaycastHit2D downRay => Physics2D.Raycast(transform.position, Vector3.down, 0.11f, 1 << 6);
        public RaycastHit2D rightRay => Physics2D.Raycast(transform.position, Vector3.right * moveX, 0.11f, 1 << 6);

        public override void OnShow(Component owner)
        {
            base.OnShow(owner);
            this.SetFloat(Attribute.MoveSpeed, 3);
        }

        public override void OnUpdate()
        {
            if (downRay)
            {
                state |= StateType.Ground;
            }
            else
            {
                state &= ~StateType.Ground;
            }

            if (rightRay)
            {
                state |= StateType.Wall;
            }
            else
            {
                state &= ~StateType.Wall;
            }
        }
    }
}