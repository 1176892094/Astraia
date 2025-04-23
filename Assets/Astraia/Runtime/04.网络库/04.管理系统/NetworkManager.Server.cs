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
    using MessageDelegate = Action<NetworkClient, MemoryGetter, int>;

    public partial class NetworkManager
    {
        public static partial class Server
        {
            private static readonly Dictionary<ushort, MessageDelegate> messages = new Dictionary<ushort, MessageDelegate>();

            internal static readonly Dictionary<int, NetworkClient> clients = new Dictionary<int, NetworkClient>();

            internal static readonly Dictionary<uint, NetworkObject> spawns = new Dictionary<uint, NetworkObject>();

            private static State state = State.Disconnect;

            private static List<NetworkClient> copies = new List<NetworkClient>();

            private static uint objectId;

            private static double sendTime;

            public static bool isActive => state != State.Disconnect;

            public static bool isReady => clients.Values.All(client => client.isReady);

            public static bool isLoadScene { get; internal set; }

            public static int hostId { get; private set; } = 0;

            public static int connections => clients.Count;

            internal static void Start(EntryMode mode)
            {
                switch (mode)
                {
                    case EntryMode.Host:
                        Transport.Instance.StartServer();
                        break;
                    case EntryMode.Server:
                        Transport.Instance.StartServer();
                        break;
                }

                Register();
                clients.Clear();
                state = State.Connected;
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
                    if (client.clientId != hostId)
                    {
                        OnServerDisconnect(client.clientId);
                    }
                }

                if (Transport.Instance != null)
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
                if (!clients.ContainsKey(client.clientId))
                {
                    clients.Add(client.clientId, client);
                    EventManager.Invoke(new ServerConnect(client));
                }
            }

            public static void Load(string sceneName)
            {
                if (string.IsNullOrWhiteSpace(sceneName))
                {
                    Debug.LogError("服务器不能加载空场景！");
                    return;
                }

                if (isLoadScene && Instance.sceneName == sceneName)
                {
                    Debug.LogError(Service.Text.Format("服务器正在加载 {0} 场景", sceneName));
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
            private static void Register()
            {
                Transport.Instance.OnServerConnect -= OnServerConnect;
                Transport.Instance.OnServerDisconnect -= OnServerDisconnect;
                Transport.Instance.OnServerReceive -= OnServerReceive;
                Transport.Instance.OnServerConnect += OnServerConnect;
                Transport.Instance.OnServerDisconnect += OnServerDisconnect;
                Transport.Instance.OnServerReceive += OnServerReceive;
                Register<PongMessage>(PongMessage);
                Register<ReadyMessage>(ReadyMessage);
                Register<EntityMessage>(EntityMessage);
                Register<ServerRpcMessage>(ServerRpcMessage);
            }

            public static void Register<T>(Action<NetworkClient, T> handle) where T : struct, IMessage
            {
                messages[Hash<T>.Id] = (client, getter, channel) =>
                {
                    try
                    {
                        var message = getter.Invoke<T>();
                        handle?.Invoke(client, message);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(Service.Text.Format("{0} 调用失败。传输通道: {1}\n{2}", typeof(T).Name, channel, e));
                        client.Disconnect();
                    }
                };
            }

            public static void Register<T>(Action<NetworkClient, T, int> handle) where T : struct, IMessage
            {
                messages[Hash<T>.Id] = (client, getter, channel) =>
                {
                    try
                    {
                        var message = getter.Invoke<T>();
                        handle?.Invoke(client, message, channel);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(Service.Text.Format("{0} 调用失败。传输通道: {1}\n{2}", typeof(T).Name, channel, e));
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
                foreach (var @object in spawns.Values.Where(@object => @object.gameObject.activeSelf))
                {
                    SpawnToClient(client, @object);
                }

                EventManager.Invoke(new ServerReady(client));
            }

            internal static void EntityMessage(NetworkClient client, EntityMessage message)
            {
                if (!spawns.TryGetValue(message.objectId, out var @object))
                {
                    Debug.LogWarning(Service.Text.Format("无法为客户端 {0} 同步网络对象: {1}", client.clientId, message.objectId));
                    return;
                }

                if (@object == null)
                {
                    Debug.LogWarning(Service.Text.Format("无法为客户端 {0} 同步网络对象: {1}", client.clientId, message.objectId));
                    return;
                }

                if (@object.connection != client)
                {
                    Debug.LogWarning(Service.Text.Format("无法为客户端 {0} 同步网络对象: {1}", client.clientId, message.objectId));
                    return;
                }

                using var getter = MemoryGetter.Pop(message.segment);
                if (!@object.ServerDeserialize(getter))
                {
                    Debug.LogWarning(Service.Text.Format("无法为客户端 {0} 反序列化网络对象: {1}", client.clientId, message.objectId));
                    client.Disconnect();
                }
            }

            internal static void ServerRpcMessage(NetworkClient client, ServerRpcMessage message, int channel)
            {
                if (!client.isReady)
                {
                    if (channel != Channel.Reliable) return;
                    Debug.LogWarning(Service.Text.Format("无法为客户端 {0} 进行远程调用，未准备就绪。", client.clientId));
                    return;
                }

                if (!spawns.TryGetValue(message.objectId, out var @object))
                {
                    Debug.LogWarning(Service.Text.Format("无法为客户端 {0} 进行远程调用，未找到对象 {1}。", client.clientId, message.objectId));
                    return;
                }

                if (NetworkAttribute.RequireReady(message.methodHash) && @object.connection != client)
                {
                    Debug.LogWarning(Service.Text.Format("无法为客户端 {0} 进行远程调用，未通过验证 {1}。", client.clientId, message.objectId));
                    return;
                }

                using var getter = MemoryGetter.Pop(message.segment);
                @object.InvokeMessage(message.componentId, message.methodHash, InvokeMode.ServerRpc, getter, client);
            }
        }

        public partial class Server
        {
            private static void OnServerConnect(int clientId)
            {
                if (clientId == 0)
                {
                    Debug.LogWarning(Service.Text.Format("无法为客户端 {0} 建立通信连接。", clientId));
                    Transport.Instance.StopClient(clientId);
                }
                else if (clients.ContainsKey(clientId))
                {
                    Transport.Instance.StopClient(clientId);
                }
                else if (clients.Count >= Instance.connection)
                {
                    Transport.Instance.StopClient(clientId);
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
                    var objects = spawns.Values.Where(@object => @object.connection == client).ToList();
                    foreach (var @object in objects)
                    {
                        Destroy(@object);
                    }

                    clients.Remove(client.clientId);
                    EventManager.Invoke(new ServerDisconnect(client));
                }
            }

            internal static void OnServerReceive(int clientId, ArraySegment<byte> segment, int channel)
            {
                if (!clients.TryGetValue(clientId, out var client))
                {
                    Debug.LogWarning(Service.Text.Format("无法为客户端 {0} 进行处理消息。未知客户端。", clientId));
                    return;
                }

                if (!client.getter.AddBatch(segment))
                {
                    Debug.LogWarning(Service.Text.Format("无法为客户端 {0} 进行处理消息。", clientId));
                    client.Disconnect();
                    return;
                }

                while (!isLoadScene && client.getter.GetMessage(out var result, out var remoteTime))
                {
                    using var getter = MemoryGetter.Pop(result);
                    if (getter.buffer.Count - getter.position < sizeof(ushort))
                    {
                        Debug.LogWarning(Service.Text.Format("无法为客户端 {0} 进行处理消息。没有头部。", clientId));
                        client.Disconnect();
                        return;
                    }

                    var message = getter.GetUShort();
                    if (!messages.TryGetValue(message, out var action))
                    {
                        Debug.LogWarning(Service.Text.Format("无法为客户端 {0} 进行处理消息。未知的消息{1}。", clientId, message));
                        client.Disconnect();
                        return;
                    }

                    client.remoteTime = remoteTime;
                    action.Invoke(client, getter, channel);
                }

                if (!isLoadScene && client.getter.Count > 0)
                {
                    Debug.LogWarning(Service.Text.Format("无法为客户端 {0} 进行处理消息。残留消息: {1}。", clientId, client.getter.Count));
                }
            }
        }

        public partial class Server
        {
            internal static void SpawnObjects()
            {
                var objects = Resources.FindObjectsOfTypeAll<NetworkObject>();
                foreach (var @object in objects)
                {
                    if (IsSceneObject(@object) && @object.objectId == 0)
                    {
                        @object.gameObject.SetActive(true);
                        var parent = @object.transform.parent;
                        if (parent == null || parent.gameObject.activeInHierarchy)
                        {
                            Spawn(@object.gameObject, @object.connection);
                        }
                    }
                }
            }

            public static void Spawn(GameObject obj, NetworkClient client = null)
            {
                if (!isActive)
                {
                    Debug.LogError("服务器不是活跃的。", obj);
                    return;
                }

                if (!obj.TryGetComponent(out NetworkObject @object))
                {
                    Debug.LogError(Service.Text.Format("网络对象 {0} 没有 NetworkObject 组件", obj), obj);
                    return;
                }

                if (spawns.ContainsKey(@object.objectId))
                {
                    Debug.LogWarning(Service.Text.Format("网络对象 {0} 已经生成。", @object), @object);
                    return;
                }

                @object.connection = client;

                if (Mode == EntryMode.Host && client?.clientId == hostId)
                {
                    @object.entityMode |= EntityMode.Owner;
                }

                if ((@object.entityMode & EntityMode.Server) == 0 && @object.objectId == 0)
                {
                    @object.objectId = ++objectId;
                    @object.entityMode |= EntityMode.Server;
                    if (Client.isActive)
                    {
                        @object.entityMode |= EntityMode.Client;
                    }
                    else
                    {
                        @object.entityMode &= ~EntityMode.Owner;
                    }

                    spawns[@object.objectId] = @object;
                    @object.OnStartServer();
                }

                SpawnToClients(@object);
            }

            private static void SpawnToClients(NetworkObject @object)
            {
                foreach (var client in clients.Values.Where(client => client.isReady))
                {
                    SpawnToClient(client, @object);
                }
            }

            private static void SpawnToClient(NetworkClient client, NetworkObject @object)
            {
                using MemorySetter setter = MemorySetter.Pop(), observer = MemorySetter.Pop();
                var isOwner = @object.connection == client;
                var transform = @object.transform;
                ArraySegment<byte> segment = default;
                if (@object.entities.Length != 0)
                {
                    @object.ServerSerialize(true, setter, observer);
                    segment = isOwner ? setter : observer;
                }
              
                var message = new SpawnMessage
                {
                    isOwner = isOwner,
                    isPool = @object.assetId.Equals(@object.name, StringComparison.OrdinalIgnoreCase),
                    assetId = @object.assetId,
                    sceneId = @object.sceneId,
                    objectId = @object.objectId,
                    position = transform.localPosition,
                    rotation = transform.localRotation,
                    localScale = transform.localScale,
                    segment = segment
                };

                client.Send(message);
            }

            public static void Despawn(GameObject obj)
            {
                if (!obj.TryGetComponent(out NetworkObject @object))
                {
                    return;
                }

                spawns.Remove(@object.objectId);
                foreach (var client in clients.Values)
                {
                    client.Send(new DespawnMessage(@object.objectId));
                }

                @object.OnStopServer();
                if (@object.assetId.Equals(@object.name, StringComparison.OrdinalIgnoreCase))
                {
                    PoolManager.Hide(@object.gameObject);
                    @object.Reset();
                    return;
                }

                @object.entityState |= EntityState.Destroy;
                Destroy(@object.gameObject);
            }
        }

        public partial class Server
        {
            internal static void EarlyUpdate()
            {
                if (Transport.Instance != null)
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

                if (Transport.Instance != null)
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
                        foreach (var @object in spawns.Values)
                        {
                            if (@object == null)
                            {
                                Debug.LogWarning(Service.Text.Format("在客户端 {0} 找到了空的网络对象。", client.clientId));
                                return;
                            }

                            @object.Synchronization(Time.frameCount);
                            if (@object.connection == client)
                            {
                                if (@object.owner.position > 0)
                                {
                                    client.Send(new EntityMessage(@object.objectId, @object.owner));
                                }
                            }
                            else
                            {
                                if (@object.observer.position > 0)
                                {
                                    client.Send(new EntityMessage(@object.objectId, @object.observer));
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