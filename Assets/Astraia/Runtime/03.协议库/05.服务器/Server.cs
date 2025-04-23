// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2025-01-08 19:01:30
// # Recently: 2025-01-08 20:01:58
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace Astraia
{
    internal partial class Server
    {
        private readonly byte[] buffer;
        private readonly Dictionary<int, Client> clients = new Dictionary<int, Client>();
        private readonly HashSet<int> copies = new HashSet<int>();
        private readonly Setting setting;
        private EndPoint endPoint;
        private Socket socket;

        public Server(Setting setting, Action<int> OnConnect, Action<int> OnDisconnect, Action<int, Error, string> OnError, Action<int, ArraySegment<byte>, int> OnReceive)
        {
            this.setting = setting;
            this.OnError = OnError;
            this.OnReceive = OnReceive;
            this.OnConnect = OnConnect;
            this.OnDisconnect = OnDisconnect;
            buffer = new byte[setting.MaxUnit];
            endPoint = setting.DualMode ? new IPEndPoint(IPAddress.IPv6Any, 0) : new IPEndPoint(IPAddress.Any, 0);
        }

        private event Action<int> OnConnect;
        private event Action<int> OnDisconnect;
        private event Action<int, Error, string> OnError;
        private event Action<int, ArraySegment<byte>, int> OnReceive;

        public void Connect(ushort port)
        {
            if (socket != null)
            {
                Log.Warn(Logs.E121);
                return;
            }

            if (setting.DualMode)
            {
                socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);
                try
                {
                    socket.DualMode = true;
                }
                catch (NotSupportedException e)
                {
                    Log.Warn(Service.Text.Format(Logs.E122, e));
                }

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    const uint IOC_IN = 0x80000000U;
                    const uint IOC_VENDOR = 0x18000000U;
                    const int SIO_UDP_RESET = unchecked((int)(IOC_IN | IOC_VENDOR | 12));
                    socket.IOControl(SIO_UDP_RESET, new byte[] { 0x00 }, null);
                }

                socket.Bind(new IPEndPoint(IPAddress.IPv6Any, port));
            }
            else
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                socket.Bind(new IPEndPoint(IPAddress.Any, port));
            }

            Utils.SetBuffer(socket);
        }

        private bool TryReceive(out ArraySegment<byte> segment, out int clientId)
        {
            clientId = 0;
            segment = default;
            if (socket == null) return false;
            try
            {
                if (!socket.Poll(0, SelectMode.SelectRead)) return false;
                var size = socket.ReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref endPoint);
                segment = new ArraySegment<byte>(buffer, 0, size);
                clientId = endPoint.GetHashCode();
                return true;
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode == SocketError.WouldBlock)
                {
                    return false;
                }

                Log.Info(Service.Text.Format(Logs.E123, e));
                return false;
            }
        }

        public void Send(int clientId, ArraySegment<byte> segment, int channel)
        {
            if (clients.TryGetValue(clientId, out var client))
            {
                client.SendData(segment, channel);
            }
        }

        public void Disconnect(int clientId)
        {
            if (clients.TryGetValue(clientId, out var client))
            {
                client.Disconnect();
            }
        }

        private Client AddClient(int clientId)
        {
            return new Client(OnConnect, OnDisconnect, OnError, OnReceive, OnSend, setting, Service.Hash.Id(), endPoint);

            void OnConnect(Client client)
            {
                clients.Add(clientId, client);
                Log.Info(Service.Text.Format(Logs.E125, clientId));
                this.OnConnect?.Invoke(clientId);
            }

            void OnDisconnect()
            {
                if (copies.Add(clientId))
                {
                    Log.Info(Service.Text.Format(Logs.E126, clientId));
                }

                this.OnDisconnect?.Invoke(clientId);
            }

            void OnError(Error error, string message)
            {
                this.OnError?.Invoke(clientId, error, message);
            }

            void OnReceive(ArraySegment<byte> message, int channel)
            {
                this.OnReceive?.Invoke(clientId, message, channel);
            }

            void OnSend(ArraySegment<byte> segment)
            {
                try
                {
                    if (!clients.TryGetValue(clientId, out var client))
                    {
                        // Log.Warn($"服务器向无效的客户端发送信息。客户端：{clientId}");
                        return;
                    }

                    if (socket.Poll(0, SelectMode.SelectWrite))
                    {
                        socket.SendTo(segment.Array, segment.Offset, segment.Count, SocketFlags.None, client.endPoint);
                    }
                }
                catch (SocketException e)
                {
                    if (e.SocketErrorCode == SocketError.WouldBlock)
                    {
                        return;
                    }

                    Log.Error(Service.Text.Format(Logs.E124, e));
                }
            }
        }

        public void EarlyUpdate()
        {
            while (TryReceive(out var segment, out var clientId))
            {
                if (!clients.TryGetValue(clientId, out var client))
                {
                    client = AddClient(clientId);
                    client.Input(segment);
                    client.EarlyUpdate();
                }

                else
                {
                    client.Input(segment);
                }
            }

            foreach (var client in clients.Values)
            {
                client.EarlyUpdate();
            }

            foreach (var client in copies)
            {
                clients.Remove(client);
            }

            copies.Clear();
        }

        public void AfterUpdate()
        {
            foreach (var client in clients.Values)
            {
                client.AfterUpdate();
            }
        }

        public void StopServer()
        {
            clients.Clear();
            socket?.Close();
            socket = null;
        }
    }
}