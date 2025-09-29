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

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Astraia.Common;

namespace Astraia.Net
{
    [Serializable]
    public sealed class NetworkClient
    {
        private Dictionary<int, WriterBatch> batches = new Dictionary<int, WriterBatch>();
        internal HashSet<NetworkEntity> entities = new HashSet<NetworkEntity>();
        internal ReaderBatch reader = new ReaderBatch();
        internal int clientId;
        internal bool isReady;

        public NetworkClient(int clientId = 0)
        {
            this.clientId = clientId;
        }

        internal void Update()
        {
            foreach (var batch in batches)
            {
                using var writer = MemoryWriter.Pop();
                while (batch.Value.GetBatch(writer))
                {
                    Transport.Instance.SendToClient(clientId, writer, batch.Key);
                    writer.Reset();
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Send<T>(T message, int channel = Channel.Reliable) where T : struct, IMessage
        {
            using var writer = MemoryWriter.Pop();
            writer.WriteUShort(NetworkMessage<T>.Id);
            writer.Invoke(message);

            if (writer.position > Transport.Instance.GetLength(channel))
            {
                Log.Error("发送消息大小过大！消息大小: {0}", writer.position);
                return;
            }

            if (clientId != NetworkManager.Host)
            {
                NetworkDebugger.OnSend(message, writer.position);
            }

            AddMessage(writer, channel);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void AddMessage(MemoryWriter writer, int channel)
        {
            if (!batches.TryGetValue(channel, out var batch))
            {
                batch = new WriterBatch(Transport.Instance.GetLength(channel));
                batches[channel] = batch;
            }

            batch.AddMessage(writer);

            if (NetworkManager.isHost && clientId == NetworkManager.Host)
            {
                using var target = MemoryWriter.Pop();
                if (batch.GetBatch(target))
                {
                    NetworkManager.Client.OnClientReceive(target, Channel.Reliable);
                }
            }
        }

        public void Disconnect()
        {
            isReady = false;
            entities.Clear();
            Transport.Instance.Disconnect(clientId);
        }

        public static implicit operator int(NetworkClient client)
        {
            return client.clientId;
        }

        public static implicit operator NetworkClient(int id)
        {
            return NetworkManager.Server.clients.TryGetValue(id, out var client) ? client : null;
        }
    }
}