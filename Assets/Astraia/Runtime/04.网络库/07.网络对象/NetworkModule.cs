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
    public abstract partial class NetworkModule : Module<NetworkEntity>
    {
        internal byte moduleId;

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

        public NetworkClient client => owner.client;

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
                Log.Error("序列化对象失败。对象名称: {0}[{1}][{2}]\n{3}", owner.name, GetType(), owner.sceneId, e);
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
            var startPosition = reader.position;
            try
            {
                OnDeserialize(reader, initialize);
            }
            catch (Exception e)
            {
                Log.Error("序列化对象失败。对象名称: {0}[{1}][{2}]\n{3}", owner.name, GetType(), owner.sceneId, e);
                result = false;
            }

            var value = reader.position - startPosition;
            var count = (byte)(value & 0xFF);
            if (count != safety)
            {
                Log.Error("反序列化字节不匹配。读取字节: {0} 哈希对比:{1}/{2}", value, count, safety);
                var cleared = (uint)value & 0xFFFFFF00;
                reader.position = startPosition + (int)(cleared | safety);
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

        protected void SendServerRpcInternal(string name, int hash, MemoryWriter writer, int channel)
        {
            if (!NetworkManager.Client.isActive)
            {
                Debug.LogWarning("调用 {0} 但是客户端不是活跃的。对象名称：{1}".Format(name, owner.name), owner);
                return;
            }

            if (!NetworkManager.Client.isReady)
            {
                Debug.LogWarning("调用 {0} 但是客户端没有准备就绪的。对象名称：{1}".Format(name, owner.name), owner);
                return;
            }

            if ((channel & Channel.NonOwner) == 0 && !isOwner)
            {
                Debug.LogWarning("调用 {0} 但是客户端没有对象权限。对象名称：{1}".Format(name, owner.name), owner);
                return;
            }

            if (NetworkManager.Client.connection == null)
            {
                Debug.LogWarning("调用 {0} 但是客户端的连接为空。对象名称：{1}".Format(name, owner.name), owner);
                return;
            }

            var message = new ServerRpcMessage
            {
                objectId = objectId,
                sourceId = moduleId,
                methodHash = (ushort)hash,
                segment = writer,
            };

            NetworkManager.Client.connection.Send(message, (channel & Channel.Reliable) != 0 ? Channel.Reliable : Channel.Unreliable);
        }

        protected void SendClientRpcInternal(string name, int hash, MemoryWriter writer, int channel)
        {
            if (!NetworkManager.Server.isActive)
            {
                Debug.LogError("调用 {0} 但是服务器不是活跃的。对象名称：{1}".Format(name, owner.name), owner);
                return;
            }

            if (!isServer)
            {
                Debug.LogWarning("调用 {0} 但是对象未初始化。对象名称：{1}".Format(name, owner.name), owner);
                return;
            }

            var message = new ClientRpcMessage
            {
                objectId = objectId,
                sourceId = moduleId,
                methodHash = (ushort)hash,
                segment = writer
            };

            using var current = MemoryWriter.Pop();
            current.Invoke(message);

            foreach (var conn in NetworkManager.Server.clients.Values.Where(conn => conn.isReady))
            {
                if ((channel & Channel.NonOwner) == 0 || conn != client)
                {
                    conn.Send(message, (channel & Channel.Reliable) != 0 ? Channel.Reliable : Channel.Unreliable);
                }
            }
        }

        protected void SendTargetRpcInternal(NetworkClient client, string name, int hash, MemoryWriter writer, int channel)
        {
            if (!NetworkManager.Server.isActive)
            {
                Debug.LogError("调用 {0} 但是服务器不是活跃的。对象名称：{1}".Format(name, owner.name), owner);
                return;
            }

            if (!isServer)
            {
                Debug.LogWarning("调用 {0} 但是对象未初始化。对象名称：{1}".Format(name, owner.name), owner);
                return;
            }

            client ??= owner.client;

            if (client == null)
            {
                Debug.LogError("调用 {0} 但是对象的连接为空。对象名称：{1}".Format(name, owner.name), owner);
                return;
            }

            var message = new ClientRpcMessage
            {
                objectId = objectId,
                sourceId = moduleId,
                methodHash = (ushort)hash,
                segment = writer
            };

            client.Send(message, channel);
        }
    }
}