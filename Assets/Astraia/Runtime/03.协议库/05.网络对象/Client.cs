// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2025-01-08 19:01:30
// # Recently: 2025-01-08 20:01:57
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Net;
using System.Net.Sockets;
using Astraia.Common;

namespace Astraia
{
    internal sealed class Client : Peer
    {
        private readonly byte[] buffer;
        private readonly Setting setting;
        private readonly Action onConnect;
        private readonly Action onDisconnect;
        private readonly Action<Error, string> onError;
        private readonly Action<ArraySegment<byte>, int> onReceive;
        private Socket socket;
        private EndPoint endPoint;

        public Client(Setting setting, Action onConnect, Action onDisconnect, Action<Error, string> onError, Action<ArraySegment<byte>, int> onReceive) : base(setting)
        {
            this.setting = setting;
            this.onError = onError;
            this.onConnect = onConnect;
            this.onReceive = onReceive;
            this.onDisconnect = onDisconnect;
            buffer = new byte[setting.UnitData];
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
                    Rebuild(setting);
                    state = State.Connect;
                    endPoint = new IPEndPoint(addresses[0], port);
                    socket = new Socket(endPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
                    socket.Buffer();
                    socket.Connect(endPoint);
                    Service.Log.Info("客户端连接到: {0} : {1}", addresses[0], port);
                    SendReliable(Reliable.Connect);
                }
            }
            catch (SocketException e)
            {
                OnError(Error.解析失败, "无法解析主机地址: {0}\n{1}".Format(address, e));
                onDisconnect.Invoke();
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
                    Disconnect();
                }

                return false;
            }
        }

        public void Send(ArraySegment<byte> segment, int channel)
        {
            if (state == State.Disconnect)
            {
                Service.Log.Warn("客户端没有连接，发送消息失败！");
                return;
            }

            SendData(segment, channel);
        }

        private void Input(ArraySegment<byte> segment)
        {
            if (segment.Count <= 1 + 4)
            {
                return;
            }

            var channel = segment.Array![segment.Offset];
            var result = Process.Decode(segment.Array, segment.Offset + 1);
            if (result == 0)
            {
                Service.Log.Error("客户端 {0} 重新验证。旧: {1} 新: {2}", endPoint, userData, result);
            }

            if (userData == 0)
            {
                userData = result;
            }
            else if (userData != result)
            {
                Service.Log.Error("客户端 {0} 移除验证: {1} 预期: {2}", endPoint, userData, result);
                return;
            }

            Input(new ArraySegment<byte>(segment.Array, segment.Offset + 1 + 4, segment.Count - 1 - 4), channel);
        }

        protected override void OnConnected()
        {
            Service.Log.Info("客户端连接成功。");
            onConnect.Invoke();
        }

        protected override void Send(ArraySegment<byte> segment)
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

        protected override void Data(ArraySegment<byte> segment, int channel)
        {
            onReceive.Invoke(segment, channel);
        }

        protected override void OnError(Error error, string message)
        {
            onError.Invoke(error, message);
        }

        protected override void OnDisconnect()
        {
            onDisconnect.Invoke();
            Service.Log.Info("客户端断开连接。");
            endPoint = null;
            socket?.Close();
            socket = null;
        }

        public override void EarlyUpdate()
        {
            if (state != State.Disconnect)
            {
                while (TryReceive(out var segment))
                {
                    Input(segment);
                }

                base.EarlyUpdate();
            }
        }

        public override void AfterUpdate()
        {
            if (state != State.Disconnect)
            {
                base.AfterUpdate();
            }
        }
    }
}