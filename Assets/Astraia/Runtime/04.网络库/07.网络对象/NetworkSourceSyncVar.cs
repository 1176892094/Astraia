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
    public abstract partial class NetworkSource
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
        public void SyncVarGetterGeneral<T>(ref T field, Action<T, T> OnChanged, T value)
        {
            var oldValue = field;
            field = value;
            if (OnChanged != null && !SyncVarEqualGeneral(oldValue, ref field))
            {
                OnChanged(oldValue, field);
            }
        }
        
        private static bool SyncVarEqualGeneral<T>(T value, ref T field)
        {
            return EqualityComparer<T>.Default.Equals(value, field);
        }
        
        private void SetSyncVarGeneral<T>(T value, ref T field, ulong dirty)
        {
            SetSyncVarDirty(dirty);
            field = value;
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
        public void SyncVarGetterGameObject(ref GameObject field, Action<GameObject, GameObject> OnChanged, MemoryReader reader, ref uint objectId)
        {
            var oldValue = objectId;
            var oldObject = field;
            objectId = reader.GetUInt();
            field = GetSyncVarGameObject(objectId, ref field);
            if (OnChanged != null && !SyncVarEqualGeneral(oldValue, ref objectId))
            {
                OnChanged(oldObject, field);
            }
        }
        
        private static bool SyncVarEqualGameObject(GameObject newObject, uint objectId)
        {
            uint newValue = 0;
            if (newObject != null)
            {
                if (newObject.TryGetComponent(out NetworkEntity entity))
                {
                    newValue = entity.objectId;
                    if (newValue == 0)
                    {
                        Debug.LogWarning(Service.Text.Format(Log.E269, newObject.name));
                    }
                }
            }
        
            return newValue == objectId;
        }
        
        private GameObject GetSyncVarGameObject(uint objectId, ref GameObject field)
        {
            if (isServer || !isClient)
            {
                return field;
            }
        
            if (NetworkManager.Client.spawns.TryGetValue(objectId, out var oldObject) && oldObject != null)
            {
                return field = oldObject.gameObject;
            }
        
            return null;
        }
        
        private void SetSyncVarGameObject(GameObject newObject, ref GameObject objectField, ulong dirty, ref uint objectId)
        {
            if (GetSyncVarHook(dirty)) return;
            uint newValue = 0;
            if (newObject != null)
            {
                if (newObject.TryGetComponent(out NetworkEntity entity))
                {
                    newValue = entity.objectId;
                    if (newValue == 0)
                    {
                        Debug.LogWarning(Service.Text.Format(Log.E269, newObject.name));
                    }
                }
            }
        
            SetSyncVarDirty(dirty);
            objectField = newObject;
            objectId = newValue;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SyncVarSetterNetworkObject(NetworkEntity value, ref NetworkEntity field, ulong dirty, Action<NetworkEntity, NetworkEntity> OnChanged, ref uint objectId)
        {
            if (!SyncVarEqualNetworkObject(value, objectId))
            {
                var oldValue = field;
                SetSyncVarNetworkObject(value, ref field, dirty, ref objectId);
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
        public void SyncVarGetterNetworkObject(ref NetworkEntity field, Action<NetworkEntity, NetworkEntity> OnChanged, MemoryReader reader, ref uint objectId)
        {
            var oldValue = objectId;
            var oldObject = field;
            objectId = reader.GetUInt();
            field = GetSyncVarNetworkObject(objectId, ref field);
            if (OnChanged != null && !SyncVarEqualGeneral(oldValue, ref objectId))
            {
                OnChanged(oldObject, field);
            }
        }
        
        private static bool SyncVarEqualNetworkObject(NetworkEntity entity, uint objectId)
        {
            uint newValue = 0;
            if (entity != null)
            {
                newValue = entity.objectId;
                if (newValue == 0)
                {
                    Debug.LogWarning(Service.Text.Format(Log.E269, entity.gameObject.name));
                }
            }
        
            return newValue == objectId;
        }
        
        private NetworkEntity GetSyncVarNetworkObject(uint objectId, ref NetworkEntity entity)
        {
            if (isServer || !isClient) return entity;
            NetworkManager.Client.spawns.TryGetValue(objectId, out entity);
            return entity;
        }
        
        private void SetSyncVarNetworkObject(NetworkEntity entity, ref NetworkEntity field, ulong dirty, ref uint objectId)
        {
            if (GetSyncVarHook(dirty)) return;
            uint newValue = 0;
            if (entity != null)
            {
                newValue = entity.objectId;
                if (newValue == 0)
                {
                    Debug.LogWarning(Service.Text.Format(Log.E269, entity.gameObject.name));
                }
            }
        
            SetSyncVarDirty(dirty);
            objectId = newValue;
            field = entity;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SyncVarSetterNetworkSource<T>(T value, ref T field, ulong dirty, Action<T, T> OnChanged, ref NetworkVariable variable) where T : NetworkSource
        {
            if (!SyncVarEqualNetworkSource(value, variable))
            {
                var oldValue = field;
                SetSyncVarNetworkSource(value, ref field, dirty, ref variable);
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
        public void SyncVarGetterNetworkSource<T>(ref T field, Action<T, T> OnChanged, MemoryReader reader, ref NetworkVariable variable) where T : NetworkSource
        {
            var oldValue = variable;
            var oldObject = field;
            variable = reader.GetNetworkVariable();
            field = GetSyncVarNetworkSource(variable, ref field);
            if (OnChanged != null && !SyncVarEqualGeneral(oldValue, ref variable))
            {
                OnChanged(oldObject, field);
            }
        }
        
        private static bool SyncVarEqualNetworkSource<T>(T entity, NetworkVariable variable) where T : NetworkSource
        {
            uint newValue = 0;
            byte index = 0;
            if (entity != null)
            {
                newValue = entity.objectId;
                index = entity.sourceId;
                if (newValue == 0)
                {
                    Debug.LogWarning(Service.Text.Format(Log.E269, entity.gameObject.name));
                }
            }
        
            return variable.Equals(newValue, index);
        }
        
        public T GetSyncVarNetworkSource<T>(NetworkVariable variable, ref T field) where T : NetworkSource
        {
            if (isServer || !isClient)
            {
                return field;
            }
        
            if (!NetworkManager.Client.spawns.TryGetValue(variable.objectId, out var oldObject))
            {
                return null;
            }
        
            field = (T)oldObject.entities[variable.sourceId];
            return field;
        }
        
        private void SetSyncVarNetworkSource<T>(T entity, ref T field, ulong dirty, ref NetworkVariable variable) where T : NetworkSource
        {
            if (GetSyncVarHook(dirty)) return;
            uint newValue = 0;
            byte index = 0;
            if (entity != null)
            {
                newValue = entity.objectId;
                index = entity.sourceId;
                if (newValue == 0)
                {
                    Debug.LogWarning(Service.Text.Format(Log.E269, entity.gameObject.name));
                }
            }
        
            variable = new NetworkVariable(newValue, index);
            SetSyncVarDirty(dirty);
            field = entity;
        }
    }
}