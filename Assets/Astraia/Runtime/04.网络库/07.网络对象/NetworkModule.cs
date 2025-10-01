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
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Astraia.Common;
using UnityEngine;

namespace Astraia.Net
{
    [Serializable]
    public abstract class NetworkModule : Module<NetworkEntity>
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
                Debug.LogError("序列化对象失败。对象名称: {0}[{1}][{2}]\n{3}".Format(owner.name, GetType(), owner.sceneId, e), owner);
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
                Debug.LogError("序列化对象失败。对象名称: {0}[{1}][{2}]\n{3}".Format(owner.name, GetType(), owner.sceneId, e), owner);
                result = false;
            }

            var value = reader.position - startPosition;
            var count = (byte)(value & 0xFF);
            if (count != safety)
            {
                Debug.LogError("反序列化字节不匹配。读取字节: {0} 哈希对比:{1}/{2}".Format(value, count, safety), owner);
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
            if (!NetworkManager.isClient)
            {
                Debug.LogWarning("调用 {0} 但是客户端不是活跃的。对象名称：{1}".Format(name, owner.name), owner);
                return;
            }

            if (!NetworkManager.Client.isReady)
            {
                Debug.LogWarning("调用 {0} 但是客户端没有准备就绪的。对象名称：{1}".Format(name, owner.name), owner);
                return;
            }

            if ((channel & Channel.IgnoreOwner) == 0 && !isOwner)
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
            if (!NetworkManager.isServer)
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

            foreach (var result in NetworkSpawner.Query(owner))
            {
                if (result.isReady)
                {
                    if ((channel & Channel.IgnoreOwner) == 0 || result != client)
                    {
                        result.Send(message, (channel & Channel.Reliable) != 0 ? Channel.Reliable : Channel.Unreliable);
                    }
                }
            }
        }

        protected void SendTargetRpcInternal(NetworkClient client, string name, int hash, MemoryWriter writer, int channel)
        {
            if (!NetworkManager.isServer)
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SyncVarSetterGeneral<T>(T value, ref T field, ulong dirty, Action<T, T> OnChanged)
        {
            if (!SyncVarEqualGeneral(value, ref field))
            {
                var oldValue = field;
                SetSyncVarGeneral(value, ref field, dirty);
                if (OnChanged != null)
                {
                    if (NetworkManager.isHost && !GetSyncVarHook(dirty))
                    {
                        SetSyncVarHook(dirty, true);
                        OnChanged(oldValue, value);
                        SetSyncVarHook(dirty, false);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SyncVarSetterGameObject(GameObject value, ref GameObject field, ulong dirty, Action<GameObject, GameObject> OnChanged, ref uint objectId)
        {
            if (!SyncVarEqualGameObject(value, objectId))
            {
                var oldValue = field;
                SetSyncVarGameObject(value, ref field, dirty, ref objectId);
                if (OnChanged != null)
                {
                    if (NetworkManager.isHost && !GetSyncVarHook(dirty))
                    {
                        SetSyncVarHook(dirty, true);
                        OnChanged(oldValue, value);
                        SetSyncVarHook(dirty, false);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SyncVarSetterNetworkEntity(NetworkEntity value, ref NetworkEntity field, ulong dirty, Action<NetworkEntity, NetworkEntity> OnChanged, ref uint objectId)
        {
            if (!SyncVarEqualNetworkEntity(value, objectId))
            {
                var oldValue = field;
                SetSyncVarNetworkEntity(value, ref field, dirty, ref objectId);
                if (OnChanged != null)
                {
                    if (NetworkManager.isHost && !GetSyncVarHook(dirty))
                    {
                        SetSyncVarHook(dirty, true);
                        OnChanged(oldValue, value);
                        SetSyncVarHook(dirty, false);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SyncVarSetterNetworkModule<T>(T value, ref T field, ulong dirty, Action<T, T> OnChanged, ref NetworkVariable variable) where T : NetworkModule
        {
            if (!SyncVarEqualNetworkModule(value, variable))
            {
                var oldValue = field;
                SetSyncVarNetworkModule(value, ref field, dirty, ref variable);
                if (OnChanged != null)
                {
                    if (NetworkManager.isHost && !GetSyncVarHook(dirty))
                    {
                        SetSyncVarHook(dirty, true);
                        OnChanged(oldValue, value);
                        SetSyncVarHook(dirty, false);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SyncVarGetterGeneral<T>(ref T field, Action<T, T> OnChanged, T value)
        {
            var oldValue = field;
            field = value;
            if (OnChanged != null && !SyncVarEqualGeneral(oldValue, ref field))
            {
                OnChanged(oldValue, field);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SyncVarGetterGameObject(ref GameObject field, Action<GameObject, GameObject> OnChanged, MemoryReader reader, ref uint objectId)
        {
            var oldValue = objectId;
            var value = field;
            objectId = reader.ReadUInt();
            field = GetSyncVarGameObject(objectId, ref field);
            if (OnChanged != null && !SyncVarEqualGeneral(oldValue, ref objectId))
            {
                OnChanged(value, field);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SyncVarGetterNetworkEntity(ref NetworkEntity field, Action<NetworkEntity, NetworkEntity> OnChanged, MemoryReader reader, ref uint objectId)
        {
            var oldValue = objectId;
            var value = field;
            objectId = reader.ReadUInt();
            field = GetSyncVarNetworkEntity(objectId, ref field);
            if (OnChanged != null && !SyncVarEqualGeneral(oldValue, ref objectId))
            {
                OnChanged(value, field);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SyncVarGetterNetworkModule<T>(ref T field, Action<T, T> OnChanged, MemoryReader reader, ref NetworkVariable variable) where T : NetworkModule
        {
            var oldValue = variable;
            var value = field;
            variable = reader.ReadNetworkVariable();
            field = GetSyncVarNetworkModule(variable, ref field);
            if (OnChanged != null && !SyncVarEqualGeneral(oldValue, ref variable))
            {
                OnChanged(value, field);
            }
        }

        private static bool SyncVarEqualGeneral<T>(T value, ref T field)
        {
            return EqualityComparer<T>.Default.Equals(value, field);
        }

        private static bool SyncVarEqualGameObject(GameObject value, uint objectId)
        {
            var newValue = 0U;
            if (value)
            {
                if (value.TryGetComponent(out NetworkEntity entity))
                {
                    newValue = entity.objectId;
                    if (newValue == 0)
                    {
                        Log.Warn("设置网络变量的对象未初始化。对象名称: {0}", value.name);
                    }
                }
            }

            return newValue == objectId;
        }

        private static bool SyncVarEqualNetworkEntity(NetworkEntity value, uint objectId)
        {
            var newValue = 0U;
            if (value)
            {
                newValue = value.objectId;
                if (newValue == 0)
                {
                    Log.Warn("设置网络变量的对象未初始化。对象名称: {0}", value.gameObject.name);
                }
            }

            return newValue == objectId;
        }

        private static bool SyncVarEqualNetworkModule<T>(T entity, NetworkVariable variable) where T : NetworkModule
        {
            uint newValue = 0;
            byte newIndex = 0;
            if (entity)
            {
                newValue = entity.objectId;
                newIndex = entity.moduleId;
                if (newValue == 0)
                {
                    Log.Warn("设置网络变量的对象未初始化。对象名称: {0}", entity.gameObject.name);
                }
            }

            return variable.Equals(newValue, newIndex);
        }

        // ReSharper disable once RedundantAssignment
        private void SetSyncVarGeneral<T>(T value, ref T field, ulong dirty)
        {
            SetSyncVarDirty(dirty);
            field = value;
        }

        private void SetSyncVarGameObject(GameObject value, ref GameObject field, ulong dirty, ref uint objectId)
        {
            if (GetSyncVarHook(dirty))
            {
                return;
            }

            var newValue = 0U;
            if (value)
            {
                if (value.TryGetComponent(out NetworkEntity entity))
                {
                    newValue = entity.objectId;
                    if (newValue == 0)
                    {
                        Log.Warn("设置网络变量的对象未初始化。对象名称: {0}", value.name);
                    }
                }
            }

            SetSyncVarDirty(dirty);
            objectId = newValue;
            field = value;
        }

        private void SetSyncVarNetworkEntity(NetworkEntity value, ref NetworkEntity field, ulong dirty, ref uint objectId)
        {
            if (GetSyncVarHook(dirty))
            {
                return;
            }

            var newValue = 0U;
            if (value)
            {
                newValue = value.objectId;
                if (newValue == 0)
                {
                    Log.Warn("设置网络变量的对象未初始化。对象名称: {0}", value.gameObject.name);
                }
            }

            SetSyncVarDirty(dirty);
            objectId = newValue;
            field = value;
        }

        private void SetSyncVarNetworkModule<T>(T value, ref T field, ulong dirty, ref NetworkVariable variable) where T : NetworkModule
        {
            if (GetSyncVarHook(dirty))
            {
                return;
            }

            uint newValue = 0;
            byte newIndex = 0;
            if (value)
            {
                newValue = value.objectId;
                newIndex = value.moduleId;
                if (newValue == 0)
                {
                    Log.Warn("设置网络变量的对象未初始化。对象名称: {0}", value.gameObject.name);
                }
            }

            variable = new NetworkVariable(newValue, newIndex);
            SetSyncVarDirty(dirty);
            field = value;
        }

        private GameObject GetSyncVarGameObject(uint objectId, ref GameObject field)
        {
            if (isServer || !isClient)
            {
                return field;
            }

            if (NetworkManager.Client.spawns.TryGetValue(objectId, out var entity) && entity)
            {
                return field = entity.gameObject;
            }

            return null;
        }

        private NetworkEntity GetSyncVarNetworkEntity(uint objectId, ref NetworkEntity entity)
        {
            if (isServer || !isClient)
            {
                return entity;
            }

            NetworkManager.Client.spawns.TryGetValue(objectId, out entity);
            return entity;
        }

        public T GetSyncVarNetworkModule<T>(NetworkVariable variable, ref T field) where T : NetworkModule
        {
            if (isServer || !isClient)
            {
                return field;
            }

            if (!NetworkManager.Client.spawns.TryGetValue(variable.objectId, out var entity))
            {
                return null;
            }

            field = (T)entity.modules[variable.moduleId];
            return field;
        }
    }
}