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

namespace Astraia.Net
{
    public sealed class KcpTransport : Transport
    {
        public uint unitData = 1200;
        public uint overTime = 10000;
        public uint interval = 10;
        public uint deadLink = 40;
        public uint fastResend = 2;
        public uint sendWindow = 1024 * 4;
        public uint receiveWindow = 1024 * 4;

        private Client client;
        private Server server;

        private void Awake()
        {
            var setting = new Setting(unitData, overTime, interval, deadLink, fastResend, sendWindow, receiveWindow);
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
                Log.Warn("{0}: {1}", error, message);
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

        public override int GetLength(int channel)
        {
            return channel == Channel.Reliable ? Module.KcpLength(unitData, receiveWindow) : Module.UdpLength(unitData);
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