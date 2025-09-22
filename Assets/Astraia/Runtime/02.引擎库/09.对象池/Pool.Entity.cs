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
using UnityEngine;

namespace Astraia.Common
{
    public static partial class PoolManager
    {
        [Serializable]
        private class Pool : IPool
        {
            private readonly LinkedList<GameObject> cached = new LinkedList<GameObject>();
            private readonly Queue<GameObject> unused = new Queue<GameObject>();
            private readonly Queue<float> record = new Queue<float>();
            private int Capacity;
            private int Threshold;
            private float Timestamp;
            private Func<string, GameObject> OnCreate;

            public Type Type { get; private set; }
            public string Path { get; private set; }
            public int Acquire { get; private set; }
            public int Release { get; private set; }
            public int Dequeue { get; private set; }
            public int Enqueue { get; private set; }

            public GameObject Load()
            {
                Dequeue++;
                Acquire++;
                GameObject item;
                if (Timestamp > 0)
                {
                    var time = Time.time;
                    while (record.Count > 0 && record.Peek() + Timestamp < time)
                    {
                        record.Dequeue();
                    }

                    if (record.Count >= Threshold && cached.Count > 0)
                    {
                        item = cached.Last.Value;
                        cached.RemoveLast();
                        Acquire--;
                        if (item)
                        {
                            cached.AddFirst(item);
                            return item;
                        }
                    }

                    record.Enqueue(time);
                }

                if (Capacity > 0)
                {
                    if (cached.Count >= Capacity)
                    {
                        item = cached.Last.Value;
                        cached.RemoveLast();
                        Acquire--;
                        if (item)
                        {
                            cached.AddFirst(item);
                            return item;
                        }
                    }
                }

                if (unused.Count > 0)
                {
                    item = unused.Dequeue();
                    Release--;
                    if (item)
                    {
                        cached.AddFirst(item);
                        return item;
                    }
                }

                item = OnCreate.Invoke(Path);
                item.name = Path;
                cached.AddFirst(item);
                return item;
            }

            public void Push(GameObject item)
            {
                Enqueue++;
                if (cached.Remove(item))
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

            public static Pool Create(Type type, string path, Func<string, GameObject> onCreate, int capacity = 0, int threshold = 0, float timestamp = 0)
            {
                var instance = new Pool();
                instance.Type = type;
                instance.Path = path;
                instance.OnCreate = onCreate;
                instance.Capacity = capacity;
                instance.Threshold = threshold;
                instance.Timestamp = timestamp;
                return instance;
            }
        }
    }
}