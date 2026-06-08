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

using System;

namespace Astraia.Net
{
    [AttributeUsage(AttributeTargets.Field)]
    public class SyncVarAttribute : Attribute
    {
        public SyncVarAttribute(string func = null)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class ClientRpcAttribute : Attribute
    {
        public ClientRpcAttribute(int pass = Pass.KCP)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class ServerRpcAttribute : Attribute
    {
        public ServerRpcAttribute(int pass = Pass.KCP)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class TargetRpcAttribute : Attribute
    {
        public TargetRpcAttribute(int pass = Pass.KCP)
        {
        }
    }

    public static class Pass
    {
        public const byte KCP = 1 << 0;
        public const byte UDP = 1 << 1;
        public const byte EXT = 1 << 2;
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