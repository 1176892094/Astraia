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
        public static void SetVector2(this MemoryWriter writer, Vector2 value)
        {
            writer.Set(value);
        }

        public static void SetVector2Null(this MemoryWriter writer, Vector2? value)
        {
            writer.Setable(value);
        }

        public static void SetVector3(this MemoryWriter writer, Vector3 value)
        {
            writer.Set(value);
        }

        public static void SetVector3Null(this MemoryWriter writer, Vector3? value)
        {
            writer.Setable(value);
        }

        public static void SetVector4(this MemoryWriter writer, Vector4 value)
        {
            writer.Set(value);
        }

        public static void SetVector4Null(this MemoryWriter writer, Vector4? value)
        {
            writer.Setable(value);
        }

        public static void SetVector2Int(this MemoryWriter writer, Vector2Int value)
        {
            writer.Set(value);
        }

        public static void SetVector2IntNull(this MemoryWriter writer, Vector2Int? value)
        {
            writer.Setable(value);
        }

        public static void SetVector3Int(this MemoryWriter writer, Vector3Int value)
        {
            writer.Set(value);
        }

        public static void SetVector3IntNull(this MemoryWriter writer, Vector3Int? value)
        {
            writer.Setable(value);
        }

        public static void SetQuaternion(this MemoryWriter writer, Quaternion value)
        {
            writer.Set(value);
        }

        public static void SetQuaternionNull(this MemoryWriter writer, Quaternion? value)
        {
            writer.Setable(value);
        }

        public static void SetColor(this MemoryWriter writer, Color value)
        {
            writer.Set(value);
        }

        public static void SetColor32(this MemoryWriter writer, Color32 value)
        {
            writer.Set(value);
        }

        public static void SetRect(this MemoryWriter writer, Rect value)
        {
            writer.SetVector2(value.position);
            writer.SetVector2(value.size);
        }

        public static void SetPlane(this MemoryWriter writer, Plane value)
        {
            writer.SetVector3(value.normal);
            writer.SetFloat(value.distance);
        }

        public static void SetRay(this MemoryWriter writer, Ray value)
        {
            writer.SetVector3(value.origin);
            writer.SetVector3(value.direction);
        }

        public static void SetMatrix4x4(this MemoryWriter writer, Matrix4x4 value)
        {
            writer.Set(value);
        }

        public static void SetNetworkObject(this MemoryWriter writer, NetworkEntity value)
        {
            if (value == null)
            {
                writer.SetUInt(0);
                return;
            }

            if (value.objectId == 0)
            {
                Debug.LogWarning(Log.E209);
                writer.SetUInt(0);
                return;
            }

            writer.SetUInt(value.objectId);
        }

        public static void SetNetworkSource(this MemoryWriter writer, NetworkAgent value)
        {
            if (value == null)
            {
                writer.SetUInt(0);
                return;
            }

            writer.SetNetworkObject(value.owner);
            writer.SetByte(value.sourceId);
        }

        public static void SetTransform(this MemoryWriter writer, Transform value)
        {
            if (value == null)
            {
                writer.SetUInt(0);
                return;
            }

            writer.SetNetworkObject(value.GetComponent<NetworkEntity>());
        }

        public static void SetGameObject(this MemoryWriter writer, GameObject value)
        {
            if (value == null)
            {
                writer.SetUInt(0);
                return;
            }

            writer.SetNetworkObject(value.GetComponent<NetworkEntity>());
        }

        public static void SetTexture2D(this MemoryWriter writer, Texture2D value)
        {
            if (value == null)
            {
                writer.SetShort(-1);
                return;
            }

            writer.SetShort((short)value.width);
            writer.SetShort((short)value.height);
            writer.SetArray(value.GetPixels32());
        }

        public static void SetSprite(this MemoryWriter writer, Sprite value)
        {
            if (value == null)
            {
                writer.SetTexture2D(null);
                return;
            }

            writer.SetTexture2D(value.texture);
            writer.SetRect(value.rect);
            writer.SetVector2(value.pivot);
        }

        public static void SetArraySegment<T>(this MemoryWriter writer, ArraySegment<T> value)
        {
            writer.SetInt(value.Count);
            for (var i = 0; i < value.Count; i++)
            {
                writer.Invoke(value.Array[value.Offset + i]);
            }
        }
    }
}