// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-10-01 19:10:11
// // # Recently: 2025-10-01 19:10:11
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System.Collections.Generic;
using Astraia.Common;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Astraia.Net
{
    public static class NetworkSpawner
    {
        private static readonly Dictionary<uint, Queue<GameObject>> spawns = new Dictionary<uint, Queue<GameObject>>();
        private static readonly HashSet<GameObject> cached = new HashSet<GameObject>();

        internal static GameObject Spawn(byte opcode, uint assetId)
        {
            if ((opcode & 2) != 0)
            {
                if (!spawns.TryGetValue(assetId, out var queue))
                {
                    queue = new Queue<GameObject>();
                    spawns[assetId] = queue;
                }

                if (queue.Count > 0)
                {
                    var item = queue.Dequeue();
                    cached.Remove(item);
                    if (item)
                    {
                        return item;
                    }
                }
            }

            return AssetManager.Load<GameObject>(GlobalSetting.Prefab.Format(assetId));
        }

        internal static void Despawn(NetworkEntity entity)
        {
            if (entity.visible == Visible.Pool)
            {
                if (spawns.TryGetValue(entity.assetId, out var queue) && cached.Add(entity.gameObject))
                {
                    queue.Enqueue(entity.gameObject);
                }
                
                entity.gameObject.SetActive(false);
            }
            else if (entity.sceneId != 0)
            {
                entity.gameObject.SetActive(false);
                entity.Reset();
            }
            else
            {
                entity.state |= EntityState.Destroy;
                Object.Destroy(entity.gameObject);
            }
        }
    }
}