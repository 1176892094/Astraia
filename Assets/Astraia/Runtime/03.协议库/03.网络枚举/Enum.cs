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

namespace Astraia
{
    internal static class Channel
    {
        public const byte Reliable = 1 << 0;
        public const byte Unreliable = 1 << 1;
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