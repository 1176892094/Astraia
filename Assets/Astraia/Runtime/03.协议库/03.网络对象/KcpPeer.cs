using System;
using System.Diagnostics;
using System.Net.Sockets;
using Event = Astraia.KcpClient.Event;

namespace Astraia
{
    internal sealed class KcpPeer
    {
        private const int PING_INTERVAL = 1000;
        private const int METADATA_SIZE = sizeof(byte) + sizeof(int);
        private readonly byte[] rawSendBuffer;
        private readonly byte[] kcpSendBuffer;
        private readonly byte[] kcpDataBuffer;
        private readonly uint kcpLength;
        private readonly uint udpLength;
        private readonly string userName;
        private readonly Stopwatch watch = new Stopwatch();
        private readonly Event onEvent;
        private uint overTime;
        private uint pingTime;
        private uint prevTime;
        private uint userData;
        private Protocol kcp;
        private State state;
        private uint Time => (uint)watch.ElapsedMilliseconds;

        public KcpPeer(Setting setting, Event onEvent, string userName, uint userData = 0)
        {
            Rebuild(setting);
            this.onEvent = onEvent;
            this.userName = userName;
            this.userData = userData;
            udpLength = UdpLength(setting.UnitData);
            kcpLength = KcpLength(setting.UnitData, setting.ReceiveWindow);
            kcpDataBuffer = new byte[1 + kcpLength];
            kcpSendBuffer = new byte[1 + kcpLength];
            rawSendBuffer = new byte[setting.UnitData];
        }

        public void Rebuild(Setting setting)
        {
            userData = 0;
            pingTime = 0;
            prevTime = 0;
            overTime = setting.Timeout;
            kcp = new Protocol(0, SendReliable);
            kcp.SetData(setting.UnitData - METADATA_SIZE, setting.DeadLink);
            kcp.SetDelay(setting.NoDelay, setting.Interval, setting.FastResend, !setting.Congestion);
            kcp.SetWindow(setting.SendWindow, setting.ReceiveWindow);
            state = State.Connect;
            watch.Restart();
        }

        private void SendReliable(byte[] bytes, int count)
        {
            rawSendBuffer[0] = Channel.Reliable;
            Common.Encode(rawSendBuffer, 1, userData);
            Buffer.BlockCopy(bytes, 0, rawSendBuffer, 1 + 4, count);
            onEvent.Send(new ArraySegment<byte>(rawSendBuffer, 0, count + 1 + 4));
        }

        public static uint KcpLength(uint count, uint window)
        {
            return (count - Kcp.IKCP_OVERHEAD - METADATA_SIZE) * (Math.Min(window, 255) - 1) - 1;
        }

        public static uint UdpLength(uint count)
        {
            return count - METADATA_SIZE;
        }

        public void Handshake()
        {
            SendReliable(Opcode.Connect, new ArraySegment<byte>(BitConverter.GetBytes(userData)));
        }

        private bool TryReceive(out Opcode message, out ArraySegment<byte> segment)
        {
            segment = default;
            message = Opcode.Disconnect;
            var count = kcp.PeekSize();
            if (count <= 0)
            {
                return false;
            }

            if (count > kcpDataBuffer.Length)
            {
                onEvent.Error(Error.无效接收, "{0}接收网络消息过大。消息大小: {1} < {2}。".Format(userName, kcpDataBuffer.Length, count));
                Disconnect();
                return false;
            }

            if (kcp.Receive(kcpDataBuffer, count) < 0)
            {
                onEvent.Error(Error.无效接收, "{0}接收网络消息失败。".Format(userName));
                Disconnect();
                return false;
            }

            message = (Opcode)kcpDataBuffer[0];
            segment = new ArraySegment<byte>(kcpDataBuffer, 1, count - 1);
            prevTime = Time;
            return true;
        }

        public void Input(ArraySegment<byte> segment)
        {
            if (segment.Count <= 1 + 4)
            {
                return;
            }

            var channel = segment.Array![segment.Offset];
            var newData = Common.Decode(segment.Array, segment.Offset + 1);
            if (state == State.Connected && newData != userData)
            {
                Service.Log.Warn("{0}数据校验失败。旧: {1} 新: {2}", userName, userData, newData);
                return;
            }

            var message = new ArraySegment<byte>(segment.Array, segment.Offset + 1 + 4, segment.Count - 1 - 4);
            if (channel == Channel.Reliable)
            {
                if (kcp.Input(message.Array, message.Offset, message.Count) != 0)
                {
                    Service.Log.Warn("{0}发送可靠消息失败。消息大小: {1}", userName, message.Count - 1);
                }
            }
            else if (channel == Channel.Unreliable)
            {
                if (state == State.Connected)
                {
                    onEvent.Receive(message, Channel.Unreliable);
                    prevTime = Time;
                }
            }
        }

        private void SendReliable(Opcode message, ArraySegment<byte> segment = default)
        {
            if (segment.Count + 1 > kcpSendBuffer.Length)
            {
                onEvent.Error(Error.无效发送, "{0}发送网络消息过大。消息大小: {1} < {2}".Format(userName, segment.Count, kcpLength));
                return;
            }

            kcpSendBuffer[0] = (byte)message;
            if (segment.Count > 0)
            {
                Buffer.BlockCopy(segment.Array!, segment.Offset, kcpSendBuffer, 1, segment.Count);
            }

            if (kcp.Send(kcpSendBuffer, 0, segment.Count + 1) < 0)
            {
                onEvent.Error(Error.无效发送, "{0}发送网络消息失败。消息大小: {1}。".Format(userName, segment.Count));
            }
        }

        private void SendUnreliable(ArraySegment<byte> segment)
        {
            if (segment.Count > udpLength)
            {
                Service.Log.Error("{0}发送不可靠消息失败。消息大小: {1}", userName, segment.Count);
                return;
            }

            rawSendBuffer[0] = Channel.Unreliable;
            Common.Encode(rawSendBuffer, 1, userData);
            if (segment.Count > 0)
            {
                Buffer.BlockCopy(segment.Array!, segment.Offset, rawSendBuffer, 1 + 4, segment.Count);
            }

            onEvent.Send(new ArraySegment<byte>(rawSendBuffer, 0, segment.Count + 1 + 4));
        }

        public void SendData(ArraySegment<byte> segment, int channel)
        {
            if (segment.Count == 0)
            {
                onEvent.Error(Error.无效发送, "{0}尝试发送空消息。".Format(userName));
                Disconnect();
                return;
            }

            switch (channel)
            {
                case Channel.Reliable:
                    SendReliable(Opcode.Data, segment);
                    break;
                case Channel.Unreliable:
                    SendUnreliable(segment);
                    break;
            }
        }

        public void Disconnect()
        {
            if (state == State.Disconnect) return;
            try
            {
                SendReliable(Opcode.Disconnect);
                kcp.Flush();
            }
            finally
            {
                state = State.Disconnect;
                onEvent.Disconnect();
            }
        }

        private void BeforeReceive(uint sinceTime)
        {
            if (sinceTime >= prevTime + overTime)
            {
                onEvent.Error(Error.连接超时, "{0}在{1}秒内没有收到任何消息后的连接超时！".Format(userName, overTime / 1000));
                Disconnect();
            }

            if (kcp.State == unchecked((uint)-1))
            {
                onEvent.Error(Error.连接超时, "{0}网络消息被重传了{1}次而没有得到确认！".Format(userName, kcp.Death));
                Disconnect();
            }

            if (sinceTime >= pingTime + PING_INTERVAL)
            {
                SendReliable(Opcode.Ping);
                pingTime = sinceTime;
            }

            if (kcp.Count >= 10000)
            {
                onEvent.Error(Error.网络拥塞, "{0}断开连接，因为它处理数据的速度不够快！".Format(userName));
                kcp.Clear();
                Disconnect();
            }
        }

        private void UpdateConnect()
        {
            BeforeReceive(Time);
            if (TryReceive(out var message, out var segment))
            {
                switch (message)
                {
                    case Opcode.Data:
                        onEvent.Error(Error.无效接收, "{0}接收无效的网络消息。消息类型: {1}".Format(userName, message));
                        Disconnect();
                        break;
                    case Opcode.Connect when segment.Count != 4:
                        onEvent.Error(Error.无效接收, "{0}接收无效的网络消息。消息类型: {1}".Format(userName, message));
                        Disconnect();
                        return;
                    case Opcode.Connect:
                        state = State.Connected;
                        userData = Common.Decode(segment.Array, segment.Offset);
                        onEvent.Connect();
                        break;
                }
            }
        }

        private void UpdateConnected()
        {
            BeforeReceive(Time);
            while (TryReceive(out var message, out var segment))
            {
                switch (message)
                {
                    case Opcode.Connect:
                        onEvent.Error(Error.无效接收, "{0}接收无效的网络消息。消息类型: {1}".Format(userName, message));
                        Disconnect();
                        break;
                    case Opcode.Data when segment.Count == 0:
                        onEvent.Error(Error.无效接收, "{0}收到无效的网络消息。消息类型: {1}".Format(userName, message));
                        Disconnect();
                        break;
                    case Opcode.Data:
                        onEvent.Receive(segment, Channel.Reliable);
                        break;
                    case Opcode.Disconnect:
                        Disconnect();
                        break;
                }
            }
        }

        public void EarlyUpdate()
        {
            try
            {
                switch (state)
                {
                    case State.Connect:
                        UpdateConnect();
                        break;
                    case State.Connected:
                        UpdateConnected();
                        break;
                }
            }
            catch (SocketException e)
            {
                onEvent.Error(Error.连接关闭, "{0}网络发生异常，断开连接。\n{1}".Format(userName, e));
                Disconnect();
            }
            catch (ObjectDisposedException e)
            {
                onEvent.Error(Error.连接关闭, "{0}网络发生异常，断开连接。\n{1}".Format(userName, e));
                Disconnect();
            }
            catch (Exception e)
            {
                onEvent.Error(Error.未知异常, "{0}网络发生异常，断开连接。\n{1}".Format(userName, e));
                Disconnect();
            }
        }

        public void AfterUpdate()
        {
            try
            {
                if (state != State.Disconnect)
                {
                    kcp.Update(Time);
                }
            }
            catch (SocketException e)
            {
                onEvent.Error(Error.连接关闭, "{0}网络发生异常，断开连接。\n{1}".Format(userName, e));
                Disconnect();
            }
            catch (ObjectDisposedException e)
            {
                onEvent.Error(Error.连接关闭, "{0}网络发生异常，断开连接。\n{1}".Format(userName, e));
                Disconnect();
            }
            catch (Exception e)
            {
                onEvent.Error(Error.未知异常, "{0}网络发生异常，断开连接。\n{1}".Format(userName, e));
                Disconnect();
            }
        }
    }
}