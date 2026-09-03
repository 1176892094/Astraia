// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-09-02 21:09:43
// # Recently: 2026-09-03 14:21:03
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Collections.Generic;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Astraia.Net
{
    [Serializable]
    public sealed class NetworkClient : Connection
    {
#if ODIN_INSPECTOR
        [LabelText("@\"客户端: \" + clientId")]
#endif
        [SerializeField]
        internal List<NetworkEntity> entities = new List<NetworkEntity>();

        internal int clientId;
        internal bool isReady;

        internal override void SendInternal(MemoryWriter writer, int pass)
        {
            NetworkManager.current.SendToClient(clientId, writer, pass);
        }

        internal override void DataInternal(NetworkWriter writer, int pass)
        {
            if (NetworkManager.isHost && clientId == 0)
            {
                using var copied = MemoryWriter.Pop();
                if (writer.GetBatch(copied))
                {
                    NetworkManager.Client.Receive(copied, pass);
                }
            }
        }

        internal override void OnSend<T>(T message, int count)
        {
            if (clientId != 0)
            {
                Debugger.OnSend(message, count);
            }
        }

        internal override void OnData<T>(T message, int count)
        {
            if (clientId != 0)
            {
                Debugger.OnData(message, count);
            }
        }

        public override void Disconnect()
        {
            isReady = false;
            NetworkManager.current.Disconnect(clientId);
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