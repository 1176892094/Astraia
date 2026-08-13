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
using System.Diagnostics;
using System.Net.Sockets;

namespace Astraia
{
    internal sealed class KcpPeer
    {
        private readonly byte[] rawSendBuffer = new byte[Const.MTU_DEF];
        private readonly byte[] kcpSendBuffer = new byte[Const.KCP_LEN + 1];
        private readonly byte[] kcpDataBuffer = new byte[Const.KCP_LEN + 1];

        private readonly string userName;
        private readonly Stopwatch watch = new();
        private uint pingTime;
        private uint nextTime;
        private uint waitTime;
        private int userData;
        private Protocol kcp;
        private State state;

        public Action<int> onConnect;
        public Action<int> onDisconnect;
        public Action<Error, string> onError;
        public Action<ArraySegment<byte>> onSend;
        public Action<ArraySegment<byte>, int> onReceive;

        private uint Time => (uint)watch.ElapsedMilliseconds;

        public KcpPeer(string userName)
        {
            this.userName = userName;
        }

        public void Rebuild()
        {
            pingTime = 0;
            nextTime = 0;
            waitTime = Const.WAIT_TIME;
            kcp = new Protocol(0, SendReliable);
            kcp.SetData(Const.MTU_DEF - Const.HEAD_SIZE);
            kcp.SetDelay(Const.INTERVAL, Const.FAST_RESEND);
            kcp.SetWindow(Const.SED_WIN, Const.REV_WIN, Const.DEAD_LINK);
            state = State.正在连接;
            watch.Restart();
        }

        private void SendReliable(byte[] bytes, int count)
        {
            rawSendBuffer[0] = Pass.KCP;
            Common.Encode(rawSendBuffer, 1, userData);
            Buffer.BlockCopy(bytes, 0, rawSendBuffer, Const.HEAD_SIZE, count);
            onSend(new ArraySegment<byte>(rawSendBuffer, 0, Const.HEAD_SIZE + count));
        }

        public void Handshake(int userData)
        {
            this.userData = userData;
            SendReliable(Opcode.握手, BitConverter.GetBytes(userData));
        }

        private bool TryReceive(out Opcode message, out ArraySegment<byte> segment)
        {
            segment = default;
            message = Opcode.断连;
            var count = kcp.PeekSize();
            if (count <= 0)
            {
                return false;
            }

            if (count > kcpDataBuffer.Length)
            {
                onError(Error.无效接收, "{0}接收网络消息过大。消息大小: {1} < {2}。".Format(userName, kcpDataBuffer.Length, count));
                Disconnect();
                return false;
            }

            if (kcp.Receive(kcpDataBuffer, count) < 0)
            {
                onError(Error.无效接收, "{0}接收网络消息失败。".Format(userName));
                Disconnect();
                return false;
            }

            message = (Opcode)kcpDataBuffer[0];
            segment = new ArraySegment<byte>(kcpDataBuffer, 1, count - 1);
            nextTime = Time;
            return true;
        }

        public void Input(ArraySegment<byte> segment)
        {
            if (segment.Count <= Const.HEAD_SIZE)
            {
                return;
            }

            var pass = segment.Array![segment.Offset];
            var readData = Common.Decode(segment.Array, segment.Offset + 1);
            if (state == State.连接成功 && readData != userData)
            {
                Log.Warn("{0}数据校验失败。旧: {1} 新: {2}", userName, userData, readData);
                return;
            }

            var message = new ArraySegment<byte>(segment.Array, segment.Offset + Const.HEAD_SIZE, segment.Count - Const.HEAD_SIZE);
            if (pass == Pass.KCP)
            {
                if (kcp.Input(message.Array, message.Offset, message.Count) != 0)
                {
                    Log.Warn("{0}发送可靠消息失败。消息大小: {1}", userName, message.Count - 1);
                }
            }
            else if (pass == Pass.UDP)
            {
                if (state == State.连接成功)
                {
                    onReceive(message, Pass.UDP);
                    nextTime = Time;
                }
            }
        }

        private void SendReliable(Opcode message, ArraySegment<byte> segment = default)
        {
            if (segment.Count > Const.KCP_LEN)
            {
                onError(Error.无效发送, "{0}发送网络消息过大。消息大小: {1} < {2}".Format(userName, segment.Count, Const.KCP_LEN));
                return;
            }

            kcpSendBuffer[0] = (byte)message;
            if (segment.Count > 0)
            {
                Buffer.BlockCopy(segment.Array!, segment.Offset, kcpSendBuffer, 1, segment.Count);
            }

            if (kcp.Send(kcpSendBuffer, 0, segment.Count + 1) < 0)
            {
                onError(Error.无效发送, "{0}发送网络消息失败。消息大小: {1}。".Format(userName, segment.Count));
            }
        }

        private void SendUnreliable(ArraySegment<byte> segment)
        {
            if (segment.Count > Const.UDP_LEN)
            {
                onError(Error.无效发送, "{0}发送网络消息过大。消息大小: {1} < {2}".Format(userName, segment.Count, Const.UDP_LEN));
                return;
            }

            rawSendBuffer[0] = Pass.UDP;
            Common.Encode(rawSendBuffer, 1, userData);
            if (segment.Count > 0)
            {
                Buffer.BlockCopy(segment.Array!, segment.Offset, rawSendBuffer, Const.HEAD_SIZE, segment.Count);
            }

            onSend(new ArraySegment<byte>(rawSendBuffer, 0, segment.Count + Const.HEAD_SIZE));
        }

        public void SendData(ArraySegment<byte> segment, int pass)
        {
            if (segment.Count == 0)
            {
                onError(Error.无效发送, "{0}尝试发送空消息。".Format(userName));
                Disconnect();
                return;
            }

            switch (pass)
            {
                case Pass.KCP:
                    SendReliable(Opcode.数据, segment);
                    break;
                case Pass.UDP:
                    SendUnreliable(segment);
                    break;
            }
        }

        public void Disconnect()
        {
            if (state == State.断开连接)
            {
                return;
            }

            try
            {
                SendReliable(Opcode.断连);
                kcp.Flush();
            }
            finally
            {
                state = State.断开连接;
                onDisconnect(userData);
            }
        }

        private void BeforeReceive(uint sinceTime)
        {
            if (sinceTime >= nextTime + waitTime)
            {
                onError(Error.连接超时, "{0}在{1}秒内没有收到任何消息后的连接超时！".Format(userName, waitTime / 1000));
                Disconnect();
                return;
            }

            if (kcp.State == unchecked((uint)-1))
            {
                onError(Error.连接超时, "{0}网络消息被重传了{1}次而没有得到确认！".Format(userName, kcp.Death));
                Disconnect();
                return;
            }

            if (sinceTime >= pingTime + Const.PING_TIME)
            {
                SendReliable(Opcode.心跳);
                pingTime = sinceTime;
            }

            if (kcp.Count >= 10000)
            {
                onError(Error.网络拥塞, "{0}断开连接，因为它处理数据的速度不够快！".Format(userName));
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
                    case Opcode.握手 when segment.Count != 4:
                        onError(Error.无效接收, "{0}接收无效的网络消息。消息类型: {1}".Format(userName, message));
                        Disconnect();
                        return;
                    case Opcode.握手:
                        state = State.连接成功;
                        userData = Common.Decode(segment.Array, segment.Offset);
                        onConnect(userData);
                        break;
                    case Opcode.数据:
                        onError(Error.无效接收, "{0}接收无效的网络消息。消息类型: {1}".Format(userName, message));
                        Disconnect();
                        break;
                    case Opcode.断连:
                        Disconnect();
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
                    case Opcode.握手:
                        onError(Error.无效接收, "{0}接收无效的网络消息。消息类型: {1}".Format(userName, message));
                        Disconnect();
                        break;
                    case Opcode.数据 when segment.Count == 0:
                        onError(Error.无效接收, "{0}收到无效的网络消息。消息类型: {1}".Format(userName, message));
                        Disconnect();
                        break;
                    case Opcode.数据:
                        onReceive(segment, Pass.KCP);
                        break;
                    case Opcode.断连:
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
                    case State.正在连接:
                        UpdateConnect();
                        break;
                    case State.连接成功:
                        UpdateConnected();
                        break;
                }
            }
            catch (SocketException e)
            {
                onError(Error.连接关闭, "{0}网络发生异常，断开连接。\n{1}".Format(userName, e));
                Disconnect();
            }
            catch (ObjectDisposedException e)
            {
                onError(Error.连接关闭, "{0}网络发生异常，断开连接。\n{1}".Format(userName, e));
                Disconnect();
            }
            catch (Exception e)
            {
                onError(Error.未知异常, "{0}网络发生异常，断开连接。\n{1}".Format(userName, e));
                Disconnect();
            }
        }

        public void AfterUpdate()
        {
            try
            {
                if (state != State.断开连接)
                {
                    kcp.Update(Time);
                }
            }
            catch (SocketException e)
            {
                onError(Error.连接关闭, "{0}网络发生异常，断开连接。\n{1}".Format(userName, e));
                Disconnect();
            }
            catch (ObjectDisposedException e)
            {
                onError(Error.连接关闭, "{0}网络发生异常，断开连接。\n{1}".Format(userName, e));
                Disconnect();
            }
            catch (Exception e)
            {
                onError(Error.未知异常, "{0}网络发生异常，断开连接。\n{1}".Format(userName, e));
                Disconnect();
            }
        }
    }
}