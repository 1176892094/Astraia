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
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace Astraia.Net
{
    [Serializable]
    public sealed partial class NetworkEntity : Entity
    {
        [SerializeField] internal string entityPath;
        
        [SerializeField] internal EntityMode entityMode;
        
        [SerializeField] [HideInInspector] internal uint sceneId;

        [SerializeField] [HideInInspector] internal uint objectId;

        private int frameCount;

        internal EntityState entityState;

        internal NetworkClient connection;

        internal MemoryWriter owner = new MemoryWriter();

        internal MemoryWriter observer = new MemoryWriter();

        internal List<NetworkSource> entities = new List<NetworkSource>();

        protected override void Awake()
        {
            base.Awake();
            foreach (var source in sourceDict.Values)
            {
                if (source is NetworkSource entity)
                {
                    entities.Add(entity);
                }
            }

            for (byte i = 0; i < entities.Count; ++i)
            {
                entities[i].entity = this;
                entities[i].sourceId = i;
            }
        }

        public void Reset()
        {
            objectId = 0;
            connection = null;
            owner.position = 0;
            observer.position = 0;
            entityMode = EntityMode.None;
            entityState = EntityState.None;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (PrefabUtility.IsPartOfPrefabAsset(gameObject))
            {
                sceneId = 0;
                AssignAssetId(AssetDatabase.GetAssetPath(gameObject));
            }
            else if (PrefabStageUtility.GetCurrentPrefabStage() != null)
            {
                if (PrefabStageUtility.GetPrefabStage(gameObject) != null)
                {
                    sceneId = 0;
                    AssignAssetId(PrefabStageUtility.GetPrefabStage(gameObject).assetPath);
                }
            }
            else if (IsSceneWithParent(out var prefab))
            {
                AssignSceneId();
                AssignAssetId(AssetDatabase.GetAssetPath(prefab));
            }
            else
            {
                AssignSceneId();
            }
        }

        private void AssignAssetId(string assetPath)
        {
            if (!string.IsNullOrWhiteSpace(assetPath))
            {
                var importer = AssetImporter.GetAtPath(assetPath);
                if (importer != null)
                {
                    var asset = importer.assetBundleName;
                    if (!string.IsNullOrEmpty(importer.assetBundleName))
                    {
                        entityPath = char.ToUpper(asset[0]) + asset.Substring(1) + "/" + name;
                    }
                    else
                    {
                        entityPath = Path.GetFileName(Path.GetDirectoryName(assetPath)) + "/" + name;
                    }
                }
            }
        }

        private bool IsSceneWithParent(out GameObject prefab)
        {
            prefab = null;
            if (!PrefabUtility.IsPartOfPrefabInstance(gameObject))
            {
                return false;
            }

            prefab = PrefabUtility.GetCorrespondingObjectFromSource(gameObject);
            if (prefab != null)
            {
                return true;
            }

            Debug.LogError(Service.Text.Format(Log.E273, name));
            return false;
        }

        private void AssignSceneId()
        {
            if (Application.isPlaying) return;
            var duplicate = GlobalManager.objectData.TryGetValue(sceneId, out var entity) && entity != null && entity != gameObject;
            if (sceneId == 0 || duplicate)
            {
                sceneId = 0;
                if (BuildPipeline.isBuildingPlayer)
                {
                    throw new InvalidOperationException(Log.E274);
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
#endif

        protected override void OnDestroy()
        {
            if ((entityMode & EntityMode.Server) == EntityMode.Server && (entityState & EntityState.Destroy) == 0)
            {
                gameObject.name += " (Destroy)";
                NetworkManager.Server.Despawn(gameObject);
            }

            if ((entityMode & EntityMode.Client) != 0)
            {
                NetworkManager.Client.spawns.Remove(objectId);
            }

            owner = null;
            observer = null;
            entities = null;
            connection = null;
            base.OnDestroy();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsDirty(ulong mask, int index)
        {
            return (mask & (ulong)(1 << index)) != 0;
        }

        internal void InvokeMessage(byte sourceId, ushort function, InvokeMode mode, MemoryReader reader, NetworkClient client = null)
        {
            if (transform == null)
            {
                Debug.LogWarning(Service.Text.Format(Log.E276, mode, function, objectId));
                return;
            }

            if (sourceId >= entities.Count)
            {
                Debug.LogWarning(Service.Text.Format(Log.E277, objectId, sourceId));
                return;
            }

            if (!NetworkAttribute.Invoke(function, mode, client, reader, entities[sourceId]))
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
                observer.position = 0;
                ServerSerialize(false, owner, observer);
                ClearDirty(true);
            }
        }

        internal void ClearDirty(bool total)
        {
            foreach (var source in entities)
            {
                if (source.IsDirty() || total)
                {
                    source.ClearDirty();
                }
            }
        }

        internal void OnStartClient()
        {
            if ((entityState & EntityState.Spawn) != 0)
            {
                return;
            }

            entityState |= EntityState.Spawn;

            foreach (var source in entities)
            {
                try
                {
                    (source as IStartClient)?.OnStartClient();
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e, source.gameObject);
                }
            }
        }

        internal void OnStopClient()
        {
            if ((entityState & EntityState.Spawn) == 0)
            {
                return;
            }

            foreach (var source in entities)
            {
                try
                {
                    (source as IStopClient)?.OnStopClient();
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e, source.gameObject);
                }
            }
        }

        internal void OnStartServer()
        {
            foreach (var source in entities)
            {
                try
                {
                    (source as IStartServer)?.OnStartServer();
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e, source.gameObject);
                }
            }
        }

        internal void OnStopServer()
        {
            foreach (var source in entities)
            {
                try
                {
                    (source as IStopServer)?.OnStopServer();
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e, source.gameObject);
                }
            }
        }

        private void OnStartAuthority()
        {
            foreach (var source in entities)
            {
                try
                {
                    (source as IStartAuthority)?.OnStartAuthority();
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e, source.gameObject);
                }
            }
        }

        private void OnStopAuthority()
        {
            foreach (var source in entities)
            {
                try
                {
                    (source as IStopAuthority)?.OnStopAuthority();
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e, source.gameObject);
                }
            }
        }

        internal void OnNotifyAuthority()
        {
            if ((entityState & EntityState.Authority) == 0 && (entityMode & EntityMode.Owner) != 0)
            {
                OnStartAuthority();
            }
            else if ((entityState & EntityState.Authority) != 0 && (entityMode & EntityMode.Owner) == 0)
            {
                OnStopAuthority();
            }

            if ((entityMode & EntityMode.Owner) != 0)
            {
                entityState |= EntityState.Authority;
            }
            else
            {
                entityState &= ~EntityState.Authority;
            }
        }
    }
}