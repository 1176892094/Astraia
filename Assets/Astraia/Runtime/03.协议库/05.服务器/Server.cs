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
using NativeSockets;
using NativeSockets.Udp;

namespace Astraia
{
    internal partial class Server
    {
        private readonly byte[] buffer;
        private readonly Dictionary<int, Client> clients = new Dictionary<int, Client>();
        private readonly HashSet<int> copies = new HashSet<int>();
        private readonly Setting setting;
        private readonly byte[] endPoint;
        private Socket socket;
        private int _size;

        ~Server()
        {
            SocketPal.Cleanup();
        }
        
        public Server(Setting setting, Action<int> OnConnect, Action<int> OnDisconnect, Action<int, Error, string> OnError, Action<int, ArraySegment<byte>, int> OnReceive)
        {
            SocketPal.Initialize();

            this.setting = setting;
            this.OnError = OnError;
            this.OnReceive = OnReceive;
            this.OnConnect = OnConnect;
            this.OnDisconnect = OnDisconnect;
            buffer = new byte[setting.MaxUnit];
            endPoint = new byte[28];
        }

        private event Action<int> OnConnect;
        private event Action<int> OnDisconnect;
        private event Action<int, Error, string> OnError;
        private event Action<int, ArraySegment<byte>, int> OnReceive;

        public void Connect(ushort port)
        {
            if (socket != null)
            {
                Logs.Warn(Log.E121);
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
                    Logs.Warn(Service.Text.Format(Log.E122, e));
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
                Span<byte> socketAddress = endPoint.AsSpan();
                _size = UdpPal.ReceiveFrom(socket, buffer, ref socketAddress);
                segment = new ArraySegment<byte>(buffer, 0, _size);
                var hashCode = new HashCode();
                for (; socketAddress.Length >= 4; socketAddress = socketAddress.Slice(4))
                    hashCode.Add(MemoryMarshal.Read<int>(socketAddress));
                for (int i = 0; i < socketAddress.Length; ++i)
                    hashCode.Add((int)socketAddress[i]);
                clientId = hashCode.ToHashCode();
                return true;
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode == SocketError.WouldBlock)
                {
                    return false;
                }

                Logs.Info(Service.Text.Format(Log.E123, e));
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
            var cookie = (uint)Service.Random.Next();
            UdpPal.CreateIPEndPoint(endPoint.AsSpan(0, _size), out var ipEndPoint);
            return new Client(OnConnect, OnDisconnect, OnError, OnReceive, OnSend, setting, cookie, ipEndPoint);

            void OnConnect(Client client)
            {
                clients.Add(clientId, client);
                Logs.Info(Service.Text.Format(Log.E125, clientId));
                this.OnConnect?.Invoke(clientId);
            }

            void OnDisconnect()
            {
                if (copies.Add(clientId))
                {
                    Logs.Info(Service.Text.Format(Log.E126, clientId));
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
                        UdpPal.SendTo(socket, segment, (IPEndPoint)client.endPoint);
                    }
                }
                catch (SocketException e)
                {
                    if (e.SocketErrorCode == SocketError.WouldBlock)
                    {
                        return;
                    }

                    Logs.Error(Service.Text.Format(Log.E124, e));
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