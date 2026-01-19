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
using System.Linq;
using System.Runtime.CompilerServices;
using Astraia.Core;

namespace Astraia.Net
{
    [Serializable]
    public sealed class NetworkClient
    {
        private Dictionary<int, PacketWriter> packets = new Dictionary<int, PacketWriter>();
        internal PacketReader reader = new PacketReader();
        internal int clientId;
        internal bool isReady;
        internal HashSet<NetworkEntity> entities = new HashSet<NetworkEntity>();

        public NetworkClient(int clientId = 0)
        {
            this.clientId = clientId;
        }

        internal void Update()
        {
            foreach (var packet in packets)
            {
                using var writer = MemoryWriter.Pop();
                while (packet.Value.GetPacket(writer))
                {
                    NetworkManager.Transport.SendToClient(clientId, writer, packet.Key);
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

            if (writer.position > NetworkManager.Transport.GetLength(channel))
            {
                Service.Log.Error("发送消息大小过大！消息大小: {0}", writer.position);
                return;
            }

            if (clientId != 0)
            {
                Debugger.OnSend(message, writer.position);
            }

            AddMessage(writer, channel);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void AddMessage(MemoryWriter writer, int channel)
        {
            if (!packets.TryGetValue(channel, out var batch))
            {
                batch = new PacketWriter(NetworkManager.Transport.GetLength(channel));
                packets[channel] = batch;
            }

            batch.AddMessage(writer);

            if (NetworkManager.isHost && clientId == 0)
            {
                using var target = MemoryWriter.Pop();
                if (batch.GetPacket(target))
                {
                    NetworkManager.Client.Receive(target, Channel.Reliable);
                }
            }
        }

        public void Disconnect()
        {
            isReady = false;
            NetworkManager.Transport.Disconnect(clientId);
        }

        public void ClearObserver()
        {
            var copies = entities.Where(entity => entity).ToList();
            foreach (var entity in copies)
            {
                if (entity.clients.Remove(this))
                {
                    if (entity.clients.Count == 0)
                    {
                        entity.ClearDirty(true);
                    }

                    entities.Remove(entity);
                }
            }
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