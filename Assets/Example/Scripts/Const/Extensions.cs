// // *********************************************************************************
// // # Project: JFramework
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-04-21 02:04:46
// // # Recently: 2025-04-21 02:04:47
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using Astraia;
using UnityEngine;

namespace Const
{
    public static class Extensions
    {
        public static Tween DOFade(this SpriteRenderer component, float endValue, float duration)
        {
            var color = component.color;
            return component.Tween(duration).OnUpdate(progress =>
            {
                var colorA = Mathf.Lerp(color.a, endValue, progress);
                component.color = new Color(color.r, color.g, color.b, colorA);
            });
        }
    }
}