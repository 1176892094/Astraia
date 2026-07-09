// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2024-12-21 23:12:50
// # Recently: 2024-12-22 21:12:45
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Astraia.Core;

namespace Astraia.Net
{
    public sealed class NetworkClient
    {
        private readonly Dictionary<int, WriterQueue> packets = new Dictionary<int, WriterQueue>();
        internal readonly HashSet<NetworkEntity> entities = new HashSet<NetworkEntity>();
        internal readonly ReaderQueue reader = new ReaderQueue();
        internal readonly int clientId;
        internal bool isReady;

        public NetworkClient(int clientId)
        {
            this.clientId = clientId;
        }

        internal void Update()
        {
            foreach (var packet in packets)
            {
                using var writer = MemoryWriter.Pop();
                while (packet.Value.GetBatch(writer))
                {
                    NetworkManager.Kcp.SendToClient(clientId, writer, packet.Key);
                    writer.Reset();
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Send<T>(T message, int pass = Pass.KCP) where T : struct, IMessage
        {
            using var writer = MemoryWriter.Pop();
            writer.WriteUInt16(NetworkMessage<T>.Id);
            writer.Invoke(message);

            if (writer.position > NetworkManager.Kcp.GetLength(pass))
            {
                Log.Error("发送消息大小过大！消息大小: {0}", writer.position);
                return;
            }

            if (clientId != 0)
            {
                Debugger.OnSend(message, writer.position);
            }

            AddMessage(writer, pass);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void AddMessage(MemoryWriter writer, int pass)
        {
            if (!packets.TryGetValue(pass, out var batch))
            {
                batch = new WriterQueue(NetworkManager.Kcp.GetLength(pass));
                packets[pass] = batch;
            }

            batch.AddMessage(writer);

            if (NetworkManager.isHost && clientId == 0)
            {
                using var target = MemoryWriter.Pop();
                if (batch.GetBatch(target))
                {
                    NetworkManager.Client.Receive(target, Pass.KCP);
                }
            }
        }

        public void Disconnect()
        {
            isReady = false;
            NetworkManager.Kcp.Disconnect(clientId);
        }

        public static implicit operator int(NetworkClient client)
        {
            return client.clientId;
        }

        public static implicit operator NetworkClient(int id)
        {
            return NetworkManager.Server.clients.GetValueOrDefault(id);
        }
    }
}