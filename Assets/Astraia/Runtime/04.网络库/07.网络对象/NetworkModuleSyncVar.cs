// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2024-12-03 13:12:30
// # Recently: 2024-12-22 22:12:01
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************


using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Astraia.Net
{
    public abstract partial class NetworkModule
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SyncVarSetterGeneral<T>(T value, ref T field, ulong dirty, Action<T, T> OnChanged)
        {
            if (!SyncVarEqualGeneral(value, ref field))
            {
                var oldValue = field;
                SetSyncVarGeneral(value, ref field, dirty);
                if (OnChanged != null)
                {
                    if (NetworkManager.Mode == EntryMode.Host && !GetSyncVarHook(dirty))
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
                    if (NetworkManager.Mode == EntryMode.Host && !GetSyncVarHook(dirty))
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
                    if (NetworkManager.Mode == EntryMode.Host && !GetSyncVarHook(dirty))
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
                    if (NetworkManager.Mode == EntryMode.Host && !GetSyncVarHook(dirty))
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