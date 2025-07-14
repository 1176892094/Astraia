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
using UnityEngine;

namespace Astraia.Net
{
    [Serializable]
    public sealed class NetworkClient
    {
        private Dictionary<int, WriterBatch> batches = new Dictionary<int, WriterBatch>();
        internal ReaderBatch reader = new ReaderBatch();
        internal int clientId;
        internal bool isReady;

        public NetworkClient(int clientId)
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
            writer.SetUShort(NetworkMessage<T>.Id);
            writer.Invoke(message);

            if (writer.position > Transport.Instance.SendLength(channel))
            {
                Debug.LogError(Service.Text.Format(Log.E291, writer.position));
                return;
            }

            NetworkSimulator.Instance?.OnSend(message, writer.position);
            AddMessage(writer, channel);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void AddMessage(MemoryWriter writer, int channel)
        {
            if (!batches.TryGetValue(channel, out var batch))
            {
                batch = new WriterBatch(Transport.Instance.SendLength(channel));
                batches[channel] = batch;
            }

            batch.AddMessage(writer);

            if (clientId == NetworkManager.Server.hostId)
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
            Transport.Instance.StopClient(clientId);
        }
    }
}