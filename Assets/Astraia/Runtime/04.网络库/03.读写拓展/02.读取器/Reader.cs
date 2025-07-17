// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2024-12-22 19:12:21
// # Recently: 2024-12-22 20:12:59
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using UnityEngine;

namespace Astraia.Net
{
    public static partial class Extensions
    {
        public static Vector2 ReadVector2(this MemoryReader reader)
        {
            return reader.Read<Vector2>();
        }

        public static Vector2? ReadVector2Nullable(this MemoryReader reader)
        {
            return reader.ReadNullable<Vector2>();
        }

        public static Vector3 ReadVector3(this MemoryReader reader)
        {
            return reader.Read<Vector3>();
        }

        public static Vector3? ReadVector3Nullable(this MemoryReader reader)
        {
            return reader.ReadNullable<Vector3>();
        }

        public static Vector4 ReadVector4(this MemoryReader reader)
        {
            return reader.Read<Vector4>();
        }

        public static Vector4? ReadVector4Nullable(this MemoryReader reader)
        {
            return reader.ReadNullable<Vector4>();
        }

        public static Vector2Int ReadVector2Int(this MemoryReader reader)
        {
            return reader.Read<Vector2Int>();
        }

        public static Vector2Int? ReadVector2IntNullable(this MemoryReader reader)
        {
            return reader.ReadNullable<Vector2Int>();
        }

        public static Vector3Int ReadVector3Int(this MemoryReader reader)
        {
            return reader.Read<Vector3Int>();
        }

        public static Vector3Int? ReadVector3IntNullable(this MemoryReader reader)
        {
            return reader.ReadNullable<Vector3Int>();
        }

        public static Quaternion ReadQuaternion(this MemoryReader reader)
        {
            return reader.Read<Quaternion>();
        }

        public static Quaternion? ReadQuaternionNullable(this MemoryReader reader)
        {
            return reader.ReadNullable<Quaternion>();
        }

        public static Color ReadColor(this MemoryReader reader)
        {
            return reader.Read<Color>();
        }

        public static Color32 ReadColor32(this MemoryReader reader)
        {
            return reader.Read<Color32>();
        }

        public static Rect ReadRect(this MemoryReader reader)
        {
            return new Rect(reader.ReadVector2(), reader.ReadVector2());
        }

        public static Plane ReadPlane(this MemoryReader reader)
        {
            return new Plane(reader.ReadVector3(), reader.ReadFloat());
        }

        public static Ray ReadRay(this MemoryReader reader)
        {
            return new Ray(reader.ReadVector3(), reader.ReadVector3());
        }

        public static Matrix4x4 ReadMatrix4x4(this MemoryReader reader)
        {
            return reader.Read<Matrix4x4>();
        }

        public static NetworkEntity ReadNetworkEntity(this MemoryReader reader)
        {
            var objectId = reader.ReadUInt();
            return objectId != 0 ? NetworkManager.GetNetworkEntity(objectId) : null;
        }

        public static NetworkAgent ReadNetworkAgent(this MemoryReader reader)
        {
            var entity = reader.ReadNetworkEntity();
            return entity != null ? entity.agents[reader.ReadByte()] : null;
        }

        public static Transform ReadTransform(this MemoryReader reader)
        {
            var entity = reader.ReadNetworkEntity();
            return entity != null ? entity.transform : null;
        }

        public static GameObject ReadGameObject(this MemoryReader reader)
        {
            var entity = reader.ReadNetworkEntity();
            return entity != null ? entity.gameObject : null;
        }

        public static Texture2D ReadTexture2D(this MemoryReader reader)
        {
            var width = reader.ReadShort();
            if (width < 0)
            {
                return null;
            }

            var height = reader.ReadShort();
            var texture = new Texture2D(width, height);
            var pixels = reader.ReadArray<Color32>();
            texture.SetPixels32(pixels);
            texture.Apply();
            return texture;
        }

        public static Sprite ReadSprite(this MemoryReader reader)
        {
            var texture = reader.ReadTexture2D();
            return texture == null ? null : Sprite.Create(texture, reader.ReadRect(), reader.ReadVector2());
        }
        
        public static T ReadNetworkAgent<T>(this MemoryReader reader) where T : NetworkAgent
        {
            return reader.ReadNetworkAgent() as T;
        }

        public static NetworkVariable ReadNetworkVariable(this MemoryReader reader)
        {
            var objectId = reader.ReadUInt();
            byte agentId = 0;

            if (objectId != 0)
            {
                agentId = reader.ReadByte();
            }

            return new NetworkVariable(objectId, agentId);
        }
    }
}