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
using Sirenix.OdinInspector;
using UnityEngine;

namespace Astraia.Net
{
    public abstract partial class NetworkSource : Source
    {
       [ShowInInspector] internal byte sourceId;

        public SyncMode syncDirection;

        public float syncInterval;

        [ShowInInspector]    private ulong syncVarHook;

        [ShowInInspector]     protected double syncVarTime;

        [ShowInInspector]    protected ulong syncVarDirty { get; set; }

        public NetworkEntity entity { get; internal set; }

        public uint objectId => entity.objectId;

        public bool isOwner => (entity?.entityMode & EntityMode.Owner) != 0;

        public bool isServer => (entity?.entityMode & EntityMode.Server) != 0;

        public bool isClient => (entity?.entityMode & EntityMode.Client) != 0;

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

        public NetworkClient connection => entity.connection;

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

        internal void Serialize(MemoryWriter writer, bool status)
        {
            var headerPosition = writer.position;
            writer.SetByte(0);
            var contentPosition = writer.position;

            try
            {
                OnSerialize(writer, status);
            }
            catch (Exception e)
            {
                Debug.LogError(Service.Text.Format(Log.E260, gameObject.name, GetType(), entity.sceneId, e));
            }

            var endPosition = writer.position;
            writer.position = headerPosition;
            var size = endPosition - contentPosition;
            var safety = (byte)(size & 0xFF);
            writer.SetByte(safety);
            writer.position = endPosition;
        }

        internal bool Deserialize(MemoryReader reader, bool status)
        {
            var result = true;
            var safety = reader.GetByte();
            var chunkStart = reader.position;
            try
            {
                OnDeserialize(reader, status);
            }
            catch (Exception e)
            {
                Debug.LogError(Service.Text.Format(Log.E260, gameObject.name, GetType(), entity.sceneId, e));
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

        protected virtual void OnSerialize(MemoryWriter writer, bool status)
        {
            SerializeSyncVars(writer, status);
        }

        protected virtual void OnDeserialize(MemoryReader reader, bool status)
        {
            DeserializeSyncVars(reader, status);
        }

        protected virtual void SerializeSyncVars(MemoryWriter writer, bool status)
        {
        }

        protected virtual void DeserializeSyncVars(MemoryReader reader, bool status)
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

            if (client == null)
            {
                client = connection;
            }

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