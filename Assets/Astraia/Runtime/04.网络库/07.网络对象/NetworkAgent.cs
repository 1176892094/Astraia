// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2024-12-21 23:12:50
// # Recently: 2024-12-22 22:12:01
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************


using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Astraia.Common;
using UnityEngine;

namespace Astraia.Net
{
    [Serializable]
    public abstract partial class NetworkAgent : Agent<NetworkEntity>
    {
        internal byte sourceId;

        public SyncMode syncDirection;

        public float syncInterval;

        private ulong syncVarHook;

        private double syncVarTime;

        protected ulong syncVarDirty { get; set; }

        public uint objectId => owner.objectId;

        public bool isOwner => owner.isOwner;

        public bool isServer => owner.isServer;

        public bool isClient => owner.isClient;

        public bool isVerify
        {
            get
            {
                if (isClient && isServer)
                {
                    return syncDirection == SyncMode.Server || isOwner;
                }

                if (isClient)
                {
                    return syncDirection == SyncMode.Client && isOwner;
                }

                return syncDirection == SyncMode.Server;
            }
        }

        public NetworkClient connection => owner.connection;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsDirty() => syncVarDirty != 0UL && Time.unscaledTimeAsDouble - syncVarTime >= syncInterval;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SetSyncVarDirty(ulong dirty) => syncVarDirty |= dirty;

        private bool GetSyncVarHook(ulong dirty) => (syncVarHook & dirty) != 0UL;

        private void SetSyncVarHook(ulong dirty, bool value) => syncVarHook = value ? syncVarHook | dirty : syncVarHook & ~dirty;

        public void ClearDirty()
        {
            syncVarDirty = 0UL;
            syncVarTime = Time.unscaledTimeAsDouble;
        }

        internal void Serialize(MemoryWriter writer, bool initialize)
        {
            var headerPosition = writer.position;
            writer.WriteByte(0);
            var contentPosition = writer.position;

            try
            {
                OnSerialize(writer, initialize);
            }
            catch (Exception e)
            {
                Debug.LogError(Service.Text.Format(Log.E260, gameObject.name, GetType(), owner.sceneId, e));
            }

            var endPosition = writer.position;
            writer.position = headerPosition;
            var size = endPosition - contentPosition;
            var safety = (byte)(size & 0xFF);
            writer.WriteByte(safety);
            writer.position = endPosition;
        }

        internal bool Deserialize(MemoryReader reader, bool initialize)
        {
            var result = true;
            var safety = reader.ReadByte();
            var chunkStart = reader.position;
            try
            {
                OnDeserialize(reader, initialize);
            }
            catch (Exception e)
            {
                Debug.LogError(Service.Text.Format(Log.E260, gameObject.name, GetType(), owner.sceneId, e));
                result = false;
            }

            var size = reader.position - chunkStart;
            var sizeHash = (byte)(size & 0xFF);
            if (sizeHash != safety)
            {
                Debug.LogError(Service.Text.Format(Log.E261, size, sizeHash, safety));
                var cleared = (uint)size & 0xFFFFFF00;
                reader.position = chunkStart + (int)(cleared | safety);
                result = false;
            }

            return result;
        }

        protected virtual void OnSerialize(MemoryWriter writer, bool initialize)
        {
            SerializeSyncVars(writer, initialize);
        }

        protected virtual void OnDeserialize(MemoryReader reader, bool initialize)
        {
            DeserializeSyncVars(reader, initialize);
        }

        protected virtual void SerializeSyncVars(MemoryWriter writer, bool initialize)
        {
        }

        protected virtual void DeserializeSyncVars(MemoryReader reader, bool initialize)
        {
        }

        protected void SendServerRpcInternal(string methodName, int methodHash, MemoryWriter writer, int channel)
        {
            if (!NetworkManager.Client.isActive)
            {
                Debug.LogError(Service.Text.Format(Log.E262, methodName), gameObject);
                return;
            }

            if (!NetworkManager.Client.isReady)
            {
                Debug.LogWarning(Service.Text.Format(Log.E263, methodName, gameObject.name), gameObject);
                return;
            }

            if ((channel & Channel.NonOwner) == 0 && !isOwner)
            {
                Debug.LogWarning(Service.Text.Format(Log.E264, methodName, gameObject.name), gameObject);
                return;
            }

            if (NetworkManager.Client.connection == null)
            {
                Debug.LogError(Service.Text.Format(Log.E265, methodName, gameObject.name), gameObject);
                return;
            }

            var message = new ServerRpcMessage
            {
                objectId = objectId,
                sourceId = sourceId,
                methodHash = (ushort)methodHash,
                segment = writer,
            };

            NetworkManager.Client.connection.Send(message, (channel & Channel.Reliable) != 0 ? Channel.Reliable : Channel.Unreliable);
        }

        protected void SendClientRpcInternal(string methodName, int methodHash, MemoryWriter writer, int channel)
        {
            if (!NetworkManager.Server.isActive)
            {
                Debug.LogError(Service.Text.Format(Log.E266, methodName), gameObject);
                return;
            }

            if (!isServer)
            {
                Debug.LogWarning(Service.Text.Format(Log.E267, methodName, gameObject.name), gameObject);
                return;
            }

            var message = new ClientRpcMessage
            {
                objectId = objectId,
                sourceId = sourceId,
                methodHash = (ushort)methodHash,
                segment = writer
            };

            using var current = MemoryWriter.Pop();
            current.Invoke(message);

            foreach (var client in NetworkManager.Server.clients.Values.Where(client => client.isReady))
            {
                if ((channel & Channel.NonOwner) == 0 || client != connection)
                {
                    client.Send(message, (channel & Channel.Reliable) != 0 ? Channel.Reliable : Channel.Unreliable);
                }
            }
        }

        protected void SendTargetRpcInternal(NetworkClient client, string methodName, int methodHash, MemoryWriter writer, int channel)
        {
            if (!NetworkManager.Server.isActive)
            {
                Debug.LogError(Service.Text.Format(Log.E266, methodName), gameObject);
                return;
            }

            if (!isServer)
            {
                Debug.LogWarning(Service.Text.Format(Log.E267, methodName, gameObject.name), gameObject);
                return;
            }

            client ??= connection;

            if (client == null)
            {
                Debug.LogError(Service.Text.Format(Log.E268, methodName, gameObject.name), gameObject);
                return;
            }

            var message = new ClientRpcMessage
            {
                objectId = objectId,
                sourceId = sourceId,
                methodHash = (ushort)methodHash,
                segment = writer
            };

            client.Send(message, channel);
        }
    }
}