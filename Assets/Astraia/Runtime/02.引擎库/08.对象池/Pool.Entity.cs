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

namespace Astraia.Common
{
    public static partial class PoolManager
    {
        [Serializable]
        private class Pool : IPool
        {
            private readonly HashSet<GameObject> cached = new HashSet<GameObject>();
            private readonly Queue<GameObject> unused = new Queue<GameObject>();

            public Type Type { get; private set; }
            public string Path { get; private set; }
            public int Acquire { get; private set; }
            public int Release { get; private set; }
            public int Dequeue { get; private set; }
            public int Enqueue { get; private set; }

            public async Task<GameObject> Load()
            {
                Dequeue++;
                Acquire++;
                GameObject item;
                if (unused.Count > 0)
                {
                    item = unused.Dequeue();
                    cached.Remove(item);
                    Release--;
                    if (item)
                    {
                        return item;
                    }

                    Enqueue++;
                    Dequeue++;
                }

                item = await AssetManager.Load<GameObject>(Path);
                item.name = Path;
                return item;
            }

            public void Push(GameObject item)
            {
                Enqueue++;
                if (cached.Add(item))
                {
                    Acquire--;
                    Release++;
                    unused.Enqueue(item);
                }
            }

            void IDisposable.Dispose()
            {
                cached.Clear();
                unused.Clear();
            }

            public static Pool Create(Type type, string path)
            {
                var instance = Activator.CreateInstance<Pool>();
                instance.Type = type;
                instance.Path = path;
                return instance;
            }
        }
    }
}