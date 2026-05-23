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
using Astraia.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Runtime
{
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
        碰撞 = 地面 | 左墙 | 右墙 | 头顶,
    }

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
        private static readonly Enumerable<RaycastHit2D> Hits = new Enumerable<RaycastHit2D>(8);

        public static Enumerable<RaycastHit2D> Cast(this Collider2D collider, Vector2 direction, float distance)
        {
            Hits.count = collider.Cast(direction, LayerConst.Ground, Hits.items, distance);
            return Hits;
        }

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
        public static void Step(Camera camera, Bounds bounds)
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