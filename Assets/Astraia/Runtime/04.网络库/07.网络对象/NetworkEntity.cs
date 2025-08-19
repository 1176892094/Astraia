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
        private int frameCount;

        [HideInInspector] public uint objectId;

        [SerializeField] [HideInInspector] internal uint assetId;

        [SerializeField] [HideInInspector] internal uint sceneId;
        
        [SerializeField] [HideInInspector] internal AgentMode agentMode;

        internal AgentState agentState;

        internal NetworkClient connection;

        internal MemoryWriter owner = new MemoryWriter();

        internal MemoryWriter other = new MemoryWriter();

        internal List<NetworkAgent> agents = new List<NetworkAgent>();

        public bool isOwner => (agentMode & AgentMode.Owner) != 0;

        public bool isServer => (agentMode & AgentMode.Server) != 0;

        public bool isClient => (agentMode & AgentMode.Client) != 0;

        protected override void Awake()
        {
            base.Awake();
            foreach (var agent in agentDict.Values)
            {
                if (agent is NetworkAgent entity)
                {
                    agents.Add(entity);
                }
            }

            for (byte i = 0; i < agents.Count; ++i)
            {
                agents[i].sourceId = i;
            }
        }

        protected override void OnDestroy()
        {
            if ((agentMode & AgentMode.Server) != 0)
            {
                if ((agentState & AgentState.Destroy) == 0)
                {
                    NetworkManager.Server.Destroy(gameObject);
                }
            }

            if ((agentMode & AgentMode.Client) != 0)
            {
                NetworkManager.Client.spawns.Remove(objectId);
            }

            ClearDirty(true);

            owner = null;
            other = null;
            agents = null;
            connection = null;
            base.OnDestroy();
        }

        public virtual void Reset()
        {
            objectId = 0;
            connection = null;
            owner.position = 0;
            other.position = 0;
            agentMode = AgentMode.None;
            agentState = AgentState.None;
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
                if (PrefabStageUtility.GetPrefabStage(gameObject))
                {
                    sceneId = 0;
                    AssignAssetId(PrefabStageUtility.GetPrefabStage(gameObject).assetPath);
                }
            }
            else if (PrefabUtility.IsPartOfPrefabInstance(gameObject))
            {
                var prefab = PrefabUtility.GetCorrespondingObjectFromSource(gameObject);
                if (prefab != null)
                {
                    AssignSceneId();
                    AssignAssetId(AssetDatabase.GetAssetPath(prefab));
                }
            }
            else
            {
                AssignSceneId();
            }

            if (sceneId == 0 && assetId == 0)
            {
                Debug.LogWarning(Service.Text.Format("请将 {0} 名称修改为纯数字!", gameObject), gameObject);
            }

            return;

            void AssignAssetId(string assetPath)
            {
                if (!string.IsNullOrWhiteSpace(assetPath))
                {
                    Undo.RecordObject(gameObject, Log.E275);
                    uint.TryParse(name, out assetId);
                }
            }

            void AssignSceneId()
            {
                if (Application.isPlaying) return;
                var duplicate = GlobalManager.objectData.TryGetValue(sceneId, out var entity) && entity != null && entity != gameObject;
                if (sceneId == 0 || duplicate)
                {
                    sceneId = 0;
                    if (BuildPipeline.isBuildingPlayer)
                    {
                        throw new InvalidOperationException(Service.Text.Format(Log.E274, gameObject.scene.path, name));
                    }

                    Undo.RecordObject(gameObject, Log.E275);
                    var random = (uint)Service.Random.Next();
                    duplicate = GlobalManager.objectData.TryGetValue(random, out entity) && entity != null && entity != gameObject;
                    if (!duplicate)
                    {
                        sceneId = random;
                    }
                }

                GlobalManager.objectData[sceneId] = gameObject;
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
            if ((agentState & AgentState.Spawn) != 0)
            {
                return;
            }

            agentState |= AgentState.Spawn;

            foreach (var agent in agents)
            {
                try
                {
                    (agent as IStartClient)?.OnStartClient();
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e, agent.gameObject);
                }
            }
        }

        internal void OnStopClient()
        {
            if ((agentState & AgentState.Spawn) == 0)
            {
                return;
            }

            foreach (var agent in agents)
            {
                try
                {
                    (agent as IStopClient)?.OnStopClient();
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e, agent.gameObject);
                }
            }
        }

        internal void OnStartServer()
        {
            foreach (var agent in agents)
            {
                try
                {
                    (agent as IStartServer)?.OnStartServer();
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e, agent.gameObject);
                }
            }
        }

        internal void OnStopServer()
        {
            foreach (var agent in agents)
            {
                try
                {
                    (agent as IStopServer)?.OnStopServer();
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e, agent.gameObject);
                }
            }
        }

        private void OnStartAuthority()
        {
            foreach (var agent in agents)
            {
                try
                {
                    (agent as IStartAuthority)?.OnStartAuthority();
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e, agent.gameObject);
                }
            }
        }

        private void OnStopAuthority()
        {
            foreach (var agent in agents)
            {
                try
                {
                    (agent as IStopAuthority)?.OnStopAuthority();
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e, agent.gameObject);
                }
            }
        }

        internal void OnNotifyAuthority()
        {
            if ((agentState & AgentState.Authority) == 0 && (agentMode & AgentMode.Owner) != 0)
            {
                OnStartAuthority();
            }
            else if ((agentState & AgentState.Authority) != 0 && (agentMode & AgentMode.Owner) == 0)
            {
                OnStopAuthority();
            }

            if ((agentMode & AgentMode.Owner) != 0)
            {
                agentState |= AgentState.Authority;
            }
            else
            {
                agentState &= ~AgentState.Authority;
            }
        }
    }
}