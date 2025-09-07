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
    internal abstract class Agent
    {
        private const int PING_INTERVAL = 1000;
        private const int METADATA_SIZE = sizeof(byte) + sizeof(int);

        private readonly byte[] kcpSendBuffer;
        private readonly byte[] rawSendBuffer;
        private readonly byte[] receiveBuffer;
        private readonly int unreliableSize;
        private readonly Stopwatch watch = new Stopwatch();
        private uint timeout;
        private uint pingTime;
        private uint receiveTime;
        private Protocol protocol;
        protected State state;
        protected uint cookie;

        protected Agent(Setting setting, uint cookie = 0)
        {
            Reset(setting);
            this.cookie = cookie;
            rawSendBuffer = new byte[setting.MaxUnit];
            receiveBuffer = new byte[1 + ReliableSize(setting.MaxUnit, setting.ReceiveWindow)];
            kcpSendBuffer = new byte[1 + ReliableSize(setting.MaxUnit, setting.ReceiveWindow)];
            unreliableSize = UnreliableSize(setting.MaxUnit);
            state = State.Disconnect;
        }

        protected void Reset(Setting config)
        {
            cookie = 0;
            pingTime = 0;
            receiveTime = 0;
            state = State.Disconnect;
            watch.Restart();

            protocol = new Protocol(0, SendReliable);
            protocol.SetMtu((int)config.MaxUnit - METADATA_SIZE);
            protocol.SetWindowSize((int)config.SendWindow, (int)config.ReceiveWindow);
            protocol.SetNoDelay(config.NoDelay ? 1 : 0, (int)config.Interval, (int)config.FastResend, !config.Congestion ? 1 : 0);
            protocol.Death = config.DeadLink;
            timeout = config.Timeout;
        }

        public static int ReliableSize(uint mtu, uint rcv_wnd)
        {
            return (int)(mtu - 24 - METADATA_SIZE) * ((int)Math.Min(rcv_wnd, byte.MaxValue) - 1) - 1;
        }

        public static int UnreliableSize(uint mtu)
        {
            return (int)mtu - METADATA_SIZE - 1;
        }

        private bool TryReceive(out Reliable header, out ArraySegment<byte> message)
        {
            message = default;
            header = Reliable.Ping;
            var size = protocol.PeekSize();
            if (size <= 0)
            {
                return false;
            }

            if (size > receiveBuffer.Length)
            {
                LogError(Error.InvalidReceive, Log.E142.Format(GetType(), receiveBuffer.Length, size));
                Disconnect();
                return false;
            }

            if (protocol.Receive(receiveBuffer, size) < 0)
            {
                LogError(Error.InvalidReceive, Log.E143.Format(GetType()));
                Disconnect();
                return false;
            }

            if (!Utils.IsReliable(receiveBuffer[0], out header))
            {
                LogError(Error.InvalidReceive, Log.E144.Format(GetType(), header));
                Disconnect();
                return false;
            }

            message = new ArraySegment<byte>(receiveBuffer, 1, size - 1);
            receiveTime = (uint)watch.ElapsedMilliseconds;
            return true;
        }

        protected void Input(int channel, ArraySegment<byte> segment)
        {
            if (channel == Channel.Reliable)
            {
                if (protocol.Input(segment.Array, segment.Offset, segment.Count) != 0)
                {
                    Logs.Warn(Log.E112.Format(GetType(), segment.Count - 1));
                }
            }
            else if (channel == Channel.Unreliable)
            {
                if (segment.Count < 1) return;
                var headerByte = segment.Array[segment.Offset];
                if (!Utils.IsUnreliable(headerByte, out var header))
                {
                    LogError(Error.InvalidReceive, Log.E144.Format(GetType(), header));
                    Disconnect();
                    return;
                }

                if (header == Unreliable.Data)
                {
                    if (state == State.Connected)
                    {
                        segment = new ArraySegment<byte>(segment.Array, segment.Offset + 1, segment.Count - 1);
                        Receive(segment, Channel.Unreliable);
                        receiveTime = (uint)watch.ElapsedMilliseconds;
                    }
                }
                else if (header == Unreliable.Disconnect)
                {
                    //Log.Info($"{GetType()}: 接收到断开连接的消息");
                    Disconnect();
                }
            }
        }

        private void SendReliable(byte[] data, int length)
        {
            rawSendBuffer[0] = Channel.Reliable;
            Utils.Encode32U(rawSendBuffer, 1, cookie);
            Buffer.BlockCopy(data, 0, rawSendBuffer, 1 + 4, length);
            var segment = new ArraySegment<byte>(rawSendBuffer, 0, length + 1 + 4);
            Send(segment);
        }

        internal void SendReliable(Reliable header, ArraySegment<byte> segment = default)
        {
            if (segment.Count > kcpSendBuffer.Length - 1)
            {
                LogError(Error.InvalidSend, Log.E145.Format(GetType(), segment.Count));
                return;
            }

            kcpSendBuffer[0] = (byte)header;
            if (segment.Count > 0)
            {
                Buffer.BlockCopy(segment.Array, segment.Offset, kcpSendBuffer, 1, segment.Count);
            }

            if (protocol.Send(kcpSendBuffer, 0, 1 + segment.Count) < 0)
            {
                LogError(Error.InvalidSend, Log.E146.Format(GetType(), segment.Count));
            }
        }

        private void SendUnreliable(Unreliable header, ArraySegment<byte> segment = default)
        {
            if (segment.Count > unreliableSize)
            {
                Logs.Error(Log.E113.Format(GetType(), segment.Count));
                return;
            }

            rawSendBuffer[0] = Channel.Unreliable;
            Utils.Encode32U(rawSendBuffer, 1, cookie);
            rawSendBuffer[5] = (byte)header;
            if (segment.Count > 0)
            {
                Buffer.BlockCopy(segment.Array, segment.Offset, rawSendBuffer, 1 + 4 + 1, segment.Count);
            }

            Send(new ArraySegment<byte>(rawSendBuffer, 0, segment.Count + 1 + 4 + 1));
        }

        internal void SendData(ArraySegment<byte> data, int channel)
        {
            if (data.Count == 0)
            {
                LogError(Error.InvalidSend, Log.E147.Format(GetType()));
                Disconnect();
                return;
            }

            switch (channel)
            {
                case Channel.Reliable:
                    SendReliable(Reliable.Data, data);
                    break;
                case Channel.Unreliable:
                    SendUnreliable(Unreliable.Data, data);
                    break;
                default:
                    Logs.Warn(Log.E111);
                    break;
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
                Disconnected();
            }
        }

        public virtual void EarlyUpdate()
        {
            if (protocol.State == uint.MaxValue)
            {
                LogError(Error.Timeout, Log.E148.Format(GetType(), protocol.Death));
                Disconnect();
                return;
            }

            var time = (uint)watch.ElapsedMilliseconds;
            if (time >= receiveTime + timeout)
            {
                LogError(Error.Timeout, Log.E149.Format(GetType(), timeout));
                Disconnect();
                return;
            }

            var total = protocol.Count;
            if (total >= 10000)
            {
                LogError(Error.Congestion, Log.E150.Format(GetType()));
                Disconnect();
                return;
            }

            if (time >= pingTime + PING_INTERVAL)
            {
                SendReliable(Reliable.Ping);
                pingTime = time;
            }

            try
            {
                if (state == State.Connect)
                {
                    if (TryReceive(out var header, out _))
                    {
                        if (header == Reliable.Connect)
                        {
                            state = State.Connected;
                            Connected();
                        }
                        else if (header == Reliable.Data)
                        {
                            LogError(Error.InvalidReceive, Log.E151.Format(GetType(), header));
                            Disconnect();
                        }
                    }
                }
                else if (state == State.Connected)
                {
                    while (TryReceive(out var header, out var segment))
                    {
                        if (header == Reliable.Connect)
                        {
                            Logs.Warn(Log.E114.Format(GetType(), header));
                            Disconnect();
                        }
                        else if (header == Reliable.Data)
                        {
                            if (segment.Count == 0)
                            {
                                LogError(Error.InvalidReceive, Log.E152.Format(GetType(), header));
                                Disconnect();
                                return;
                            }

                            Receive(segment, Channel.Reliable);
                        }
                    }
                }
            }
            catch (SocketException e)
            {
                LogError(Error.ConnectionClosed, Log.E153.Format(GetType(), e));
                Disconnect();
            }
            catch (ObjectDisposedException e)
            {
                LogError(Error.ConnectionClosed, Log.E153.Format(GetType(), e));
                Disconnect();
            }
            catch (Exception e)
            {
                LogError(Error.Unexpected, Log.E153.Format(GetType(), e));
                Disconnect();
            }
        }

        public virtual void AfterUpdate()
        {
            try
            {
                if (state != State.Disconnect)
                {
                    protocol.Update((uint)watch.ElapsedMilliseconds);
                }
            }
            catch (SocketException e)
            {
                LogError(Error.ConnectionClosed, Log.E153.Format(GetType(), e));
                Disconnect();
            }
            catch (ObjectDisposedException e)
            {
                LogError(Error.ConnectionClosed, Log.E153.Format(GetType(), e));
                Disconnect();
            }
            catch (Exception e)
            {
                LogError(Error.Unexpected, Log.E153.Format(GetType(), e));
                Disconnect();
            }
        }

        protected abstract void Connected();
        protected abstract void Send(ArraySegment<byte> segment);
        protected abstract void Receive(ArraySegment<byte> message, int channel);
        protected abstract void LogError(Error error, string message);
        protected abstract void Disconnected();
    }
}