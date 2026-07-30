// // *********************************************************************************
// // # Project: JFramework
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-04-20 19:04:31
// // # Recently: 2025-04-20 19:04:31
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using Astraia;
using UnityEngine;

namespace Runtime
{
    public static class LayerConst
    {
        public static ContactFilter2D GroundAndCollision;
        public static ContactFilter2D Collision;


        static LayerConst()
        {
            GroundAndCollision = new ContactFilter2D();
            GroundAndCollision.SetLayerMask(LayerMask.GetMask("Ground") | LayerMask.GetMask("Collision"));
            GroundAndCollision.useTriggers = false;

            Collision = new ContactFilter2D();
            Collision.SetLayerMask(LayerMask.GetMask("Collision"));
            Collision.useTriggers = false;
        }
    }

    public static class Animations
    {
        public static readonly int Idle = Animator.StringToHash(nameof(Idle));
        public static readonly int Wait = Animator.StringToHash(nameof(Wait));
        public static readonly int Walk = Animator.StringToHash(nameof(Walk));
        public static readonly int Jump = Animator.StringToHash(nameof(Jump));
        public static readonly int Hold = Animator.StringToHash(nameof(Hold));
        public static readonly int Dash = Animator.StringToHash(nameof(Dash));
        public static readonly int Fall = Animator.StringToHash(nameof(Fall));
        public static readonly int Grab = Animator.StringToHash(nameof(Grab));
        public static readonly int Rush = Animator.StringToHash(nameof(Rush));
    }

    public static class Extensions
    {
        public static Tween DOFade(this SpriteRenderer component, float endValue, float duration)
        {
            var color = component.color;
            return component.Play(duration).OnUpdate(progress =>
            {
                var alpha = Mathf.Lerp(color.a, endValue, progress);
                component.color = new Color(color.r, color.g, color.b, alpha);
            });
        }
    }

    [Flags]
    public enum State
    {
        默认,
        地面 = 1 << 0,
        左墙 = 1 << 1,
        右墙 = 1 << 2,
        头顶 = 1 << 3,

        跳跃 = 1 << 4,
        缓冲 = 1 << 5,
        攀爬 = 1 << 6,
        冲刺 = 1 << 7,
        横冲 = 1 << 8,
        竖冲 = 1 << 9,
        悬挂 = 1 << 10,
        平台 = 1 << 11,

        墙面 = 左墙 | 右墙,
        平面 = 平台 | 地面,
        墙顶 = 左墙 | 右墙 | 头顶,
        碰撞 = 地面 | 左墙 | 右墙 | 头顶 | 平台,
    }
}