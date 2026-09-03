// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-09-02 21:09:58
// # Recently: 2026-09-03 14:21:03
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;

namespace Astraia.Net
{
    [Serializable]
    public sealed class NetworkServer : Connection
    {
        internal int serverId;
        internal bool isReady;

        internal override void SendInternal(MemoryWriter writer, int pass)
        {
            NetworkManager.current.SendToServer(writer, pass);
        }

        internal override void DataInternal(NetworkWriter writer, int pass)
        {
            if (NetworkManager.isHost)
            {
                using var copied = MemoryWriter.Pop();
                if (writer.GetBatch(copied))
                {
                    NetworkManager.Server.Receive(0, copied, pass);
                }
            }
        }

        internal override void OnSend<T>(T message, int count)
        {
            if (!NetworkManager.isHost)
            {
                Debugger.OnSend(message, count);
            }
        }

        internal override void OnData<T>(T message, int count)
        {
            if (!NetworkManager.isHost)
            {
                Debugger.OnData(message, count);
            }
        }

        public override void Disconnect()
        {
            isReady = false;
            NetworkManager.current.Disconnect();
        }

        public static implicit operator int(NetworkServer server)
        {
            return server.serverId;
        }

        public static implicit operator NetworkServer(int id)
        {
            return NetworkManager.Client.connection == id ? NetworkManager.Client.connection : null;
        }
    }
}