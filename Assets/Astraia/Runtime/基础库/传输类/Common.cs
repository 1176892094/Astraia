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
    internal static class Const
    {
        public const uint MTU_DEF = 1200;                  // 传输单元
        public const uint SED_WIN = 1024;                  // 发送窗口
        public const uint REV_WIN = 1024;                  // 接收窗口
                                                           
        public const uint INTERVAL = 10;                   // 更新间隔
        public const uint DEAD_LINK = 40;                  // 死亡链接
        public const uint PING_TIME = 1000;                // 心跳计时
        public const uint WAIT_TIME = 10000;               // 超时计时
        public const uint FAST_RESEND = 2;                 // 快速重传
                                                           
        public const int HEAD_PASS = sizeof(byte);          // 传输头部
        public const int HEAD_DATA = sizeof(uint);          // 用户头部
        public const int HEAD_META = sizeof(byte);          // 操作指令
        public const int HEAD_SIZE = HEAD_PASS + HEAD_DATA; // 头部大小
        
        public const uint KCP_DEF = MTU_DEF - HEAD_SIZE - Kcp.IKCP_OVERHEAD;
        public const uint UDP_LEN = MTU_DEF - HEAD_SIZE;
        public const uint KCP_LEN = KCP_DEF * 254 - HEAD_META;
    }

    internal static class Common
    {
        public static void Encode(byte[] p, int offset, int value)
        {
            p[0 + offset] = (byte)(value >> 0);
            p[1 + offset] = (byte)(value >> 8);
            p[2 + offset] = (byte)(value >> 16);
            p[3 + offset] = (byte)(value >> 24);
        }

        public static int Decode(byte[] p, int offset)
        {
            var result = 0;
            result |= p[0 + offset];
            result |= p[1 + offset] << 8;
            result |= p[2 + offset] << 16;
            result |= p[3 + offset] << 24;
            return result;
        }

        public static void Blocked(this Socket socket, int buffer = 1024 * 1024 * 7)
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
                Log.Info("发送缓冲: {0} => {1} : {2:F}", buffer, sendBuffer, sendBuffer / buffer);
                Log.Info("接收缓冲: {0} => {1} : {2:F}", buffer, receiveBuffer, receiveBuffer / buffer);
            }
        }
    }

    internal static class Pass
    {
        public const byte KCP = 1 << 0;
        public const byte UDP = 1 << 1;
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