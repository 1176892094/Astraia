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

using System;

namespace Astraia.Net
{
    public static class Channel
    {
        public const byte Reliable = 1 << 0;
        public const byte Unreliable = 1 << 1;
        public const byte IgnoreOwner = 1 << 2;
    }

    public enum RoomMode : byte
    {
        Public,
        Private
    }

    public enum SyncMode : byte
    {
        Server,
        Client
    }

    internal enum InvokeMode : byte
    {
        ServerRpc,
        ClientRpc,
    }

    public enum Visible : byte
    {
        Show = 0,
        Hide = 1,
        Auto = 2,
    }

    [Flags]
    internal enum EntityMode : byte
    {
        None = 0,
        Owner = 1 << 0,
        Client = 1 << 1,
        Server = 1 << 2,
    }

    [Flags]
    internal enum EntityState : byte
    {
        None = 0,
        Awake = 1 << 0,
        Spawn = 1 << 1,
        Owner = 1 << 2,
        Destroy = 1 << 3,
    }

    [Flags]
    internal enum TransformOption : byte
    {
        None,
        Position = 1 << 0,
        Rotation = 1 << 1,
        Mutation = 1 << 2,
    }
}