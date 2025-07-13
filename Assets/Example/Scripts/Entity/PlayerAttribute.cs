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
    public class PlayerAttribute : Feature<Attribute>
    {
        public Player player => owner.GetSource<Player>();
        public StateType state = StateType.None;
        public float moveX;
        public float moveY;
        [ShowInInspector] public bool isWalk => moveX != 0 || moveY != 0;
        [ShowInInspector] public float moveSpeed => GetFloat(Attribute.MoveSpeed);
        [ShowInInspector] public float jumpForce => GetFloat(Attribute.JumpForce);
        [ShowInInspector] public float dashSpeed => GetFloat(Attribute.DashSpeed);
        [ShowInInspector] public float dashFrame => GetFloat(Attribute.DashFrame);
        [ShowInInspector] public float waitFrame => GetFloat(Attribute.WaitFrame);

        public RaycastHit2D downLeftRay => Physics2D.Raycast(player.downLeftRay.origin, player.downLeftRay.direction, 0.12f, 1 << 6);
        public RaycastHit2D downRightRay => Physics2D.Raycast(player.downRightRay.origin, player.downRightRay.direction, 0.12f, 1 << 6);
        public RaycastHit2D rightUpRay => Physics2D.Raycast(player.rightUpRay.origin, player.rightUpRay.direction, 0.12f, 1 << 6);
        public RaycastHit2D rightDownRay => Physics2D.Raycast(player.rightDownRay.origin, player.rightDownRay.direction, 0.12f, 1 << 6);


        public override void OnAwake()
        {
            SetFloat(Attribute.MoveSpeed, 2);
            SetFloat(Attribute.JumpForce, 4);
            SetFloat(Attribute.DashSpeed, 5);
        }

        public override void OnUpdate()
        {
            if (!player.isOwner) return;
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