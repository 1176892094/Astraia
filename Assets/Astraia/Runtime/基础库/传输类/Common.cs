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

using System.Net.Sockets;

namespace Astraia
{
    internal unsafe delegate void SendDelegate(byte* bytes, int count);

    internal static class Pass
    {
        public const byte KCP = 1 << 0;
        public const byte UDP = 1 << 1;
    }

    internal static class Const
    {
        public const int MTU_DEF = 1200;
        public const int SED_WIN = 1024 * 4;
        public const int REV_WIN = 1024 * 4;

        public const int FAST_SEND = 2;
        public const int DEAD_LINK = 40;
        public const int STEP_TIME = 10;
        public const int PING_TIME = 1000;
        public const int WAIT_TIME = 10000;
        public const int HEAD_SIZE = sizeof(byte) + sizeof(int);

        public const int MAX_FRG = byte.MaxValue - 1;
        public const int UDP_LEN = MTU_DEF - HEAD_SIZE;
        public const int MAX_LEN = MTU_DEF - HEAD_SIZE - (int)Kcp.IKCP_OVERHEAD;
        public const int KCP_LEN = MAX_LEN * MAX_FRG - sizeof(byte);
    }

    internal static class Common
    {
        public static void Encode(byte[] p, int offset, int value)
        {
            p[0 + offset] = (byte)(value >> 00);
            p[1 + offset] = (byte)(value >> 08);
            p[2 + offset] = (byte)(value >> 16);
            p[3 + offset] = (byte)(value >> 24);
        }

        public static int Decode(byte[] p, int offset)
        {
            var result = 0;
            result |= p[0 + offset];
            result |= p[1 + offset] << 08;
            result |= p[2 + offset] << 16;
            result |= p[3 + offset] << 24;
            return result;
        }

        public static void Blocked(this Socket socket, int buffer = 1024 * 1024 * 7)
        {
            socket.Blocking = false;
            var sendBuffer = socket.SendBufferSize;
            var dataBuffer = socket.ReceiveBufferSize;
            try
            {
                socket.SendBufferSize = buffer;
                socket.ReceiveBufferSize = buffer;
            }
            catch (SocketException)
            {
                Log.Info($"发送缓冲: {buffer} => {sendBuffer} : {sendBuffer / buffer:F}");
                Log.Info($"接收缓冲: {buffer} => {dataBuffer} : {dataBuffer / buffer:F}");
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

    internal enum State : byte
    {
        正在连接 = 0,
        连接成功 = 1,
        断开连接 = 2
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

    internal enum Opcode : byte
    {
        握手 = 1,
        心跳 = 2,
        数据 = 3,
        断连 = 4
    }
}