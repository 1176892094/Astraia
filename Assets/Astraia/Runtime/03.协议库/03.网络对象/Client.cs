using System;
using System.Net;
using System.Net.Sockets;

namespace Astraia
{
    internal sealed class Client
    {
        private readonly Setting setting;
        private readonly byte[] buffer;
        private readonly Event onEvent;
        private Agent agent;
        private State state;
        private Socket socket;
        private EndPoint endPoint;

        public Client(Setting setting, Event onEvent)
        {
            this.setting = setting;
            this.onEvent = onEvent;
            buffer = new byte[setting.UnitData];
            state = State.Disconnect;
        }

        public void Connect(string address, ushort port)
        {
            try
            {
                if (state != State.Disconnect)
                {
                    Service.Log.Warn("客户端已经连接!");
                    return;
                }

                var addresses = Dns.GetHostAddresses(address);
                if (addresses.Length >= 1)
                {
                    Register(setting);
                    state = State.Connect;
                    endPoint = new IPEndPoint(addresses[0], port);
                    socket = new Socket(endPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
                    Common.Blocked(socket);
                    socket.Connect(endPoint);
                    Service.Log.Info("客户端连接到: {0} : {1}", addresses[0], port);
                    agent.Handshake();
                }
            }
            catch (SocketException e)
            {
                onEvent.Error(Error.解析失败, "无法解析主机地址: {0}\n{1}".Format(address, e));
                onEvent.Disconnect();
            }
        }

        public void Send(ArraySegment<byte> segment, int channel)
        {
            if (state != State.Disconnect)
            {
                agent.SendData(segment, channel);
            }
        }

        private bool TryReceive(out ArraySegment<byte> segment)
        {
            segment = default;
            try
            {
                if (socket != null && socket.Poll(0, SelectMode.SelectRead))
                {
                    var count = socket.Receive(buffer, 0, buffer.Length, SocketFlags.None);
                    segment = new ArraySegment<byte>(buffer, 0, count);
                    return true;
                }

                return false;
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode != SocketError.WouldBlock)
                {
                    Service.Log.Info("客户端接收消息失败!\n{0}", e);
                    agent.Disconnect();
                }

                return false;
            }
        }

        public void Disconnect()
        {
            if (state != State.Disconnect)
            {
                agent.Disconnect();
            }
        }

        private void Register(Setting setting)
        {
            if (agent == null)
            {
                var newEvent = new Event();
                agent = new Agent(setting, newEvent, "客户端");
                newEvent.Connect = OnConnect;
                newEvent.Disconnect = OnDisconnect;
                newEvent.Error = OnError;
                newEvent.Receive = OnReceive;
                newEvent.Send = OnSend;
            }
            else
            {
                agent.Rebuild(setting);
            }
        }

        private void OnConnect()
        {
            Service.Log.Info("客户端连接成功。");
            state = State.Connected;
            onEvent.Connect.Invoke();
        }

        private void OnDisconnect()
        {
            Service.Log.Info("客户端断开连接。");
            state = State.Disconnect;
            socket.Close();
            socket = null;
            endPoint = null;
            onEvent.Disconnect.Invoke();
        }

        private void OnError(Error error, string message)
        {
            onEvent.Error?.Invoke(error, message);
        }

        private void OnReceive(ArraySegment<byte> segment, int channel)
        {
            onEvent.Receive.Invoke(segment, channel);
        }

        private void OnSend(ArraySegment<byte> segment)
        {
            if (socket == null) return;
            try
            {
                if (socket.Poll(0, SelectMode.SelectWrite))
                {
                    socket.Send(segment.Array!, segment.Offset, segment.Count, SocketFlags.None);
                }
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode == SocketError.WouldBlock)
                {
                    return;
                }

                Service.Log.Info("客户端发送消息失败!\n{0}", e);
            }
        }

        public void EarlyUpdate()
        {
            if (state != State.Disconnect)
            {
                while (TryReceive(out var segment))
                {
                    agent.Input(segment);
                }

                agent.EarlyUpdate();
            }
        }

        public void AfterUpdate()
        {
            if (state != State.Disconnect)
            {
                agent.AfterUpdate();
            }
        }
    }
}