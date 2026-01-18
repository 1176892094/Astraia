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
    public partial class NetworkManager
    {
        public static partial class Server
        {
            internal static readonly Dictionary<int, NetworkClient> clients = new Dictionary<int, NetworkClient>();

            internal static readonly Dictionary<uint, NetworkEntity> spawns = new Dictionary<uint, NetworkEntity>();

            private static readonly List<NetworkClient> copies = new List<NetworkClient>();

            internal static State state = State.Disconnect;

            private static bool isLoadScene;

            private static uint objectId;

            private static double sendTime;

            internal static int connection = byte.MaxValue;

            internal static void Start(bool isHost)
            {
                if (isHost)
                {
                    Instance.StartServer();
                }

                state = State.Connected;
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

                state = State.Disconnect;
                Instance.StopServer();
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
                            client.isReady = false;
                            client.ClearObserver();
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
                var entities = Object.FindObjectsByType<NetworkEntity>(FindObjectsSortMode.None);
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
                Instance.OnServerConnect -= Connect;
                Instance.OnServerDisconnect -= Disconnect;
                Instance.OnServerReceive -= Receive;
                Instance.OnServerConnect += Connect;
                Instance.OnServerDisconnect += Disconnect;
                Instance.OnServerReceive += Receive;
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
                foreach (var entity in spawns.Values)
                {
                    if (entity.isActiveAndEnabled)
                    {
                        if (entity.visible == Visible.Observer)
                        {
                            NetworkObserver.Instance.Tick(entity, client);
                        }
                        else
                        {
                            entity.AddObserver(client);
                        }
                    }
                }

                EventManager.Invoke(new ServerReady(client));
                if (clients.Values.All(connection => connection.isReady))
                {
                    EventManager.Invoke(new ServerComplete());
                }
            }

            private static void EntityMessage(NetworkClient client, EntityMessage message)
            {
                if (!spawns.TryGetValue(message.objectId, out var entity))
                {
                    Service.Log.Warn("无法为客户端 {0} 同步网络对象: {1}", client.clientId, message.objectId);
                    return;
                }

                if (!entity)
                {
                    Service.Log.Warn("无法为客户端 {0} 同步网络对象: {1}", client.clientId, message.objectId);
                    return;
                }

                if (entity.client != client)
                {
                    Service.Log.Warn("无法为客户端 {0} 同步网络对象: {1}", client.clientId, message.objectId);
                    return;
                }

                using var reader = MemoryReader.Pop(message.segment);
                if (!NetworkSerialize.ServerDeserialize(entity.modules, reader))
                {
                    Service.Log.Warn("无法为客户端 {0} 反序列化网络对象: {1}", client.clientId, message.objectId);
                    client.Disconnect();
                }
            }

            private static void ServerRpcMessage(NetworkClient client, ServerRpcMessage message, int channel)
            {
                if (!client.isReady)
                {
                    if (channel != Channel.Reliable) return;
                    Service.Log.Warn("无法为客户端 {0} 进行远程调用，未准备就绪。", client.clientId);
                    return;
                }

                if (!spawns.TryGetValue(message.objectId, out var entity))
                {
                    Service.Log.Warn("无法为客户端 {0} 进行远程调用，未找到对象 {1}。", client.clientId, message.objectId);
                    return;
                }

                if (NetworkAttribute.HasInvoke(message.methodHash) && entity.client != client)
                {
                    Service.Log.Warn("无法为客户端 {0} 进行远程调用，未通过验证 {1}。", client.clientId, message.objectId);
                    return;
                }

                using var reader = MemoryReader.Pop(message.segment);
                entity.InvokeMessage(message.moduleId, message.methodHash, InvokeMode.ServerRpc, reader, client);
            }
        }

        public partial class Server
        {
            internal static void Connect(int id)
            {
                if (clients.Count >= connection)
                {
                    Instance.Disconnect(id);
                }
                else if (clients.ContainsKey(id))
                {
                    Instance.Disconnect(id);
                }
                else
                {
                    clients.Add(id, new NetworkClient(id));
                    EventManager.Invoke(new ServerConnect(id));
                }
            }

            internal static void Disconnect(int id)
            {
                if (clients.Remove(id, out var client))
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

                    client.ClearObserver();
                    EventManager.Invoke(new ServerDisconnect(id));
                }
            }

            internal static void Receive(int id, ArraySegment<byte> segment, int channel)
            {
                if (!clients.TryGetValue(id, out var client))
                {
                    Service.Log.Warn("无法为客户端 {0} 进行处理消息。未知客户端。", id);
                    return;
                }

                if (!client.reader.AddPacket(segment))
                {
                    Service.Log.Warn("无法为客户端 {0} 进行处理消息。", id);
                    client.Disconnect();
                    return;
                }

                while (!isLoadScene && client.reader.GetMessage(out var result))
                {
                    using var reader = MemoryReader.Pop(result);
                    if (reader.buffer.Count - reader.position < sizeof(ushort))
                    {
                        Service.Log.Warn("无法为客户端 {0} 进行处理消息。没有头部。", id);
                        client.Disconnect();
                        return;
                    }

                    var message = reader.ReadUShort();
                    if (!NetworkMessage.server.TryGetValue(message, out var onMessage))
                    {
                        Service.Log.Warn("无法为客户端 {0} 进行处理消息。未知的消息 {1}。", id, message);
                        client.Disconnect();
                        return;
                    }

                    onMessage.Invoke(client, reader, channel);
                }

                if (!isLoadScene && client.reader.Count > 0)
                {
                    Service.Log.Warn("无法为客户端 {0} 进行处理消息。残留消息: {1}。", id, client.reader.Count);
                }
            }
        }

        public partial class Server
        {
            public static void Spawn(GameObject obj, NetworkClient client = null)
            {
                if (!isServer)
                {
                    Service.Log.Warn("服务器不是活跃的。");
                    return;
                }

                if (!obj.TryGetComponent(out NetworkEntity entity))
                {
                    Service.Log.Error("网络对象 {0} 没有 NetworkEntity 组件", entity);
                    return;
                }

                if (spawns.ContainsKey(entity.objectId))
                {
                    Service.Log.Warn("网络对象 {0} 已经生成。", entity);
                    return;
                }

                entity.client = client;
                entity.label = client?.clientId == 0 ? entity.label | NetworkEntity.Label.Owner : entity.label & ~NetworkEntity.Label.Owner;
                entity.label = isServer ? entity.label | NetworkEntity.Label.Server : entity.label & ~NetworkEntity.Label.Server;
                entity.label = isClient ? entity.label | NetworkEntity.Label.Client : entity.label & ~NetworkEntity.Label.Client;
                if (entity.objectId == 0)
                {
                    entity.objectId = ++objectId;
                    spawns[entity.objectId] = entity;
                    entity.OnStartServer();
                }

                if (entity.visible == Visible.Observer)
                {
                    NetworkObserver.Instance.Tick(entity);
                }
                else
                {
                    foreach (var result in clients.Values)
                    {
                        if (result.isReady)
                        {
                            entity.AddObserver(result);
                        }
                    }
                }
            }

            internal static void SpawnMessage(NetworkEntity entity, NetworkClient client)
            {
                using var owner = MemoryWriter.Pop();
                using var agent = MemoryWriter.Pop();

                if (entity.modules.Length > 0)
                {
                    NetworkSerialize.ServerSerialize(entity.modules, owner, agent, true);
                }

                var message = new SpawnMessage
                {
                    isOwner = entity.client == client,
                    assetId = entity.assetId,
                    sceneId = entity.sceneId,
                    objectId = entity.objectId,
                    position = entity.transform.localPosition,
                    rotation = entity.transform.localRotation,
                    localScale = entity.transform.localScale,
                    segment = entity.modules.Length > 0 ? entity.client == client ? owner : agent : null,
                };
                client.Send(message);
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
                        entity.state |= NetworkEntity.State.Destroy;
                        Object.Destroy(entity.gameObject);
                    }
                }
            }

            internal static void EarlyUpdate()
            {
            Instance?.ServerEarlyUpdate();
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
                                        entity.agent.position = 0;
                                        NetworkSerialize.ServerSerialize(entity.modules, entity.owner, entity.agent);
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
                                        if (entity.agent.position > 0)
                                        {
                                            client.Send(new EntityMessage(entity.objectId, entity.agent));
                                        }
                                    }
                                }
                            }
                        }

                        client.Update();
                    }
                }

                Instance?.ServerAfterUpdate();
            }
        }
    }
}