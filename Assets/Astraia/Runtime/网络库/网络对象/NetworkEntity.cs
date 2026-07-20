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
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace Astraia.Net
{
    [Serializable]
    public class NetworkEntity : Entity
    {
        [HideInInspector] public uint objectId;

        [HideInInspector] public uint assetId;

        [HideInInspector] public uint sceneId;

        internal int count;

        internal NetworkClient client;

        internal NetworkModule[] modules;

        internal MemoryWriter owner = new MemoryWriter();

        internal MemoryWriter other = new MemoryWriter();

        internal HashSet<NetworkClient> clients = new HashSet<NetworkClient>();

        public bool isReady => NetworkManager.Client.isReady;
        
        public bool isOwner => (state & OWNING) != 0;

        public bool isServer => (state & SERVER) != 0 && NetworkManager.isServer;

        public bool isClient => (state & CLIENT) != 0 && NetworkManager.isClient;

        protected override void OnEnable()
        {
            if ((state & CREATE) == 0)
            {
                modules = moduleList.OfType<NetworkModule>().ToArray();
                for (byte i = 0; i < modules.Length; ++i)
                {
                    modules[i].moduleId = i;
                }
            }

            base.OnEnable();
        }

        protected override void OnDestroy()
        {
            if (isClient)
            {
                NetworkManager.Client.spawns.Remove(objectId);
            }

            if (isServer && (state & DESTROY) == 0)
            {
                NetworkManager.Server.Destroy(gameObject);
            }

            owner = null;
            other = null;
            client = null;
            modules = null;
            NetworkSpawner.Clear(this);
            base.OnDestroy();
        }

        public virtual void Reset()
        {
            objectId = 0;
            client = null;
            state = CREATE;
            owner.position = 0;
            other.position = 0;
            NetworkSpawner.Clear(this);
        }

        protected virtual void OnValidate()
        {
#if UNITY_EDITOR
            AssignAssetId();
#endif
        }
#if UNITY_EDITOR
        private static readonly Dictionary<uint, GameObject> sceneData = new Dictionary<uint, GameObject>();

        private void AssignAssetId()
        {
            uint.TryParse(name, out assetId);
            if (PrefabUtility.IsPartOfPrefabAsset(gameObject))
            {
                sceneId = 0;
            }
            else if (PrefabStageUtility.GetCurrentPrefabStage())
            {
                if (PrefabStageUtility.GetPrefabStage(gameObject))
                {
                    sceneId = 0;
                }
            }
            else if (PrefabUtility.IsPartOfPrefabInstance(gameObject))
            {
                if (PrefabUtility.GetCorrespondingObjectFromSource(gameObject))
                {
                    AssignSceneId();
                }
            }
            else
            {
                AssignSceneId();
            }

            assetId = sceneId != 0 ? 0 : assetId;
        }

        private void AssignSceneId()
        {
            if (sceneId == 0 || sceneData.TryGetValue(sceneId, out var entity) && entity && entity != gameObject)
            {
                sceneId = (uint)Seed.Next();
            }

            sceneData[sceneId] = gameObject;
        }
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsDirty(ulong mask, int index)
        {
            return (mask & (ulong)(1 << index)) != 0;
        }

        internal void InvokeMessage(byte moduleId, ushort function, HookMode mode, MemoryReader reader, NetworkClient client = null)
        {
            if (!transform)
            {
                Log.Warn("调用了已经删除的网络对象。{0} [{1}] {2}", mode, function, objectId);
                return;
            }

            if (moduleId >= modules.Length)
            {
                Log.Warn("网络对象 {0} 没有找到网络行为组件 {1}", objectId, moduleId);
                return;
            }

            if (!NetworkAttribute.Invoke(function, mode, client, reader, modules[moduleId]))
            {
                Log.Warn("无法调用{0} [{1}] 网络对象: {2} 网络标识: {3}", mode, function, gameObject.name, objectId);
            }
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
            if ((state & ENABLE) == 0)
            {
                foreach (var module in modules)
                {
                    if (module is IStartClient result)
                    {
                        result.OnStartClient();
                    }
                }

                state |= ENABLE;
            }
        }

        internal void OnStopClient()
        {
            if ((state & ENABLE) != 0)
            {
                foreach (var module in modules)
                {
                    if (module is IStopClient result)
                    {
                        result.OnStopClient();
                    }
                }

                state &= ~ENABLE;
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

        internal void OnNotifyAuthority()
        {
            if ((state & NOTIFY) == 0 && isOwner)
            {
                foreach (var module in modules)
                {
                    if (module is IStartAuthority result)
                    {
                        result.OnStartAuthority();
                    }
                }
            }
            else if ((state & NOTIFY) != 0 && !isOwner)
            {
                foreach (var module in modules)
                {
                    if (module is IStopAuthority result)
                    {
                        result.OnStopAuthority();
                    }
                }
            }

            state = isOwner ? state | NOTIFY : state & ~NOTIFY;
        }

        public static implicit operator uint(NetworkEntity entity)
        {
            return entity.objectId;
        }

        public static explicit operator NetworkEntity(uint objectId)
        {
            if (NetworkManager.isServer)
            {
                return NetworkManager.Server.spawns.GetValueOrDefault(objectId);
            }

            return NetworkManager.Client.spawns.GetValueOrDefault(objectId);
        }
    }
}