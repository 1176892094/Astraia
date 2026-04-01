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

    public static class CameraUtils
    {
        public static void Clamp(Camera camera, Bounds bounds)
        {
            var cam = camera.transform.parent;
            var pos = cam.position;

            var min = bounds.min;
            var max = bounds.max;

            var w = max.x - min.x;
            var h = max.y - min.y;

            var x = camera.orthographicSize;
            var y = camera.orthographicSize * camera.aspect;

            pos.x = w <= y * 2 ? bounds.center.x : Mathf.Clamp(pos.x, min.x + y, max.x - y);
            pos.y = h <= x * 2 ? bounds.center.y : Mathf.Clamp(pos.y, min.y + x, max.y - x);

            cam.position = pos;
        }

        public static void Move(Camera camera, Transform target, ref Vector3 smooth, float speed)
        {
            var cam = camera.transform.parent;
            var targetPos = new Vector3(target.position.x, target.position.y, cam.position.z);
            Vector3 smoothPos;
            if (camera.targetTexture)
            {
                var pixelate = camera.orthographicSize * 2 / camera.targetTexture.height;
                smoothPos = Vector3.Distance(target.position, cam.position) > pixelate ? Vector3.SmoothDamp(cam.position, targetPos, ref smooth, speed) : target.position;
                smoothPos.x = Mathf.Round(smoothPos.x / pixelate) * pixelate;
                smoothPos.y = Mathf.Round(smoothPos.y / pixelate) * pixelate;
            }
            else
            {
                smoothPos = Vector3.SmoothDamp(cam.position, targetPos, ref smooth, speed);
            }

            cam.position = new Vector3(smoothPos.x, smoothPos.y, cam.position.z);
        }
    }
}