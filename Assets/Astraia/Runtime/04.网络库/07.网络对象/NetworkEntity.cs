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

        internal AgentMode mode;

        internal AgentState state;

        internal NetworkClient client;

        internal MemoryWriter owner = new MemoryWriter();

        internal MemoryWriter other = new MemoryWriter();

        internal List<NetworkAgent> agents = new List<NetworkAgent>();

        public bool isOwner => (mode & AgentMode.Owner) != 0;

        public bool isServer => (mode & AgentMode.Server) != 0;

        public bool isClient => (mode & AgentMode.Client) != 0;


        protected override void OnEnable()
        {
            base.OnEnable();
            if ((state & AgentState.Awake) == 0)
            {
                if (GlobalManager.agentData.TryGetValue(this, out var agentData))
                {
                    foreach (var agent in agentData.Values)
                    {
                        if (agent is NetworkAgent entity)
                        {
                            agents.Add(entity);
                        }
                    }
                }

                for (byte i = 0; i < agents.Count; ++i)
                {
                    agents[i].sourceId = i;
                }

                state |= AgentState.Awake;
            }
        }

        protected override void OnDestroy()
        {
            if (isServer && (state & AgentState.Destroy) == 0)
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
            agents = null;
            client = null;
            base.OnDestroy();
        }

        public virtual void Reset()
        {
            objectId = 0;
            owner.position = 0;
            other.position = 0;
            mode = AgentMode.None;
            state = AgentState.None;
            client = null;
        }

#if UNITY_EDITOR
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
                            Debug.LogWarning(Service.Text.Format("请将 {0} 名称修改为纯数字!", gameObject), gameObject);
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
                var duplicate = GlobalManager.sceneData.TryGetValue(sceneId, out var entity) && entity != null && entity != gameObject;
                if (sceneId == 0 || duplicate)
                {
                    sceneId = 0;
                    if (BuildPipeline.isBuildingPlayer)
                    {
                        throw new InvalidOperationException(Service.Text.Format(Log.E274, gameObject.scene.path, name));
                    }

                    Undo.RecordObject(gameObject, Log.E275);
                    var random = (uint)Service.Random.Next();
                    duplicate = GlobalManager.sceneData.TryGetValue(random, out entity) && entity != null && entity != gameObject;
                    if (!duplicate)
                    {
                        sceneId = random;
                    }
                }

                GlobalManager.sceneData[sceneId] = gameObject;
            }
        }
#endif


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsDirty(ulong mask, int index)
        {
            return (mask & (ulong)(1 << index)) != 0;
        }

        internal void InvokeMessage(byte agentId, ushort function, InvokeMode mode, MemoryReader reader, NetworkClient client = null)
        {
            if (transform == null)
            {
                Debug.LogWarning(Service.Text.Format(Log.E276, mode, function, objectId));
                return;
            }

            if (agentId >= agents.Count)
            {
                Debug.LogWarning(Service.Text.Format(Log.E277, objectId, agentId));
                return;
            }

            if (!NetworkAttribute.Invoke(function, mode, client, reader, agents[agentId]))
            {
                Debug.LogError(Service.Text.Format(Log.E278, mode, function, gameObject.name, objectId));
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
            foreach (var agent in agents)
            {
                if (agent.IsDirty() || total)
                {
                    agent.ClearDirty();
                }
            }
        }

        internal void OnStartClient()
        {
            if ((state & AgentState.Spawn) == 0)
            {
                foreach (var agent in agents)
                {
                    if (agent is IStartClient result)
                    {
                        result.OnStartClient();
                    }
                }

                state |= AgentState.Spawn;
            }
        }

        internal void OnStopClient()
        {
            if ((state & AgentState.Spawn) != 0)
            {
                foreach (var agent in agents)
                {
                    if (agent is IStopClient result)
                    {
                        result.OnStopClient();
                    }
                }
            }
        }

        internal void OnStartServer()
        {
            foreach (var agent in agents)
            {
                if (agent is IStartServer result)
                {
                    result.OnStartServer();
                }
            }
        }

        internal void OnStopServer()
        {
            foreach (var agent in agents)
            {
                if (agent is IStopServer result)
                {
                    result.OnStopServer();
                }
            }
        }

        private void OnStartAuthority()
        {
            foreach (var agent in agents)
            {
                if (agent is IStartAuthority result)
                {
                    result.OnStartAuthority();
                }
            }
        }

        private void OnStopAuthority()
        {
            foreach (var agent in agents)
            {
                if (agent is IStopAuthority result)
                {
                    result.OnStopAuthority();
                }
            }
        }

        internal void OnNotifyAuthority()
        {
            if ((state & AgentState.Authority) == 0 && isOwner)
            {
                OnStartAuthority();
            }
            else if ((state & AgentState.Authority) != 0 && !isOwner)
            {
                OnStopAuthority();
            }

            if (isOwner)
            {
                state |= AgentState.Authority;
            }
            else
            {
                state &= ~AgentState.Authority;
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