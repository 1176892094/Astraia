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
using UnityEngine;
using State = Astraia.Async.State;

namespace Astraia.Net
{
    public partial class NetworkManager
    {
        public static partial class Server
        {
            internal static readonly Dictionary<uint, NetworkEntity> spawns = new Dictionary<uint, NetworkEntity>();

            public static readonly Dictionary<int, NetworkClient> clients = new Dictionary<int, NetworkClient>();

            private static readonly List<NetworkClient> copies = new List<NetworkClient>();

            internal static State state = State.Failure;

            private static bool isLoadScene;

            private static uint objectId;

            private static double sendTime;

            private static bool isObserver => NetworkObserving.Instance != null;

            public static bool isReady => clients.Values.All(connection => connection.isReady);
            public static int connections => clients.Count;

            internal static void Start(bool isHost)
            {
                if (isHost)
                {
                    Kcp.StartServer();
                }

                state = State.Success;
                AddMessage();
                SpawnObjects();
            }

            internal static void Stop()
            {
                copies.Clear();
                copies.AddRange(clients.Values);
                foreach (var client in copies)
                {
                    Disconnect(client.clientId);
                }

                state = State.Failure;
                Kcp.StopServer();
                sendTime = 0;
                objectId = 0;
                spawns.Clear();
                clients.Clear();
                isLoadScene = false;
            }

            public static void Load(string sceneName)
            {
                if (!isLoadScene)
                {
                    EventManager.Invoke(new ServerLoadScene(sceneName));
                    if (isServer)
                    {
                        isLoadScene = true;
                        foreach (var client in clients.Values)
                        {
                            NetworkSpawner.Clear(client);
                            client.isReady = false;
                            client.Send(new SceneMessage(sceneName));
                        }

                        AssetManager.LoadScene(sceneName);
                    }
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
                NetworkObserving.Instance?.Dispose();
#if UNITY_6000_4_OR_NEWER
                var entities = FindObjectsByType<NetworkEntity>();
#else
                var entities = FindObjectsByType<NetworkEntity>(FindObjectsSortMode.None);
#endif
                foreach (var entity in entities)
                {
                    if (entity.sceneId != 0 && entity.objectId == 0)
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
                Kcp.server.onConnect -= Connect;
                Kcp.server.onDisconnect -= Disconnect;
                Kcp.server.onReceive -= Receive;
                Kcp.server.onConnect += Connect;
                Kcp.server.onDisconnect += Disconnect;
                Kcp.server.onReceive += Receive;
                NetworkMessage<PongMessage>.Add<NetworkClient>(PongMessage);
                NetworkMessage<ReadyMessage>.Add<NetworkClient>(ReadyMessage);
                NetworkMessage<EntityMessage>.Add<NetworkClient>(EntityMessage);
                NetworkMessage<ServerRpcMessage>.Add<NetworkClient>(ServerRpcMessage);
            }

            private static void PongMessage(NetworkClient client, PongMessage message)
            {
                client.Send(new PingMessage(message.clientTime), Pass.UDP);
            }

            private static void ReadyMessage(NetworkClient client, ReadyMessage message)
            {
                client.isReady = true;
                client.Send(new SpawnBeginMessage());
                EventManager.Invoke(new ServerReady(client));

                foreach (var entity in spawns.Values)
                {
                    if (isObserver && (entity.state & Entity.VISIBLE) == 0)
                    {
                        NetworkObserving.Instance.Add(entity);
                        NetworkObserving.Instance.Tick(entity, client);
                    }
                    else
                    {
                        NetworkSpawner.Add(entity, client);
                    }
                }
            }

            private static void EntityMessage(NetworkClient client, EntityMessage message)
            {
                if (!spawns.TryGetValue(message.objectId, out var entity))
                {
                    Log.Warn($"无法为客户端 {client.clientId} 同步网络对象: {message.objectId}");
                    return;
                }

                if (!entity)
                {
                    Log.Warn($"无法为客户端 {client.clientId} 同步网络对象: {message.objectId}");
                    return;
                }

                if (entity.client != client)
                {
                    Log.Warn($"无法为客户端 {client.clientId} 同步网络对象: {message.objectId}");
                    return;
                }

                using var reader = MemoryReader.Pop(message.segment);
                if (!entity.modules.ServerReceive(reader))
                {
                    Log.Warn($"无法为客户端 {client.clientId} 反序列化网络对象: {message.objectId}");
                    client.Disconnect();
                }
            }

            private static void ServerRpcMessage(NetworkClient client, ServerRpcMessage message, int pass)
            {
                if (!client.isReady)
                {
                    if (pass != Pass.KCP) return;
                    Log.Warn($"无法为客户端 {client.clientId} 进行远程调用，未准备就绪。");
                    return;
                }

                if (!spawns.TryGetValue(message.objectId, out var entity))
                {
                    Log.Warn($"无法为客户端 {client.clientId} 进行远程调用，未找到对象 {message.objectId}。");
                    return;
                }

                if (NetworkAttribute.HasHook(message.methodId) && entity.client != client)
                {
                    Log.Warn($"无法为客户端 {client.clientId} 进行远程调用，对象无权限 {message.objectId}。");
                    return;
                }

                using var reader = MemoryReader.Pop(message.segment);
                entity.InvokeMessage(message.moduleId, message.methodId, SyncMode.服务器, reader, client);
            }
        }

        public partial class Server
        {
            internal static void Connect(int id)
            {
                if (clients.Count >= Instance.maxPlayer)
                {
                    Kcp.Disconnect(id);
                }
                else if (clients.ContainsKey(id))
                {
                    Kcp.Disconnect(id);
                }
                else
                {
                    clients.Add(id, new NetworkClient { clientId = id });
                    EventManager.Invoke(new ServerConnect(id));
                }
            }

            internal static void Disconnect(int id)
            {
                if (clients.TryGetValue(id, out var client))
                {
                    if (id != 0)
                    {
                        client.Disconnect();
                    }

                    var entities = spawns.Values.Where(entity => entity).ToList();
                    foreach (var entity in entities)
                    {
                        if (client.clientId == 0)
                        {
                            Destroy(entity.gameObject);
                        }
                        else if (entity.client == client)
                        {
                            Destroy(entity.gameObject);
                        }
                    }

                    NetworkSpawner.Clear(client);
                    EventManager.Invoke(new ServerDisconnect(id));
                    clients.Remove(id);
                }
            }

            internal static void Receive(int id, ArraySegment<byte> segment, int pass)
            {
                if (!clients.TryGetValue(id, out var client))
                {
                    Log.Warn($"无法为客户端 {id} 进行处理消息。未知客户端。");
                    return;
                }

                if (!client.AddBatch(segment))
                {
                    Log.Warn($"无法为客户端 {id} 进行处理消息。");
                    client.Disconnect();
                    return;
                }

                while (!isLoadScene && client.GetMessage(out var result))
                {
                    using var reader = MemoryReader.Pop(result);
                    if (reader.buffer.Count - reader.position < sizeof(ushort))
                    {
                        Log.Warn($"无法为客户端 {id} 进行处理消息。没有头部。");
                        client.Disconnect();
                        return;
                    }

                    var message = reader.ReadUInt16();
                    if (!NetworkMessage.GetValueByServer(message, out var onMessage))
                    {
                        Log.Warn($"无法为客户端 {id} 进行处理消息。未知的消息 {message}。");
                        client.Disconnect();
                        return;
                    }

                    onMessage.Invoke(client, reader, pass);
                }

                if (!isLoadScene && client.Count > 0)
                {
                    Log.Warn($"无法为客户端 {id} 进行处理消息。残留消息: {client.Count}。");
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
                    Log.Error($"网络对象 {entity} 没有 NetworkEntity 组件");
                    return;
                }

                if (spawns.ContainsKey(entity.objectId))
                {
                    Log.Warn($"网络对象 {entity} 已经生成。");
                    return;
                }

                entity.client = client;
                entity.state = client?.clientId == 0 ? entity.state | Entity.OWNING : entity.state & ~Entity.OWNING;
                entity.state = isServer ? entity.state | Entity.SERVER : entity.state & ~Entity.SERVER;
                entity.state = isClient ? entity.state | Entity.CLIENT : entity.state & ~Entity.CLIENT;
                if (entity.objectId == 0)
                {
                    entity.objectId = ++objectId;
                    spawns[entity.objectId] = entity;
                    entity.OnStartServer();
                }

                if (isObserver && (entity.state & Entity.VISIBLE) == 0)
                {
                    NetworkObserving.Instance.Add(entity);
                    NetworkObserving.Instance.Tick(entity);
                }
                else
                {
                    foreach (var result in clients.Values)
                    {
                        if (result.isReady)
                        {
                            NetworkSpawner.Add(entity, result);
                        }
                    }
                }
            }

            public static void Destroy(GameObject obj)
            {
                if (obj.TryGetComponent(out NetworkEntity entity))
                {
                    spawns.Remove(entity.objectId);
                    foreach (var client in entity.clients)
                    {
                        client.Send(new DestroyMessage(entity.objectId));
                    }

                    entity.OnStopServer();
                    if (entity.sceneId != 0)
                    {
                        entity.gameObject.SetActive(false);
                        entity.Reset();
                    }
                    else
                    {
                        if (isObserver && (entity.state & Entity.VISIBLE) == 0)
                        {
                            NetworkObserving.Instance.Remove(entity);
                        }

                        entity.state |= Entity.DESTROY;
                        UnityEngine.Object.Destroy(entity.gameObject);
                    }
                }
            }

            internal static void EarlyUpdate()
            {
                Kcp?.ServerEarlyUpdate();
            }

            internal static void AfterUpdate()
            {
                if (isServer && NetworkSystem.Tick(ref sendTime))
                {
                    copies.Clear();
                    copies.AddRange(clients.Values);
                    foreach (var client in copies)
                    {
                        if (client.isReady)
                        {
                            foreach (var entity in client.entities)
                            {
                                if (entity)
                                {
                                    if (entity.count != Time.frameCount)
                                    {
                                        entity.count = Time.frameCount;
                                        entity.owner.position = 0;
                                        entity.other.position = 0;
                                        entity.modules.ServerSend(entity.owner, entity.other);
                                        entity.ClearDirty(true);
                                    }

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
                        }

                        client.Update();
                    }
                }

                Kcp?.ServerAfterUpdate();
            }
        }
    }
}