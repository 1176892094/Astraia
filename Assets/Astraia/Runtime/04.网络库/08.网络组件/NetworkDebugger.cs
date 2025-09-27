// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-06-01 19:06:54
// // # Recently: 2025-06-01 19:06:54
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using Astraia.Common;
using UnityEngine;

namespace Astraia.Net
{
    public abstract class NetworkDebugger : MonoBehaviour, IEvent<PingUpdate>
    {
        private static bool isRunning;
        protected string address;
        protected double framePing;
        private double waitTime;

        private int clientIntervalReceivedPackets;
        private long clientIntervalReceivedBytes;
        private int clientIntervalSentPackets;
        private long clientIntervalSentBytes;

        private int clientReceivedPacketsPerSecond;
        private long clientReceivedBytesPerSecond;
        private int clientSentPacketsPerSecond;
        private long clientSentBytesPerSecond;

        private int serverIntervalReceivedPackets;
        private long serverIntervalReceivedBytes;
        private int serverIntervalSentPackets;
        private long serverIntervalSentBytes;

        private int serverReceivedPacketsPerSecond;
        private long serverReceivedBytesPerSecond;
        private int serverSentPacketsPerSecond;
        private long serverSentBytesPerSecond;

        private static readonly Items sendItems = new Items();
        private static readonly Items receiveItems = new Items();

        protected virtual void Awake()
        {
            isRunning = true;
            address = Service.Host.Ip();
        }

        protected virtual void Start()
        {
            if (Transport.Instance)
            {
                Transport.Instance.OnClientSend += OnClientSend;
                Transport.Instance.OnServerSend += OnServerSend;
                Transport.Instance.OnClientReceive += OnClientReceive;
                Transport.Instance.OnServerReceive += OnServerReceive;
            }
        }

        protected virtual void OnEnable()
        {
            EventManager.Listen(this);
        }

        protected virtual void OnDisable()
        {
            EventManager.Remove(this);
        }

        protected virtual void Update()
        {
            if (waitTime < Time.unscaledTimeAsDouble)
            {
                if (NetworkManager.Client.isActive)
                {
                    UpdateClient();
                }

                if (NetworkManager.Server.isActive)
                {
                    UpdateServer();
                }

                waitTime = Time.unscaledTimeAsDouble + 1;
            }
        }

        private void OnDestroy()
        {
            if (Transport.Instance)
            {
                Transport.Instance.OnClientSend -= OnClientSend;
                Transport.Instance.OnServerSend -= OnServerSend;
                Transport.Instance.OnClientReceive -= OnClientReceive;
                Transport.Instance.OnServerReceive -= OnServerReceive;
            }

            sendItems.Clear();
            receiveItems.Clear();
        }

        public void Execute(PingUpdate message)
        {
            framePing = message.pingTime;
        }

        private void OnClientReceive(ArraySegment<byte> data, int channel)
        {
            clientIntervalReceivedPackets++;
            clientIntervalReceivedBytes += data.Count;
        }

        private void OnClientSend(ArraySegment<byte> data, int channel)
        {
            clientIntervalSentPackets++;
            clientIntervalSentBytes += data.Count;
        }

        private void OnServerReceive(int connectionId, ArraySegment<byte> data, int channel)
        {
            serverIntervalReceivedPackets++;
            serverIntervalReceivedBytes += data.Count;
        }

        private void OnServerSend(int connectionId, ArraySegment<byte> data, int channel)
        {
            serverIntervalSentPackets++;
            serverIntervalSentBytes += data.Count;
        }

        private void UpdateClient()
        {
            clientReceivedPacketsPerSecond = clientIntervalReceivedPackets;
            clientReceivedBytesPerSecond = clientIntervalReceivedBytes;
            clientSentPacketsPerSecond = clientIntervalSentPackets;
            clientSentBytesPerSecond = clientIntervalSentBytes;

            clientIntervalReceivedPackets = 0;
            clientIntervalReceivedBytes = 0;
            clientIntervalSentPackets = 0;
            clientIntervalSentBytes = 0;
        }

        private void UpdateServer()
        {
            serverReceivedPacketsPerSecond = serverIntervalReceivedPackets;
            serverReceivedBytesPerSecond = serverIntervalReceivedBytes;
            serverSentPacketsPerSecond = serverIntervalSentPackets;
            serverSentBytesPerSecond = serverIntervalSentBytes;

            serverIntervalReceivedPackets = 0;
            serverIntervalReceivedBytes = 0;
            serverIntervalSentPackets = 0;
            serverIntervalSentBytes = 0;
        }

        protected void OnGUIServer()
        {
            GUILayout.Label("向服务器发送数量:\t\t{0}".Format(clientSentPacketsPerSecond));
            GUILayout.Label("向服务器发送大小:\t\t{0}/s".Format(PrettyBytes(clientSentBytesPerSecond)));
            GUILayout.Label("从服务器接收数量:\t\t{0}".Format(clientReceivedPacketsPerSecond));
            GUILayout.Label("从服务器接收大小:\t\t{0}/s".Format(PrettyBytes(clientReceivedBytesPerSecond)));
        }

        protected void OnGUIClient()
        {
            GUILayout.Label("向客户端发送数量:\t\t{0}".Format(serverSentPacketsPerSecond));
            GUILayout.Label("向客户端发送大小:\t\t{0}/s".Format(PrettyBytes(serverSentBytesPerSecond)));
            GUILayout.Label("从客户端接收数量:\t\t{0}".Format(serverReceivedPacketsPerSecond));
            GUILayout.Label("从客户端接收大小:\t\t{0}/s".Format(PrettyBytes(serverReceivedBytesPerSecond)));
        }

        protected static void ItemReset()
        {
            sendItems.Reset();
            receiveItems.Reset();
        }

        protected static string PrettyBytes(long bytes)
        {
            if (bytes < 1024)
            {
                return "{0} B".Format(bytes);
            }

            if (bytes < 1024 * 1024)
            {
                return "{0:F2} KB".Format(bytes / 1024F);
            }

            if (bytes < 1024 * 1024 * 1024)
            {
                return "{0:F2} MB".Format(bytes / 1024F / 1024F);
            }

            return "{0:F2} GB".Format(bytes / 1024F / 1024F / 1024F);
        }

        internal static IList<Pool> SendReference()
        {
            var pools = sendItems.messages.Values.Select(item => new Pool(item)).ToList();
            pools.AddRange(sendItems.function.Values.Select(item => new Pool(item)));
            return pools;
        }

        internal static IList<Pool> ReceiveReference()
        {
            var pools = receiveItems.messages.Values.Select(item => new Pool(item)).ToList();
            pools.AddRange(receiveItems.function.Values.Select(item => new Pool(item)));
            return pools;
        }

        internal static void OnSend<T>(T message, int bytes) where T : struct, IMessage
        {
            if (!isRunning) return;
            sendItems.Record(message, Service.Bit.Invoke((uint)bytes) + bytes);
        }

        internal static void OnReceive<T>(T message, int bytes) where T : struct, IMessage
        {
            if (!isRunning) return;
            receiveItems.Record(message, Service.Bit.Invoke((uint)bytes + 2) + bytes + 2);
        }

        private class Items
        {
            public readonly Dictionary<Type, Item> messages = new Dictionary<Type, Item>();
            public readonly Dictionary<ushort, Item> function = new Dictionary<ushort, Item>();

            public void Record<T>(T message, int bytes) where T : struct, IMessage
            {
                if (!messages.TryGetValue(typeof(T), out var item))
                {
                    item = HeapManager.Dequeue<Item>();
                    item.Type = typeof(T);
                    messages[typeof(T)] = item;
                }

                switch (message)
                {
                    case ServerRpcMessage server:
                        Record(server.methodHash, bytes, typeof(T));
                        break;
                    case ClientRpcMessage client:
                        Record(client.methodHash, bytes, typeof(T));
                        break;
                }

                item.Add(bytes);
            }

            private void Record(ushort method, int bytes, Type type)
            {
                if (!function.TryGetValue(method, out var item))
                {
                    item = HeapManager.Dequeue<Item>();
                    var data = NetworkAttribute.GetInvoke(method);
                    if (data != null)
                    {
                        var name = data.Method.Name;
                        if (name.EndsWith("_0"))
                        {
                            name = name.Substring(0, name.Length - 2);
                        }

                        item.Path = "{0}.{1}".Format(data.Method.DeclaringType, name);
                    }

                    item.Type = type;
                    function[method] = item;
                }

                item.Add(bytes);
            }

            public void Reset()
            {
                foreach (var item in function.Values)
                {
                    item.Dispose();
                }

                foreach (var item in messages.Values)
                {
                    item.Dispose();
                }
            }

            public void Clear()
            {
                foreach (var item in messages.Values)
                {
                    item.Dispose();
                    HeapManager.Enqueue(item);
                }

                messages.Clear();

                foreach (var item in function.Values)
                {
                    item.Dispose();
                    HeapManager.Enqueue(item);
                }

                function.Clear();
            }

            [Serializable]
            public class Item : IPool
            {
                public Type Type { get; set; }
                public string Path { get; set; }
                public int Acquire { get; set; }
                public int Release { get; set; }
                public int Dequeue { get; set; }
                public int Enqueue { get; set; }

                public void Add(int bytes)
                {
                    Release++;
                    Acquire += bytes;
                    Dequeue++;
                    Enqueue += bytes;
                }

                public void Dispose()
                {
                    Acquire = 0;
                    Release = 0;
                }
            }
        }
    }
}