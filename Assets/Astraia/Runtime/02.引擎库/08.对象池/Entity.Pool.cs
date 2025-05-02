// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2024-12-23 18:12:21
// # Recently: 2025-01-08 17:01:39
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Astraia.Common
{
    public static partial class PoolManager
    {
        private class EntityPool : IPool
        {
            private readonly HashSet<GameObject> cached = new HashSet<GameObject>();
            private readonly Queue<GameObject> unused = new Queue<GameObject>();

            public EntityPool(Type type, string path)
            {
                this.type = type;
                this.path = path;
            }

            public Type type { get; }
            public string path { get; }
            public int acquire => cached.Count;
            public int release => unused.Count;
            public int dequeue { get; private set; }
            public int enqueue { get; private set; }

            public async Task<GameObject> Dequeue()
            {
                dequeue++;
                GameObject item;
                if (unused.Count > 0)
                {
                    item = unused.Dequeue();
                    if (item != null)
                    {
                        cached.Add(item);
                        return item;
                    }

                    enqueue++;
                    cached.Remove(item);
                }

                item = await AssetManager.Load<GameObject>(path);
                Object.DontDestroyOnLoad(item);
                item.name = path;
                cached.Add(item);
                return item;
            }

            public bool Enqueue(GameObject item)
            {
                if (cached.Remove(item))
                {
                    enqueue++;
                    unused.Enqueue(item);
                    return true;
                }

                return false;
            }

            void IDisposable.Dispose()
            {
                cached.Clear();
                unused.Clear();
            }
        }
    }
}