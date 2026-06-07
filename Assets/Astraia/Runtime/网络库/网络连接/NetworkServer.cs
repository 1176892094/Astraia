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
        private Dictionary<int, WriterQueue> packets = new Dictionary<int, WriterQueue>();
        internal ReaderQueue reader = new ReaderQueue();
        internal bool isReady;

        internal void Update()
        {
            foreach (var packet in packets)
            {
                using var writer = MemoryWriter.Pop();
                while (packet.Value.GetBatch(writer))
                {
                    NetworkManager.Transport.SendToServer(writer, packet.Key);
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

            if (writer.position > NetworkManager.Transport.GetLength(pass))
            {
                Log.Error("发送消息大小过大！消息大小: {0}", writer.position);
                return;
            }

            if (!NetworkManager.isHost)
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
                batch = new WriterQueue(NetworkManager.Transport.GetLength(pass));
                packets[pass] = batch;
            }

            batch.AddMessage(writer);

            if (NetworkManager.isHost)
            {
                using var target = MemoryWriter.Pop();
                if (batch.GetBatch(target))
                {
                    NetworkManager.Server.Receive(0, target, Pass.KCP);
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