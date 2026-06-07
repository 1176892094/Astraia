// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2024-12-05 20:12:40
// # Recently: 2024-12-22 20:12:19
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

namespace Astraia.Net
{
    public static class Pass
    {
        public const byte KCP = 1 << 0;
        public const byte UDP = 1 << 1;
        public const byte ANY = 1 << 2;
    }

    public enum RoomMode : byte
    {
        公开,
        私有,
        锁定,
    }

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
}