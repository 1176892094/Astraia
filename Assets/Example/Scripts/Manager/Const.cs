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
    public enum Label
    {
        移动速度,
        跳跃力度,
        土狼时间,
        跳跃输入,
        跳跃次数,
        冲刺输入,
        冲刺次数,
        冲刺速度,
        冲刺时间,
        等待时间,
    }

    [Flags]
    public enum StateType
    {
        默认,
        跳跃 = 1 << 0,
        跳跃缓冲 = 1 << 1,
        抓墙 = 1 << 2,
        抓墙缓冲 = 1 << 3,
        地面 = 1 << 4,
        墙面 = 1 << 5,
        冲刺 = 1 << 6,
        冲刺跳 = 1 << 7,
        墙蹬跳 = 1 << 8,
    }

    public static class StateConst
    {
        public static readonly int Hop = Animator.StringToHash(nameof(Hop));
        public static readonly int Idle = Animator.StringToHash(nameof(Idle));
        public static readonly int Walk = Animator.StringToHash(nameof(Walk));
        public static readonly int Jump = Animator.StringToHash(nameof(Jump));
        public static readonly int Dash = Animator.StringToHash(nameof(Dash));
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
}