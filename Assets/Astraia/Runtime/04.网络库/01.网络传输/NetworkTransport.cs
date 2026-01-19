// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2024-11-29 13:11:20
// # Recently: 2024-12-22 20:12:20
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using UnityEngine;

namespace Astraia.Net
{
    internal abstract class Transport : MonoBehaviour
    {
        public string address = "localhost";
        public ushort port = 20974;

        public readonly Event client = new Event();
        public readonly Event<int> server = new Event<int>();

        public abstract uint GetLength(int channel);
        public abstract void SendToClient(int clientId, ArraySegment<byte> segment, int channel = Channel.Reliable);
        public abstract void SendToServer(ArraySegment<byte> segment, int channel = Channel.Reliable);
        public abstract void StartServer();
        public abstract void StopServer();
        public abstract void Disconnect(int clientId);
        public abstract void StartClient();
        public abstract void StartClient(Uri uri);
        public abstract void Disconnect();
        public abstract void ClientEarlyUpdate();
        public abstract void ClientAfterUpdate();
        public abstract void ServerEarlyUpdate();
        public abstract void ServerAfterUpdate();
    }

    internal sealed class NetworkTransport : Transport
    {
        private const uint MAX_MTU = 1200;
        private const uint OVER_TIME = 10000;
        private const uint INTERVAL = 10;
        private const uint DEAD_LINK = 40;
        private const uint FAST_RESEND = 2;
        private const uint SEND_WIN = 1024 * 4;
        private const uint RECEIVE_WIN = 1024 * 4;

        private Client clientAgent;
        private Server serverAgent;

        private void Awake()
        {
            var setting = new Setting(MAX_MTU, OVER_TIME, INTERVAL, DEAD_LINK, FAST_RESEND, SEND_WIN, RECEIVE_WIN);
            clientAgent = new Client(setting, client);
            serverAgent = new Server(setting, server);
            client.Error = (error, message) => Service.Log.Warn("{0}: {1}", error, message);
        }

        public override uint GetLength(int channel)
        {
            return channel == Channel.Reliable ? Agent.KcpLength(MAX_MTU, RECEIVE_WIN) : Agent.UdpLength(MAX_MTU);
        }

        public override void SendToClient(int clientId, ArraySegment<byte> segment, int channel = Channel.Reliable)
        {
            serverAgent.Send(clientId, segment, channel);
            server.Send?.Invoke(clientId, segment);
        }

        public override void SendToServer(ArraySegment<byte> segment, int channel = Channel.Reliable)
        {
            clientAgent.Send(segment, channel);
            client.Send?.Invoke(segment);
        }

        public override void StartServer()
        {
            serverAgent.Connect(port);
        }

        public override void StopServer()
        {
            serverAgent.StopServer();
        }

        public override void Disconnect(int clientId)
        {
            serverAgent.Disconnect(clientId);
        }

        public override void StartClient()
        {
            clientAgent.Connect(address, port);
        }

        public override void StartClient(Uri uri)
        {
            clientAgent.Connect(uri.Host, (ushort)(uri.IsDefaultPort ? port : uri.Port));
        }

        public override void Disconnect()
        {
            clientAgent.Disconnect();
        }

        public override void ClientEarlyUpdate()
        {
            clientAgent.EarlyUpdate();
        }

        public override void ClientAfterUpdate()
        {
            clientAgent.AfterUpdate();
        }

        public override void ServerEarlyUpdate()
        {
            serverAgent.EarlyUpdate();
        }

        public override void ServerAfterUpdate()
        {
            serverAgent.AfterUpdate();
        }
    }
}