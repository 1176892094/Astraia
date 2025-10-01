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
        [Serializable]
        public static partial class Client
        {
            internal static readonly Dictionary<uint, NetworkEntity> scenes = new Dictionary<uint, NetworkEntity>();

            internal static readonly Dictionary<uint, NetworkEntity> spawns = new Dictionary<uint, NetworkEntity>();

            internal static readonly Dictionary<NetworkEntity, SpawnMessage> copies = new Dictionary<NetworkEntity, SpawnMessage>();

            internal static State state = State.Disconnect;

            private static double pingTime;

            private static double pongTime;

            private static double sendTime;

            public static bool isReady => connection != null && connection.isReady;

            public static bool isLoadScene { get; internal set; }

            public static NetworkServer connection { get; private set; }

            public static bool isActive => state == State.Connected;

            internal static void Start(bool transport)
            {
                if (!transport)
                {
                    AddMessage(false);
                    connection = new NetworkServer();
                    Server.Connect(new NetworkClient());
                    OnClientConnect();
                    return;
                }

                AddMessage(true);
                state = State.Connect;
                connection = new NetworkServer();
                Transport.Instance.StartClient();
            }

            internal static void Start(Uri uri)
            {
                AddMessage(true);
                state = State.Connect;
                connection = new NetworkServer();
                Transport.Instance.StartClient(uri);
            }

            internal static void Stop()
            {
                if (!isClient) return;
                var entities = spawns.Values.Where(entity => entity).ToList();
                foreach (var entity in entities)
                {
                    DestroyMessage(new DestroyMessage(entity.objectId));
                }

                state = State.Disconnect;
                connection.Disconnect();
                sendTime = 0;
                pongTime = 0;
                pingTime = 0;
                spawns.Clear();
                copies.Clear();
                scenes.Clear();
                isLoadScene = false;
                EventManager.Invoke(new ClientDisconnect());
            }

            public static void Ready()
            {
                if (connection == null)
                {
                    Log.Error("没有连接到有效的服务器！");
                    return;
                }

                if (isReady)
                {
                    Log.Error("客户端已经准备就绪！");
                    return;
                }

                connection.isReady = true;
                connection.Send(new ReadyMessage());
            }

            private static void Load(string sceneName)
            {
                if (string.IsNullOrWhiteSpace(sceneName))
                {
                    Log.Error("客户端不能加载空场景！");
                    return;
                }

                if (isLoadScene && Instance.sceneName == sceneName)
                {
                    Log.Error("客户端正在加载 {0} 场景", sceneName);
                    return;
                }

                EventManager.Invoke(new ClientLoadScene(sceneName));
                if (!isServer)
                {
                    isLoadScene = true;
                    Instance.sceneName = sceneName;
                    AssetManager.LoadScene(sceneName);
                }
            }

            internal static void LoadSceneComplete(string sceneName)
            {
                isLoadScene = false;
                if (isActive && !isReady)
                {
                    Ready();
                }

                EventManager.Invoke(new ClientSceneLoaded(sceneName));
            }
        }

        public static partial class Client
        {
            private static void AddMessage(bool transport)
            {
                if (transport)
                {
                    Transport.Instance.OnClientConnect -= OnClientConnect;
                    Transport.Instance.OnClientDisconnect -= OnClientDisconnect;
                    Transport.Instance.OnClientReceive -= OnClientReceive;
                    Transport.Instance.OnClientConnect += OnClientConnect;
                    Transport.Instance.OnClientDisconnect += OnClientDisconnect;
                    Transport.Instance.OnClientReceive += OnClientReceive;
                }

                NetworkMessage<PingMessage>.Add(PingMessage);
                NetworkMessage<NotReadyMessage>.Add(NotReadyMessage);
                NetworkMessage<EntityMessage>.Add(EntityMessage);
                NetworkMessage<ClientRpcMessage>.Add(ClientRpcMessage);
                NetworkMessage<SceneMessage>.Add(SceneMessage);
                NetworkMessage<SpawnBeginMessage>.Add(SpawnBeginMessage);
                NetworkMessage<SpawnMessage>.Add(SpawnMessage);
                NetworkMessage<SpawnEndMessage>.Add(SpawnEndMessage);
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
                    var delta = Time.unscaledTimeAsDouble - message.clientTime - pingTime;
                    pingTime += 2.0 / (6 + 1) * delta;
                }

                EventManager.Invoke(new PingUpdate(pingTime));
            }

            private static void NotReadyMessage(NotReadyMessage message)
            {
                connection.isReady = false;
                EventManager.Invoke(new ClientNotReady());
            }

            private static void EntityMessage(EntityMessage message)
            {
                if (isServer)
                {
                    return;
                }

                if (!spawns.TryGetValue(message.objectId, out var entity))
                {
                    Log.Warn("无法同步网络对象: {0}", message.objectId);
                    return;
                }

                if (!entity)
                {
                    Log.Warn("无法同步网络对象: {0}", message.objectId);
                    return;
                }

                using var reader = MemoryReader.Pop(message.segment);
                entity.ClientDeserialize(reader, false);
            }

            private static void ClientRpcMessage(ClientRpcMessage message)
            {
                if (spawns.TryGetValue(message.objectId, out var entity))
                {
                    using var reader = MemoryReader.Pop(message.segment);
                    entity.InvokeMessage(message.sourceId, message.methodHash, InvokeMode.ClientRpc, reader);
                }
            }

            private static void SceneMessage(SceneMessage message)
            {
                if (!isActive)
                {
                    Log.Warn("客户端没有通过验证，无法加载场景。");
                    return;
                }

                Load(message.sceneName);
            }

            private static void SpawnBeginMessage(SpawnBeginMessage message)
            {
                if (isServer) return;
                scenes.Clear();
                var entities = FindObjectsByType<NetworkEntity>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                foreach (var entity in entities.Where(entity => entity.sceneId != 0 && entity.objectId == 0))
                {
                    if (!scenes.TryAdd(entity.sceneId, entity))
                    {
                        Debug.LogWarning("客户端场景对象重复。网络对象: {0}".Format(entity.name), entity);
                    }
                }

                copies.Clear();
                connection.isSpawn = false;
            }

            private static void SpawnEndMessage(SpawnEndMessage message)
            {
                if (isServer) return;
                foreach (var entity in spawns.Values.Where(entity => entity).OrderBy(entity => entity.objectId))
                {
                    if (copies.TryGetValue(entity, out var segment))
                    {
                        Spawn(segment, entity);
                        entity.OnStartClient();
                        entity.OnNotifyAuthority();
                    }
                }

                copies.Clear();
                connection.isSpawn = true;
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

                if (LoadEntity(message, out entity))
                {
                    if (connection.isSpawn)
                    {
                        Spawn(message, entity);
                        entity.OnStartClient();
                        entity.OnNotifyAuthority();
                        return;
                    }

                    spawns[message.objectId] = entity;
                    var segment = new byte[message.segment.Count];
                    if (message.segment.Count > 0)
                    {
                        Buffer.BlockCopy(message.segment.Array!, message.segment.Offset, segment, 0, message.segment.Count);
                    }

                    message.segment = new ArraySegment<byte>(segment);
                    copies[entity] = message;
                }
            }

            private static void DespawnMessage(DespawnMessage message)
            {
                if (spawns.TryGetValue(message.objectId, out var entity))
                {
                    entity.OnStopClient();
                    entity.mode &= ~EntityMode.Owner;
                    entity.OnNotifyAuthority();
                    entity.gameObject.SetActive(false);
                    spawns.Remove(message.objectId);
                }
            }

            private static void DestroyMessage(DestroyMessage message)
            {
                if (spawns.TryGetValue(message.objectId, out var entity))
                {
                    entity.OnStopClient();
                    entity.mode &= ~EntityMode.Owner;
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
                            entity.state |= EntityState.Destroy;
                            Destroy(entity.gameObject);
                        }
                    }

                    spawns.Remove(message.objectId);
                }
            }
        }

        public static partial class Client
        {
            private static void OnClientConnect()
            {
                if (connection == null)
                {
                    Log.Error("没有连接到有效的服务器！");
                    return;
                }

                state = State.Connected;
                EventManager.Invoke(new ClientConnect());
                Ready();
            }

            private static void OnClientDisconnect()
            {
                Stop();
            }

            internal static void OnClientReceive(ArraySegment<byte> segment, int channel)
            {
                if (connection == null)
                {
                    Log.Error("没有连接到有效的服务器！");
                    return;
                }

                if (!connection.reader.AddBatch(segment))
                {
                    Log.Warn("无法处理来自服务器的消息。");
                    connection.Disconnect();
                    return;
                }

                while (!isLoadScene && connection.reader.GetMessage(out var result))
                {
                    using var reader = MemoryReader.Pop(result);
                    if (reader.buffer.Count - reader.position < sizeof(ushort))
                    {
                        Log.Warn("无法处理来自服务器的消息。没有头部。");
                        connection.Disconnect();
                        return;
                    }

                    var message = reader.ReadUShort();
                    if (!NetworkMessage.client.TryGetValue(message, out var action))
                    {
                        Log.Warn("无法处理来自服务器的消息。未知的消息{0}", message);
                        connection.Disconnect();
                        return;
                    }

                    action.Invoke(null, reader, channel);
                }

                if (!isLoadScene && connection.reader.Count > 0)
                {
                    Log.Warn("无法处理来自服务器的消息。残留消息: {0}", connection.reader.Count);
                }
            }
        }

        public static partial class Client
        {
            private static bool LoadEntity(SpawnMessage message, out NetworkEntity entity)
            {
                if (spawns.TryGetValue(message.objectId, out entity) && entity)
                {
                    return true;
                }

                if (message.sceneId == 0)
                {
                    var prefab = AssetManager.Load<GameObject>(GlobalSetting.Prefab.Format(message.assetId));
                    if (!prefab.TryGetComponent(out entity))
                    {
                        Log.Error("无法注册网络对象 {0} 没有网络对象组件。", prefab.name);
                        return false;
                    }

                    if (entity.sceneId != 0)
                    {
                        Log.Error("无法注册网络对象 {0}。因为该预置体为场景对象。", entity.name);
                        return false;
                    }
                }
                else if (!scenes.Remove(message.sceneId, out entity))
                {
                    Log.Error("无法注册网络对象 {0}。场景标识无效。", message.sceneId);
                    return false;
                }

                return entity;
            }

            private static void Spawn(SpawnMessage message, NetworkEntity entity)
            {
                spawns[message.objectId] = entity;
                entity.gameObject.SetActive(true);
                entity.objectId = message.objectId;
                entity.transform.localPosition = message.position;
                entity.transform.localRotation = message.rotation;
                entity.transform.localScale = message.localScale;
                entity.mode = (message.opcode & 1) != 0 ? entity.mode | EntityMode.Owner : entity.mode & ~EntityMode.Owner;
                entity.mode |= EntityMode.Client;

                if (message.segment.Count > 0)
                {
                    using var reader = MemoryReader.Pop(message.segment);
                    entity.ClientDeserialize(reader, true);
                }
            }
        }

        public static partial class Client
        {
            internal static void EarlyUpdate()
            {
                if (Transport.Instance)
                {
                    Transport.Instance.ClientEarlyUpdate();
                }
            }

            internal static void AfterUpdate()
            {
                if (isClient)
                {
                    if (NetworkSystem.Tick(Instance.sendRate, ref sendTime))
                    {
                        Broadcast();
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
                            if (pongTime + 2 <= Time.unscaledTimeAsDouble)
                            {
                                pongTime = Time.unscaledTimeAsDouble;
                                connection.Send(new PongMessage(pongTime), Channel.Unreliable);
                            }

                            connection.Update();
                        }
                    }
                }

                if (Transport.Instance)
                {
                    Transport.Instance.ClientAfterUpdate();
                }
            }

            private static void Broadcast()
            {
                if (isServer)
                {
                    return;
                }

                if (!connection.isReady)
                {
                    return;
                }

                foreach (var entity in spawns.Values)
                {
                    using var writer = MemoryWriter.Pop();
                    entity.ClientSerialize(writer);
                    if (writer.position > 0)
                    {
                        connection.Send(new EntityMessage(entity.objectId, writer));
                        entity.ClearDirty(false);
                    }
                }
            }
        }
    }
}