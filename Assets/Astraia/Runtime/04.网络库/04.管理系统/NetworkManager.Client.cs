// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2024-12-21 23:12:50
// # Recently: 2024-12-22 21:12:48
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using Astraia.Common;
using UnityEngine;

namespace Astraia.Net
{
    public partial class NetworkManager
    {
        public static partial class Client
        {
            private static readonly Dictionary<uint, NetworkEntity> scenes = new Dictionary<uint, NetworkEntity>();

            internal static readonly Dictionary<uint, NetworkEntity> spawns = new Dictionary<uint, NetworkEntity>();

            private static readonly Dictionary<uint, NetworkEntity> copies = new Dictionary<uint, NetworkEntity>();

            internal static State state = State.Disconnect;

            private static bool isLoadScene;

            internal static double pingTime;

            private static double pongTime;

            private static double sendTime;

            public static NetworkServer connection;

            public static bool isReady => connection.isReady;

            public static bool isActive => state == State.Connected;

            internal static void Start(int id)
            {
                if (id == 0)
                {
                    AddMessage(true);
                    connection = new NetworkServer();
                    Server.Connect(id);
                    Connect();
                }
                else
                {
                    AddMessage(false);
                    state = State.Connect;
                    connection = new NetworkServer();
                    Transport.StartClient();
                }
            }

            internal static void Start(Uri uri)
            {
                AddMessage(false);
                state = State.Connect;
                connection = new NetworkServer();
                Transport.StartClient(uri);
            }

            internal static void Stop()
            {
                if (!isClient) return;
                var entities = spawns.Values.Where(entity => entity).ToList();
                foreach (var entity in entities)
                {
                    DestroyMessage(new DestroyMessage(entity.objectId));
                }

                EventManager.Invoke(new ClientDisconnect());
                state = State.Disconnect;
                connection.Disconnect();
                sendTime = 0;
                pongTime = 0;
                pingTime = 0;
                copies.Clear();
                spawns.Clear();
                scenes.Clear();
                isLoadScene = false;
            }

            private static void Load(string sceneName)
            {
                if (!isLoadScene)
                {
                    EventManager.Invoke(new ClientLoadScene(sceneName));
                    if (!isServer)
                    {
                        isLoadScene = true;
                        AssetManager.LoadScene(sceneName);
                    }
                }
            }

            internal static void LoadSceneComplete(string sceneName)
            {
                isLoadScene = false;
                if (isActive && !isReady)
                {
                    connection.isReady = true;
                    connection.Send(new ReadyMessage());
                }

                EventManager.Invoke(new ClientSceneLoaded(sceneName));
            }
        }

        public static partial class Client
        {
            private static void AddMessage(bool isHost)
            {
                if (!isHost)
                {
                    Transport.OnClientConnect -= Connect;
                    Transport.OnClientDisconnect -= Disconnect;
                    Transport.OnClientReceive -= Receive;
                    Transport.OnClientConnect += Connect;
                    Transport.OnClientDisconnect += Disconnect;
                    Transport.OnClientReceive += Receive;
                }

                NetworkMessage<PingMessage>.Add(PingMessage);
                NetworkMessage<EntityMessage>.Add(EntityMessage);
                NetworkMessage<ClientRpcMessage>.Add(ClientRpcMessage);
                NetworkMessage<SceneMessage>.Add(SceneMessage);
                NetworkMessage<SpawnBeginMessage>.Add(SpawnBeginMessage);
                NetworkMessage<SpawnMessage>.Add(SpawnMessage);
                NetworkMessage<DespawnMessage>.Add(DespawnMessage);
                NetworkMessage<DestroyMessage>.Add(DestroyMessage);
            }

            private static void PingMessage(PingMessage message)
            {
                if (pingTime <= 0)
                {
                    pingTime = Time.unscaledTimeAsDouble - message.clientTime;
                }
                else
                {
                    pingTime += 2.0 / (6 + 1) * (Time.unscaledTimeAsDouble - message.clientTime - pingTime);
                }

                EventManager.Invoke(new PingUpdate(pingTime));
            }

            private static void EntityMessage(EntityMessage message)
            {
                if (isServer)
                {
                    return;
                }

                if (!spawns.TryGetValue(message.objectId, out var entity))
                {
                    Service.Log.Warn("无法同步网络对象: {0}", message.objectId);
                    return;
                }

                if (!entity)
                {
                    Service.Log.Warn("无法同步网络对象: {0}", message.objectId);
                    return;
                }

                using var reader = MemoryReader.Pop(message.segment);
                NetworkSyncVar.ClientDeserialize(entity.modules, reader);
            }

            private static void ClientRpcMessage(ClientRpcMessage message)
            {
                if (spawns.TryGetValue(message.objectId, out var entity))
                {
                    using var reader = MemoryReader.Pop(message.segment);
                    entity.InvokeMessage(message.moduleId, message.methodHash, InvokeMode.ClientRpc, reader);
                }
            }

            private static void SceneMessage(SceneMessage message)
            {
                if (isActive)
                {
                    connection.isReady = false;
                    Load(message.sceneName);
                }
            }
        }

        public static partial class Client
        {
            private static void Connect()
            {
                state = State.Connected;
                connection.isReady = true;
                connection.Send(new ReadyMessage());
                EventManager.Invoke(new ClientConnect());
            }

            private static void Disconnect()
            {
                Stop();
            }

            internal static void Receive(ArraySegment<byte> segment, int channel)
            {
                if (connection == null)
                {
                    Service.Log.Error("没有连接到有效的服务器！");
                    return;
                }

                if (!connection.reader.AddPacket(segment))
                {
                    Service.Log.Warn("无法处理来自服务器的消息。");
                    connection.Disconnect();
                    return;
                }

                while (!isLoadScene && connection.reader.GetMessage(out var result))
                {
                    using var reader = MemoryReader.Pop(result);
                    if (reader.buffer.Count - reader.position < sizeof(ushort))
                    {
                        Service.Log.Warn("无法处理来自服务器的消息。没有头部。");
                        connection.Disconnect();
                        return;
                    }

                    var message = reader.ReadUShort();
                    if (!NetworkMessage.client.TryGetValue(message, out var onMessage))
                    {
                        Service.Log.Warn("无法处理来自服务器的消息。未知的消息{0}", message);
                        connection.Disconnect();
                        return;
                    }

                    onMessage.Invoke(null, reader, channel);
                }

                if (!isLoadScene && connection.reader.Count > 0)
                {
                    Service.Log.Warn("无法处理来自服务器的消息。残留消息: {0}", connection.reader.Count);
                }
            }
        }

        public static partial class Client
        {
            private static void SpawnBeginMessage(SpawnBeginMessage message)
            {
                if (isServer)
                {
                    return;
                }

                scenes.Clear();
                var entities = FindObjectsByType<NetworkEntity>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                foreach (var entity in entities)
                {
                    if (entity.sceneId != 0 && entity.objectId == 0)
                    {
                        scenes[entity.sceneId] = entity;
                    }
                }
            }

            private static void SpawnMessage(SpawnMessage message)
            {
                if (isServer && Server.spawns.TryGetValue(message.objectId, out var entity))
                {
                    spawns[message.objectId] = entity;
                    entity.gameObject.SetActive(true);
                    entity.OnStartClient();
                    entity.OnNotifyAuthority();
                    return;
                }

                if (copies.Remove(message.objectId, out entity) || Spawn(message, out entity))
                {
                    entity.objectId = message.objectId;
                    entity.transform.localPosition = message.position;
                    entity.transform.localRotation = message.rotation;
                    entity.transform.localScale = message.localScale;
                    entity.label = message.isOwner ? entity.label | NetworkEntity.Label.Owner : entity.label & ~NetworkEntity.Label.Owner;
                    entity.label |= NetworkEntity.Label.Client;

                    if (message.segment.Count > 0)
                    {
                        using var reader = MemoryReader.Pop(message.segment);
                        NetworkSyncVar.ClientDeserialize(entity.modules, reader, true);
                    }

                    spawns[message.objectId] = entity;
                    entity.gameObject.SetActive(true);
                    entity.OnStartClient();
                    entity.OnNotifyAuthority();
                }
            }

            private static void DespawnMessage(DespawnMessage message)
            {
                if (spawns.TryGetValue(message.objectId, out var entity))
                {
                    entity.OnStopClient();
                    entity.label &= ~NetworkEntity.Label.Owner;
                    entity.OnNotifyAuthority();
                    entity.gameObject.SetActive(false);
                    if (!isServer)
                    {
                        copies[message.objectId] = entity;
                    }

                    spawns.Remove(message.objectId);
                }
            }

            private static void DestroyMessage(DestroyMessage message)
            {
                if (spawns.TryGetValue(message.objectId, out var entity))
                {
                    entity.OnStopClient();
                    entity.label &= ~NetworkEntity.Label.Owner;
                    entity.OnNotifyAuthority();
                    if (!isServer)
                    {
                        if (entity.sceneId != 0)
                        {
                            entity.gameObject.SetActive(false);
                            entity.Reset();
                        }
                        else
                        {
                            entity.state |= NetworkEntity.State.Destroy;
                            Destroy(entity.gameObject);
                        }
                    }

                    spawns.Remove(message.objectId);
                }
            }

            private static bool Spawn(SpawnMessage message, out NetworkEntity entity)
            {
                if (spawns.TryGetValue(message.objectId, out entity))
                {
                    return true;
                }

                if (message.sceneId == 0)
                {
                    var prefab = AssetManager.Load<GameObject>(GlobalSetting.Prefab.Format(message.assetId));
                    if (!prefab.TryGetComponent(out entity))
                    {
                        Service.Log.Error("无法注册网络对象 {0} 没有网络对象组件。", prefab.name);
                        return false;
                    }

                    if (entity.sceneId != 0)
                    {
                        Service.Log.Error("无法注册网络对象 {0}。因为该预置体为场景对象。", entity.name);
                        return false;
                    }
                }
                else if (!scenes.Remove(message.sceneId, out entity))
                {
                    Service.Log.Error("无法注册网络对象 {0}。场景标识无效。", message.sceneId);
                    return false;
                }

                return entity;
            }

            internal static void EarlyUpdate()
            {
                Transport?.ClientEarlyUpdate();
            }

            internal static void AfterUpdate()
            {
                if (isClient && NetworkSystem.Tick(ref sendTime))
                {
                    if (!isServer && connection.isReady)
                    {
                        foreach (var entity in spawns.Values)
                        {
                            using var writer = MemoryWriter.Pop();
                            NetworkSyncVar.ClientSerialize(entity.modules, writer, entity.isOwner);
                            if (writer.position > 0)
                            {
                                connection.Send(new EntityMessage(entity.objectId, writer));
                                entity.ClearDirty(false);
                            }
                        }
                    }
                }

                if (connection != null)
                {
                    if (isHost)
                    {
                        connection.Update();
                    }
                    else
                    {
                        if (isActive)
                        {
                            if (pongTime < Time.unscaledTimeAsDouble - 2)
                            {
                                pongTime = Time.unscaledTimeAsDouble;
                                connection.Send(new PongMessage(pongTime), Channel.Unreliable);
                            }

                            connection.Update();
                        }
                    }
                }

                Transport?.ClientAfterUpdate();
            }
        }
    }
}