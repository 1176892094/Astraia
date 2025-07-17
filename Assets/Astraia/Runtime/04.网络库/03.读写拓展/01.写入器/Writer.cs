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
        public static void WriteVector2(this MemoryWriter writer, Vector2 value)
        {
            writer.Write(value);
        }

        public static void WriteVector2Nullable(this MemoryWriter writer, Vector2? value)
        {
            writer.WriteNullable(value);
        }

        public static void WriteVector3(this MemoryWriter writer, Vector3 value)
        {
            writer.Write(value);
        }

        public static void WriteVector3Nullable(this MemoryWriter writer, Vector3? value)
        {
            writer.WriteNullable(value);
        }

        public static void WriteVector4(this MemoryWriter writer, Vector4 value)
        {
            writer.Write(value);
        }

        public static void WriteVector4Nullable(this MemoryWriter writer, Vector4? value)
        {
            writer.WriteNullable(value);
        }

        public static void WriteVector2Int(this MemoryWriter writer, Vector2Int value)
        {
            writer.Write(value);
        }

        public static void WriteVector2IntNullable(this MemoryWriter writer, Vector2Int? value)
        {
            writer.WriteNullable(value);
        }

        public static void WriteVector3Int(this MemoryWriter writer, Vector3Int value)
        {
            writer.Write(value);
        }

        public static void WriteVector3IntNullable(this MemoryWriter writer, Vector3Int? value)
        {
            writer.WriteNullable(value);
        }

        public static void WriteQuaternion(this MemoryWriter writer, Quaternion value)
        {
            writer.Write(value);
        }

        public static void WriteQuaternionNullable(this MemoryWriter writer, Quaternion? value)
        {
            writer.WriteNullable(value);
        }

        public static void WriteColor(this MemoryWriter writer, Color value)
        {
            writer.Write(value);
        }

        public static void WriteColor32(this MemoryWriter writer, Color32 value)
        {
            writer.Write(value);
        }

        public static void WriteRect(this MemoryWriter writer, Rect value)
        {
            writer.WriteVector2(value.position);
            writer.WriteVector2(value.size);
        }

        public static void WritePlane(this MemoryWriter writer, Plane value)
        {
            writer.WriteVector3(value.normal);
            writer.WriteFloat(value.distance);
        }

        public static void WriteRay(this MemoryWriter writer, Ray value)
        {
            writer.WriteVector3(value.origin);
            writer.WriteVector3(value.direction);
        }

        public static void WriteMatrix4x4(this MemoryWriter writer, Matrix4x4 value)
        {
            writer.Write(value);
        }

        public static void WriteNetworkEntity(this MemoryWriter writer, NetworkEntity value)
        {
            if (value == null)
            {
                writer.WriteUInt(0);
                return;
            }

            if (value.objectId == 0)
            {
                Debug.LogWarning(Log.E209);
                writer.WriteUInt(0);
                return;
            }

            writer.WriteUInt(value.objectId);
        }

        public static void WriteNetworkAgent(this MemoryWriter writer, NetworkAgent value)
        {
            if (value == null)
            {
                writer.WriteUInt(0);
                return;
            }

            writer.WriteNetworkEntity(value.owner);
            writer.WriteByte(value.sourceId);
        }

        public static void WriteTransform(this MemoryWriter writer, Transform value)
        {
            if (value == null)
            {
                writer.WriteUInt(0);
                return;
            }

            writer.WriteNetworkEntity(value.GetComponent<NetworkEntity>());
        }

        public static void WriteGameObject(this MemoryWriter writer, GameObject value)
        {
            if (value == null)
            {
                writer.WriteUInt(0);
                return;
            }

            writer.WriteNetworkEntity(value.GetComponent<NetworkEntity>());
        }

        public static void WriteTexture2D(this MemoryWriter writer, Texture2D value)
        {
            if (value == null)
            {
                writer.WriteShort(-1);
                return;
            }

            writer.WriteShort((short)value.width);
            writer.WriteShort((short)value.height);
            writer.WriteArray(value.GetPixels32());
        }

        public static void WriteSprite(this MemoryWriter writer, Sprite value)
        {
            if (value == null)
            {
                writer.WriteTexture2D(null);
                return;
            }

            writer.WriteTexture2D(value.texture);
            writer.WriteRect(value.rect);
            writer.WriteVector2(value.pivot);
        }

        public static void WriteArraySegment<T>(this MemoryWriter writer, ArraySegment<T> value)
        {
            writer.WriteInt(value.Count);
            for (var i = 0; i < value.Count; i++)
            {
                writer.Invoke(value.Array[value.Offset + i]);
            }
        }
    }
}