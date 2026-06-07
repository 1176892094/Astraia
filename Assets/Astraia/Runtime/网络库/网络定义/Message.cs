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

namespace Astraia.Core
{
    public interface IMessage
    {
    }

    internal struct ReadyMessage : IMessage
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

    internal readonly struct ServerRpcMessage : IMessage
    {
        public readonly uint objectId;
        public readonly byte moduleId;
        public readonly ushort methodHash;
        public readonly ArraySegment<byte> segment;

        public ServerRpcMessage(uint objectId, byte moduleId, ushort methodHash, ArraySegment<byte> segment)
        {
            this.objectId = objectId;
            this.moduleId = moduleId;
            this.methodHash = methodHash;
            this.segment = segment;
        }
    }

    internal readonly struct ClientRpcMessage : IMessage
    {
        public readonly uint objectId;
        public readonly byte moduleId;
        public readonly ushort methodHash;
        public readonly ArraySegment<byte> segment;

        public ClientRpcMessage(uint objectId, byte moduleId, ushort methodHash, ArraySegment<byte> segment)
        {
            this.objectId = objectId;
            this.moduleId = moduleId;
            this.methodHash = methodHash;
            this.segment = segment;
        }
    }

    internal struct SpawnMessage : IMessage
    {
        public bool isOwner;
        public uint assetId;
        public uint sceneId;
        public uint objectId;
        public Vector3 mutation;
        public Vector3 position;
        public Vector3 rotation;
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
        public readonly ushort port;
        public ResponseMessage(ushort port) => this.port = port;
    }
}