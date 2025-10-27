// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2024-12-23 18:12:21
// # Recently: 2025-01-08 17:01:33
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Astraia.Common
{
    using static GlobalManager;

    public static partial class PoolManager
    {
        public static void Config(string path, int capacity = 0, int threshold = 0, float timestamp = 0)
        {
            LoadPool(path).Config(capacity, threshold, timestamp);
        }

        public static GameObject Show(string path)
        {
            if (!Instance) return null;
            var item = LoadPool(path).Load();
            item.transform.SetParent(null);
            item.SetActive(true);
            item.name = path;
            return item;
        }

        public static GameObject Show(string path, string name)
        {
            if (!Instance) return null;
            var item = LoadPool(path, name).Load();
            item.transform.SetParent(null);
            item.SetActive(true);
            item.name = name;
            return item;
        }

        public static GameObject Show(string path, Vector3 position)
        {
            if (!Instance) return null;
            var item = LoadPool(path).Load();
            item.transform.SetParent(null);
            item.SetActive(true);
            item.transform.position = position;
            return item;
        }

        public static GameObject Show(string path, Vector3 position, Quaternion rotation)
        {
            if (!Instance) return null;
            var item = LoadPool(path).Load();
            item.transform.SetParent(null);
            item.SetActive(true);
            item.transform.position = position;
            item.transform.rotation = rotation;
            return item;
        }

        public static GameObject Show(string path, Transform parent)
        {
            if (!Instance) return null;
            var item = LoadPool(path).Load();
            item.transform.SetParent(parent);
            item.SetActive(true);
            item.transform.localScale = Vector3.one;
            item.transform.localPosition = Vector3.zero;
            return item;
        }

        public static void Hide(GameObject item)
        {
            if (!Instance) return;
            if (!poolRoot.TryGetValue(item.name, out var pool))
            {
                pool = new GameObject("Pool - {0}".Format(item.name));
                pool.transform.SetParent(Instance.transform);
                poolRoot.Add(item.name, pool);
            }

            item.SetActive(false);
            item.transform.SetParent(pool.transform);
            LoadPool(item.name).Push(item);
        }

        private static Pool LoadPool(string path)
        {
            if (poolData.TryGetValue(path, out var pool))
            {
                return (Pool)pool;
            }

            pool = Pool.Create(typeof(GameObject), path, Load);
            poolData.Add(path, pool);
            return (Pool)pool;
        }

        private static Pool LoadPool(string path, string name)
        {
            if (poolData.TryGetValue(name, out var pool))
            {
                return (Pool)pool;
            }

            pool = Pool.Create(typeof(GameObject), path, Load);
            poolData.Add(name, pool);
            return (Pool)pool;
        }

        private static GameObject Load(string path)
        {
            return AssetManager.Load<GameObject>(path);
        }

        internal static void Dispose()
        {
            foreach (var item in poolData.Values)
            {
                item.Dispose();
            }

            poolData.Clear();
            poolRoot.Clear();
        }

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

            public void Config(int capacity, int threshold, float timestamp)
            {
                Capacity = capacity;
                Threshold = threshold;
                Timestamp = timestamp;
            }

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

            public static Pool Create(Type type, string path, Func<string, GameObject> onCreate)
            {
                var instance = new Pool();
                instance.Type = type;
                instance.Path = path;
                instance.OnCreate = onCreate;
                return instance;
            }
        }
    }

    public static partial class PoolManager
    {
        internal static AudioSource Show(string path, Type type)
        {
            if (!Instance) return null;
            var item = LoadPool(path, type).Load();
            item.transform.SetParent(null);
            item.gameObject.SetActive(true);
            return item.GetComponent<AudioSource>();
        }

        private static Pool LoadPool(string path, Type type)
        {
            if (poolData.TryGetValue(path, out var pool))
            {
                return (Pool)pool;
            }

            pool = Pool.Create(type, path, Create);
            poolData.Add(path, pool);
            Config(path, 3, 3, 1);
            return (Pool)pool;
        }

        private static GameObject Create(string path)
        {
            return new GameObject(path, typeof(AudioSource));
        }
    }
}