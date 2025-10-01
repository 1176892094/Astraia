// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2024-12-21 23:12:15
// # Recently: 2024-12-22 20:12:19
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using UnityEngine;

namespace Astraia.Common
{
    public interface IMessage
    {
    }

    internal struct ReadyMessage : IMessage
    {
    }

    internal struct NotReadyMessage : IMessage
    {
    }

    internal struct SceneMessage : IMessage
    {
        public readonly string sceneName;
        public SceneMessage(string sceneName) => this.sceneName = sceneName;
    }

    internal struct PongMessage : IMessage
    {
        public readonly double clientTime;
        public PongMessage(double clientTime) => this.clientTime = clientTime;
    }

    internal struct PingMessage : IMessage
    {
        public readonly double clientTime;
        public PingMessage(double clientTime) => this.clientTime = clientTime;
    }

    internal struct ServerRpcMessage : IMessage
    {
        public byte sourceId;
        public uint objectId;
        public ushort methodHash;
        public ArraySegment<byte> segment;
    }

    internal struct ClientRpcMessage : IMessage
    {
        public byte sourceId;
        public uint objectId;
        public ushort methodHash;
        public ArraySegment<byte> segment;
    }

    internal struct SpawnMessage : IMessage
    {
        public byte opcode;
        public uint assetId;
        public uint sceneId;
        public uint objectId;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 localScale;
        public ArraySegment<byte> segment;
    }

    internal struct SpawnBeginMessage : IMessage
    {
    }

    internal struct DespawnMessage : IMessage
    {
        public readonly uint objectId;
        public DespawnMessage(uint objectId) => this.objectId = objectId;
    }
    
    internal struct DestroyMessage : IMessage
    {
        public readonly uint objectId;
        public DestroyMessage(uint objectId) => this.objectId = objectId;
    }

  
    internal struct EntityMessage : IMessage
    {
        public readonly uint objectId;
        public readonly ArraySegment<byte> segment;

        public EntityMessage(uint objectId, ArraySegment<byte> segment)
        {
            this.objectId = objectId;
            this.segment = segment;
        }
    }

    internal struct RequestMessage : IMessage
    {
    }

    internal struct ResponseMessage : IMessage
    {
        public readonly Uri uri;
        public ResponseMessage(Uri uri) => this.uri = uri;
    }
}