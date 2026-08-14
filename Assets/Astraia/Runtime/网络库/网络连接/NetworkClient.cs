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

namespace Astraia.Net
{
    public sealed class NetworkClient : Connection
    {
        internal readonly HashSet<NetworkEntity> entities = new HashSet<NetworkEntity>();
        internal readonly int clientId;
        internal bool isReady;

        public NetworkClient(int clientId)
        {
            this.clientId = clientId;
        }

        internal override void SendInternal(MemoryWriter writer, int pass)
        {
            NetworkManager.Kcp.SendToClient(clientId, writer, pass);
        }

        internal override void DataInternal(NetworkWriter writer, int pass)
        {
            if (NetworkManager.isHost && clientId == 0)
            {
                using var target = MemoryWriter.Pop();
                if (writer.GetBatch(target))
                {
                    NetworkManager.Client.Receive(target, pass);
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