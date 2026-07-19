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
using Astraia.Net;
using UnityEngine;

namespace Astraia
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
        public readonly ushort methodId;
        public readonly ArraySegment<byte> segment;

        public ServerRpcMessage(uint objectId, byte moduleId, ushort methodId, ArraySegment<byte> segment)
        {
            this.objectId = objectId;
            this.moduleId = moduleId;
            this.methodId = methodId;
            this.segment = segment;
        }
    }

    internal readonly struct ClientRpcMessage : IMessage
    {
        public readonly uint objectId;
        public readonly byte moduleId;
        public readonly ushort methodId;
        public readonly ArraySegment<byte> segment;

        public ClientRpcMessage(uint objectId, byte moduleId, ushort methodId, ArraySegment<byte> segment)
        {
            this.objectId = objectId;
            this.moduleId = moduleId;
            this.methodId = methodId;
            this.segment = segment;
        }
    }

    internal readonly struct SpawnMessage : IMessage
    {
        public readonly bool isOwner;
        public readonly uint assetId;
        public readonly uint sceneId;
        public readonly uint objectId;
        public readonly Vector3 mutation;
        public readonly Vector3 position;
        public readonly Vector3 rotation;
        public readonly ArraySegment<byte> segment;

        public SpawnMessage(NetworkEntity entity, NetworkClient client, ArraySegment<byte> message)
        {
            isOwner = entity.client == client;
            assetId = entity.assetId;
            sceneId = entity.sceneId;
            objectId = entity.objectId;
            mutation = entity.transform.localScale;
            position = entity.transform.localPosition;
            rotation = entity.transform.localRotation.eulerAngles;
            segment = message;
        }
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