using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace Astraia
{
    internal sealed class KcpServer
    {
        public class Event
        {
            public Action<int> Connect;
            public Action<int> Disconnect;
            public Action<int, Error, string> Error;
            public Action<int, ArraySegment<byte>> Send;
            public Action<int, ArraySegment<byte>, int> Receive;
        }

        private readonly Dictionary<int, KcpClient> clients = new Dictionary<int, KcpClient>();
        private readonly HashSet<int> removes = new HashSet<int>();
        private readonly byte[] buffer;
        private readonly Event onEvent;
        private readonly Setting setting;
        private Socket socket;
        private EndPoint endPoint;

        public KcpServer(Setting setting, Event onEvent)
        {
            this.setting = setting;
            this.onEvent = onEvent;
            buffer = new byte[setting.UnitData];
            endPoint = setting.DualMode ? new IPEndPoint(IPAddress.IPv6Any, 0) : new IPEndPoint(IPAddress.Any, 0);
        }

        public void Connect(ushort port)
        {
            if (socket != null)
            {
                Service.Log.Warn("服务器已经连接!");
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
                    Service.Log.Warn("服务器不支持双连接模式!\n{0}", e);
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

        public void Send(int id, ArraySegment<byte> segment, int channel)
        {
            if (clients.TryGetValue(id, out var client))
            {
                client.kcpPeer.SendData(segment, channel);
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
                    Service.Log.Info("服务器发送消息失败!\n{0}", e);
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
            var newEvent = new Astraia.KcpClient.Event();
            var client = new KcpClient(new KcpPeer(setting, newEvent, "服务器"), endPoint);
            newEvent.Connect = OnConnect;
            newEvent.Disconnect = OnDisconnect;
            newEvent.Error = OnError;
            newEvent.Receive = OnReceive;
            newEvent.Send = OnSend;
            return client;

            void OnConnect()
            {
                Service.Log.Info("客户端 {0} 连接到服务器。", id);
                clients.Add(id, client);
                client.kcpPeer.Handshake();
                onEvent.Connect.Invoke(id);
            }

            void OnDisconnect()
            {
                Service.Log.Info("客户端 {0} 从服务器断开。", id);
                removes.Add(id);
                onEvent.Disconnect.Invoke(id);
            }

            void OnError(Error error, string reason)
            {
                onEvent.Error?.Invoke(id, error, reason);
            }

            void OnReceive(ArraySegment<byte> message, int channel)
            {
                onEvent.Receive.Invoke(id, message, channel);
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
                        Service.Log.Error("服务器接收消息失败!\n{0}", e);
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