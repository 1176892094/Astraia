// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2025-01-08 19:01:30
// # Recently: 2025-01-08 20:01:54
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Diagnostics;
using System.Net.Sockets;

namespace Astraia.Common
{
    internal abstract class Module
    {
        private const int INTERVAL_PING = 1000;
        private const int METADATA_SIZE = sizeof(byte) + sizeof(int);
        private readonly byte[] rawSendBuffer;
        private readonly byte[] kcpSendBuffer;
        private readonly byte[] kcpDataBuffer;
        private readonly uint kcpLength;
        private readonly uint udpLength;
        private readonly Stopwatch watch = new Stopwatch();
        private uint overTime;
        private uint pingTime;
        private uint lastTime;
        private Protocol kcp;
        protected State state;
        protected uint userData;
        private uint keepTime => (uint)watch.ElapsedMilliseconds;

        protected Module(Setting setting, uint userData = 0)
        {
            Rebuild(setting);
            this.userData = userData;
            udpLength = UdpLength(setting.UnitData);
            kcpLength = KcpLength(setting.UnitData, setting.ReceiveWindow);
            kcpDataBuffer = new byte[1 + kcpLength];
            kcpSendBuffer = new byte[1 + kcpLength];
            rawSendBuffer = new byte[setting.UnitData];
        }

        protected void Rebuild(Setting setting)
        {
            userData = 0;
            pingTime = 0;
            lastTime = 0;
            overTime = setting.OverTime;
            kcp = new Protocol(0, SendReliable);
            kcp.SetData(setting.UnitData - METADATA_SIZE, setting.DeadLink);
            kcp.SetDelay(setting.NoDelay, setting.Interval, setting.FastResend, !setting.Congestion);
            kcp.SetWindow(setting.SendWindow, setting.ReceiveWindow);
            state = State.Disconnect;
            watch.Restart();
        }

        private void SendReliable(byte[] bytes, int count)
        {
            rawSendBuffer[0] = Channel.Reliable;
            Process.Encode(rawSendBuffer, 1, userData);
            Buffer.BlockCopy(bytes, 0, rawSendBuffer, 1 + 4, count);
            Send(new ArraySegment<byte>(rawSendBuffer, 0, count + 1 + 4));
        }

        public static uint KcpLength(uint count, uint window)
        {
            return (count - Kcp.IKCP_OVERHEAD - METADATA_SIZE) * (Math.Min(window, 255) - 1) - 1;
        }

        public static uint UdpLength(uint count)
        {
            return count - METADATA_SIZE - 1;
        }

        private bool TryReceive(out Reliable message, out ArraySegment<byte> segment)
        {
            segment = default;
            message = Reliable.Ping;
            var count = kcp.PeekSize();
            if (count <= 0)
            {
                return false;
            }

            if (count > kcpDataBuffer.Length)
            {
                OnError(Error.无效接收, "{0}: 接收网络消息过大。消息大小: {1} < {2}。".Format(GetType(), kcpDataBuffer.Length, count));
                Disconnect();
                return false;
            }

            if (kcp.Receive(kcpDataBuffer, count) < 0)
            {
                OnError(Error.无效接收, "{0}: 接收网络消息失败。".Format(GetType()));
                Disconnect();
                return false;
            }

            if (!Process.IsReliable(kcpDataBuffer[0], out message))
            {
                OnError(Error.无效接收, "{0}: 未知的网络消息头部 {1}。".Format(GetType(), message));
                Disconnect();
                return false;
            }

            segment = new ArraySegment<byte>(kcpDataBuffer, 1, count - 1);
            lastTime = keepTime;
            return true;
        }

        protected void Input(ArraySegment<byte> segment, int channel)
        {
            if (channel == Channel.Reliable)
            {
                if (kcp.Input(segment.Array, segment.Offset, segment.Count) != 0)
                {
                    Log.Warn("{0}: 发送可靠消息失败。消息大小: {1}", GetType(), segment.Count - 1);
                }
            }
            else if (channel == Channel.Unreliable && segment.Count >= 1)
            {
                if (!Process.IsUnreliable(segment.Array![segment.Offset], out var message))
                {
                    OnError(Error.无效接收, "{0}: 未知的网络消息头部 {1}。".Format(GetType(), message));
                    Disconnect();
                    return;
                }

                if (message == Unreliable.Data && state == State.Connected)
                {
                    segment = new ArraySegment<byte>(segment.Array, segment.Offset + 1, segment.Count - 1);
                    Data(segment, Channel.Unreliable);
                    lastTime = keepTime;
                }
                else if (message == Unreliable.Disconnect)
                {
                    Disconnect();
                }
            }
        }

        public void SendReliable(Reliable message, ArraySegment<byte> segment = default)
        {
            if (segment.Count + 1 > kcpSendBuffer.Length)
            {
                OnError(Error.无效发送, "{0}: 发送网络消息过大。消息大小: {1} < {2}".Format(GetType(), segment.Count, kcpLength));
                return;
            }

            kcpSendBuffer[0] = (byte)message;
            if (segment.Count > 0)
            {
                Buffer.BlockCopy(segment.Array!, segment.Offset, kcpSendBuffer, 1, segment.Count);
            }

            if (kcp.Send(kcpSendBuffer, 0, segment.Count + 1) < 0)
            {
                OnError(Error.无效发送, "{0}: 发送网络消息失败。消息大小: {1}。".Format(GetType(), segment.Count));
            }
        }

        private void SendUnreliable(Unreliable message, ArraySegment<byte> segment = default)
        {
            if (segment.Count > udpLength)
            {
                Log.Error("{0}: 发送不可靠消息失败。消息大小: {1}", GetType(), segment.Count);
                return;
            }

            rawSendBuffer[0] = Channel.Unreliable;
            Process.Encode(rawSendBuffer, 1, userData);
            rawSendBuffer[5] = (byte)message;
            if (segment.Count > 0)
            {
                Buffer.BlockCopy(segment.Array!, segment.Offset, rawSendBuffer, 1 + 4 + 1, segment.Count);
            }

            Send(new ArraySegment<byte>(rawSendBuffer, 0, segment.Count + 1 + 4 + 1));
        }

        public void SendData(ArraySegment<byte> segment, int channel)
        {
            if (segment.Count == 0)
            {
                OnError(Error.无效发送, "{0}: 尝试发送空消息。".Format(GetType()));
                Disconnect();
                return;
            }

            if (channel == Channel.Reliable)
            {
                SendReliable(Reliable.Data, segment);
            }
            else if (channel == Channel.Unreliable)
            {
                SendUnreliable(Unreliable.Data, segment);
            }
        }

        public void Disconnect()
        {
            if (state == State.Disconnect) return;
            try
            {
                for (var i = 0; i < 5; ++i)
                {
                    SendUnreliable(Unreliable.Disconnect);
                }
            }
            finally
            {
                state = State.Disconnect;
                OnDisconnect();
            }
        }

        private void BeforeReceive()
        {
            if (keepTime >= lastTime + overTime)
            {
                OnError(Error.连接超时, "{0}: 在 {1}ms 内没有收到任何消息后的连接超时！".Format(GetType(), overTime));
                Disconnect();
            }

            if (kcp.State == unchecked((uint)-1))
            {
                OnError(Error.连接超时, "{0}: 网络消息被重传了 {1} 次而没有得到确认！".Format(GetType(), kcp.Death));
                Disconnect();
            }

            if (keepTime >= pingTime + INTERVAL_PING)
            {
                SendReliable(Reliable.Ping);
                pingTime = keepTime;
            }

            if (kcp.Count >= 10000)
            {
                OnError(Error.网络拥塞, "{0}: 断开连接，因为它处理数据的速度不够快！".Format(GetType()));
                kcp.Clear();
                Disconnect();
            }
        }

        public virtual void EarlyUpdate()
        {
            try
            {
                if (state == State.Connect)
                {
                    BeforeReceive();
                    if (TryReceive(out var message, out var segment))
                    {
                        switch (message)
                        {
                            case Reliable.Connect:
                                state = State.Connected;
                                OnConnected();
                                break;
                            case Reliable.Data:
                                OnError(Error.无效接收, "{0}: 收到未通过验证的网络消息。消息类型: {1} 消息长度: {2}".Format(GetType(), message, segment.Count));
                                Disconnect();
                                break;
                        }
                    }
                }
                else if (state == State.Connected)
                {
                    BeforeReceive();
                    while (TryReceive(out var message, out var segment))
                    {
                        switch (message)
                        {
                            case Reliable.Connect:
                                OnError(Error.无效接收, "{0}: 接收无效的网络消息。消息类型: {1}".Format(GetType(), message));
                                Disconnect();
                                break;
                            case Reliable.Data when segment.Count == 0:
                                OnError(Error.无效接收, "{0}: 收到无效的网络消息。消息类型: {1}".Format(GetType(), message));
                                Disconnect();
                                return;
                            case Reliable.Data:
                                Data(segment, Channel.Reliable);
                                break;
                        }
                    }
                }
            }
            catch (SocketException e)
            {
                OnError(Error.连接关闭, "{0}: 网络发生异常，断开连接。\n{1}".Format(GetType(), e));
                Disconnect();
            }
            catch (ObjectDisposedException e)
            {
                OnError(Error.连接关闭, "{0}: 网络发生异常，断开连接。\n{1}".Format(GetType(), e));
                Disconnect();
            }
            catch (Exception e)
            {
                OnError(Error.未知异常, "{0}: 网络发生异常，断开连接。\n{1}".Format(GetType(), e));
                Disconnect();
            }
        }

        public virtual void AfterUpdate()
        {
            try
            {
                if (state != State.Disconnect)
                {
                    kcp.Update(keepTime);
                }
            }
            catch (SocketException e)
            {
                OnError(Error.连接关闭, "{0}: 网络发生异常，断开连接。\n{1}".Format(GetType(), e));
                Disconnect();
            }
            catch (ObjectDisposedException e)
            {
                OnError(Error.连接关闭, "{0}: 网络发生异常，断开连接。\n{1}".Format(GetType(), e));
                Disconnect();
            }
            catch (Exception e)
            {
                OnError(Error.未知异常, "{0}: 网络发生异常，断开连接。\n{1}".Format(GetType(), e));
                Disconnect();
            }
        }

        protected abstract void OnConnected();
        protected abstract void OnDisconnect();
        protected abstract void Send(ArraySegment<byte> segment);
        protected abstract void Data(ArraySegment<byte> segment, int channel);
        protected abstract void OnError(Error error, string message);
    }
}