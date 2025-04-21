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
    public sealed partial class NetworkObject : MonoBehaviour
    {
        [SerializeField] internal EntityMode entityMode;

        [SerializeField] internal uint objectId;

        [SerializeField] internal ulong sceneId;

        [SerializeField] internal string assetId;

        private int frameCount;

        internal EntityState entityState;

        internal NetworkClient connection;

        internal NetworkBehaviour[] entities;

        internal MemorySetter owner = new MemorySetter();

        internal MemorySetter observer = new MemorySetter();

        private void Awake()
        {
            entities = GetComponentsInChildren<NetworkBehaviour>(true);
            if (entities == null)
            {
                Debug.LogError("网络对象持有的 NetworkEntity 为空", gameObject);
                return;
            }

            if (entities.Length > 64)
            {
                Debug.LogError("网络对象持有的 NetworkBehaviour 的数量不能超过 64");
                return;
            }

            for (byte i = 0; i < entities.Length; ++i)
            {
                entities[i].@object = this;
                entities[i].componentId = i;
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
                        assetId = char.ToUpper(asset[0]) + asset.Substring(1) + "/" + name;
                    }
                    else
                    {
                        string directory = Path.GetDirectoryName(assetPath);
                        string folderName = Path.GetFileName(directory);
                        assetId = folderName + "/" + name;
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

            Debug.LogError(Service.Text.Format("找不到场景对象的预制父物体。对象名称: {0}", name));
            return false;
        }

        private void AssignSceneId()
        {
            if (Application.isPlaying) return;
            var duplicate = GlobalManager.objectData.TryGetValue(sceneId, out var @object) && @object != null && @object != gameObject;
            if (sceneId == 0 || duplicate)
            {
                sceneId = 0;
                if (BuildPipeline.isBuildingPlayer)
                {
                    throw new InvalidOperationException("请保存场景后，再进行构建。");
                }

                Undo.RecordObject(gameObject, "Assigned AssetId");
                var random = Service.Hash.Id();
                duplicate = GlobalManager.objectData.TryGetValue(random, out @object) && @object != null && @object != gameObject;
                if (!duplicate)
                {
                    sceneId = random;
                }
            }

            GlobalManager.objectData[sceneId] = gameObject;
        }
#endif

        private void OnDestroy()
        {
            if ((entityMode & EntityMode.Server) == EntityMode.Server && (entityState & EntityState.Destroy) == 0)
            {
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
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsDirty(ulong mask, int index)
        {
            return (mask & (ulong)(1 << index)) != 0;
        }

        internal void InvokeMessage(byte index, ushort function, InvokeMode mode, MemoryGetter getter, NetworkClient client = null)
        {
            if (this == null)
            {
                Debug.LogWarning(Service.Text.Format("调用了已经删除的网络对象。{0} [{1}] {2}", mode, function, objectId));
                return;
            }

            if (index >= entities.Length)
            {
                Debug.LogWarning(Service.Text.Format("网络对象{0}，没有找到组件{1}", objectId, index));
                return;
            }

            if (!NetworkAttribute.Invoke(function, mode, client, getter, entities[index]))
            {
                Debug.LogError(Service.Text.Format("无法调用{0} [{1}] 网络对象: {2} 网络标识: {3}", mode, function, gameObject.name, objectId));
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
            foreach (var entity in entities)
            {
                if (entity.IsDirty() || total)
                {
                    entity.ClearDirty();
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

            foreach (var entity in entities)
            {
                try
                {
                    (entity as IStartClient)?.OnStartClient();
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e, entity.gameObject);
                }
            }
        }

        internal void OnStopClient()
        {
            if ((entityState & EntityState.Spawn) == 0)
            {
                return;
            }

            foreach (var entity in entities)
            {
                try
                {
                    (entity as IStopClient)?.OnStopClient();
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e, entity.gameObject);
                }
            }
        }

        internal void OnStartServer()
        {
            foreach (var entity in entities)
            {
                try
                {
                    (entity as IStartServer)?.OnStartServer();
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e, entity.gameObject);
                }
            }
        }

        internal void OnStopServer()
        {
            foreach (var entity in entities)
            {
                try
                {
                    (entity as IStopServer)?.OnStopServer();
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e, entity.gameObject);
                }
            }
        }

        private void OnStartAuthority()
        {
            foreach (var entity in entities)
            {
                try
                {
                    (entity as IStartAuthority)?.OnStartAuthority();
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e, entity.gameObject);
                }
            }
        }

        private void OnStopAuthority()
        {
            foreach (var entity in entities)
            {
                try
                {
                    (entity as IStopAuthority)?.OnStopAuthority();
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e, entity.gameObject);
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