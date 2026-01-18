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
using Astraia.Common;
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
        [SerializeField] [HideInInspector] internal uint objectId;

        [SerializeField] [HideInInspector] internal uint assetId;

        [SerializeField] [HideInInspector] internal uint sceneId;

        [SerializeField] internal Visible visible;

        internal int count;

        internal Label label;

        internal State state;

        internal NetworkClient client;

        internal NetworkModule[] modules;

        internal MemoryWriter owner = new MemoryWriter();

        internal MemoryWriter agent = new MemoryWriter();

        internal HashSet<NetworkClient> clients = new HashSet<NetworkClient>();

        public bool isOwner => (label & Label.Owner) != 0;

        public bool isServer => (label & Label.Server) != 0 && NetworkManager.isServer;

        public bool isClient => (label & Label.Client) != 0 && NetworkManager.isClient;


        protected override void OnEnable()
        {
            base.OnEnable();
            if ((state & State.Awake) == 0)
            {
                var copies = new List<NetworkModule>();
                foreach (var module in Values)
                {
                    if (module is NetworkModule result)
                    {
                        copies.Add(result);
                    }
                }

                modules = copies.ToArray();
                for (byte i = 0; i < modules.Length; ++i)
                {
                    modules[i].moduleId = i;
                }

                state |= State.Awake;
            }
        }

        protected override void OnDestroy()
        {
            if (isClient)
            {
                NetworkManager.Client.spawns.Remove(objectId);
            }

            if (isServer && (state & State.Destroy) == 0)
            {
                NetworkManager.Server.Destroy(gameObject);
            }

            owner = null;
            agent = null;
            client = null;
            modules = null;
            ClearObserver();
            base.OnDestroy();
        }

        public virtual void Reset()
        {
            objectId = 0;
            client = null;
            owner.position = 0;
            agent.position = 0;
            label = Label.None;
            state = State.None;
            ClearObserver();
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

                    var random = (uint)Service.Seed.Next();
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
            if (!transform)
            {
                Service.Log.Warn("调用了已经删除的网络对象。{0} [{1}] {2}", mode, function, objectId);
                return;
            }

            if (moduleId >= modules.Length)
            {
                Service.Log.Warn("网络对象 {0} 没有找到网络行为组件 {1}", objectId, moduleId);
                return;
            }

            if (!NetworkAttribute.Invoke(function, mode, client, reader, modules[moduleId]))
            {
                Service.Log.Warn("无法调用{0} [{1}] 网络对象: {2} 网络标识: {3}", mode, function, gameObject.name, objectId);
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

        internal void AddObserver(NetworkClient client)
        {
            if (clients.Add(client))
            {
                if (clients.Count == 1)
                {
                    ClearDirty(true);
                }

                client.entities.Add(this);
                NetworkManager.Server.SpawnMessage(this, client);
            }
        }

        internal void SubObserver(NetworkClient client)
        {
            if (clients.Remove(client))
            {
                if (clients.Count == 0)
                {
                    ClearDirty(true);
                }

                client.entities.Remove(this);
                client.Send(new DespawnMessage(objectId));
            }
        }

        private void ClearObserver()
        {
            foreach (var item in clients.ToList())
            {
                item.entities.Remove(this);
            }

            clients.Clear();
        }

        internal void OnStartClient()
        {
            if ((state & State.Start) == 0)
            {
                foreach (var module in modules)
                {
                    if (module is IStartClient result)
                    {
                        result.OnStartClient();
                    }
                }

                state |= State.Start;
            }
        }

        internal void OnStopClient()
        {
            if ((state & State.Start) != 0)
            {
                foreach (var module in modules)
                {
                    if (module is IStopClient result)
                    {
                        result.OnStopClient();
                    }
                }

                state &= ~State.Start;
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
            if ((state & State.Owner) == 0 && isOwner)
            {
                foreach (var module in modules)
                {
                    if (module is IStartAuthority result)
                    {
                        result.OnStartAuthority();
                    }
                }
            }
            else if ((state & State.Owner) != 0 && !isOwner)
            {
                foreach (var module in modules)
                {
                    if (module is IStopAuthority result)
                    {
                        result.OnStopAuthority();
                    }
                }
            }

            state = isOwner ? state | State.Owner : state & ~State.Owner;
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
        internal enum Label : byte
        {
            None = 0,
            Owner = 1 << 0,
            Client = 1 << 1,
            Server = 1 << 2,
        }

        [Flags]
        internal enum State : byte
        {
            None = 0,
            Awake = 1 << 0,
            Start = 1 << 1,
            Owner = 1 << 2,
            Destroy = 1 << 3
        }
    }
}