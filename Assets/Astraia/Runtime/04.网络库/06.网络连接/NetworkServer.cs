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
    public sealed class NetworkServer
    {
        private Dictionary<int, SetterBatch> batches = new Dictionary<int, SetterBatch>();
        internal GetterBatch getter = new GetterBatch();
        internal bool isReady;
        internal double remoteTime;

        internal void Update()
        {
            foreach (var batch in batches)
            {
                using var setter = MemorySetter.Pop();
                while (batch.Value.GetBatch(setter))
                {
                    Transport.Instance.SendToServer(setter, batch.Key);
                    setter.Reset();
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Send<T>(T message, int channel = Channel.Reliable) where T : struct, IMessage
        {
            using var setter = MemorySetter.Pop();
            setter.SetUShort(NetworkMessage<T>.Id);
            setter.Invoke(message);

            if (setter.position > Transport.Instance.SendLength(channel))
            {
                Debug.LogError(Service.Text.Format(Log.E291, setter.position));
                return;
            }

            AddMessage(setter, channel);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void AddMessage(MemorySetter setter, int channel)
        {
            if (!batches.TryGetValue(channel, out var batch))
            {
                batch = new SetterBatch(Transport.Instance.SendLength(channel));
                batches[channel] = batch;
            }

            batch.AddMessage(setter, Time.unscaledTimeAsDouble);

            if (NetworkManager.Mode == EntryMode.Host)
            {
                using var target = MemorySetter.Pop();
                if (batch.GetBatch(target))
                {
                    NetworkManager.Server.OnServerReceive(NetworkManager.Server.hostId, target, Channel.Reliable);
                }
            }
        }

        public void Disconnect()
        {
            isReady = false;
            NetworkManager.Client.isReady = false;
            Transport.Instance.StopClient();
        }
    }
}