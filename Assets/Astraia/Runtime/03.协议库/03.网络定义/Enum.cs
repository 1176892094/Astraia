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

    internal enum OpCodes : byte
    {
        Connect = 1,
        Connected = 2,
        JoinRoom = 3,
        CreateRoom = 4,
        UpdateRoom = 5,
        LeaveRoom = 6,
        UpdateData = 7,
        KickRoom = 8,
    }

    internal enum State : byte
    {
        Connect = 0,
        Connected = 1,
        Disconnect = 2
    }

    internal enum Reliable : byte
    {
        Connect = 1,
        Ping = 2,
        Data = 3,
    }

    internal enum Unreliable : byte
    {
        Data = 4,
        Disconnect = 5,
    }
}