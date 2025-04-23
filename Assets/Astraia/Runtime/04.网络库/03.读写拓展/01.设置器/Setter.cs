// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2024-12-22 19:12:19
// # Recently: 2024-12-22 20:12:59
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using UnityEngine;

namespace Astraia.Net
{
    public static partial class Extensions
    {
        public static void SetVector2(this MemorySetter setter, Vector2 value)
        {
            setter.Set(value);
        }

        public static void SetVector2Null(this MemorySetter setter, Vector2? value)
        {
            setter.Setable(value);
        }

        public static void SetVector3(this MemorySetter setter, Vector3 value)
        {
            setter.Set(value);
        }

        public static void SetVector3Null(this MemorySetter setter, Vector3? value)
        {
            setter.Setable(value);
        }

        public static void SetVector4(this MemorySetter setter, Vector4 value)
        {
            setter.Set(value);
        }

        public static void SetVector4Null(this MemorySetter setter, Vector4? value)
        {
            setter.Setable(value);
        }

        public static void SetVector2Int(this MemorySetter setter, Vector2Int value)
        {
            setter.Set(value);
        }

        public static void SetVector2IntNull(this MemorySetter setter, Vector2Int? value)
        {
            setter.Setable(value);
        }

        public static void SetVector3Int(this MemorySetter setter, Vector3Int value)
        {
            setter.Set(value);
        }

        public static void SetVector3IntNull(this MemorySetter setter, Vector3Int? value)
        {
            setter.Setable(value);
        }

        public static void SetQuaternion(this MemorySetter setter, Quaternion value)
        {
            setter.Set(value);
        }

        public static void SetQuaternionNull(this MemorySetter setter, Quaternion? value)
        {
            setter.Setable(value);
        }

        public static void SetColor(this MemorySetter setter, Color value)
        {
            setter.Set(value);
        }

        public static void SetColor32(this MemorySetter setter, Color32 value)
        {
            setter.Set(value);
        }

        public static void SetRect(this MemorySetter setter, Rect value)
        {
            setter.SetVector2(value.position);
            setter.SetVector2(value.size);
        }

        public static void SetPlane(this MemorySetter setter, Plane value)
        {
            setter.SetVector3(value.normal);
            setter.SetFloat(value.distance);
        }

        public static void SetRay(this MemorySetter setter, Ray value)
        {
            setter.SetVector3(value.origin);
            setter.SetVector3(value.direction);
        }

        public static void SetMatrix4x4(this MemorySetter setter, Matrix4x4 value)
        {
            setter.Set(value);
        }

        public static void SetNetworkObject(this MemorySetter setter, NetworkObject value)
        {
            if (value == null)
            {
                setter.SetUInt(0);
                return;
            }

            if (value.objectId == 0)
            {
                Debug.LogWarning(Logs.E209);
                setter.SetUInt(0);
                return;
            }

            setter.SetUInt(value.objectId);
        }

        public static void SetNetworkBehaviour(this MemorySetter setter, NetworkBehaviour value)
        {
            if (value == null)
            {
                setter.SetUInt(0);
                return;
            }

            setter.SetNetworkObject(value.@object);
            setter.SetByte(value.componentId);
        }

        public static void SetTransform(this MemorySetter setter, Transform value)
        {
            if (value == null)
            {
                setter.SetUInt(0);
                return;
            }

            setter.SetNetworkObject(value.GetComponent<NetworkObject>());
        }

        public static void SetGameObject(this MemorySetter setter, GameObject value)
        {
            if (value == null)
            {
                setter.SetUInt(0);
                return;
            }

            setter.SetNetworkObject(value.GetComponent<NetworkObject>());
        }

        public static void SetTexture2D(this MemorySetter setter, Texture2D value)
        {
            if (value == null)
            {
                setter.SetShort(-1);
                return;
            }

            setter.SetShort((short)value.width);
            setter.SetShort((short)value.height);
            setter.SetArray(value.GetPixels32());
        }

        public static void SetSprite(this MemorySetter setter, Sprite value)
        {
            if (value == null)
            {
                setter.SetTexture2D(null);
                return;
            }

            setter.SetTexture2D(value.texture);
            setter.SetRect(value.rect);
            setter.SetVector2(value.pivot);
        }

        public static void SetArraySegment<T>(this MemorySetter setter, ArraySegment<T> value)
        {
            setter.SetInt(value.Count);
            for (var i = 0; i < value.Count; i++)
            {
                setter.Invoke(value.Array[value.Offset + i]);
            }
        }
    }
}