// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-09-02 21:09:30
// # Recently: 2026-09-02 21:32:30
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Astraia.Net
{
    [Serializable]
    public sealed class NetworkEntity : Entity
    {
        [SerializeField] internal bool visible;
#if ODIN_INSPECTOR
        [ShowInInspector, ReadOnly]
#endif
        [SerializeField] internal uint assetId;
#if ODIN_INSPECTOR
        [ShowInInspector, ReadOnly]
#endif
        [SerializeField] internal uint sceneId;
#if ODIN_INSPECTOR
        [ShowInInspector, ReadOnly]
#endif
        [SerializeField] internal uint objectId;
#if ODIN_INSPECTOR
        [ShowInInspector, ReadOnly]
#endif
        [SerializeField] internal State state;
#if ODIN_INSPECTOR
        [ShowInInspector, ReadOnly]
#endif
        [SerializeField]
        internal List<NetworkClient> clients = new List<NetworkClient>();

        internal int current;

        internal NetworkClient client;

        internal NetworkModule[] modules;

        internal MemoryWriter owner = new MemoryWriter();

        internal MemoryWriter other = new MemoryWriter();

        public bool isHost => isServer && isClient;

        public bool isOwner => (state & State.所有者) != 0;

        public bool isServer => (state & State.服务器) != 0 && NetworkManager.isServer;

        public bool isClient => (state & State.客户端) != 0 && NetworkManager.isClient;

        protected override void Awake()
        {
            modules = GetComponents<NetworkModule>();
            for (byte i = 0; i < modules.Length; i++)
            {
                modules[i].owner = this;
                modules[i].moduleId = i;
            }

            base.Awake();
        }

        protected override void OnDestroy()
        {
            if (isClient)
            {
                NetworkManager.Client.spawns.Remove(objectId);
            }

            if (isServer && (state & State.销毁中) == 0)
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

        public void Reset()
        {
            state = 0;
            objectId = 0;
            client = null;
            owner.position = 0;
            other.position = 0;
            NetworkSpawner.Clear(this);
        }
#if UNITY_EDITOR
        private static readonly Dictionary<uint, GameObject> sceneAssets = new Dictionary<uint, GameObject>();

        private void OnValidate()
        {
            if (PrefabUtility.IsPartOfPrefabAsset(gameObject))
            {
                sceneId = 0;
                AssignAssetId(AssetDatabase.GetAssetPath(gameObject));
            }
            else if (PrefabStageUtility.GetCurrentPrefabStage())
            {
                if (PrefabStageUtility.GetPrefabStage(gameObject))
                {
                    sceneId = 0;
                    AssignAssetId(PrefabStageUtility.GetPrefabStage(gameObject).assetPath);
                }
            }
            else if (PrefabUtility.IsPartOfPrefabInstance(gameObject))
            {
                var prefab = PrefabUtility.GetCorrespondingObjectFromSource(gameObject);
                if (prefab)
                {
                    AssignAssetId(AssetDatabase.GetAssetPath(prefab));
                }

                AssignSceneId();
            }
            else
            {
                AssignSceneId();
            }
        }

        private void AssignAssetId(string assetPath)
        {
            if (assetId == 0 && !string.IsNullOrWhiteSpace(assetPath))
            {
                Undo.RecordObject(this, "Assign AssetId");
                uint.TryParse(name, out assetId);
            }
        }

        private void AssignSceneId()
        {
            if (sceneId == 0 || sceneAssets.TryGetValue(sceneId, out var result) && result && result != gameObject)
            {
                Undo.RecordObject(this, "Assign SceneId");
                sceneId = (uint)Seed.Next();
                sceneAssets[sceneId] = gameObject;
            }
        }
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsDirty(ulong mask, int index)
        {
            return (mask & (ulong)(1 << index)) != 0;
        }

        internal void InvokeMessage(byte moduleId, ushort function, SyncMode mode, MemoryReader reader, NetworkClient client = null)
        {
            if (moduleId >= modules.Length)
            {
                Log.Warn($"网络对象 {objectId} 没有找到网络行为组件 {moduleId}");
                return;
            }

            if (!NetworkAttribute.Invoke(function, mode, client, reader, modules[moduleId]))
            {
                Log.Warn($"无法调用{mode} [{function}] 网络对象: {gameObject.name} 网络标识: {objectId}");
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
            if ((state & State.初始化) == 0)
            {
                foreach (var module in modules)
                {
                    if (module is IStartClient result)
                    {
                        result.OnStartClient();
                    }
                }

                state |= State.初始化;
            }
        }

        internal void OnStopClient()
        {
            if ((state & State.初始化) != 0)
            {
                foreach (var module in modules)
                {
                    if (module is IStopClient result)
                    {
                        result.OnStopClient();
                    }
                }

                state &= ~State.初始化;
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
            if ((state & State.序列化) == 0 && isOwner)
            {
                foreach (var module in modules)
                {
                    if (module is IStartAuthority result)
                    {
                        result.OnStartAuthority();
                    }
                }
            }
            else if ((state & State.序列化) != 0 && !isOwner)
            {
                foreach (var module in modules)
                {
                    if (module is IStopAuthority result)
                    {
                        result.OnStopAuthority();
                    }
                }
            }

            state = isOwner ? state | State.序列化 : state & ~State.序列化;
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

        [Flags]
        internal enum State
        {
            初始化 = 1 << 0,
            序列化 = 1 << 1,
            所有者 = 1 << 3,
            客户端 = 1 << 4,
            服务器 = 1 << 5,
            销毁中 = 1 << 2,
        }
    }
}