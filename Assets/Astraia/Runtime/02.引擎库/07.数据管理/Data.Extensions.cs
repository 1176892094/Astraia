// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2025-01-08 18:01:04
// # Recently: 2025-01-08 18:01:04
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Astraia
{
    public static partial class Extensions
    {
        private static readonly Dictionary<Type, Delegate> parsers = new Dictionary<Type, Delegate>();

        static Extensions()
        {
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

        public static T Parse<T>(this byte[] reason)
        {
            if (reason == null) return default;
            var value = Service.Text.GetString(reason);
            if (parsers.TryGetValue(typeof(T), out var func))
            {
                return ((Func<string, T>)func).Invoke(value);
            }

            return value.InputGeneric(typeof(T)) is T result ? result : default;
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

        private static object InputGeneric(this string reason, Type target)
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
                if (reason.EndsWith(";"))
                {
                    reason = reason.Substring(0, reason.Length - 1);
                }

                var element = target.GetElementType();
                var members = reason.Split(';');
                var instance = Array.CreateInstance(element!, members.Length);
                for (var i = 0; i < members.Length; ++i)
                {
                    instance.SetValue(InputGeneric(members[i], element), i);
                }

                return instance;
            }

            var member = reason.Split(',');
            var result = Activator.CreateInstance(target);
            var fields = target.GetFields(Service.Find.Instance);
            for (var i = 0; i < fields.Length; i++)
            {
                fields[i].SetValue(result, new Xor.Bytes(Service.Text.GetBytes(member[i])));
            }

            return result;
        }
    }
}