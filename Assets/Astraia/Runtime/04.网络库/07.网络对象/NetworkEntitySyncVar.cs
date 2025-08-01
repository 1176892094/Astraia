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
    public partial class NetworkEntity
    {
        internal void ServerSerialize(bool initialize, MemoryWriter owner, MemoryWriter observer)
        {
            var components = agents;
            var (ownerMask, observerMask) = ServerDirtyMasks(initialize);

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
                for (var i = 0; i < components.Count; ++i)
                {
                    var component = components[i];
                    var ownerDirty = IsDirty(ownerMask, i);
                    var observersDirty = IsDirty(observerMask, i);
                    if (ownerDirty || observersDirty)
                    {
                        using var writer = MemoryWriter.Pop();
                        component.Serialize(writer, initialize);
                        ArraySegment<byte> segment = writer;
                        if (ownerDirty)
                        {
                            owner.WriteBytes(segment.Array, segment.Offset, segment.Count);
                        }

                        if (observersDirty)
                        {
                            observer.WriteBytes(segment.Array, segment.Offset, segment.Count);
                        }
                    }
                }
            }
        }

        internal void ClientSerialize(MemoryWriter writer)
        {
            var components = agents;
            var dirtyMask = ClientDirtyMask();
            if (dirtyMask != 0)
            {
                Service.Length.Encode(writer, dirtyMask);
                for (var i = 0; i < components.Count; ++i)
                {
                    var component = components[i];

                    if (IsDirty(dirtyMask, i))
                    {
                        component.Serialize(writer, false);
                    }
                }
            }
        }

        internal bool ServerDeserialize(MemoryReader reader)
        {
            var components = agents;
            var mask = Service.Length.Decode(reader);

            for (var i = 0; i < components.Count; ++i)
            {
                if (IsDirty(mask, i))
                {
                    var component = components[i];

                    if (component.syncDirection == SyncMode.Client)
                    {
                        if (!component.Deserialize(reader, false))
                        {
                            return false;
                        }

                        component.SetSyncVarDirty(ulong.MaxValue);
                    }
                }
            }

            return true;
        }

        internal void ClientDeserialize(MemoryReader reader, bool initialize)
        {
            var components = agents;
            var mask = Service.Length.Decode(reader);

            for (var i = 0; i < components.Count; ++i)
            {
                if (IsDirty(mask, i))
                {
                    var component = components[i];
                    component.Deserialize(reader, initialize);
                }
            }
        }

        private (ulong, ulong) ServerDirtyMasks(bool initialize)
        {
            ulong ownerMask = 0;
            ulong observerMask = 0;

            var components = agents;
            for (var i = 0; i < components.Count; ++i)
            {
                var component = components[i];
                var dirty = component.IsDirty();
                ulong mask = 1U << i;
                if (initialize || (component.syncDirection == SyncMode.Server && dirty))
                {
                    ownerMask |= mask;
                }

                if (initialize || dirty)
                {
                    observerMask |= mask;
                }
            }

            return (ownerMask, observerMask);
        }

        private ulong ClientDirtyMask()
        {
            ulong mask = 0;
            var components = agents;
            for (var i = 0; i < components.Count; ++i)
            {
                var component = components[i];
                if ((agentMode & AgentMode.Owner) != 0 && component.syncDirection == SyncMode.Client)
                {
                    if (component.IsDirty())
                    {
                        mask |= 1U << i;
                    }
                }
            }

            return mask;
        }
    }
}