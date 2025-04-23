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
    public abstract partial class NetworkBehaviour
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
        public void SyncVarGetterGameObject(ref GameObject field, Action<GameObject, GameObject> OnChanged, MemoryGetter getter, ref uint objectId)
        {
            var oldValue = objectId;
            var oldObject = field;
            objectId = getter.GetUInt();
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
                if (newObject.TryGetComponent(out NetworkObject entity))
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
                if (newObject.TryGetComponent(out NetworkObject entity))
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
        public void SyncVarSetterNetworkObject(NetworkObject value, ref NetworkObject field, ulong dirty, Action<NetworkObject, NetworkObject> OnChanged, ref uint objectId)
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
        public void SyncVarGetterNetworkObject(ref NetworkObject field, Action<NetworkObject, NetworkObject> OnChanged, MemoryGetter getter, ref uint objectId)
        {
            var oldValue = objectId;
            var oldObject = field;
            objectId = getter.GetUInt();
            field = GetSyncVarNetworkObject(objectId, ref field);
            if (OnChanged != null && !SyncVarEqualGeneral(oldValue, ref objectId))
            {
                OnChanged(oldObject, field);
            }
        }
        
        private static bool SyncVarEqualNetworkObject(NetworkObject @object, uint objectId)
        {
            uint newValue = 0;
            if (@object != null)
            {
                newValue = @object.objectId;
                if (newValue == 0)
                {
                    Debug.LogWarning(Service.Text.Format(Log.E269, @object.name));
                }
            }

            return newValue == objectId;
        }

        private NetworkObject GetSyncVarNetworkObject(uint objectId, ref NetworkObject @object)
        {
            if (isServer || !isClient) return @object;
            NetworkManager.Client.spawns.TryGetValue(objectId, out @object);
            return @object;
        }
        
        private void SetSyncVarNetworkObject(NetworkObject @object, ref NetworkObject field, ulong dirty, ref uint objectId)
        {
            if (GetSyncVarHook(dirty)) return;
            uint newValue = 0;
            if (@object != null)
            {
                newValue = @object.objectId;
                if (newValue == 0)
                {
                    Debug.LogWarning(Service.Text.Format(Log.E269, @object.name));
                }
            }

            SetSyncVarDirty(dirty);
            objectId = newValue;
            field = @object;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SyncVarSetterNetworkBehaviour<T>(T value, ref T field, ulong dirty, Action<T, T> OnChanged, ref NetworkVariable variable) where T : NetworkBehaviour
        {
            if (!SyncVarEqualNetworkBehaviour(value, variable))
            {
                var oldValue = field;
                SetSyncVarNetworkBehaviour(value, ref field, dirty, ref variable);
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
        public void SyncVarGetterNetworkBehaviour<T>(ref T field, Action<T, T> OnChanged, MemoryGetter getter, ref NetworkVariable variable) where T : NetworkBehaviour
        {
            var oldValue = variable;
            var oldObject = field;
            variable = getter.GetNetworkVariable();
            field = GetSyncVarNetworkBehaviour(variable, ref field);
            if (OnChanged != null && !SyncVarEqualGeneral(oldValue, ref variable))
            {
                OnChanged(oldObject, field);
            }
        }

        private static bool SyncVarEqualNetworkBehaviour<T>(T @object, NetworkVariable variable) where T : NetworkBehaviour
        {
            uint newValue = 0;
            byte index = 0;
            if (@object != null)
            {
                newValue = @object.objectId;
                index = @object.componentId;
                if (newValue == 0)
                {
                    Debug.LogWarning(Service.Text.Format(Log.E269, @object.name));
                }
            }

            return variable.Equals(newValue, index);
        }
        
        public T GetSyncVarNetworkBehaviour<T>(NetworkVariable variable, ref T field) where T : NetworkBehaviour
        {
            if (isServer || !isClient)
            {
                return field;
            }

            if (!NetworkManager.Client.spawns.TryGetValue(variable.objectId, out var oldObject))
            {
                return null;
            }

            field = (T)oldObject.entities[variable.componentId];
            return field;
        }

        private void SetSyncVarNetworkBehaviour<T>(T @object, ref T field, ulong dirty, ref NetworkVariable variable) where T : NetworkBehaviour
        {
            if (GetSyncVarHook(dirty)) return;
            uint newValue = 0;
            byte index = 0;
            if (@object != null)
            {
                newValue = @object.objectId;
                index = @object.componentId;
                if (newValue == 0)
                {
                    Debug.LogWarning(Service.Text.Format(Log.E269, @object.name));
                }
            }

            variable = new NetworkVariable(newValue, index);
            SetSyncVarDirty(dirty);
            field = @object;
        }
    }
}