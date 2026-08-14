// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2025-08-03 02:08:22
// # Recently: 2025-08-03 02:08:22
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;

namespace Astraia
{
    [Serializable]
    internal abstract class Transport
    {
        public string address = "localhost";
        public ushort port = 20974;

        public KcpClient client;
        public KcpServer server;

        public abstract void Register(bool isRemote);
        public abstract void SendToClient(int clientId, ArraySegment<byte> segment, int pass = Pass.KCP);
        public abstract void SendToServer(ArraySegment<byte> segment, int pass = Pass.KCP);
        public abstract void StartServer();
        public abstract void StopServer();
        public abstract void Disconnect(int clientId);
        public abstract void StartClient();
        public abstract void StopClient();
        public abstract void ClientEarlyUpdate();
        public abstract void ClientAfterUpdate();
        public abstract void ServerEarlyUpdate();
        public abstract void ServerAfterUpdate();
    }

    [Serializable]
    internal sealed class NetworkTransport : Transport
    {
        public override void Register(bool isRemote)
        {
            client = new KcpClient();
            server = new KcpServer();
            if (isRemote)
            {
                server.onError = OnServerError;
            }
            else
            {
                client.onError = OnClientError;
            }
        }

        private static void OnServerError(int clientId, Error error, string message)
        {
            if (error != Error.解析失败 && error != Error.连接超时)
            {
                Log.Warn($"客户端: {clientId}  错误代码: {error}\n{message}");
            }
        }

        private static void OnClientError(Error error, string message)
        {
            Log.Warn($"错误代码: {error}\n{message}");
        }

        public override void SendToClient(int clientId, ArraySegment<byte> segment, int pass = Pass.KCP)
        {
            server.Send(clientId, segment, pass);
        }

        public override void SendToServer(ArraySegment<byte> segment, int pass = Pass.KCP)
        {
            client.Send(segment, pass);
        }

        public override void StartServer()
        {
            server.Connect(port);
        }

        public override void StopServer()
        {
            server.StopServer();
        }

        public override void Disconnect(int clientId)
        {
            server.Disconnect(clientId);
        }

        public override void StartClient()
        {
            client.Connect(address, port);
        }

        public override void StopClient()
        {
            client.Disconnect();
        }

        public override void ClientEarlyUpdate()
        {
            client.EarlyUpdate();
        }

        public override void ClientAfterUpdate()
        {
            client.AfterUpdate();
        }

        public override void ServerEarlyUpdate()
        {
            server.EarlyUpdate();
        }

        public override void ServerAfterUpdate()
        {
            server.AfterUpdate();
        }
    }
}