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
    public class NetworkSimulator : MonoBehaviour
    {
        public static NetworkSimulator Instance;
        private double waitTime;

        public int clientIntervalReceivedPackets;
        public long clientIntervalReceivedBytes;
        public int clientIntervalSentPackets;
        public long clientIntervalSentBytes;

        public int clientReceivedPacketsPerSecond;
        public long clientReceivedBytesPerSecond;
        public int clientSentPacketsPerSecond;
        public long clientSentBytesPerSecond;

        public int serverIntervalReceivedPackets;
        public long serverIntervalReceivedBytes;
        public int serverIntervalSentPackets;
        public long serverIntervalSentBytes;

        public int serverReceivedPacketsPerSecond;
        public long serverReceivedBytesPerSecond;
        public int serverSentPacketsPerSecond;
        public long serverSentBytesPerSecond;

        private readonly Items sendItems = new Items();
        private readonly Items receiveItems = new Items();

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            if (Transport.Instance != null)
            {
                Transport.Instance.OnClientSend += OnClientSend;
                Transport.Instance.OnServerSend += OnServerSend;
                Transport.Instance.OnClientReceive += OnClientReceive;
                Transport.Instance.OnServerReceive += OnServerReceive;
            }
        }

        private void OnDestroy()
        {
            if (Transport.Instance != null)
            {
                Transport.Instance.OnClientSend -= OnClientSend;
                Transport.Instance.OnServerSend -= OnServerSend;
                Transport.Instance.OnClientReceive -= OnClientReceive;
                Transport.Instance.OnServerReceive -= OnServerReceive;
            }

            sendItems.Clear();
            receiveItems.Clear();
        }

        private void OnClientReceive(ArraySegment<byte> data, int channelId)
        {
            clientIntervalReceivedPackets++;
            clientIntervalReceivedBytes += data.Count;
        }

        private void OnClientSend(ArraySegment<byte> data, int channelId)
        {
            clientIntervalSentPackets++;
            clientIntervalSentBytes += data.Count;
        }

        private void OnServerReceive(int connectionId, ArraySegment<byte> data, int channelId)
        {
            serverIntervalReceivedPackets++;
            serverIntervalReceivedBytes += data.Count;
        }

        private void OnServerSend(int connectionId, ArraySegment<byte> data, int channelId)
        {
            serverIntervalSentPackets++;
            serverIntervalSentBytes += data.Count;
        }

        private void Update()
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

        public void OnGUIServer()
        {
            GUILayout.Label(Service.Text.Format("向服务器发送数量:\t\t{0}", clientSentPacketsPerSecond));
            GUILayout.Label(Service.Text.Format("向服务器发送大小:\t\t{0}/s", PrettyBytes(clientSentBytesPerSecond)));
            GUILayout.Label(Service.Text.Format("从服务器接收数量:\t\t{0}", clientReceivedPacketsPerSecond));
            GUILayout.Label(Service.Text.Format("从服务器接收大小:\t\t{0}/s", PrettyBytes(clientReceivedBytesPerSecond)));
        }

        public void OnGUIClient()
        {
            GUILayout.Label(Service.Text.Format("向客户端发送数量:\t\t{0}", serverSentPacketsPerSecond));
            GUILayout.Label(Service.Text.Format("向客户端发送大小:\t\t{0}/s", PrettyBytes(serverSentBytesPerSecond)));
            GUILayout.Label(Service.Text.Format("从客户端接收数量:\t\t{0}", serverReceivedPacketsPerSecond));
            GUILayout.Label(Service.Text.Format("从客户端接收大小:\t\t{0}/s", PrettyBytes(serverReceivedBytesPerSecond)));
        }


        public void ItemReset()
        {
            sendItems.Reset();
            receiveItems.Reset();
        }

        private static string PrettyBytes(long bytes)
        {
            if (bytes < 1024)
            {
                return Service.Text.Format("{0} B", bytes);
            }

            if (bytes < 1024 * 1024)
            {
                return Service.Text.Format("{0:F2} KB", bytes / 1024F);
            }

            if (bytes < 1024 * 1024 * 1024)
            {
                return Service.Text.Format("{0:F2} MB", bytes / 1024F / 1024F);
            }

            return Service.Text.Format("{0:F2} GB", bytes / 1024F / 1024F / 1024F);
        }

        internal IList<Pool> SendReference()
        {
            var pools = sendItems.messages.Values.Select(item => new Pool(item)).ToList();
            pools.AddRange(sendItems.function.Values.Select(item => new Pool(item)));
            return pools;
        }

        internal IList<Pool> ReceiveReference()
        {
            var pools = receiveItems.messages.Values.Select(item => new Pool(item)).ToList();
            pools.AddRange(receiveItems.function.Values.Select(item => new Pool(item)));
            return pools;
        }

        internal void OnSend<T>(T message, int bytes) where T : struct, IMessage
        {
            var count = Service.Length.Invoke((uint)bytes);
            sendItems.Record(message, count + bytes);
        }

        internal void OnReceive<T>(T message, int bytes) where T : struct, IMessage
        {
            var count = Service.Length.Invoke((uint)bytes + 2);
            receiveItems.Record(message, count + bytes + 2);
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
                    item.type = typeof(T);
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
                        item.path = Service.Text.Format("{0}.{1}", data.Method.DeclaringType, data.Method.Name.Replace("Cmd_", ""));
                    }

                    item.type = type;
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
                public Type type { get; set; }
                public string path { get; set; }
                public int acquire { get; set; }
                public int release { get; set; }
                public int dequeue { get; set; }
                public int enqueue { get; set; }

                public void Add(int bytes)
                {
                    release++;
                    acquire += bytes;
                    dequeue++;
                    enqueue += bytes;
                }

                public void Dispose()
                {
                    acquire = 0;
                    release = 0;
                }
            }
        }
    }
}