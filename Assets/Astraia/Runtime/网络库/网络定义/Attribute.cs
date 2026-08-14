// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2024-11-29 13:11:20
// # Recently: 2024-12-22 20:12:18
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

namespace Astraia.Net
{
    internal enum SyncMode : byte
    {
        服务器,
        客户端
    }

    internal enum HookMode : byte
    {
        服务器,
        客户端
    }

    internal enum State : byte
    {
        正在连接 = 0,
        连接成功 = 1,
        断开连接 = 2
    }
}