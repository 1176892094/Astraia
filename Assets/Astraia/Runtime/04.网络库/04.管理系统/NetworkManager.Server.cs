// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2024-12-21 23:12:50
// # Recently: 2024-12-22 21:12:49
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
        public static partial class Server
        {
            internal static readonly Dictionary<int, NetworkClient> clients = new Dictionary<int, NetworkClient>();

            internal static readonly Dictionary<uint, NetworkEntity> spawns = new Dictionary<uint, NetworkEntity>();

            private static readonly List<NetworkClient> copies = new List<NetworkClient>();

            internal static State state = State.Disconnect;

            private static uint objectId;

            private static double sendTime;

            public static bool isReady => clients.Values.All(client => client.isReady);

            public static bool isLoadScene { get; internal set; }

            public static NetworkObserver observer { get; internal set; }

            public static int connections => clients.Count;

            internal static void Start(bool transport)
            {
                if (transport)
                {
                    Transport.Instance.StartServer();
                }

                state = State.Connected;
                AddMessage();
                SpawnObjects();
            }

            internal static void Stop()
            {
                if (!isServer) return;
                copies.Clear();
                copies.AddRange(clients.Values);
                foreach (var client in copies)
                {
                    OnServerDisconnect(client.clientId);
                }

                state = State.Disconnect;
                Transport.Instance.StopServer();
                sendTime = 0;
                objectId = 0;
                spawns.Clear();
                clients.Clear();
                isLoadScene = false;
                NetworkListener.Dispose();
            }

            internal static void Connect(NetworkClient client)
            {
                if (clients.TryAdd(client.clientId, client))
                {
                    EventManager.Invoke(new ServerConnect(client));
                }
            }

            public static void Load(string sceneName)
            {
                if (string.IsNullOrWhiteSpace(sceneName))
                {
                    Log.Error("服务器不能加载空场景！");
                    return;
                }

                if (isLoadScene && Instance.sceneName == sceneName)
                {
                    Log.Error("服务器正在加载 {0} 场景", sceneName);
                    return;
                }

                foreach (var client in clients.Values)
                {
                    client.isReady = false;
                    NetworkListener.Destroy(client);
                    client.Send(new NotReadyMessage());
                }

                EventManager.Invoke(new ServerLoadScene(sceneName));
                if (isServer)
                {
                    isLoadScene = true;
                    Instance.sceneName = sceneName;
                    foreach (var client in clients.Values)
                    {
                        client.Send(new SceneMessage(sceneName));
                    }

                    AssetManager.LoadScene(sceneName);
                }
            }

            internal static void LoadSceneComplete(string sceneName)
            {
                isLoadScene = false;
                SpawnObjects();
                EventManager.Invoke(new ServerSceneLoaded(sceneName));
            }

            private static void SpawnObjects()
            {
                var entities = FindObjectsByType<NetworkEntity>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                foreach (var entity in entities)
                {
                    if (entity.sceneId != 0 && entity.objectId == 0 && entity.isActiveAndEnabled)
                    {
                        Spawn(entity.gameObject, entity.client);
                    }
                }
            }
        }

        public static partial class Server
        {
            private static void AddMessage()
            {
                Transport.Instance.OnServerConnect -= OnServerConnect;
                Transport.Instance.OnServerDisconnect -= OnServerDisconnect;
                Transport.Instance.OnServerReceive -= OnServerReceive;
                Transport.Instance.OnServerConnect += OnServerConnect;
                Transport.Instance.OnServerDisconnect += OnServerDisconnect;
                Transport.Instance.OnServerReceive += OnServerReceive;
                NetworkMessage<PongMessage>.Add(PongMessage);
                NetworkMessage<ReadyMessage>.Add(ReadyMessage);
                NetworkMessage<EntityMessage>.Add(EntityMessage);
                NetworkMessage<ServerRpcMessage>.Add(ServerRpcMessage);
            }

            private static void PongMessage(NetworkClient client, PongMessage message)
            {
                client.Send(new PingMessage(message.clientTime), Channel.Unreliable);
            }

            private static void ReadyMessage(NetworkClient client, ReadyMessage message)
            {
                client.isReady = true;
                client.Send(new SpawnBeginMessage());
                foreach (var entity in spawns.Values.Where(entity => entity.isActiveAndEnabled))
                {
                    switch (entity.visible)
                    {
                        case Visible.Show:
                            NetworkListener.Listen(entity, client);
                            break;
                        case Visible.Pool when observer:
                            if (observer.OnExecute(entity, client))
                            {
                                NetworkListener.Listen(entity, client);
                            }

                            break;
                        case Visible.Pool:
                            NetworkListener.Listen(entity, client);
                            break;
                    }
                }

                client.Send(new SpawnEndMessage());
                EventManager.Invoke(new ServerReady(client));
            }

            private static void EntityMessage(NetworkClient client, EntityMessage message)
            {
                if (!spawns.TryGetValue(message.objectId, out var entity))
                {
                    Log.Warn("无法为客户端 {0} 同步网络对象: {1}", client.clientId, message.objectId);
                    return;
                }

                if (!entity)
                {
                    Log.Warn("无法为客户端 {0} 同步网络对象: {1}", client.clientId, message.objectId);
                    return;
                }

                if (entity.client != client)
                {
                    Log.Warn("无法为客户端 {0} 同步网络对象: {1}", client.clientId, message.objectId);
                    return;
                }

                using var reader = MemoryReader.Pop(message.segment);
                if (!entity.ServerDeserialize(reader))
                {
                    Log.Warn("无法为客户端 {0} 反序列化网络对象: {1}", client.clientId, message.objectId);
                    client.Disconnect();
                }
            }

            private static void ServerRpcMessage(NetworkClient client, ServerRpcMessage message, int channel)
            {
                if (!client.isReady)
                {
                    if (channel != Channel.Reliable) return;
                    Log.Warn("无法为客户端 {0} 进行远程调用，未准备就绪。", client.clientId);
                    return;
                }

                if (!spawns.TryGetValue(message.objectId, out var entity))
                {
                    Log.Warn("无法为客户端 {0} 进行远程调用，未找到对象 {1}。", client.clientId, message.objectId);
                    return;
                }

                if (NetworkAttribute.HasInvoke(message.methodHash) && entity.client != client)
                {
                    Log.Warn("无法为客户端 {0} 进行远程调用，未通过验证 {1}。", client.clientId, message.objectId);
                    return;
                }

                using var reader = MemoryReader.Pop(message.segment);
                entity.InvokeMessage(message.sourceId, message.methodHash, InvokeMode.ServerRpc, reader, client);
            }
        }

        public partial class Server
        {
            private static void OnServerConnect(int clientId)
            {
                if (clientId == 0)
                {
                    Transport.Instance.Disconnect(clientId);
                }
                else if (clients.ContainsKey(clientId))
                {
                    Transport.Instance.Disconnect(clientId);
                }
                else if (clients.Count >= Instance.connection)
                {
                    Transport.Instance.Disconnect(clientId);
                }
                else
                {
                    Connect(new NetworkClient(clientId));
                }
            }

            internal static void OnServerDisconnect(int clientId)
            {
                if (clients.TryGetValue(clientId, out var client))
                {
                    if (client.clientId != Host)
                    {
                        client.Disconnect();
                    }

                    var entities = spawns.Values.Where(entity => entity.client == client).ToList();
                    foreach (var entity in entities)
                    {
                        Despawn(entity.gameObject);
                    }

                    NetworkListener.Destroy(client);
                    clients.Remove(client.clientId);
                    EventManager.Invoke(new ServerDisconnect(client));
                }
            }

            internal static void OnServerReceive(int clientId, ArraySegment<byte> segment, int channel)
            {
                if (!clients.TryGetValue(clientId, out var client))
                {
                    Log.Warn("无法为客户端 {0} 进行处理消息。未知客户端。", clientId);
                    return;
                }

                if (!client.reader.AddBatch(segment))
                {
                    Log.Warn("无法为客户端 {0} 进行处理消息。", clientId);
                    client.Disconnect();
                    return;
                }

                while (!isLoadScene && client.reader.GetMessage(out var result))
                {
                    using var reader = MemoryReader.Pop(result);
                    if (reader.buffer.Count - reader.position < sizeof(ushort))
                    {
                        Log.Warn("无法为客户端 {0} 进行处理消息。没有头部。", clientId);
                        client.Disconnect();
                        return;
                    }

                    var message = reader.ReadUShort();
                    if (!NetworkMessage.server.TryGetValue(message, out var action))
                    {
                        Log.Warn("无法为客户端 {0} 进行处理消息。未知的消息 {1}。", clientId, message);
                        client.Disconnect();
                        return;
                    }

                    action.Invoke(client, reader, channel);
                }

                if (!isLoadScene && client.reader.Count > 0)
                {
                    Log.Warn("无法为客户端 {0} 进行处理消息。残留消息: {1}。", clientId, client.reader.Count);
                }
            }
        }

        public partial class Server
        {
            public static void Spawn(GameObject obj, NetworkClient client = null)
            {
                if (!isServer)
                {
                    Log.Warn("服务器不是活跃的。");
                    return;
                }

                if (!obj.TryGetComponent(out NetworkEntity entity))
                {
                    Log.Error("网络对象 {0} 没有 NetworkEntity 组件", entity);
                    return;
                }

                if (spawns.ContainsKey(entity.objectId))
                {
                    Log.Warn("网络对象 {0} 已经生成。", entity);
                    return;
                }

                entity.client = client;
                entity.mode = client != null && client.clientId == Host ? entity.mode | EntityMode.Owner : entity.mode & ~EntityMode.Owner;
                entity.mode = isServer ? entity.mode | EntityMode.Server : entity.mode & ~EntityMode.Server;
                entity.mode = isClient ? entity.mode | EntityMode.Client : entity.mode & ~EntityMode.Client;
                if (entity.objectId == 0)
                {
                    entity.objectId = ++objectId;
                    spawns[entity.objectId] = entity;
                    entity.OnStartServer();
                }

                if (observer && entity.visible != Visible.Show)
                {
                    observer.Rebuild(entity, true);
                    return;
                }

                if (entity.visible == Visible.Hide)
                {
                    if (client != null)
                    {
                        NetworkListener.Listen(entity, client);
                    }

                    return;
                }

                foreach (var result in clients.Values)
                {
                    if (result.isReady)
                    {
                        NetworkListener.Listen(entity, result);
                    }
                }
            }

            internal static void SpawnMessage(NetworkEntity entity, NetworkClient client)
            {
                using var owner = MemoryWriter.Pop();
                using var other = MemoryWriter.Pop();
                ArraySegment<byte> segment = default;
                if (entity.modules.Count > 0)
                {
                    entity.ServerSerialize(true, owner, other);
                    segment = entity.client == client ? owner : other;
                }

                byte opcode = 0;
                opcode = (byte)(entity.client == client ? opcode | 1 : opcode & ~1);
                opcode = (byte)(entity.visible == Visible.Pool ? opcode | 2 : opcode & ~2);
                var message = new SpawnMessage
                {
                    opcode = opcode,
                    assetId = entity.assetId,
                    sceneId = entity.sceneId,
                    objectId = entity.objectId,
                    position = entity.transform.localPosition,
                    rotation = entity.transform.localRotation,
                    localScale = entity.transform.localScale,
                    segment = segment
                };
                client.Send(message);
            }

            public static void Despawn(GameObject obj)
            {
                if (obj.TryGetComponent(out NetworkEntity entity))
                {
                    foreach (var client in NetworkListener.Query(entity))
                    {
                        client.Send(new DespawnMessage(entity.objectId));
                    }

                    entity.OnStopServer();
                    NetworkSpawner.Despawn(entity);
                    spawns.Remove(entity.objectId);
                }
            }
        }

        public partial class Server
        {
            internal static void EarlyUpdate()
            {
                if (Transport.Instance)
                {
                    Transport.Instance.ServerEarlyUpdate();
                }
            }

            internal static void AfterUpdate()
            {
                if (isServer)
                {
                    if (NetworkSystem.Tick(Instance.sendRate, ref sendTime))
                    {
                        Broadcast();
                    }
                }

                if (Transport.Instance)
                {
                    Transport.Instance.ServerAfterUpdate();
                }
            }

            private static void Broadcast()
            {
                copies.Clear();
                copies.AddRange(clients.Values);
                foreach (var client in copies)
                {
                    if (client.isReady)
                    {
                        foreach (var entity in NetworkListener.Query(client))
                        {
                            if (!entity)
                            {
                                Log.Warn("在客户端 {0} 找到了空的网络对象。", client.clientId);
                                continue;
                            }

                            entity.Synchronization(Time.frameCount);
                            if (entity.client == client)
                            {
                                if (entity.owner.position > 0)
                                {
                                    client.Send(new EntityMessage(entity.objectId, entity.owner));
                                }
                            }
                            else
                            {
                                if (entity.other.position > 0)
                                {
                                    client.Send(new EntityMessage(entity.objectId, entity.other));
                                }
                            }
                        }
                    }

                    client.Update();
                }
            }
        }
    }
}