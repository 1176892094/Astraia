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
using Object = UnityEngine.Object;

namespace Astraia.Net
{
    using MessageDelegate = Action<NetworkClient, MemoryReader, int>;

    public partial class NetworkManager
    {
        [Serializable]
        public static partial class Server
        {
            private static readonly Dictionary<ushort, MessageDelegate> messages = new Dictionary<ushort, MessageDelegate>();

            internal static readonly Dictionary<uint, NetworkEntity> spawns = new Dictionary<uint, NetworkEntity>();

            public static readonly Dictionary<int, NetworkClient> clients = new Dictionary<int, NetworkClient>();

            private static State state = State.Disconnect;

            private static List<NetworkClient> copies = new List<NetworkClient>();

            private static uint objectId;

            private static double sendTime;

            public static bool isActive => state != State.Disconnect;

            public static bool isReady => clients.Values.All(client => client.isReady);

            public static bool isLoadScene { get; internal set; }

            public static int connections => clients.Count;

            internal static void Start(EntryMode mode)
            {
                if (mode is EntryMode.Host or EntryMode.Server)
                {
                    Transport.Instance.StartServer();
                }

                state = State.Connected;
                AddMessage();
                SpawnObjects();
            }

            internal static void Stop()
            {
                if (!isActive) return;
                state = State.Disconnect;
                copies = clients.Values.ToList();
                foreach (var client in copies)
                {
                    client.Disconnect();
                    if (client.clientId != Host)
                    {
                        OnServerDisconnect(client.clientId);
                    }
                }

                if (Transport.Instance)
                {
                    Transport.Instance.StopServer();
                }

                sendTime = 0;
                objectId = 0;
                spawns.Clear();
                clients.Clear();
                messages.Clear();
                isLoadScene = false;
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
                    client.Send(new NotReadyMessage());
                }

                EventManager.Invoke(new ServerChangeScene(sceneName));
                if (!isActive) return;
                isLoadScene = true;
                Instance.sceneName = sceneName;

                foreach (var client in clients.Values)
                {
                    client.Send(new SceneMessage(sceneName));
                }

                AssetManager.LoadScene(sceneName);
            }

            internal static void LoadSceneComplete(string sceneName)
            {
                isLoadScene = false;
                SpawnObjects();
                EventManager.Invoke(new ServerSceneChanged(sceneName));
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
                AddMessage<PongMessage>(PongMessage);
                AddMessage<ReadyMessage>(ReadyMessage);
                AddMessage<EntityMessage>(EntityMessage);
                AddMessage<ServerRpcMessage>(ServerRpcMessage);
            }

            public static void AddMessage<T>(Action<NetworkClient, T> onReceive) where T : struct, IMessage
            {
                messages[NetworkMessage<T>.Id] = (client, reader, channel) =>
                {
                    try
                    {
                        var position = reader.position;
                        var message = reader.Invoke<T>();
                        NetworkDebugger.OnReceive(message, reader.position - position);
                        onReceive.Invoke(client, message);
                    }
                    catch (Exception e)
                    {
                        Log.Error("{0} 调用失败。传输通道: {1}\n{2}", typeof(T).Name, channel, e);
                        client.Disconnect();
                    }
                };
            }

            public static void AddMessage<T>(Action<NetworkClient, T, int> onReceive) where T : struct, IMessage
            {
                messages[NetworkMessage<T>.Id] = (client, reader, channel) =>
                {
                    try
                    {
                        var position = reader.position;
                        var message = reader.Invoke<T>();
                        NetworkDebugger.OnReceive(message, reader.position - position);
                        onReceive.Invoke(client, message, channel);
                    }
                    catch (Exception e)
                    {
                        Log.Error("{0} 调用失败。传输通道: {1}\n{2}", typeof(T).Name, channel, e);
                        client.Disconnect();
                    }
                };
            }

            internal static void PongMessage(NetworkClient client, PongMessage message)
            {
                client.Send(new PingMessage(message.clientTime), Channel.Unreliable);
            }

            internal static void ReadyMessage(NetworkClient client, ReadyMessage message)
            {
                client.isReady = true;
                foreach (var entity in spawns.Values.Where(entity => entity.gameObject.activeSelf))
                {
                    SpawnToClient(client, entity);
                }

                EventManager.Invoke(new ServerReady(client));
            }

            internal static void EntityMessage(NetworkClient client, EntityMessage message)
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

            internal static void ServerRpcMessage(NetworkClient client, ServerRpcMessage message, int channel)
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

                if (NetworkAttribute.RequireReady(message.methodHash) && entity.client != client)
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
                    Log.Warn("无法为客户端 {0} 建立连接服务。", clientId);
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
                    var entities = new List<NetworkEntity>();
                    foreach (var entity in spawns.Values)
                    {
                        if (entity.client == client)
                        {
                            entities.Add(entity);
                        }
                    }

                    foreach (var entity in entities)
                    {
                        Object.Destroy(entity);
                    }

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

                    if (!messages.TryGetValue(message, out var action))
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
            internal static void SpawnObjects()
            {
                var entities = FindObjectsByType<NetworkEntity>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                foreach (var entity in entities)
                {
                    if (entity.sceneId != 0 && entity.objectId == 0)
                    {
                        entity.gameObject.SetActive(true);
                        var parent = entity.transform.parent;
                        if (parent == null || parent.gameObject.activeInHierarchy)
                        {
                            Spawn(entity.gameObject, entity.client);
                        }
                    }
                }
            }

            public static void Spawn(GameObject gameObject, NetworkClient client = null)
            {
                if (!isActive)
                {
                    Log.Warn("服务器不是活跃的。");
                    return;
                }

                if (!gameObject.TryGetComponent(out NetworkEntity entity))
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

                if (client != null && Mode == EntryMode.Host && client.clientId == Host)
                {
                    entity.mode |= EntityMode.Owner;
                }

                if (!entity.isServer && entity.objectId == 0)
                {
                    entity.objectId = ++objectId;
                    entity.mode |= EntityMode.Server;
                    entity.mode = Client.isActive ? entity.mode | EntityMode.Client : entity.mode & ~EntityMode.Owner;
                    spawns[entity.objectId] = entity;
                    entity.OnStartServer();
                }

                SpawnToClients(entity);
            }

            private static void SpawnToClients(NetworkEntity entity)
            {
                foreach (var client in clients.Values.Where(client => client.isReady))
                {
                    SpawnToClient(client, entity);
                }
            }

            private static void SpawnToClient(NetworkClient client, NetworkEntity entity)
            {
                using MemoryWriter owner = MemoryWriter.Pop(), other = MemoryWriter.Pop();
                var isOwner = entity.client == client;
                var transform = entity.transform;
                ArraySegment<byte> segment = default;
                if (entity.modules.Count != 0)
                {
                    entity.ServerSerialize(true, owner, other);
                    segment = isOwner ? owner : other;
                }

                var message = new SpawnMessage
                {
                    isOwner = isOwner,
                    assetId = entity.assetId,
                    sceneId = entity.sceneId,
                    objectId = entity.objectId,
                    position = transform.localPosition,
                    rotation = transform.localRotation,
                    localScale = transform.localScale,
                    segment = segment
                };

                client.Send(message);
            }

            public static void Despawn(GameObject gameObject)
            {
                if (!gameObject.TryGetComponent(out NetworkEntity entity))
                {
                    return;
                }

                spawns.Remove(entity.objectId);
                foreach (var client in clients.Values)
                {
                    client.Send(new DespawnMessage(entity.objectId));
                }

                entity.OnStopServer();
                entity.gameObject.SetActive(false);
                entity.Reset();
            }

            public static void Destroy(GameObject gameObject)
            {
                if (!gameObject.TryGetComponent(out NetworkEntity entity))
                {
                    return;
                }

                spawns.Remove(entity.objectId);
                foreach (var client in clients.Values)
                {
                    client.Send(new DestroyMessage(entity.objectId));
                }

                entity.OnStopServer();
                entity.state |= EntityState.Destroy;
                Object.Destroy(entity.gameObject);
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
                if (isActive)
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
                        foreach (var entity in spawns.Values)
                        {
                            if (entity == null)
                            {
                                Log.Warn("在客户端 {0} 找到了空的网络对象。", client.clientId);
                                return;
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