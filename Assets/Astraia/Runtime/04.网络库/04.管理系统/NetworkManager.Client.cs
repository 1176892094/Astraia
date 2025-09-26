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
    using MessageDelegate = Action<NetworkClient, MemoryReader, int>;

    public partial class NetworkManager
    {
        [Serializable]
        public static partial class Client
        {
            private static readonly Dictionary<ushort, MessageDelegate> messages = new Dictionary<ushort, MessageDelegate>();

            private static readonly Dictionary<ulong, NetworkEntity> scenes = new Dictionary<ulong, NetworkEntity>();

            internal static readonly Dictionary<uint, NetworkEntity> spawns = new Dictionary<uint, NetworkEntity>();

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
                    Server.Connect(new NetworkClient());
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
                var entities = spawns.Values.Where(entity => entity != null).ToList();
                foreach (var entity in entities)
                {
                    entity.OnStopClient();
                    if (entity.sceneId != 0)
                    {
                        entity.gameObject.SetActive(false);
                        entity.Reset();
                    }
                    else
                    {
                        Destroy(entity.gameObject);
                    }
                }

                state = State.Disconnect;
                if (Transport.Instance != null)
                {
                    Transport.Instance.Disconnect();
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
                    Debug.LogError(Log.E211);
                    return;
                }

                if (isReady)
                {
                    Debug.LogError(Log.E212);
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
                    Debug.LogError(Log.E213);
                    return;
                }

                if (isLoadScene && Instance.sceneName == sceneName)
                {
                    Debug.LogError(Log.E214.Format(sceneName));
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
                Register<DestroyMessage>(DestroyMessage);
            }

            public static void Register<T>(Action<T> handle) where T : struct, IMessage
            {
                messages[NetworkMessage<T>.Id] = (client, reader, channel) =>
                {
                    try
                    {
                        var position = reader.position;
                        var message = reader.Invoke<T>();
                        NetworkDebugger.OnReceive(message, reader.position - position);
                        handle?.Invoke(message);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(Log.E215.Format(typeof(T).Name, channel, e));
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

                if (!spawns.TryGetValue(message.objectId, out var entity))
                {
                    Debug.LogWarning(Log.E216.Format(message.objectId));
                    return;
                }

                if (entity == null)
                {
                    Debug.LogWarning(Log.E216.Format(message.objectId));
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
                if (!isConnected)
                {
                    Debug.LogWarning(Log.E217);
                    return;
                }

                Load(message.sceneName);
            }

            private static void SpawnMessage(SpawnMessage message)
            {
                if (Server.isActive)
                {
                    if (Server.spawns.TryGetValue(message.objectId, out var entity))
                    {
                        spawns[message.objectId] = entity;
                        entity.gameObject.SetActive(true);
                        entity.mode = message.isOwner ? entity.mode | EntityMode.Owner : entity.mode & ~EntityMode.Owner;
                        entity.mode |= EntityMode.Client;
                        entity.OnNotifyAuthority();
                        entity.OnStartClient();
                    }

                    return;
                }

                scenes.Clear();
                var entities = FindObjectsByType<NetworkEntity>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                foreach (var entity in entities)
                {
                    if (entity.sceneId != 0)
                    {
                        if (scenes.TryGetValue(entity.sceneId, out var obj))
                        {
                            Log.E218.Format(entity.name, obj.name);
                            continue;
                        }

                        scenes.Add(entity.sceneId, entity);
                    }
                }

                SpawnObject(message);
            }

            private static void DespawnMessage(DespawnMessage message)
            {
                if (!spawns.TryGetValue(message.objectId, out var entity))
                {
                    return;
                }

                entity.OnStopClient();
                entity.mode &= ~EntityMode.Owner;
                entity.OnNotifyAuthority();
                spawns.Remove(message.objectId);

                if (Server.isActive)
                {
                    return;
                }

                PoolManager.Hide(entity.gameObject);
                entity.Reset();
            }

            private static void DestroyMessage(DestroyMessage message)
            {
                if (!spawns.TryGetValue(message.objectId, out var entity))
                {
                    return;
                }

                entity.OnStopClient();
                entity.mode &= ~EntityMode.Owner;
                entity.OnNotifyAuthority();
                spawns.Remove(message.objectId);

                if (Server.isActive)
                {
                    return;
                }

                Destroy(entity.gameObject);
            }
        }

        public static partial class Client
        {
            private static void OnClientConnect()
            {
                if (connection == null)
                {
                    Debug.LogError(Log.E219);
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
                    Debug.LogError(Log.E219);
                    return;
                }

                if (!connection.reader.AddBatch(segment))
                {
                    Debug.LogWarning(Log.E220);
                    connection.Disconnect();
                    return;
                }

                while (!isLoadScene && connection.reader.GetMessage(out var result))
                {
                    using var reader = MemoryReader.Pop(result);
                    if (reader.buffer.Count - reader.position < sizeof(ushort))
                    {
                        Debug.LogWarning(Log.E221);
                        connection.Disconnect();
                        return;
                    }


                    var message = reader.ReadUShort();

                    if (!messages.TryGetValue(message, out var action))
                    {
                        Debug.LogWarning(Log.E222.Format(message));
                        connection.Disconnect();
                        return;
                    }

                    action.Invoke(null, reader, channel);
                }

                if (!isLoadScene && connection.reader.Count > 0)
                {
                    Debug.LogWarning(Log.E223.Format(connection.reader.Count));
                }
            }
        }

        public static partial class Client
        {
            private static void SpawnObject(SpawnMessage message)
            {
                if (spawns.TryGetValue(message.objectId, out var entity))
                {
                    Spawn(message, entity);
                    return;
                }

                if (message.sceneId == 0)
                {
                    var result = GlobalSetting.Prefab.Format(message.assetId);
                    var prefab = AssetManager.Load<GameObject>(result);
                    if (!prefab.TryGetComponent(out entity))
                    {
                        Debug.LogError(Log.E224.Format(prefab.name));
                        return;
                    }

                    if (entity.sceneId != 0)
                    {
                        Debug.LogError(Log.E225.Format(entity.name));
                        return;
                    }
                }
                else
                {
                    if (!scenes.Remove(message.sceneId, out entity))
                    {
                        Debug.LogError(Log.E227.Format(message.sceneId));
                        return;
                    }
                }

                Spawn(message, entity);
            }

            private static void Spawn(SpawnMessage message, NetworkEntity entity)
            {
                if (!entity.gameObject.activeSelf)
                {
                    entity.gameObject.SetActive(true);
                }

                entity.objectId = message.objectId;
                entity.mode = message.isOwner ? entity.mode | EntityMode.Owner : entity.mode & ~EntityMode.Owner;
                entity.mode |= EntityMode.Client;
                entity.transform.localPosition = message.position;
                entity.transform.localRotation = message.rotation;
                entity.transform.localScale = message.localScale;

                if (message.segment.Count > 0)
                {
                    using var reader = MemoryReader.Pop(message.segment);
                    entity.ClientDeserialize(reader, true);
                }

                spawns[message.objectId] = entity;
                entity.OnNotifyAuthority();
                entity.OnStartClient();
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