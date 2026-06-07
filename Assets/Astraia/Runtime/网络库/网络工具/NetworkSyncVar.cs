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
        internal static void ServerSerialize(NetworkModule[] modules, MemoryWriter owner, MemoryWriter other, bool isInit = false)
        {
            var mask = ServerDirtyMasks(modules, isInit);
            if (mask.owner != 0)
            {
                Compress.EncodeUInt64(owner, mask.owner);
            }

            if (mask.other != 0)
            {
                Compress.EncodeUInt64(other, mask.other);
            }

            if ((mask.owner | mask.other) != 0)
            {
                for (var i = 0; i < modules.Length; ++i)
                {
                    var ownerDirty = IsDirty(mask.owner, i);
                    var otherDirty = IsDirty(mask.other, i);
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

        internal static void ClientSerialize(NetworkModule[] modules, MemoryWriter writer, bool isOwner)
        {
            var mask = ClientDirtyMask(modules, isOwner);
            if (mask != 0)
            {
                Compress.EncodeUInt64(writer, mask);
                for (var i = 0; i < modules.Length; ++i)
                {
                    if (IsDirty(mask, i))
                    {
                        modules[i].Serialize(writer, false);
                    }
                }
            }
        }

        internal static bool ServerDeserialize(NetworkModule[] modules, MemoryReader reader)
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

        internal static void ClientDeserialize(NetworkModule[] modules, MemoryReader reader, bool isInit = false)
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsDirty(ulong mask, int index)
        {
            return (mask & (ulong)(1 << index)) != 0;
        }

        private static (ulong owner, ulong other) ServerDirtyMasks(NetworkModule[] modules, bool isInit)
        {
            var owner = 0UL;
            var other = 0UL;
            for (var i = 0; i < modules.Length; ++i)
            {
                if (isInit || (modules[i].syncMode == SyncMode.服务器 && modules[i].IsDirty()))
                {
                    owner |= 1U << i;
                }

                if (isInit || modules[i].IsDirty())
                {
                    other |= 1U << i;
                }
            }

            return (owner, other);
        }

        private static ulong ClientDirtyMask(NetworkModule[] modules, bool isOwner)
        {
            var mask = 0UL;
            for (var i = 0; i < modules.Length; ++i)
            {
                if (isOwner && modules[i].syncMode == SyncMode.客户端)
                {
                    if (modules[i].IsDirty())
                    {
                        mask |= 1U << i;
                    }
                }
            }

            return mask;
        }
    }
}