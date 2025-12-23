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

    public static class PoolManager
    {
        public static T Show<T>(string path, Vector3 position) where T : Component
        {
            return Show(path, position).GetOrAddComponent<T>();
        }

        public static T Show<T>(string path, Vector3 position, Quaternion rotation) where T : Component
        {
            return Show(path, position, rotation).GetOrAddComponent<T>();
        }

        public static T Show<T>(string path, Transform parent) where T : Component
        {
            return Show(path, parent).GetOrAddComponent<T>();
        }

        public static T Show<T>(string path, string name) where T : Component
        {
            return Show(path, name).GetOrAddComponent<T>();
        }

        public static T Show<T>(string path, params Type[] args) where T : Component
        {
            return Show(path, args).GetOrAddComponent<T>();
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

        public static GameObject Show(string path, string name)
        {
            if (!Instance) return null;
            var item = LoadPool(path, name).Load();
            item.transform.SetParent(null);
            item.SetActive(true);
            item.name = name;
            return item;
        }

        public static GameObject Show(string path, params Type[] args)
        {
            if (!Instance) return null;
            var item = LoadPool(path, args).Load();
            item.transform.SetParent(null);
            item.SetActive(true);
            return item;
        }

        public static void Hide(Component item)
        {
            if (!Instance || !item) return;
            if (!poolRoot.TryGetValue(item.name, out var pool))
            {
                pool = new GameObject("Pool - {0}".Format(item.name));
                pool.transform.SetParent(Instance.transform);
                poolRoot.Add(item.name, pool);
            }

            item.gameObject.SetActive(false);
            item.transform.SetParent(pool.transform);
            LoadPool(item.name).Push(item.gameObject);
        }

        public static void Hide(GameObject item)
        {
            if (!Instance || !item) return;
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

            pool = Pool.Create(typeof(GameObject), path, AssetManager.Load<GameObject>);
            poolData.Add(path, pool);
            return (Pool)pool;
        }

        private static Pool LoadPool(string path, string name)
        {
            if (poolData.TryGetValue(name, out var pool))
            {
                return (Pool)pool;
            }

            pool = Pool.Create(typeof(GameObject), path, AssetManager.Load<GameObject>);
            poolData.Add(name, pool);
            return (Pool)pool;
        }

        private static Pool LoadPool(string path, params Type[] args)
        {
            if (poolData.TryGetValue(path, out var pool))
            {
                return (Pool)pool;
            }

            pool = Pool.Create(args[0], path, item => new GameObject(item, args));
            poolData.Add(path, pool);
            return (Pool)pool;
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
            private int capacity;
            private int threshold;
            private float timestamp;
            private Func<string, GameObject> OnCreate;

            public Type Type { get; private set; }
            public string Path { get; private set; }
            public int Acquire { get; private set; }
            public int Release { get; private set; }
            public int Dequeue { get; private set; }
            public int Enqueue { get; private set; }

            public void Build(int capacity, int threshold, float timestamp)
            {
                this.capacity = capacity;
                this.threshold = threshold;
                this.timestamp = timestamp;
            }

            public GameObject Load()
            {
                Dequeue++;
                Acquire++;
                GameObject item;
                if (timestamp > 0)
                {
                    var time = Time.time;
                    while (record.Count > 0 && record.Peek() + timestamp < time)
                    {
                        record.Dequeue();
                    }

                    if (record.Count >= threshold && cached.Count > 0)
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

                if (capacity > 0)
                {
                    if (cached.Count >= capacity)
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
}