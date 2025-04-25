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
    public abstract partial class NetworkBehaviour : MonoBehaviour
    {
        internal byte componentId;

        public SyncMode syncDirection;

        public float syncInterval;

        private ulong syncVarHook;

        private double syncVarTime;

        protected ulong syncVarDirty { get; set; }

        public NetworkObject @object { get; internal set; }

        public uint objectId => @object.objectId;

        public bool isOwner => (@object?.entityMode & EntityMode.Owner) != 0;

        public bool isServer => (@object?.entityMode & EntityMode.Server) != 0;

        public bool isClient => (@object?.entityMode & EntityMode.Client) != 0;

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

        public NetworkClient connection => @object.connection;

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

        internal void Serialize(MemorySetter setter, bool status)
        {
            var headerPosition = setter.position;
            setter.SetByte(0);
            var contentPosition = setter.position;

            try
            {
                OnSerialize(setter, status);
            }
            catch (Exception e)
            {
                Debug.LogError(Service.Text.Format(Log.E260, name, GetType(), @object.sceneId, e));
            }

            var endPosition = setter.position;
            setter.position = headerPosition;
            var size = endPosition - contentPosition;
            var safety = (byte)(size & 0xFF);
            setter.SetByte(safety);
            setter.position = endPosition;
        }

        internal bool Deserialize(MemoryGetter getter, bool status)
        {
            var result = true;
            var safety = getter.GetByte();
            var chunkStart = getter.position;
            try
            {
                OnDeserialize(getter, status);
            }
            catch (Exception e)
            {
                Debug.LogError(Service.Text.Format(Log.E260, name, GetType(), @object.sceneId, e));
                result = false;
            }

            var size = getter.position - chunkStart;
            var sizeHash = (byte)(size & 0xFF);
            if (sizeHash != safety)
            {
                Debug.LogError(Service.Text.Format(Log.E261, size, sizeHash, safety));
                var cleared = (uint)size & 0xFFFFFF00;
                getter.position = chunkStart + (int)(cleared | safety);
                result = false;
            }

            return result;
        }

        protected virtual void OnSerialize(MemorySetter setter, bool status)
        {
            SerializeSyncVars(setter, status);
        }

        protected virtual void OnDeserialize(MemoryGetter getter, bool status)
        {
            DeserializeSyncVars(getter, status);
        }

        protected virtual void SerializeSyncVars(MemorySetter setter, bool status)
        {
        }

        protected virtual void DeserializeSyncVars(MemoryGetter getter, bool status)
        {
        }

        protected void SendServerRpcInternal(string methodName, int methodHash, MemorySetter setter, int channel)
        {
            if (!NetworkManager.Client.isActive)
            {
                Debug.LogError(Service.Text.Format(Log.E262, methodName), gameObject);
                return;
            }

            if (!NetworkManager.Client.isReady)
            {
                Debug.LogWarning(Service.Text.Format(Log.E263, methodName, name), gameObject);
                return;
            }

            if ((channel & Channel.NonOwner) == 0 && !isOwner)
            {
                Debug.LogWarning(Service.Text.Format(Log.E264, methodName, name), gameObject);
                return;
            }

            if (NetworkManager.Client.connection == null)
            {
                Debug.LogError(Service.Text.Format(Log.E265, methodName, name), gameObject);
                return;
            }

            var message = new ServerRpcMessage
            {
                objectId = objectId,
                componentId = componentId,
                methodHash = (ushort)methodHash,
                segment = setter,
            };

            NetworkManager.Client.connection.Send(message, (channel & Channel.Reliable) != 0 ? Channel.Reliable : Channel.Unreliable);
        }

        protected void SendClientRpcInternal(string methodName, int methodHash, MemorySetter setter, int channel)
        {
            if (!NetworkManager.Server.isActive)
            {
                Debug.LogError(Service.Text.Format(Log.E266, methodName), gameObject);
                return;
            }

            if (!isServer)
            {
                Debug.LogWarning(Service.Text.Format(Log.E267, methodName, name), gameObject);
                return;
            }

            var message = new ClientRpcMessage
            {
                objectId = objectId,
                componentId = componentId,
                methodHash = (ushort)methodHash,
                segment = setter
            };

            using var current = MemorySetter.Pop();
            current.Invoke(message);

            foreach (var client in NetworkManager.Server.clients.Values.Where(client => client.isReady))
            {
                if ((channel & Channel.NonOwner) == 0 || client != connection)
                {
                    client.Send(message, (channel & Channel.Reliable) != 0 ? Channel.Reliable : Channel.Unreliable);
                }
            }
        }

        protected void SendTargetRpcInternal(NetworkClient client, string methodName, int methodHash, MemorySetter setter, int channel)
        {
            if (!NetworkManager.Server.isActive)
            {
                Debug.LogError(Service.Text.Format(Log.E266, methodName), gameObject);
                return;
            }

            if (!isServer)
            {
                Debug.LogWarning(Service.Text.Format(Log.E267, methodName, name), gameObject);
                return;
            }

            if (client == null)
            {
                client = connection;
            }

            if (client == null)
            {
                Debug.LogError(Service.Text.Format(Log.E268, methodName, name), gameObject);
                return;
            }

            var message = new ClientRpcMessage
            {
                objectId = objectId,
                componentId = componentId,
                methodHash = (ushort)methodHash,
                segment = setter
            };

            client.Send(message, channel);
        }
    }
}