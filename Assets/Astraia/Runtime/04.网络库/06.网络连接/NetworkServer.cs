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
using Astraia.Core;

namespace Astraia.Net
{
    [Serializable]
    public sealed class NetworkServer
    {
        private Dictionary<int, PacketWriter> packets = new Dictionary<int, PacketWriter>();
        internal PacketReader reader = new PacketReader();
        internal bool isReady;

        internal void Update()
        {
            foreach (var packet in packets)
            {
                using var writer = MemoryWriter.Pop();
                while (packet.Value.GetPacket(writer))
                {
                    NetworkManager.Transport.SendToServer(writer, packet.Key);
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

            if (NetworkManager.isHost)
            {
                using var target = MemoryWriter.Pop();
                if (batch.GetPacket(target))
                {
                    NetworkManager.Server.Receive(0, target, Channel.Reliable);
                }
            }
        }

        public void Disconnect()
        {
            isReady = false;
            NetworkManager.Transport.StopClient();
        }
    }
}