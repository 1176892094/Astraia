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
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace Astraia
{
    [Serializable]
    internal sealed class KcpServer
    {
        private readonly Dictionary<int, KcpClient> clients = new Dictionary<int, KcpClient>();
        private readonly HashSet<int> removes = new HashSet<int>();
        private readonly byte[] buffer;

        private readonly Setting setting;

        private Socket socket;
        private EndPoint endPoint;

        public Action<int> onConnect;
        public Action<int> onDisconnect;
        public Action<int, Error, string> onError;
        public Action<int, ArraySegment<byte>> onSend;
        public Action<int, ArraySegment<byte>, int> onReceive;

        public KcpServer(Setting setting)
        {
            this.setting = setting;
            buffer = new byte[setting.MaxUnit];
            endPoint = setting.DualMode ? new IPEndPoint(IPAddress.IPv6Any, 0) : new IPEndPoint(IPAddress.Any, 0);
        }

        public void Connect(ushort port)
        {
            if (socket != null)
            {
                Log.Warn("服务器已经连接!");
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
                    Log.Warn("服务器不支持双连接模式!\n{0}", e);
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

            Common.Blocked(socket);
        }

        public void Send(int id, ArraySegment<byte> segment, int pass)
        {
            if (clients.TryGetValue(id, out var client))
            {
                client.kcpPeer.SendData(segment, pass);
                onSend?.Invoke(id, segment);
            }
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
                    Log.Info("服务器发送消息失败!\n{0}", e);
                }

                return false;
            }
        }

        public void Disconnect(int id)
        {
            if (clients.TryGetValue(id, out var client))
            {
                client.kcpPeer.Disconnect();
            }
        }

        private KcpClient Register(int id)
        {
            var kcpPeer = new KcpPeer(setting, "服务器");
            var client = new KcpClient(kcpPeer, endPoint);
            kcpPeer.onConnect = OnConnect;
            kcpPeer.onDisconnect = OnDisconnect;
            kcpPeer.onError = OnError;
            kcpPeer.onReceive = OnReceive;
            kcpPeer.onSend = OnSend;
            return client;

            void OnConnect()
            {
                Log.Info("客户端 {0} 连接到服务器。", id);
                clients.Add(id, client);
                client.kcpPeer.Handshake();
                onConnect.Invoke(id);
            }

            void OnDisconnect()
            {
                Log.Info("客户端 {0} 从服务器断开。", id);
                removes.Add(id);
                onDisconnect.Invoke(id);
            }

            void OnError(Error error, string reason)
            {
                onError?.Invoke(id, error, reason);
            }

            void OnReceive(ArraySegment<byte> message, int pass)
            {
                onReceive.Invoke(id, message, pass);
            }

            void OnSend(ArraySegment<byte> segment)
            {
                try
                {
                    if (clients.TryGetValue(id, out var result))
                    {
                        if (socket.Poll(0, SelectMode.SelectWrite))
                        {
                            socket.SendTo(segment.Array!, segment.Offset, segment.Count, SocketFlags.None, result.endPoint);
                        }
                    }
                }
                catch (SocketException e)
                {
                    if (e.SocketErrorCode != SocketError.WouldBlock)
                    {
                        Log.Error("服务器发送消息失败!\n{0}", e);
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
                    client.kcpPeer.Input(segment);
                    client.kcpPeer.EarlyUpdate();
                }
                else
                {
                    client.kcpPeer.Input(segment);
                }
            }

            foreach (var client in clients.Values)
            {
                client.kcpPeer.EarlyUpdate();
            }

            foreach (var client in removes)
            {
                clients.Remove(client);
            }

            removes.Clear();
        }

        public void AfterUpdate()
        {
            foreach (var client in clients.Values)
            {
                client.kcpPeer.AfterUpdate();
            }
        }

        public void StopServer()
        {
            clients.Clear();
            socket?.Close();
            socket = null;
        }

        private class KcpClient
        {
            public readonly KcpPeer kcpPeer;
            public readonly EndPoint endPoint;

            public KcpClient(KcpPeer kcpPeer, EndPoint endPoint)
            {
                this.kcpPeer = kcpPeer;
                this.endPoint = endPoint;
            }
        }
    }
}