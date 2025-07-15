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
        public static Vector2 GetVector2(this MemoryReader reader)
        {
            return reader.Get<Vector2>();
        }

        public static Vector2? GetVector2Null(this MemoryReader reader)
        {
            return reader.Getable<Vector2>();
        }

        public static Vector3 GetVector3(this MemoryReader reader)
        {
            return reader.Get<Vector3>();
        }

        public static Vector3? GetVector3Null(this MemoryReader reader)
        {
            return reader.Getable<Vector3>();
        }

        public static Vector4 GetVector4(this MemoryReader reader)
        {
            return reader.Get<Vector4>();
        }

        public static Vector4? GetVector4Null(this MemoryReader reader)
        {
            return reader.Getable<Vector4>();
        }

        public static Vector2Int GetVector2Int(this MemoryReader reader)
        {
            return reader.Get<Vector2Int>();
        }

        public static Vector2Int? GetVector2IntNull(this MemoryReader reader)
        {
            return reader.Getable<Vector2Int>();
        }

        public static Vector3Int GetVector3Int(this MemoryReader reader)
        {
            return reader.Get<Vector3Int>();
        }

        public static Vector3Int? GetVector3IntNull(this MemoryReader reader)
        {
            return reader.Getable<Vector3Int>();
        }

        public static Quaternion GetQuaternion(this MemoryReader reader)
        {
            return reader.Get<Quaternion>();
        }

        public static Quaternion? GetQuaternionNull(this MemoryReader reader)
        {
            return reader.Getable<Quaternion>();
        }

        public static Color GetColor(this MemoryReader reader)
        {
            return reader.Get<Color>();
        }

        public static Color32 GetColor32(this MemoryReader reader)
        {
            return reader.Get<Color32>();
        }

        public static Rect GetRect(this MemoryReader reader)
        {
            return new Rect(reader.GetVector2(), reader.GetVector2());
        }

        public static Plane GetPlane(this MemoryReader reader)
        {
            return new Plane(reader.GetVector3(), reader.GetFloat());
        }

        public static Ray GetRay(this MemoryReader reader)
        {
            return new Ray(reader.GetVector3(), reader.GetVector3());
        }

        public static Matrix4x4 GetMatrix4x4(this MemoryReader reader)
        {
            return reader.Get<Matrix4x4>();
        }

        public static NetworkEntity GetNetworkEntity(this MemoryReader reader)
        {
            var objectId = reader.GetUInt();
            return objectId != 0 ? NetworkManager.GetNetworkEntity(objectId) : null;
        }

        public static NetworkAgent GetNetworkAgent(this MemoryReader reader)
        {
            var entity = reader.GetNetworkEntity();
            return entity != null ? entity.agents[reader.GetByte()] : null;
        }

        public static Transform GetTransform(this MemoryReader reader)
        {
            var entity = reader.GetNetworkEntity();
            return entity != null ? entity.transform : null;
        }

        public static GameObject GetGameObject(this MemoryReader reader)
        {
            var entity = reader.GetNetworkEntity();
            return entity != null ? entity.gameObject : null;
        }

        public static Texture2D GetTexture2D(this MemoryReader reader)
        {
            var width = reader.GetShort();
            if (width < 0)
            {
                return null;
            }

            var height = reader.GetShort();
            var texture = new Texture2D(width, height);
            var pixels = reader.GetArray<Color32>();
            texture.SetPixels32(pixels);
            texture.Apply();
            return texture;
        }

        public static Sprite GetSprite(this MemoryReader reader)
        {
            var texture = reader.GetTexture2D();
            return texture == null ? null : Sprite.Create(texture, reader.GetRect(), reader.GetVector2());
        }
        
        public static T GetNetworkAgent<T>(this MemoryReader reader) where T : NetworkAgent
        {
            return reader.GetNetworkAgent() as T;
        }

        public static NetworkVariable GetNetworkVariable(this MemoryReader reader)
        {
            var objectId = reader.GetUInt();
            byte agentId = 0;

            if (objectId != 0)
            {
                agentId = reader.GetByte();
            }

            return new NetworkVariable(objectId, agentId);
        }
    }
}