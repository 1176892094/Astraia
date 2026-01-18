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
using Astraia.Common;
using UnityEngine;

namespace Astraia.Net
{
    public abstract class Transport : MonoBehaviour
    {
        public string address = "localhost";
        public ushort port = 20974;

        public Action OnClientConnect;
        public Action OnClientDisconnect;
        public Action<ArraySegment<byte>, int> OnClientSend;
        public Action<ArraySegment<byte>, int> OnClientReceive;
        public Action<int> OnServerConnect;
        public Action<int> OnServerDisconnect;
        public Action<int, ArraySegment<byte>, int> OnServerSend;
        public Action<int, ArraySegment<byte>, int> OnServerReceive;

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

    public sealed class NetworkTransport : Transport
    {
        private const uint MAX_MTU = 1200;
        private const uint OVER_TIME = 10000;
        private const uint INTERVAL = 10;
        private const uint DEAD_LINK = 40;
        private const uint FAST_RESEND = 2;
        private const uint SEND_WIN = 1024 * 4;
        private const uint RECEIVE_WIN = 1024 * 4;

        private Client client;
        private Server server;

        private void Awake()
        {
            var setting = new Setting(MAX_MTU, OVER_TIME, INTERVAL, DEAD_LINK, FAST_RESEND, SEND_WIN, RECEIVE_WIN);
            client = new Client(setting, ClientConnect, ClientDisconnect, ClientError, ClientReceive);
            server = new Server(setting, ServerConnect, ServerDisconnect, ServerError, ServerReceive);
            return;

            void ClientConnect()
            {
                OnClientConnect.Invoke();
            }

            void ClientDisconnect()
            {
                OnClientDisconnect.Invoke();
            }

            void ClientError(Error error, string message)
            {
                Service.Log.Warn("{0}: {1}", error, message);
            }

            void ClientReceive(ArraySegment<byte> message, int channel)
            {
                OnClientReceive.Invoke(message, channel);
            }

            void ServerConnect(int id)
            {
                OnServerConnect.Invoke(id);
            }

            void ServerDisconnect(int id)
            {
                OnServerDisconnect.Invoke(id);
            }

            void ServerError(int id, Error error, string message)
            {
            }

            void ServerReceive(int id, ArraySegment<byte> message, int channel)
            {
                OnServerReceive.Invoke(id, message, channel);
            }
        }

        public override uint GetLength(int channel)
        {
            return channel == Channel.Reliable ? Peer.KcpLength(MAX_MTU, RECEIVE_WIN) : Peer.UdpLength(MAX_MTU);
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

        public override void SendToClient(int clientId, ArraySegment<byte> segment, int channel = Channel.Reliable)
        {
            server.Send(clientId, segment, channel);
            OnServerSend?.Invoke(clientId, segment, channel);
        }

        public override void StartClient()
        {
            client.Connect(address, port);
        }

        public override void StartClient(Uri uri)
        {
            client.Connect(uri.Host, (ushort)(uri.IsDefaultPort ? port : uri.Port));
        }

        public override void Disconnect()
        {
            client.Disconnect();
        }

        public override void SendToServer(ArraySegment<byte> segment, int channel = Channel.Reliable)
        {
            client.Send(segment, channel);
            OnClientSend?.Invoke(segment, channel);
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