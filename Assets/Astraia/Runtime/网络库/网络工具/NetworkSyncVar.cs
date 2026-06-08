// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-01-18 12:01:43
// # Recently: 2026-01-18 12:01:43
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System.Runtime.CompilerServices;

namespace Astraia.Net
{
    public static class NetworkSyncVar
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsDirty(ulong mask, int index)
        {
            return (mask & (ulong)(1 << index)) != 0;
        }

        internal static void ServerSend(NetworkModule[] modules, MemoryWriter owner, MemoryWriter other, bool isInit = false)
        {
            var ownerMask = 0UL;
            var otherMask = 0UL;
            for (var i = 0; i < modules.Length; ++i)
            {
                if (isInit || (modules[i].syncMode == SyncMode.服务器 && modules[i].IsDirty()))
                {
                    ownerMask |= 1UL << i;
                }

                if (isInit || modules[i].IsDirty())
                {
                    otherMask |= 1UL << i;
                }
            }

            if (ownerMask != 0)
            {
                Compress.EncodeUInt64(owner, ownerMask);
            }

            if (otherMask != 0)
            {
                Compress.EncodeUInt64(other, otherMask);
            }

            if ((ownerMask | otherMask) != 0)
            {
                for (var i = 0; i < modules.Length; ++i)
                {
                    var ownerDirty = IsDirty(ownerMask, i);
                    var otherDirty = IsDirty(otherMask, i);
                    if (ownerDirty || otherDirty)
                    {
                        using var writer = MemoryWriter.Pop();
                        modules[i].Serialize(writer, isInit);
                        if (ownerDirty)
                        {
                            owner.WriteBytes(writer.buffer, 0, writer.position);
                        }

                        if (otherDirty)
                        {
                            other.WriteBytes(writer.buffer, 0, writer.position);
                        }
                    }
                }
            }
        }

        internal static void ClientReceive(NetworkModule[] modules, MemoryReader reader, bool isInit = false)
        {
            var mask = Compress.DecodeUInt64(reader);
            for (var i = 0; i < modules.Length; ++i)
            {
                if (IsDirty(mask, i))
                {
                    modules[i].Deserialize(reader, isInit);
                }
            }
        }

        internal static void ClientSend(NetworkModule[] modules, MemoryWriter writer, bool isOwner)
        {
            var ownerMask = 0UL;
            for (var i = 0; i < modules.Length; ++i)
            {
                if (isOwner && modules[i].syncMode == SyncMode.客户端 && modules[i].IsDirty())
                {
                    ownerMask |= 1UL << i;
                }
            }

            if (ownerMask != 0)
            {
                Compress.EncodeUInt64(writer, ownerMask);
                for (var i = 0; i < modules.Length; ++i)
                {
                    if (IsDirty(ownerMask, i))
                    {
                        modules[i].Serialize(writer, false);
                    }
                }
            }
        }

        internal static bool ServerReceive(NetworkModule[] modules, MemoryReader reader)
        {
            var mask = Compress.DecodeUInt64(reader);
            for (var i = 0; i < modules.Length; ++i)
            {
                if (IsDirty(mask, i))
                {
                    if (modules[i].syncMode == SyncMode.客户端)
                    {
                        if (modules[i].Deserialize(reader, false))
                        {
                            modules[i].SetSyncVarDirty(ulong.MaxValue);
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }
    }
}