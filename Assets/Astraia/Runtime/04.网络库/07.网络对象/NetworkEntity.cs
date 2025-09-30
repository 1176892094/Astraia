// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2024-12-21 23:12:50
// # Recently: 2024-12-22 23:12:53
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Astraia.Common;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
#if UNITY_EDITOR && ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Astraia.Net
{
    [Serializable]
    public class NetworkEntity : Entity
    {
        [HideInInspector] [SerializeField] internal uint assetId;

        [HideInInspector] [SerializeField] internal uint sceneId;
#if UNITY_EDITOR && ODIN_INSPECTOR
        [EnumToggleButtons, HideLabel]
#endif
        [SerializeField]
        internal EntityType visible;

        private int frameCount;

        internal uint objectId;

        internal EntityMode mode;

        internal EntityState state;

        internal NetworkClient client;

        internal MemoryWriter owner = new MemoryWriter();

        internal MemoryWriter other = new MemoryWriter();

        internal List<NetworkModule> modules = new List<NetworkModule>();

        public bool isOwner => (mode & EntityMode.Owner) != 0;

        public bool isServer => (mode & EntityMode.Server) != 0;

        public bool isClient => (mode & EntityMode.Client) != 0;


        protected override void OnEnable()
        {
            base.OnEnable();
            if ((state & EntityState.Awake) == 0)
            {
                foreach (var module in Values)
                {
                    if (module is NetworkModule entity)
                    {
                        modules.Add(entity);
                    }
                }

                for (byte i = 0; i < modules.Count; ++i)
                {
                    modules[i].moduleId = i;
                }

                state |= EntityState.Awake;
            }
        }

        protected override void OnDestroy()
        {
            if (isServer && (state & EntityState.Destroy) == 0)
            {
                NetworkManager.Server.Destroy(gameObject);
            }

            if (isClient)
            {
                NetworkManager.Client.spawns.Remove(objectId);
            }

            owner = null;
            other = null;
            client = null;
            modules = null;
            base.OnDestroy();
        }

        public virtual void Reset()
        {
            objectId = 0;
            client = null;
            owner.position = 0;
            other.position = 0;
            mode = EntityMode.None;
            state = EntityState.None;
            NetworkServerListener.Release(this);
        }

#if UNITY_EDITOR
        private static readonly Dictionary<uint, GameObject> sceneData = new Dictionary<uint, GameObject>();
        protected virtual void OnValidate()
        {
            if (PrefabUtility.IsPartOfPrefabAsset(gameObject))
            {
                sceneId = 0;
                AssignAssetId(AssetDatabase.GetAssetPath(gameObject));
            }
            else if (PrefabStageUtility.GetCurrentPrefabStage())
            {
                var prefab = PrefabStageUtility.GetPrefabStage(gameObject);
                if (prefab)
                {
                    sceneId = 0;
                    AssignAssetId(prefab.assetPath);
                }
            }
            else if (PrefabUtility.IsPartOfPrefabInstance(gameObject))
            {
                var prefab = PrefabUtility.GetCorrespondingObjectFromSource(gameObject);
                if (prefab)
                {
                    AssignSceneId();
                    AssignAssetId(AssetDatabase.GetAssetPath(prefab));
                }
            }
            else
            {
                AssignSceneId();
            }

            return;

            void AssignAssetId(string assetPath)
            {
                if (!string.IsNullOrWhiteSpace(assetPath))
                {
                    if (sceneId == 0)
                    {
                        if (!uint.TryParse(name, out var id))
                        {
                            Debug.LogWarning("请将 {0} 名称修改为数字格式!".Format(gameObject), gameObject);
                            return;
                        }

                        assetId = id;
                    }
                    else
                    {
                        assetId = 0;
                    }
                }
            }

            void AssignSceneId()
            {
                if (Application.isPlaying) return;
                var duplicate = sceneData.TryGetValue(sceneId, out var entity) && entity && entity != gameObject;
                if (sceneId == 0 || duplicate)
                {
                    sceneId = 0;
                    if (BuildPipeline.isBuildingPlayer)
                    {
                        throw new Exception("网络对象 {0} 在构建前需要打开并重新保存。因为网络对象 {1} 没有场景Id".Format(gameObject.scene.path, name));
                    }

                    var random = (uint)Service.Rng.Next();
                    duplicate = sceneData.TryGetValue(random, out entity) && entity && entity != gameObject;
                    if (!duplicate)
                    {
                        sceneId = random;
                    }
                }

                sceneData[sceneId] = gameObject;
            }
        }
#endif


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsDirty(ulong mask, int index)
        {
            return (mask & (ulong)(1 << index)) != 0;
        }

        internal void InvokeMessage(byte moduleId, ushort function, InvokeMode mode, MemoryReader reader, NetworkClient client = null)
        {
            if (transform == null)
            {
                Log.Warn("调用了已经删除的网络对象。{0} [{1}] {2}", mode, function, objectId);
                return;
            }

            if (moduleId >= modules.Count)
            {
                Log.Warn("网络对象 {0} 没有找到网络行为组件 {1}", objectId, moduleId);
                return;
            }

            if (!NetworkAttribute.Invoke(function, mode, client, reader, modules[moduleId]))
            {
                Log.Warn("无法调用{0} [{1}] 网络对象: {2} 网络标识: {3}", mode, function, gameObject.name, objectId);
            }
        }

        internal void Synchronization(int frame)
        {
            if (frameCount != frame)
            {
                frameCount = frame;
                owner.position = 0;
                other.position = 0;
                ServerSerialize(false, owner, other);
                ClearDirty(true);
            }
        }

        internal void ServerSerialize(bool initialize, MemoryWriter owner, MemoryWriter other)
        {
            var components = modules;
            var (ownerMask, otherMask) = ServerDirtyMasks(initialize);

            if (ownerMask != 0)
            {
                Service.Bit.EncodeULong(owner, ownerMask);
            }

            if (otherMask != 0)
            {
                Service.Bit.EncodeULong(other, otherMask);
            }

            if ((ownerMask | otherMask) != 0)
            {
                for (var i = 0; i < components.Count; ++i)
                {
                    var component = components[i];
                    var ownerDirty = IsDirty(ownerMask, i);
                    var otherDirty = IsDirty(otherMask, i);
                    if (ownerDirty || otherDirty)
                    {
                        using var writer = MemoryWriter.Pop();
                        component.Serialize(writer, initialize);
                        ArraySegment<byte> segment = writer;
                        if (ownerDirty)
                        {
                            owner.WriteBytes(segment.Array, segment.Offset, segment.Count);
                        }

                        if (otherDirty)
                        {
                            other.WriteBytes(segment.Array, segment.Offset, segment.Count);
                        }
                    }
                }
            }
        }

        internal void ClientSerialize(MemoryWriter writer)
        {
            var components = modules;
            var dirtyMask = ClientDirtyMask();
            if (dirtyMask != 0)
            {
                Service.Bit.EncodeULong(writer, dirtyMask);
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
            var components = modules;
            var mask = Service.Bit.DecodeULong(reader);

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
            var components = modules;
            var mask = Service.Bit.DecodeULong(reader);

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
            ulong otherMask = 0;

            var components = modules;
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
                    otherMask |= mask;
                }
            }

            return (ownerMask, otherMask);
        }

        private ulong ClientDirtyMask()
        {
            ulong mask = 0;
            var components = modules;
            for (var i = 0; i < components.Count; ++i)
            {
                var component = components[i];
                if (isOwner && component.syncDirection == SyncMode.Client)
                {
                    if (component.IsDirty())
                    {
                        mask |= 1U << i;
                    }
                }
            }

            return mask;
        }

        internal void ClearDirty(bool total)
        {
            foreach (var module in modules)
            {
                if (module.IsDirty() || total)
                {
                    module.ClearDirty();
                }
            }
        }

        internal void OnStartClient()
        {
            if ((state & EntityState.Spawn) == 0)
            {
                foreach (var module in modules)
                {
                    if (module is IStartClient result)
                    {
                        result.OnStartClient();
                    }
                }

                state |= EntityState.Spawn;
            }
        }

        internal void OnStopClient()
        {
            if ((state & EntityState.Spawn) != 0)
            {
                foreach (var module in modules)
                {
                    if (module is IStopClient result)
                    {
                        result.OnStopClient();
                    }
                }
            }
        }

        internal void OnStartServer()
        {
            foreach (var module in modules)
            {
                if (module is IStartServer result)
                {
                    result.OnStartServer();
                }
            }
        }

        internal void OnStopServer()
        {
            foreach (var module in modules)
            {
                if (module is IStopServer result)
                {
                    result.OnStopServer();
                }
            }
        }

        private void OnStartAuthority()
        {
            foreach (var module in modules)
            {
                if (module is IStartAuthority result)
                {
                    result.OnStartAuthority();
                }
            }
        }

        private void OnStopAuthority()
        {
            foreach (var module in modules)
            {
                if (module is IStopAuthority result)
                {
                    result.OnStopAuthority();
                }
            }
        }

        internal void OnNotifyAuthority()
        {
            if ((state & EntityState.Owner) == 0 && isOwner)
            {
                OnStartAuthority();
            }
            else if ((state & EntityState.Owner) != 0 && !isOwner)
            {
                OnStopAuthority();
            }

            if (isOwner)
            {
                state |= EntityState.Owner;
            }
            else
            {
                state &= ~EntityState.Owner;
            }
        }

        public static implicit operator uint(NetworkEntity entity)
        {
            return entity.objectId;
        }

        public static explicit operator NetworkEntity(uint objectId)
        {
            if (NetworkManager.isServer)
            {
                if (NetworkManager.Server.spawns.TryGetValue(objectId, out var entity))
                {
                    return entity;
                }
            }

            if (NetworkManager.isClient)
            {
                if (NetworkManager.Client.spawns.TryGetValue(objectId, out var entity))
                {
                    return entity;
                }
            }

            return null;
        }
    }
}