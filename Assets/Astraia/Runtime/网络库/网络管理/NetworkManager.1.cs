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
using Astraia.Core;
using UnityEngine;

namespace Astraia.Net
{
    public partial class NetworkManager
    {
        public static partial class Server
        {
            internal static readonly Dictionary<uint, NetworkEntity> spawns = new Dictionary<uint, NetworkEntity>();

            public static readonly Dictionary<int, NetworkClient> clients = new Dictionary<int, NetworkClient>();

            private static readonly List<NetworkClient> copies = new List<NetworkClient>();

            internal static State state = State.断开连接;

            private static bool isLoadScene;

            private static uint objectId;

            private static double sendTime;
            private static bool isReady => clients.Values.All(connection => connection.isReady);
            public static int connections => clients.Count;

            internal static void Start(bool isHost)
            {
                if (isHost)
                {
                    kcp.StartServer();
                }

                state = State.连接成功;
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

                state = State.断开连接;
                kcp.StopServer();
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
                            client.Clear();
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
                kcp.sEvent.Connect -= Connect;
                kcp.sEvent.Disconnect -= Disconnect;
                kcp.sEvent.Receive -= Receive;
                kcp.sEvent.Connect += Connect;
                kcp.sEvent.Disconnect += Disconnect;
                kcp.sEvent.Receive += Receive;
                NetworkMessage<PongMessage>.Add(PongMessage);
                NetworkMessage<ReadyMessage>.Add(ReadyMessage);
                NetworkMessage<EntityMessage>.Add(EntityMessage);
                NetworkMessage<ServerRpcMessage>.Add(ServerRpcMessage);
            }

            private static void PongMessage(NetworkClient client, PongMessage message)
            {
                client.Send(new PingMessage(message.clientTime), Pass.UDP);
            }

            private static void ReadyMessage(NetworkClient client, ReadyMessage message)
            {
                client.isReady = true;
                client.Send(new SpawnBeginMessage());
                EventManager.Invoke(new ServerReady(client, isReady));

                if (NetworkObserver.Instance != null)
                {
                    NetworkObserver.Instance.Clear();
                }

                foreach (var entity in spawns.Values)
                {
                    if (NetworkObserver.Instance != null && !entity.visible)
                    {
                        NetworkObserver.Instance.Add(entity);
                        NetworkObserver.Instance.Tick(entity, client);
                    }
                    else
                    {
                        entity.Add(client);
                    }
                }
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
                if (!entity.modules.ServerReceive(reader))
                {
                    Log.Warn("无法为客户端 {0} 反序列化网络对象: {1}", client.clientId, message.objectId);
                    client.Disconnect();
                }
            }

            private static void ServerRpcMessage(NetworkClient client, ServerRpcMessage message, int pass)
            {
                if (!client.isReady)
                {
                    if (pass != Pass.KCP) return;
                    Log.Warn("无法为客户端 {0} 进行远程调用，未准备就绪。", client.clientId);
                    return;
                }

                if (!spawns.TryGetValue(message.objectId, out var entity))
                {
                    Log.Warn("无法为客户端 {0} 进行远程调用，未找到对象 {1}。", client.clientId, message.objectId);
                    return;
                }

                if (NetworkAttribute.HasHook(message.methodId) && entity.client != client)
                {
                    Log.Warn("无法为客户端 {0} 进行远程调用，对象无权限 {1}。", client.clientId, message.objectId);
                    return;
                }

                using var reader = MemoryReader.Pop(message.segment);
                entity.InvokeMessage(message.moduleId, message.methodId, HookMode.服务器, reader, client);
            }
        }

        public partial class Server
        {
            internal static void Connect(int id)
            {
                if (clients.Count >= Instance.maxPlayer)
                {
                    kcp.Disconnect(id);
                }
                else if (clients.ContainsKey(id))
                {
                    kcp.Disconnect(id);
                }
                else
                {
                    var client = new NetworkClient(id);
                    clients.Add(id, client);
                    EventManager.Invoke(new ServerConnect(client));
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

                    client.Clear();
                    EventManager.Invoke(new ServerDisconnect(client));
                }
            }

            internal static void Receive(int id, ArraySegment<byte> segment, int pass)
            {
                if (!clients.TryGetValue(id, out var client))
                {
                    Log.Warn("无法为客户端 {0} 进行处理消息。未知客户端。", id);
                    return;
                }

                if (!client.reader.AddBatch(segment))
                {
                    Log.Warn("无法为客户端 {0} 进行处理消息。", id);
                    client.Disconnect();
                    return;
                }

                while (!isLoadScene && client.reader.GetMessage(out var result))
                {
                    using var reader = MemoryReader.Pop(result);
                    if (reader.buffer.Count - reader.position < sizeof(ushort))
                    {
                        Log.Warn("无法为客户端 {0} 进行处理消息。没有头部。", id);
                        client.Disconnect();
                        return;
                    }

                    var message = reader.ReadUInt16();
                    if (!NetworkMessage.server.TryGetValue(message, out var onMessage))
                    {
                        Log.Warn("无法为客户端 {0} 进行处理消息。未知的消息 {1}。", id, message);
                        client.Disconnect();
                        return;
                    }

                    onMessage.Invoke(client, reader, pass);
                }

                if (!isLoadScene && client.reader.Count > 0)
                {
                    Log.Warn("无法为客户端 {0} 进行处理消息。残留消息: {1}。", id, client.reader.Count);
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
                entity.state = client?.clientId == 0 ? entity.state | NetworkEntity.State.所有者 : entity.state & ~NetworkEntity.State.所有者;
                entity.state = isServer ? entity.state | NetworkEntity.State.服务器 : entity.state & ~NetworkEntity.State.服务器;
                entity.state = isClient ? entity.state | NetworkEntity.State.客户端 : entity.state & ~NetworkEntity.State.客户端;
                if (entity.objectId == 0)
                {
                    entity.objectId = ++objectId;
                    spawns[entity.objectId] = entity;
                    entity.OnStartServer();
                }

                if (NetworkObserver.Instance != null && !entity.visible)
                {
                    NetworkObserver.Instance.Add(entity);
                    NetworkObserver.Instance.Tick(entity);
                }
                else
                {
                    foreach (var result in clients.Values)
                    {
                        if (result.isReady)
                        {
                            entity.Add(result);
                        }
                    }
                }
            }

            public static void Destroy(GameObject obj)
            {
                if (obj.TryGetComponent(out NetworkEntity entity))
                {
                    spawns.Remove(entity.objectId);
                    foreach (var client in entity.Clients())
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
                        if (NetworkObserver.Instance != null && !entity.visible)
                        {
                            NetworkObserver.Instance.Remove(entity);
                        }

                        entity.state |= NetworkEntity.State.销毁;
                        UnityEngine.Object.Destroy(entity.gameObject);
                    }
                }
            }

            internal static void EarlyUpdate()
            {
                kcp?.ServerEarlyUpdate();
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
                            foreach (var entity in client.Entities())
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

                kcp?.ServerAfterUpdate();
            }
        }
    }
}