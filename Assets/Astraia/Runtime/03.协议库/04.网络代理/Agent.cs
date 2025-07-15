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
        private readonly byte[] kcpSendBuffer;
        private readonly byte[] rawSendBuffer;
        private readonly byte[] receiveBuffer;
        private readonly int unreliableSize;
        private readonly Stopwatch watch = new Stopwatch();
        private Kcp kcp;
        private uint timeout;
        private uint pingTime;
        private uint receiveTime;
        protected State state;
        protected uint cookie;

        protected Agent(Setting setting, uint cookie = 0)
        {
            Reset(setting);
            this.cookie = cookie;
            unreliableSize = Kcp.UnreliableSize(setting.MaxUnit);
            var reliableSize = Kcp.ReliableSize(setting.MaxUnit, setting.ReceiveWindow);
            rawSendBuffer = new byte[setting.MaxUnit];
            receiveBuffer = new byte[1 + reliableSize];
            kcpSendBuffer = new byte[1 + reliableSize];
            state = State.Disconnect;
        }

        protected void Reset(Setting config)
        {
            cookie = 0;
            pingTime = 0;
            receiveTime = 0;
            state = State.Disconnect;
            watch.Restart();

            kcp = new Kcp(0, SendReliable);
            kcp.SetMtu((uint)config.MaxUnit - Kcp.METADATA_SIZE);
            kcp.SetWindowSize(config.SendWindow, config.ReceiveWindow);
            kcp.SetNoDelay(config.NoDelay ? 1U : 0U, config.Interval, config.FastResend, !config.Congestion);
            kcp.dead_link = config.DeadLink;
            timeout = config.Timeout;
        }

        private bool TryReceive(out Reliable header, out ArraySegment<byte> message)
        {
            message = default;
            header = Reliable.Ping;
            var size = kcp.PeekSize();
            if (size <= 0)
            {
                return false;
            }

            if (size > receiveBuffer.Length)
            {
                LogError(Error.InvalidReceive, Service.Text.Format(Log.E142, GetType(), receiveBuffer.Length, size));
                Disconnect();
                return false;
            }

            if (kcp.Receive(receiveBuffer, size) < 0)
            {
                LogError(Error.InvalidReceive, Service.Text.Format(Log.E143, GetType()));
                Disconnect();
                return false;
            }

            if (!Utils.ParseReliable(receiveBuffer[0], out header))
            {
                LogError(Error.InvalidReceive, Service.Text.Format(Log.E144, GetType(), header));
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
                if (kcp.Input(segment.Array, segment.Offset, segment.Count) != 0)
                {
                    Logs.Warn(Service.Text.Format(Log.E112, GetType(), segment.Count - 1));
                }
            }
            else if (channel == Channel.Unreliable)
            {
                if (segment.Count < 1) return;
                var headerByte = segment.Array[segment.Offset];
                if (!Utils.ParseUnreliable(headerByte, out var header))
                {
                    LogError(Error.InvalidReceive, Service.Text.Format(Log.E144, GetType(), header));
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
                LogError(Error.InvalidSend, Service.Text.Format(Log.E145, GetType(), segment.Count));
                return;
            }

            kcpSendBuffer[0] = (byte)header;
            if (segment.Count > 0)
            {
                Buffer.BlockCopy(segment.Array, segment.Offset, kcpSendBuffer, 1, segment.Count);
            }

            if (kcp.Send(kcpSendBuffer, 0, 1 + segment.Count) < 0)
            {
                LogError(Error.InvalidSend, Service.Text.Format(Log.E146, GetType(), segment.Count));
            }
        }

        private void SendUnreliable(Unreliable header, ArraySegment<byte> segment = default)
        {
            if (segment.Count > unreliableSize)
            {
                Logs.Error(Service.Text.Format(Log.E113, GetType(), segment.Count));
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
                LogError(Error.InvalidSend, Service.Text.Format(Log.E147, GetType()));
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
            if (kcp.state == -1)
            {
                LogError(Error.Timeout, Service.Text.Format(Log.E148, GetType(), kcp.dead_link));
                Disconnect();
                return;
            }

            var time = (uint)watch.ElapsedMilliseconds;
            if (time >= receiveTime + timeout)
            {
                LogError(Error.Timeout, Service.Text.Format(Log.E149, GetType(), timeout));
                Disconnect();
                return;
            }

            var total = kcp.receiveQueue.Count + kcp.sendQueue.Count + kcp.receiveBuffer.Count + kcp.sendBuffer.Count;
            if (total >= 10000)
            {
                LogError(Error.Congestion, Service.Text.Format(Log.E150, GetType()));
                kcp.sendQueue.Clear();
                Disconnect();
                return;
            }

            if (time >= pingTime + Kcp.PING_INTERVAL)
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
                            LogError(Error.InvalidReceive, Service.Text.Format(Log.E151, GetType(), header));
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
                            Logs.Warn(Service.Text.Format(Log.E114, GetType(), header));
                            Disconnect();
                        }
                        else if (header == Reliable.Data)
                        {
                            if (segment.Count == 0)
                            {
                                LogError(Error.InvalidReceive, Service.Text.Format(Log.E152, GetType(), header));
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
                LogError(Error.ConnectionClosed, Service.Text.Format(Log.E153, GetType(), e));
                Disconnect();
            }
            catch (ObjectDisposedException e)
            {
                LogError(Error.ConnectionClosed, Service.Text.Format(Log.E153, GetType(), e));
                Disconnect();
            }
            catch (Exception e)
            {
                LogError(Error.Unexpected, Service.Text.Format(Log.E153, GetType(), e));
                Disconnect();
            }
        }

        public virtual void AfterUpdate()
        {
            try
            {
                if (state != State.Disconnect)
                {
                    kcp.Update((uint)watch.ElapsedMilliseconds);
                }
            }
            catch (SocketException e)
            {
                LogError(Error.ConnectionClosed, Service.Text.Format(Log.E153, GetType(), e));
                Disconnect();
            }
            catch (ObjectDisposedException e)
            {
                LogError(Error.ConnectionClosed, Service.Text.Format(Log.E153, GetType(), e));
                Disconnect();
            }
            catch (Exception e)
            {
                LogError(Error.Unexpected, Service.Text.Format(Log.E153, GetType(), e));
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