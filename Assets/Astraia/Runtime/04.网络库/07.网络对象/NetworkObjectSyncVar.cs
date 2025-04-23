// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2024-12-21 23:12:45
// # Recently: 2024-12-22 22:12:02
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************


using System;

namespace Astraia.Net
{
    public sealed partial class NetworkObject
    {
        internal void ServerSerialize(bool status, MemorySetter owner, MemorySetter observer)
        {
            var components = entities;
            var (ownerMask, observerMask) = ServerDirtyMasks(status);

            if (ownerMask != 0)
            {
                Service.Length.Encode(owner, ownerMask);
            }

            if (observerMask != 0)
            {
                Service.Length.Encode(observer, observerMask);
            }

            if ((ownerMask | observerMask) != 0)
            {
                for (var i = 0; i < components.Length; ++i)
                {
                    var component = components[i];
                    var ownerDirty = IsDirty(ownerMask, i);
                    var observersDirty = IsDirty(observerMask, i);
                    if (ownerDirty || observersDirty)
                    {
                        using var setter = MemorySetter.Pop();
                        component.Serialize(setter, status);
                        ArraySegment<byte> segment = setter;
                        if (ownerDirty)
                        {
                            owner.SetBytes(segment.Array, segment.Offset, segment.Count);
                        }

                        if (observersDirty)
                        {
                            observer.SetBytes(segment.Array, segment.Offset, segment.Count);
                        }
                    }
                }
            }
        }

        internal void ClientSerialize(MemorySetter setter)
        {
            var components = entities;
            var dirtyMask = ClientDirtyMask();
            if (dirtyMask != 0)
            {
                Service.Length.Encode(setter, dirtyMask);
                for (var i = 0; i < components.Length; ++i)
                {
                    var component = components[i];

                    if (IsDirty(dirtyMask, i))
                    {
                        component.Serialize(setter, false);
                    }
                }
            }
        }

        internal bool ServerDeserialize(MemoryGetter getter)
        {
            var components = entities;
            var mask = Service.Length.Decode(getter);

            for (var i = 0; i < components.Length; ++i)
            {
                if (IsDirty(mask, i))
                {
                    var component = components[i];

                    if (component.syncDirection == SyncMode.Client)
                    {
                        if (!component.Deserialize(getter, false))
                        {
                            return false;
                        }

                        component.SetSyncVarDirty(ulong.MaxValue);
                    }
                }
            }

            return true;
        }

        internal void ClientDeserialize(MemoryGetter getter, bool status)
        {
            var components = entities;
            var mask = Service.Length.Decode(getter);

            for (var i = 0; i < components.Length; ++i)
            {
                if (IsDirty(mask, i))
                {
                    var component = components[i];
                    component.Deserialize(getter, status);
                }
            }
        }

        private (ulong, ulong) ServerDirtyMasks(bool status)
        {
            ulong ownerMask = 0;
            ulong observerMask = 0;

            var components = entities;
            for (var i = 0; i < components.Length; ++i)
            {
                var component = components[i];
                var dirty = component.IsDirty();
                ulong mask = 1U << i;
                if (status || (component.syncDirection == SyncMode.Server && dirty))
                {
                    ownerMask |= mask;
                }

                if (status || dirty)
                {
                    observerMask |= mask;
                }
            }

            return (ownerMask, observerMask);
        }

        private ulong ClientDirtyMask()
        {
            ulong mask = 0;
            var components = entities;
            for (var i = 0; i < components.Length; ++i)
            {
                var component = components[i];
                if ((entityMode & EntityMode.Owner) != 0 && component.syncDirection == SyncMode.Client)
                {
                    if (component.IsDirty()) mask |= 1U << i;
                }
            }

            return mask;
        }
    }
}