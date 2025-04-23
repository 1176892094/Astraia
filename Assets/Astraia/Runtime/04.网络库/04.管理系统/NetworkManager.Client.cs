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
    using MessageDelegate = Action<NetworkClient, MemoryGetter, int>;

    public partial class NetworkManager
    {
        public static partial class Client
        {
            private static readonly Dictionary<ushort, MessageDelegate> messages = new Dictionary<ushort, MessageDelegate>();

            private static readonly Dictionary<ulong, NetworkObject> scenes = new Dictionary<ulong, NetworkObject>();

            internal static readonly Dictionary<uint, NetworkObject> spawns = new Dictionary<uint, NetworkObject>();

            private static State state = State.Disconnect;
            
            private static double pingTime;

            private static double waitTime;

            private static double sendTime;

            public static bool isActive => state != State.Disconnect;

            public static bool isReady { get; internal set; }

            public static bool isLoadScene { get; internal set; }

            public static NetworkServer connection { get; private set; }

            public static bool isConnected => state == State.Connected;

            internal static void Start(EntryMode mode)
            {
                if (mode == EntryMode.Host)
                {
                    Register(EntryMode.Host);
                    state = State.Connected;
                    connection = new NetworkServer();
                    Server.Connect(new NetworkClient(Server.hostId));
                    Ready();
                    return;
                }

                Register(EntryMode.Client);
                state = State.Connect;
                connection = new NetworkServer();
                Transport.Instance.StartClient();
            }

            internal static void Start(Uri uri)
            {
                Register(EntryMode.Client);
                state = State.Connect;
                connection = new NetworkServer();
                Transport.Instance.StartClient(uri);
            }

            internal static void Stop()
            {
                if (!isActive) return;
                var objects = spawns.Values.Where(@object => @object != null).ToList();
                foreach (var @object in objects)
                {
                    @object.OnStopClient();
                    if (@object.sceneId != 0)
                    {
                        @object.gameObject.SetActive(false);
                        @object.Reset();
                    }
                    else
                    {
                        Destroy(@object.gameObject);
                    }
                }

                state = State.Disconnect;
                if (Transport.Instance != null)
                {
                    Transport.Instance.StopClient();
                }

                sendTime = 0;
                waitTime = 0;
                pingTime = 0;
                isReady = false;
                spawns.Clear();
                scenes.Clear();
                messages.Clear();
                connection = null;
                isLoadScene = false;
                EventManager.Invoke(new ClientDisconnect());
            }

            private static void Pong()
            {
                if (waitTime + 2 <= Time.unscaledTimeAsDouble)
                {
                    waitTime = Time.unscaledTimeAsDouble;
                    connection.Send(new PongMessage(waitTime), Channel.Unreliable);
                }
            }

            public static void Ready()
            {
                if (connection == null)
                {
                    Debug.LogError(Logs.E211);
                    return;
                }

                if (isReady)
                {
                    Debug.LogError(Logs.E212);
                    return;
                }

                isReady = true;
                connection.isReady = true;
                connection.Send(new ReadyMessage());
            }

            private static void Load(string sceneName)
            {
                if (string.IsNullOrWhiteSpace(sceneName))
                {
                    Debug.LogError(Logs.E213);
                    return;
                }

                if (isLoadScene && Instance.sceneName == sceneName)
                {
                    Debug.LogError(Service.Text.Format(Logs.E214, sceneName));
                    return;
                }

                EventManager.Invoke(new ClientChangeScene(sceneName));
                if (Server.isActive) return;
                isLoadScene = true;
                Instance.sceneName = sceneName;

                AssetManager.LoadScene(sceneName);
            }

            internal static void LoadSceneComplete(string sceneName)
            {
                isLoadScene = false;
                if (isConnected && !isReady)
                {
                    Ready();
                }

                EventManager.Invoke(new ClientSceneChanged(sceneName));
            }
        }

        public static partial class Client
        {
            private static void Register(EntryMode mode)
            {
                if (mode == EntryMode.Client)
                {
                    Transport.Instance.OnClientConnect -= OnClientConnect;
                    Transport.Instance.OnClientDisconnect -= OnClientDisconnect;
                    Transport.Instance.OnClientReceive -= OnClientReceive;
                    Transport.Instance.OnClientConnect += OnClientConnect;
                    Transport.Instance.OnClientDisconnect += OnClientDisconnect;
                    Transport.Instance.OnClientReceive += OnClientReceive;
                }

                Register<PingMessage>(PingMessage);
                Register<NotReadyMessage>(NotReadyMessage);
                Register<EntityMessage>(EntityMessage);
                Register<ClientRpcMessage>(ClientRpcMessage);
                Register<SceneMessage>(SceneMessage);
                Register<SpawnMessage>(SpawnMessage);
                Register<DespawnMessage>(DespawnMessage);
            }

            public static void Register<T>(Action<T> handle) where T : struct, IMessage
            {
                messages[Hash<T>.Id] = (client, getter, channel) =>
                {
                    try
                    {
                        var message = getter.Invoke<T>();
                        handle?.Invoke(message);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(Service.Text.Format(Logs.E215, typeof(T).Name, channel, e));
                        client.Disconnect();
                    }
                };
            }

            private static void PingMessage(PingMessage message)
            {
                if (Server.isActive)
                {
                    return;
                }
                
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
                isReady = false;
                EventManager.Invoke(new ClientNotReady());
            }

            private static void EntityMessage(EntityMessage message)
            {
                if (Server.isActive)
                {
                    return;
                }

                if (!spawns.TryGetValue(message.objectId, out var @object))
                {
                    Debug.LogWarning(Service.Text.Format(Logs.E216, message.objectId));
                    return;
                }

                if (@object == null)
                {
                    Debug.LogWarning(Service.Text.Format(Logs.E216, message.objectId));
                    return;
                }

                using var getter = MemoryGetter.Pop(message.segment);
                @object.ClientDeserialize(getter, false);
            }

            private static void ClientRpcMessage(ClientRpcMessage message)
            {
                if (spawns.TryGetValue(message.objectId, out var @object))
                {
                    using var getter = MemoryGetter.Pop(message.segment);
                    @object.InvokeMessage(message.componentId, message.methodHash, InvokeMode.ClientRpc, getter);
                }
            }

            private static void SceneMessage(SceneMessage message)
            {
                if (!isConnected)
                {
                    Debug.LogWarning(Logs.E217);
                    return;
                }

                Load(message.sceneName);
            }

            private static void SpawnMessage(SpawnMessage message)
            {
                if (Server.isActive)
                {
                    if (Server.spawns.TryGetValue(message.objectId, out var @object))
                    {
                        spawns[message.objectId] = @object;
                        @object.gameObject.SetActive(true);
                        if (message.isOwner)
                        {
                            @object.entityMode |= EntityMode.Owner;
                        }
                        else
                        {
                            @object.entityMode &= ~EntityMode.Owner;
                        }

                        @object.entityMode |= EntityMode.Client;
                        @object.OnNotifyAuthority();
                        @object.OnStartClient();
                    }

                    return;
                }

                scenes.Clear();
                var objects = Resources.FindObjectsOfTypeAll<NetworkObject>();
                foreach (var @object in objects)
                {
                    if (IsSceneObject(@object))
                    {
                        if (scenes.TryGetValue(@object.sceneId, out var obj))
                        {
                            Service.Text.Format(Logs.E218, @object.name, obj.name);
                            continue;
                        }

                        scenes.Add(@object.sceneId, @object);
                    }
                }

                SpawnObject(message);
            }

            private static void DespawnMessage(DespawnMessage message)
            {
                if (!spawns.TryGetValue(message.objectId, out var @object))
                {
                    return;
                }

                @object.OnStopClient();
                @object.entityMode &= ~EntityMode.Owner;
                @object.OnNotifyAuthority();
                spawns.Remove(message.objectId);

                if (Server.isActive)
                {
                    return;
                }

                if (@object.assetId.Equals(@object.name, StringComparison.OrdinalIgnoreCase))
                {
                    PoolManager.Hide(@object.gameObject);
                    @object.Reset();
                    return;
                }

                Destroy(@object.gameObject);
            }
        }

        public static partial class Client
        {
            private static void OnClientConnect()
            {
                if (connection == null)
                {
                    Debug.LogError(Logs.E219);
                    return;
                }

                state = State.Connected;
                EventManager.Invoke(new ClientConnect());
                Pong();
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
                    Debug.LogError(Logs.E219);
                    return;
                }

                if (!connection.getter.AddBatch(segment))
                {
                    Debug.LogWarning(Logs.E220);
                    connection.Disconnect();
                    return;
                }

                while (!isLoadScene && connection.getter.GetMessage(out var result, out var remoteTime))
                {
                    using var getter = MemoryGetter.Pop(result);
                    if (getter.buffer.Count - getter.position < sizeof(ushort))
                    {
                        Debug.LogWarning(Logs.E221);
                        connection.Disconnect();
                        return;
                    }

                    var message = getter.GetUShort();
                    if (!messages.TryGetValue(message, out var action))
                    {
                        Debug.LogWarning(Service.Text.Format(Logs.E222, message));
                        connection.Disconnect();
                        return;
                    }

                    connection.remoteTime = remoteTime;
                    action.Invoke(null, getter, channel);
                }

                if (!isLoadScene && connection.getter.Count > 0)
                {
                    Debug.LogWarning(Service.Text.Format(Logs.E223, connection.getter.Count));
                }
            }
        }

        public static partial class Client
        {
            private static async void SpawnObject(SpawnMessage message)
            {
                if (spawns.TryGetValue(message.objectId, out var @object))
                {
                    Spawn(message, @object);
                    return;
                }

                if (message.sceneId == 0)
                {
                    GameObject prefab;
                    if (message.isPool)
                    {
                        prefab = await PoolManager.Show(message.assetId);
                    }
                    else
                    {
                        prefab = await AssetManager.Load<GameObject>(message.assetId);
                    }

                    if (!prefab.TryGetComponent(out @object))
                    {
                        Debug.LogError(Service.Text.Format(Logs.E224, prefab.name));
                        return;
                    }

                    if (@object.sceneId != 0)
                    {
                        Debug.LogError(Service.Text.Format(Logs.E225, @object.name));
                        return;
                    }

                    if (@object.GetComponentsInChildren<NetworkObject>().Length > 1)
                    {
                        Debug.LogError(Service.Text.Format(Logs.E226, @object.name));
                        return;
                    }
                }
                else
                {
                    if (!scenes.TryGetValue(message.sceneId, out @object))
                    {
                        Debug.LogError(Service.Text.Format(Logs.E227, message.sceneId));
                        return;
                    }

                    scenes.Remove(message.sceneId);
                }

                Spawn(message, @object);
            }

            private static void Spawn(SpawnMessage message, NetworkObject @object)
            {
                if (!@object.gameObject.activeSelf)
                {
                    @object.gameObject.SetActive(true);
                }

                @object.objectId = message.objectId;
                if (message.isOwner)
                {
                    @object.entityMode |= EntityMode.Owner;
                }
                else
                {
                    @object.entityMode &= ~EntityMode.Owner;
                }

                @object.entityMode |= EntityMode.Client;
                var transform = @object.transform;
                transform.localPosition = message.position;
                transform.localRotation = message.rotation;
                transform.localScale = message.localScale;

                if (message.segment.Count > 0)
                {
                    using var getter = MemoryGetter.Pop(message.segment);
                    @object.ClientDeserialize(getter, true);
                }

                spawns[message.objectId] = @object;
                @object.OnNotifyAuthority();
                @object.OnStartClient();
            }
        }

        public static partial class Client
        {
            internal static void EarlyUpdate()
            {
                if (Transport.Instance != null)
                {
                    Transport.Instance.ClientEarlyUpdate();
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

                if (connection != null)
                {
                    if (Mode == EntryMode.Host)
                    {
                        connection.Update();
                    }
                    else
                    {
                        if (isConnected)
                        {
                            Pong();
                            connection.Update();
                        }
                    }
                }

                if (Transport.Instance != null)
                {
                    Transport.Instance.ClientAfterUpdate();
                }
            }

            private static void Broadcast()
            {
                if (Server.isActive)
                {
                    return;
                }

                if (!connection.isReady)
                {
                    return;
                }

                foreach (var @object in spawns.Values)
                {
                    using var setter = MemorySetter.Pop();
                    @object.ClientSerialize(setter);
                    if (setter.position > 0)
                    {
                        connection.Send(new EntityMessage(@object.objectId, setter));
                        @object.ClearDirty(false);
                    }
                }
            }
        }
    }
}