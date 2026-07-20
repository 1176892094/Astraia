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
using UnityEngine.UI;

namespace Runtime
{
    public static class LayerConst
    {
        public static ContactFilter2D Ground;

        static LayerConst()
        {
            Ground = new ContactFilter2D();
            Ground.SetLayerMask(LayerMask.GetMask("Ground"));
            Ground.useTriggers = false;
        }
    }

    public static class Animations
    {
        public static readonly int Idle = Animator.StringToHash(nameof(Idle));
        public static readonly int Walk = Animator.StringToHash(nameof(Walk));
        public static readonly int Jump = Animator.StringToHash(nameof(Jump));
        public static readonly int Dash = Animator.StringToHash(nameof(Dash));
        public static readonly int Fall = Animator.StringToHash(nameof(Fall));
        public static readonly int Grab = Animator.StringToHash(nameof(Grab));
        public static readonly int Crash = Animator.StringToHash(nameof(Crash));
    }

    public static class Extensions
    {
        public static Tween DOFade(this SpriteRenderer component, float endValue, float duration)
        {
            var color = component.color;
            return component.Play(duration).OnUpdate(progress =>
            {
                var colorA = Mathf.Lerp(color.a, endValue, progress);
                component.color = new Color(color.r, color.g, color.b, colorA);
            });
        }

        public static Tween DOFade(this Graphic component, float endValue, float duration)
        {
            var color = component.color;
            return component.Play(duration).OnUpdate(progress =>
            {
                var colorA = Mathf.Lerp(color.a, endValue, progress);
                component.color = new Color(color.r, color.g, color.b, colorA);
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
        下落 = 1 << 7,
        冲刺 = 1 << 8,
        冲跳 = 1 << 9,
        侧跳 = 1 << 10,
        墙面 = 左墙 | 右墙,
        碰撞 = 地面 | 左墙 | 右墙 | 头顶,
    }
}