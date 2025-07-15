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
    internal sealed class Client : Agent
    {
        private readonly byte[] buffer;
        private readonly Setting setting;
        private EndPoint endPoint;
        private Socket socket;

        public Client(Setting setting, Action OnConnect, Action OnDisconnect, Action<Error, string> OnError, Action<ArraySegment<byte>, int> OnReceive) : base(setting)
        {
            this.setting = setting;
            this.OnError = OnError;
            this.OnConnect = OnConnect;
            this.OnReceive = OnReceive;
            this.OnDisconnect = OnDisconnect;
            buffer = new byte[setting.MaxUnit];
        }

        private event Action OnConnect;
        private event Action OnDisconnect;
        private event Action<Error, string> OnError;
        private event Action<ArraySegment<byte>, int> OnReceive;

        public void Connect(string address, ushort port)
        {
            try
            {
                if (state != State.Disconnect)
                {
                    Logs.Warn(Log.E128);
                    return;
                }

                var addresses = Dns.GetHostAddresses(address);
                if (addresses.Length >= 1)
                {
                    Reset(setting);
                    state = State.Connect;
                    endPoint = new IPEndPoint(addresses[0], port);
                    socket = new Socket(endPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
                    Utils.SetBuffer(socket);
                    socket.Connect(endPoint);
                    Logs.Info(Service.Text.Format(Log.E130, addresses[0], port));
                    SendReliable(Reliable.Connect);
                }
            }
            catch (SocketException e)
            {
                LogError(Error.DnsResolve, Service.Text.Format(Log.E141, address, e));
                OnDisconnect?.Invoke();
            }
        }

        private bool TryReceive(out ArraySegment<byte> segment)
        {
            segment = default;
            if (socket == null) return false;
            try
            {
                if (!socket.Poll(0, SelectMode.SelectRead)) return false;
                var size = socket.Receive(buffer, 0, buffer.Length, SocketFlags.None);
                segment = new ArraySegment<byte>(buffer, 0, size);
                return true;
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode == SocketError.WouldBlock)
                {
                    return false;
                }

                Logs.Info(Service.Text.Format(Log.E132, e));
                Disconnect();
                return false;
            }
        }

        public void Send(ArraySegment<byte> segment, int channel)
        {
            if (state == State.Disconnect)
            {
                Logs.Warn(Log.E129);
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

            var channel = segment.Array[segment.Offset];
            Utils.Decode32U(segment.Array, segment.Offset + 1, out var newCookie);
            if (newCookie == 0)
            {
                Logs.Error(Service.Text.Format(Log.E133, cookie, newCookie));
            }

            if (cookie == 0)
            {
                cookie = newCookie;
            }
            else if (cookie != newCookie)
            {
                Logs.Error(Service.Text.Format(Log.E127, endPoint, cookie, newCookie));
                return;
            }

            Input(channel, new ArraySegment<byte>(segment.Array, segment.Offset + 1 + 4, segment.Count - 1 - 4));
        }

        protected override void Connected()
        {
            Logs.Info(Log.E134);
            OnConnect?.Invoke();
        }

        protected override void Send(ArraySegment<byte> segment)
        {
            if (socket == null) return;
            try
            {
                if (socket.Poll(0, SelectMode.SelectWrite))
                {
                    socket.Send(segment.Array, segment.Offset, segment.Count, SocketFlags.None);
                }
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode == SocketError.WouldBlock)
                {
                    return;
                }


                Logs.Info(Service.Text.Format(Log.E131, e));
            }
        }

        protected override void Receive(ArraySegment<byte> message, int channel)
        {
            OnReceive?.Invoke(message, channel);
        }

        protected override void LogError(Error error, string message)
        {
            OnError?.Invoke(error, message);
        }

        protected override void Disconnected()
        {
            Logs.Info(Log.E135);
            OnDisconnect?.Invoke();
            endPoint = null;
            socket?.Close();
            socket = null;
        }

        public override void EarlyUpdate()
        {
            if (state == State.Disconnect)
            {
                return;
            }

            while (TryReceive(out var segment))
            {
                Input(segment);
            }

            base.EarlyUpdate();
        }

        public override void AfterUpdate()
        {
            if (state == State.Disconnect)
            {
                return;
            }

            base.AfterUpdate();
        }
    }
}