// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2025-01-08 19:01:30
// # Recently: 2025-01-08 20:01:58
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Net.Sockets;

namespace Astraia
{
    [Serializable]
    internal struct Setting
    {
        public uint UnitData;
        public uint Timeout;
        public uint Interval;
        public uint DeadLink;
        public uint FastResend;
        public uint SendWindow;
        public uint ReceiveWindow;
        public bool NoDelay;
        public bool DualMode;
        public bool Congestion;

        public Setting(
            uint UnitData = Kcp.IKCP_MTU_DEF,
            uint Timeout = 10000,
            uint Interval = 10,
            uint DeadLink = Kcp.IKCP_DEADLINK,
            uint FastResend = 0,
            uint SendWindow = Kcp.IKCP_WND_SND,
            uint ReceiveWindow = Kcp.IKCP_WND_RCV,
            bool NoDelay = true,
            bool DualMode = true,
            bool Congestion = false)
        {
            this.UnitData = UnitData;
            this.Timeout = Timeout;
            this.Interval = Interval;
            this.DeadLink = DeadLink;
            this.FastResend = FastResend;
            this.SendWindow = SendWindow;
            this.ReceiveWindow = ReceiveWindow;
            this.NoDelay = NoDelay;
            this.DualMode = DualMode;
            this.Congestion = Congestion;
        }
    }

    internal class Event
    {
        public Action Connect;
        public Action Disconnect;
        public Action<Error, string> Error;
        public Action<ArraySegment<byte>> Send;
        public Action<ArraySegment<byte>, int> Receive;
    }

    internal class Event<T>
    {
        public Action<T> Connect;
        public Action<T> Disconnect;
        public Action<T, Error, string> Error;
        public Action<T, ArraySegment<byte>> Send;
        public Action<T, ArraySegment<byte>, int> Receive;
    }

    internal static class Channel
    {
        public const byte Reliable = 1 << 0;
        public const byte Unreliable = 1 << 1;
    }

    internal static class Common
    {
        public static void Encode(byte[] p, int offset, uint value)
        {
            p[0 + offset] = (byte)(value >> 0);
            p[1 + offset] = (byte)(value >> 8);
            p[2 + offset] = (byte)(value >> 16);
            p[3 + offset] = (byte)(value >> 24);
        }

        public static uint Decode(byte[] p, int offset)
        {
            uint result = 0;
            result |= p[0 + offset];
            result |= (uint)(p[1 + offset] << 8);
            result |= (uint)(p[2 + offset] << 16);
            result |= (uint)(p[3 + offset] << 24);
            return result;
        }

        public static void Blocked(Socket socket, int buffer = 1024 * 1024 * 7)
        {
            socket.Blocking = false;
            var sendBuffer = socket.SendBufferSize;
            var receiveBuffer = socket.ReceiveBufferSize;
            try
            {
                socket.SendBufferSize = buffer;
                socket.ReceiveBufferSize = buffer;
            }
            catch (SocketException)
            {
                Service.Log.Info("发送缓冲: {0} => {1} : {2:F}", buffer, sendBuffer, sendBuffer / buffer);
                Service.Log.Info("接收缓冲: {0} => {1} : {2:F}", buffer, receiveBuffer, receiveBuffer / buffer);
            }
        }
    }

    internal enum Error : byte
    {
        解析失败 = 1,
        连接超时 = 2,
        网络拥塞 = 3,
        无效接收 = 4,
        无效发送 = 5,
        连接关闭 = 6,
        未知异常 = 7
    }

    internal enum Lobby : byte
    {
        身份验证成功 = 1,
        请求进入大厅 = 2,
        进入大厅成功 = 3,
        请求创建房间 = 4,
        创建房间成功 = 5,
        请求加入房间 = 6,
        加入房间成功 = 7,
        请求离开房间 = 8,
        离开房间成功 = 9,
        请求移除玩家 = 10,
        断开玩家连接 = 11,
        更新房间数据 = 12,
        同步网络数据 = 13,
    }

    internal enum State : byte
    {
        Connect = 0,
        Connected = 1,
        Disconnect = 2
    }

    internal enum Opcode : byte
    {
        Connect = 1,
        Ping = 2,
        Data = 3,
        Disconnect = 4
    }
}