// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-08-15 03:08:10
// # Recently: 2026-08-15 03:58:10
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using Astraia.Net;
using UnityEngine;

namespace Astraia
{
    internal readonly struct ReadyMessage : IMessage { }

    internal readonly struct SceneMessage : IMessage
    {
        public readonly string sceneName;
        public SceneMessage(string sceneName) => this.sceneName = sceneName;
    }

    internal readonly struct PongMessage : IMessage
    {
        public readonly double clientTime;
        public PongMessage(double clientTime) => this.clientTime = clientTime;
    }

    internal readonly struct PingMessage : IMessage
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

    internal readonly struct SpawnBeginMessage : IMessage { }

    internal readonly struct DespawnMessage : IMessage
    {
        public readonly uint objectId;
        public DespawnMessage(uint objectId) => this.objectId = objectId;
    }

    internal readonly struct DestroyMessage : IMessage
    {
        public readonly uint objectId;
        public DestroyMessage(uint objectId) => this.objectId = objectId;
    }

    internal readonly struct EntityMessage : IMessage
    {
        public readonly uint objectId;
        public readonly ArraySegment<byte> segment;

        public EntityMessage(uint objectId, ArraySegment<byte> segment)
        {
            this.objectId = objectId;
            this.segment = segment;
        }
    }

    internal readonly struct RequestMessage : IMessage { }

    internal readonly struct ResponseMessage : IMessage
    {
        public readonly ushort port;
        public ResponseMessage(ushort port) => this.port = port;
    }
}