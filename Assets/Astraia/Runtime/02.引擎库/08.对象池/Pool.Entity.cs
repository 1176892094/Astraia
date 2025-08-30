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
                this.Type = type;
                this.Path = path;
            }

            public Type Type { get; }
            public string Path { get; }
            public int Acquire => cached.Count;
            public int Release => unused.Count;
            public int Dequeue { get; private set; }
            public int Enqueue { get; private set; }

            public async Task<GameObject> Load()
            {
                Dequeue++;
                GameObject item;
                if (unused.Count > 0)
                {
                    item = unused.Dequeue();
                    if (item)
                    {
                        cached.Add(item);
                        return item;
                    }

                    Enqueue++;
                    cached.Remove(item);
                }

                item = await AssetManager.Load<GameObject>(Path);
                Object.DontDestroyOnLoad(item);
                item.name = Path;
                cached.Add(item);
                return item;
            }

            public void Push(GameObject item)
            {
                if (cached.Remove(item))
                {
                    Enqueue++;
                    unused.Enqueue(item);
                }
            }

            void IDisposable.Dispose()
            {
                cached.Clear();
                unused.Clear();
            }
        }
    }
}