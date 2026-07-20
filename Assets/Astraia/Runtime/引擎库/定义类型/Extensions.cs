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
using System.Linq;
using UnityEngine;

namespace Astraia
{
    public static class EntityExtensions
    {
        private static readonly Dictionary<Type, Delegate> parsers = new Dictionary<Type, Delegate>();
        private static readonly Dictionary<char, string> colors = new Dictionary<char, string>();

        static EntityExtensions()
        {
            colors['R'] = "#FF0000";
            colors['G'] = "#00FF00";
            colors['B'] = "#AAAAFF";
            colors['Y'] = "#FFFF00";
            colors['O'] = "#FFAA00";
            colors['S'] = "#00FFFF";
            colors['P'] = "#FFAAFF";
            colors['W'] = "#FFFFFF";
            parsers[typeof(Vector2)] = new Func<string, Vector2>(InputVector2);
            parsers[typeof(Vector3)] = new Func<string, Vector3>(InputVector3);
            parsers[typeof(Vector4)] = new Func<string, Vector4>(InputVector4);
            parsers[typeof(Vector2Int)] = new Func<string, Vector2Int>(InputVector2Int);
            parsers[typeof(Vector3Int)] = new Func<string, Vector3Int>(InputVector3Int);
            parsers[typeof(Vector2[])] = new Func<string, Vector2[]>(InputVector2Array);
            parsers[typeof(Vector3[])] = new Func<string, Vector3[]>(InputVector3Array);
            parsers[typeof(Vector4[])] = new Func<string, Vector4[]>(InputVector4Array);
            parsers[typeof(Vector2Int[])] = new Func<string, Vector2Int[]>(InputVector2IntArray);
            parsers[typeof(Vector3Int[])] = new Func<string, Vector3Int[]>(InputVector3IntArray);
        }

        public static string Color(this string result, string format)
        {
            return "<color={0}>{1}</color>".Format(colors.GetValueOrDefault(format[0], format), result);
        }

        public static T Parse<T>(this XorEx[] reason, int index)
        {
            if (reason == null) return default;
            var value = Text.GetString(reason[index]);
            if (parsers.TryGetValue(typeof(T), out var func))
            {
                return ((Func<string, T>)func).Invoke(value);
            }

            return value.InputString(typeof(T)) is T result ? result : default;
        }

        private static Vector2 InputVector2(this string reason)
        {
            var points = reason.Split(',');
            var x = float.Parse(points[0]);
            var y = float.Parse(points[1]);
            return new Vector2(x, y);
        }

        private static Vector3 InputVector3(this string reason)
        {
            var points = reason.Split(',');
            var x = float.Parse(points[0]);
            var y = float.Parse(points[1]);
            var z = float.Parse(points[2]);
            return new Vector3(x, y, z);
        }

        private static Vector4 InputVector4(this string reason)
        {
            var points = reason.Split(',');
            var x = float.Parse(points[0]);
            var y = float.Parse(points[1]);
            var z = float.Parse(points[2]);
            var a = float.Parse(points[3]);
            return new Vector4(x, y, z, a);
        }

        private static Vector2Int InputVector2Int(this string reason)
        {
            var points = reason.Split(',');
            var x = int.Parse(points[0]);
            var y = int.Parse(points[1]);
            return new Vector2Int(x, y);
        }

        private static Vector3Int InputVector3Int(this string reason)
        {
            var points = reason.Split(',');
            var x = int.Parse(points[0]);
            var y = int.Parse(points[1]);
            var z = int.Parse(points[2]);
            return new Vector3Int(x, y, z);
        }

        private static Vector2[] InputVector2Array(this string reason)
        {
            return reason.InputArray().Select(InputVector2).ToArray();
        }

        private static Vector3[] InputVector3Array(this string reason)
        {
            return reason.InputArray().Select(InputVector3).ToArray();
        }

        private static Vector4[] InputVector4Array(this string reason)
        {
            return reason.InputArray().Select(InputVector4).ToArray();
        }

        private static Vector2Int[] InputVector2IntArray(this string reason)
        {
            return reason.InputArray().Select(InputVector2Int).ToArray();
        }

        private static Vector3Int[] InputVector3IntArray(this string reason)
        {
            return reason.InputArray().Select(InputVector3Int).ToArray();
        }

        private static List<string> InputArray(this string reason)
        {
            var result = new List<string>();
            if (!string.IsNullOrEmpty(reason))
            {
                if (reason.EndsWith(";"))
                {
                    reason = reason.Substring(0, reason.Length - 1);
                }

                result.AddRange(reason.Split(';'));
            }

            return result;
        }

        private static object InputString(this string reason, Type target)
        {
            if (string.IsNullOrEmpty(reason) || target == typeof(string))
            {
                return reason;
            }

            if (target.IsEnum)
            {
                return Enum.Parse(target, reason);
            }

            if (target.IsPrimitive)
            {
                return Convert.ChangeType(reason, target);
            }

            if (target.IsArray)
            {
                var parts = reason.Split(';');
                var member = target.GetElementType()!;
                var result = Array.CreateInstance(member, parts.Length);
                for (var i = 0; i < parts.Length; i++)
                {
                    result.SetValue(InputString(parts[i], member), i);
                }

                return result;
            }
            else
            {
                var parts = reason.Split(',');
                var member = target.GetFields(Search.Instance);
                var result = Activator.CreateInstance(target);
                for (var i = 0; i < parts.Length; i++)
                {
                    member[i].SetValue(result, InputString(parts[i], member[i].FieldType));
                }

                return result;
            }
        }

        public static T GetOrAddComponent<T>(this Component self) where T : Component
        {
            var component = self.GetComponent<T>();
            return component ? component : self.gameObject.AddComponent<T>();
        }

        public static T GetOrAddComponent<T>(this GameObject self, Type value) where T : Component
        {
            var component = (T)self.GetComponent(value);
            return component ? component : (T)self.AddComponent(value);
        }

        public static T GetOrAddComponent<T>(this GameObject self) where T : Component
        {
            var component = self.GetComponent<T>();
            return component ? component : self.AddComponent<T>();
        }

        private readonly struct AsyncAdaptor
        {
            private readonly Component owner;
            public AsyncAdaptor(Component value) => owner = value;
            public override int GetHashCode() => owner && owner.gameObject && owner.gameObject.activeInHierarchy ? 1 : 0;
        }

        public static Timer Wait(this Component current, float duration = 0)
        {
            return Timer.Create(new AsyncAdaptor(current), duration, GetTime);
        }

        public static Tween Play(this Component current, float duration)
        {
            return Tween.Create(new AsyncAdaptor(current), duration, GetTime);
        }

        private static float GetTime()
        {
            return Time.time;
        }

        public static Tween DOMoveX(this Component component, float endValue, float duration)
        {
            var position = component.transform.position;
            return component.transform.Play(duration).OnUpdate(progress =>
            {
                var positionX = Mathf.Lerp(position.x, endValue, progress);
                component.transform.position = new Vector3(positionX, position.y, position.z);
            });
        }

        public static Tween DOMoveY(this Component component, float endValue, float duration)
        {
            var position = component.transform.position;
            return component.Play(duration).OnUpdate(progress =>
            {
                var positionY = Mathf.Lerp(position.y, endValue, progress);
                component.transform.position = new Vector3(position.x, positionY, position.z);
            });
        }

        public static Tween DOMoveZ(this Component component, float endValue, float duration)
        {
            var position = component.transform.position;
            return component.Play(duration).OnUpdate(progress =>
            {
                var positionZ = Mathf.Lerp(position.z, endValue, progress);
                component.transform.position = new Vector3(position.x, position.y, positionZ);
            });
        }

        public static Tween DOMove(this Component component, Vector3 endValue, float duration)
        {
            var position = component.transform.position;
            return component.Play(duration).OnUpdate(progress =>
            {
                var positionX = Mathf.Lerp(position.x, endValue.x, progress);
                var positionY = Mathf.Lerp(position.y, endValue.y, progress);
                var positionZ = Mathf.Lerp(position.z, endValue.z, progress);
                component.transform.position = new Vector3(positionX, positionY, positionZ);
            });
        }

        public static Tween DORotateX(this Component component, float endValue, float duration)
        {
            var rotation = component.transform.rotation.eulerAngles;
            return component.Play(duration).OnUpdate(progress =>
            {
                var rotationX = Mathf.LerpAngle(rotation.x, endValue, progress);
                component.transform.rotation = Quaternion.Euler(rotationX, rotation.y, rotation.z);
            });
        }

        public static Tween DORotateY(this Component component, float endValue, float duration)
        {
            var rotation = component.transform.rotation.eulerAngles;
            return component.Play(duration).OnUpdate(progress =>
            {
                var rotationY = Mathf.LerpAngle(rotation.y, endValue, progress);
                component.transform.rotation = Quaternion.Euler(rotation.x, rotationY, rotation.z);
            });
        }

        public static Tween DORotateZ(this Component component, float endValue, float duration)
        {
            var rotation = component.transform.rotation.eulerAngles;
            return component.Play(duration).OnUpdate(progress =>
            {
                var rotationZ = Mathf.LerpAngle(rotation.z, endValue, progress);
                component.transform.rotation = Quaternion.Euler(rotation.x, rotation.y, rotationZ);
            });
        }

        public static Tween DORotate(this Component component, Vector3 endValue, float duration)
        {
            var rotation = component.transform.rotation.eulerAngles;
            return component.Play(duration).OnUpdate(progress =>
            {
                var rotationX = Mathf.Lerp(rotation.x, endValue.x, progress);
                var rotationY = Mathf.Lerp(rotation.y, endValue.y, progress);
                var rotationZ = Mathf.Lerp(rotation.z, endValue.z, progress);
                component.transform.rotation = Quaternion.Euler(rotationX, rotationY, rotationZ);
            });
        }

        public static Tween DOScaleX(this Component component, float endValue, float duration)
        {
            var localScale = component.transform.localScale;
            return component.Play(duration).OnUpdate(progress =>
            {
                var localScaleX = Mathf.Lerp(localScale.x, endValue, progress);
                component.transform.localScale = new Vector3(localScaleX, localScale.y, localScale.z);
            });
        }

        public static Tween DOScaleY(this Component component, float endValue, float duration)
        {
            var localScale = component.transform.localScale;
            return component.Play(duration).OnUpdate(progress =>
            {
                var localScaleY = Mathf.Lerp(localScale.y, endValue, progress);
                component.transform.localScale = new Vector3(localScale.x, localScaleY, localScale.z);
            });
        }

        public static Tween DOScaleZ(this Component component, float endValue, float duration)
        {
            var localScale = component.transform.localScale;
            return component.Play(duration).OnUpdate(progress =>
            {
                var localScaleZ = Mathf.Lerp(localScale.z, endValue, progress);
                component.transform.localScale = new Vector3(localScale.x, localScale.y, localScaleZ);
            });
        }

        public static Tween DOScale(this Component component, Vector3 endValue, float duration)
        {
            var localScale = component.transform.localScale;
            return component.Play(duration).OnUpdate(progress =>
            {
                var localScaleX = Mathf.Lerp(localScale.x, endValue.x, progress);
                var localScaleY = Mathf.Lerp(localScale.y, endValue.y, progress);
                var localScaleZ = Mathf.Lerp(localScale.z, endValue.z, progress);
                component.transform.localScale = new Vector3(localScaleX, localScaleY, localScaleZ);
            });
        }
    }
}