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
        private readonly Dictionary<int, Client> clients = new Dictionary<int, Client>();
        private readonly HashSet<int> copies = new HashSet<int>();
        private readonly byte[] buffer;
        private readonly Setting setting;
        private readonly Action<int> onConnect;
        private readonly Action<int> onDisconnect;
        private readonly Action<int, Error, string> onError;
        private readonly Action<int, ArraySegment<byte>, int> onReceive;

        private Socket socket;
        private EndPoint endPoint;

        public Server(Setting setting, Action<int> onConnect, Action<int> onDisconnect, Action<int, Error, string> onError, Action<int, ArraySegment<byte>, int> onReceive)
        {
            this.setting = setting;
            this.onError = onError;
            this.onConnect = onConnect;
            this.onReceive = onReceive;
            this.onDisconnect = onDisconnect;
            buffer = new byte[setting.MaxData];
            endPoint = setting.DualMode ? new IPEndPoint(IPAddress.IPv6Any, 0) : new IPEndPoint(IPAddress.Any, 0);
        }

        public void Connect(ushort port)
        {
            if (socket != null)
            {
                Log.Warn(Log.E121);
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
                    Log.Warn(Log.E122.Format(e));
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

            socket.Buffer();
        }

        private bool TryReceive(out int id, out ArraySegment<byte> segment)
        {
            id = 0;
            segment = default;
            try
            {
                if (socket != null && socket.Poll(0, SelectMode.SelectRead))
                {
                    var count = socket.ReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref endPoint);
                    segment = new ArraySegment<byte>(buffer, 0, count);
                    id = endPoint.GetHashCode();
                    return true;
                }

                return false;
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode != SocketError.WouldBlock)
                {
                    Log.Info(Log.E123.Format(e));
                }

                return false;
            }
        }

        public void Send(int id, ArraySegment<byte> segment, int channel)
        {
            if (clients.TryGetValue(id, out var client))
            {
                client.SendData(segment, channel);
            }
        }

        public void Disconnect(int id)
        {
            if (clients.TryGetValue(id, out var client))
            {
                client.Disconnect();
            }
        }

        private Client Register(int id)
        {
            return new Client(OnConnect, OnDisconnect, OnError, OnReceive, OnSend, setting, (uint)Service.Random.Next(), endPoint);

            void OnConnect(Client client)
            {
                clients.Add(id, client);
                Log.Info(Log.E125.Format(id));
                onConnect.Invoke(id);
            }

            void OnDisconnect()
            {
                copies.Add(id);
                Log.Info(Log.E126.Format(id));
                onDisconnect.Invoke(id);
            }

            void OnError(Error error, string message)
            {
                onError.Invoke(id, error, message);
            }

            void OnReceive(ArraySegment<byte> message, int channel)
            {
                onReceive.Invoke(id, message, channel);
            }

            void OnSend(ArraySegment<byte> segment)
            {
                try
                {
                    if (clients.TryGetValue(id, out var client))
                    {
                        if (socket.Poll(0, SelectMode.SelectWrite))
                        {
                            socket.SendTo(segment.Array!, segment.Offset, segment.Count, SocketFlags.None, client.endPoint);
                        }
                    }
                }
                catch (SocketException e)
                {
                    if (e.SocketErrorCode != SocketError.WouldBlock)
                    {
                        Log.Error(Log.E124.Format(e));
                    }
                }
            }
        }

        public void EarlyUpdate()
        {
            while (TryReceive(out var id, out var segment))
            {
                if (!clients.TryGetValue(id, out var client))
                {
                    client = Register(id);
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