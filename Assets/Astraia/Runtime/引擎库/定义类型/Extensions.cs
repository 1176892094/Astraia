// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2025-04-09 22:04:28
// # Recently: 2025-04-09 22:04:28
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Astraia
{
    public static class Extensions
    {
        private static readonly Dictionary<char, string> colors = new Dictionary<char, string>();

        static Extensions()
        {
            colors['R'] = "#FF0000";
            colors['G'] = "#00FF00";
            colors['B'] = "#0000FF";
            colors['Y'] = "#FFFF00";
            colors['O'] = "#FFAA00";
            colors['S'] = "#00FFFF";
            colors['P'] = "#FFAAFF";
            colors['W'] = "#FFFFFF";
        }

        public static string Color(this string result, string format)
        {
            return "<color={0}>{1}</color>".Format(colors.GetValueOrDefault(format[0], format), result);
        }

        public static T GetOrAddComponent<T>(this GameObject self) where T : Component
        {
            var component = self.GetComponent<T>();
            return component ? component : self.AddComponent<T>();
        }

        public static T GetOrAddComponent<T>(this GameObject self, Type type) where T : Component
        {
            var component = (T)self.GetComponent(type);
            return component ? component : (T)self.AddComponent(type);
        }

        private readonly struct TickAdaptor : Tick.IAdaptor
        {
            private readonly Component owner;
            public TickAdaptor(Component owner) => this.owner = owner;
            public bool isActive => owner.gameObject && owner.gameObject.activeInHierarchy;
        }

        public static Timer Wait(this Component current, float duration = 0)
        {
            return Timer.Create(new TickAdaptor(current), duration);
        }

        public static Tween Play(this Component current, float duration)
        {
            return Tween.Create(new TickAdaptor(current), duration);
        }

        public static Tween DOMoveX(this Transform transform, float endValue, float duration)
        {
            var position = transform.position;
            return transform.Play(duration).OnUpdate(progress =>
            {
                var positionX = Mathf.Lerp(position.x, endValue, progress);
                transform.position = new Vector3(positionX, position.y, position.z);
            });
        }

        public static Tween DOMoveY(this Transform transform, float endValue, float duration)
        {
            var position = transform.position;
            return transform.Play(duration).OnUpdate(progress =>
            {
                var positionY = Mathf.Lerp(position.y, endValue, progress);
                transform.position = new Vector3(position.x, positionY, position.z);
            });
        }

        public static Tween DOMoveZ(this Transform transform, float endValue, float duration)
        {
            var position = transform.position;
            return transform.Play(duration).OnUpdate(progress =>
            {
                var positionZ = Mathf.Lerp(position.z, endValue, progress);
                transform.position = new Vector3(position.x, position.y, positionZ);
            });
        }

        public static Tween DOMove(this Transform transform, Vector3 endValue, float duration)
        {
            var position = transform.position;
            return transform.Play(duration).OnUpdate(progress =>
            {
                var positionX = Mathf.Lerp(position.x, endValue.x, progress);
                var positionY = Mathf.Lerp(position.y, endValue.y, progress);
                var positionZ = Mathf.Lerp(position.z, endValue.z, progress);
                transform.position = new Vector3(positionX, positionY, positionZ);
            });
        }

        public static Tween DORotateX(this Transform transform, float endValue, float duration)
        {
            var rotation = transform.rotation.eulerAngles;
            return transform.Play(duration).OnUpdate(progress =>
            {
                var rotationX = Mathf.LerpAngle(rotation.x, endValue, progress);
                transform.rotation = Quaternion.Euler(rotationX, rotation.y, rotation.z);
            });
        }

        public static Tween DORotateY(this Transform transform, float endValue, float duration)
        {
            var rotation = transform.rotation.eulerAngles;
            return transform.Play(duration).OnUpdate(progress =>
            {
                var rotationY = Mathf.LerpAngle(rotation.y, endValue, progress);
                transform.rotation = Quaternion.Euler(rotation.x, rotationY, rotation.z);
            });
        }

        public static Tween DORotateZ(this Transform transform, float endValue, float duration)
        {
            var rotation = transform.rotation.eulerAngles;
            return transform.Play(duration).OnUpdate(progress =>
            {
                var rotationZ = Mathf.LerpAngle(rotation.z, endValue, progress);
                transform.rotation = Quaternion.Euler(rotation.x, rotation.y, rotationZ);
            });
        }

        public static Tween DORotate(this Transform transform, Vector3 endValue, float duration)
        {
            var rotation = transform.rotation.eulerAngles;
            return transform.Play(duration).OnUpdate(progress =>
            {
                var rotationX = Mathf.Lerp(rotation.x, endValue.x, progress);
                var rotationY = Mathf.Lerp(rotation.y, endValue.y, progress);
                var rotationZ = Mathf.Lerp(rotation.z, endValue.z, progress);
                transform.rotation = Quaternion.Euler(rotationX, rotationY, rotationZ);
            });
        }

        public static Tween DOScaleX(this Transform transform, float endValue, float duration)
        {
            var localScale = transform.localScale;
            return transform.Play(duration).OnUpdate(progress =>
            {
                var localScaleX = Mathf.Lerp(localScale.x, endValue, progress);
                transform.localScale = new Vector3(localScaleX, localScale.y, localScale.z);
            });
        }

        public static Tween DOScaleY(this Transform transform, float endValue, float duration)
        {
            var localScale = transform.localScale;
            return transform.Play(duration).OnUpdate(progress =>
            {
                var localScaleY = Mathf.Lerp(localScale.y, endValue, progress);
                transform.localScale = new Vector3(localScale.x, localScaleY, localScale.z);
            });
        }

        public static Tween DOScaleZ(this Transform transform, float endValue, float duration)
        {
            var localScale = transform.localScale;
            return transform.Play(duration).OnUpdate(progress =>
            {
                var localScaleZ = Mathf.Lerp(localScale.z, endValue, progress);
                transform.localScale = new Vector3(localScale.x, localScale.y, localScaleZ);
            });
        }

        public static Tween DOScale(this Transform transform, Vector3 endValue, float duration)
        {
            var localScale = transform.localScale;
            return transform.Play(duration).OnUpdate(progress =>
            {
                var localScaleX = Mathf.Lerp(localScale.x, endValue.x, progress);
                var localScaleY = Mathf.Lerp(localScale.y, endValue.y, progress);
                var localScaleZ = Mathf.Lerp(localScale.z, endValue.z, progress);
                transform.localScale = new Vector3(localScaleX, localScaleY, localScaleZ);
            });
        }
    }
}