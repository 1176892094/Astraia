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

namespace Astraia.Net
{
    [Serializable]
    public partial class NetworkEntity : Entity
    {
        [HideInInspector] [SerializeField] internal uint assetId;

        [HideInInspector] [SerializeField] internal uint sceneId;

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
                foreach (var module in moduleData.Values)
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

            ClearDirty(true);

            owner = null;
            other = null;
            modules = null;
            client = null;
            base.OnDestroy();
        }

        public virtual void Reset()
        {
            objectId = 0;
            owner.position = 0;
            other.position = 0;
            mode = EntityMode.None;
            state = EntityState.None;
            client = null;
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
                    Undo.RecordObject(gameObject, Log.E275);
                    if (sceneId == 0)
                    {
                        if (!uint.TryParse(name, out var id))
                        {
                            Debug.LogWarning("请将 {0} 名称修改为纯数字!".Format(gameObject), gameObject);
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
                var duplicate = sceneData.TryGetValue(sceneId, out var entity) && entity != null && entity != gameObject;
                if (sceneId == 0 || duplicate)
                {
                    sceneId = 0;
                    if (BuildPipeline.isBuildingPlayer)
                    {
                        throw new InvalidOperationException(Log.E274.Format(gameObject.scene.path, name));
                    }

                    Undo.RecordObject(gameObject, Log.E275);
                    var random = (uint)Service.Rng.Next();
                    duplicate = sceneData.TryGetValue(random, out entity) && entity != null && entity != gameObject;
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
                Debug.LogWarning(Log.E276.Format(mode, function, objectId));
                return;
            }

            if (moduleId >= modules.Count)
            {
                Debug.LogWarning(Log.E277.Format(objectId, moduleId));
                return;
            }

            if (!NetworkAttribute.Invoke(function, mode, client, reader, modules[moduleId]))
            {
                Debug.LogError(Log.E278.Format(mode, function, gameObject.name, objectId));
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
            if ((state & EntityState.Authority) == 0 && isOwner)
            {
                OnStartAuthority();
            }
            else if ((state & EntityState.Authority) != 0 && !isOwner)
            {
                OnStopAuthority();
            }

            if (isOwner)
            {
                state |= EntityState.Authority;
            }
            else
            {
                state &= ~EntityState.Authority;
            }
        }

        public static implicit operator uint(NetworkEntity entity)
        {
            return entity.objectId;
        }

        public static explicit operator NetworkEntity(uint objectId)
        {
            if (NetworkManager.Server.isActive)
            {
                if (NetworkManager.Server.spawns.TryGetValue(objectId, out var entity))
                {
                    return entity;
                }
            }

            if (NetworkManager.Client.isActive)
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