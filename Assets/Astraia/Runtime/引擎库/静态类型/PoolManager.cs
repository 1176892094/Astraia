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

namespace Astraia.Core
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

        public static T Show<T>(string path, Transform parent, string name) where T : Component
        {
            return Show(path, parent, name).GetOrAddComponent<T>();
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

        public static GameObject Show(string path, Transform parent, string name)
        {
            if (!Instance) return null;
            var item = LoadPool(path, name).Load();
            item.transform.SetParent(parent);
            item.SetActive(true);
            item.transform.localScale = Vector3.one;
            item.transform.localPosition = Vector3.zero;
            item.name = name;
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
            if (!poolData.TryGetValue(path, out var pool))
            {
                pool = new Pool(typeof(GameObject), path);
                poolData.Add(path, pool);
            }

            return (Pool)pool;
        }

        private static Pool LoadPool(string path, string name)
        {
            if (!poolData.TryGetValue(name, out var pool))
            {
                pool = new Pool(typeof(GameObject), path);
                poolData.Add(name, pool);
            }

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
            private readonly Queue<GameObject> unused = new Queue<GameObject>();
            public Type Type { get; private set; }
            public string Path { get; private set; }
            public int Acquire { get; private set; }
            public int Release { get; private set; }
            public int Dequeue { get; private set; }
            public int Enqueue { get; private set; }

            public Pool(Type type, string path)
            {
                Type = type;
                Path = path;
            }

            public GameObject Load()
            {
                Dequeue++;
                Acquire++;
                if (unused.TryDequeue(out var item))
                {
                    Release--;
                    if (item)
                    {
                        return item;
                    }

                    Dequeue++;
                    Enqueue++;
                }

                item = AssetManager.Load<GameObject>(Path);
                item.name = Path;
                return item;
            }

            public void Push(GameObject item)
            {
                Enqueue++;
                if (!unused.Contains(item))
                {
                    Acquire--;
                    Release++;
                    unused.Enqueue(item);
                }
            }

            void IDisposable.Dispose()
            {
                unused.Clear();
            }
        }
    }
}