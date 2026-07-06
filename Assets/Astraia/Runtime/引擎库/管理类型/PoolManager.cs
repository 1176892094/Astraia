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
    [Serializable]
    public class PoolManager : Singleton<PoolManager>
    {
        private Dictionary<string, Transform> rootData = new Dictionary<string, Transform>();
        private Dictionary<string, IPool> poolData = new Dictionary<string, IPool>();
        [SerializeField] private Transform parent;
        internal ICollection<IPool> Values => poolData.Values;

        public override void Enqueue()
        {
            Instance = null;
            foreach (var item in poolData.Values)
            {
                item.Dispose();
            }

            poolData.Clear();
            rootData.Clear();
        }

        public static T Show<T>(string path) where T : Component
        {
            return Instance?.ShowInternal(path).GetOrAddComponent<T>();
        }

        public static T Show<T>(string path, float cooldown) where T : Component
        {
            return Instance?.ShowInternal(path, cooldown).GetOrAddComponent<T>();
        }

        public static T Show<T>(string path, Vector3 position) where T : Component
        {
            return Instance?.ShowInternal(path, position).GetOrAddComponent<T>();
        }

        public static T Show<T>(string path, Vector3 position, Quaternion rotation) where T : Component
        {
            return Instance?.ShowInternal(path, position, rotation).GetOrAddComponent<T>();
        }

        public static T Show<T>(string path, Transform parent) where T : Component
        {
            return Instance?.ShowInternal(path, parent).GetOrAddComponent<T>();
        }

        public static T Show<T>(string path, string name, Transform parent) where T : Component
        {
            return Instance?.ShowInternal(path, name, parent).GetOrAddComponent<T>();
        }

        public static void Hide<T>(T item) where T : Component
        {
            Instance?.HideInternal(item);
        }

        public static GameObject Show(string path)
        {
            return Instance?.ShowInternal(path);
        }

        public static GameObject Show(string path, float cooldown)
        {
            return Instance?.ShowInternal(path, cooldown);
        }

        public static GameObject Show(string path, Vector3 position)
        {
            return Instance?.ShowInternal(path, position);
        }

        public static GameObject Show(string path, Vector3 position, Quaternion rotation)
        {
            return Instance?.ShowInternal(path, position, rotation);
        }

        public static GameObject Show(string path, Transform parent)
        {
            return Instance?.ShowInternal(path, parent);
        }

        public static GameObject Show(string path, string name, Transform parent)
        {
            return Instance?.ShowInternal(path, name, parent);
        }

        public static void Hide(GameObject item)
        {
            Instance?.HideInternal(item);
        }

        private GameObject ShowInternal(string path)
        {
            var item = LoadPool(path).Load();
            item.transform.SetParent(null);
            item.SetActive(true);
            return item;
        }

        private GameObject ShowInternal(string path, float cooldown)
        {
            var item = LoadPool(path).Load(cooldown);
            item.transform.SetParent(null);
            item.SetActive(true);
            return item;
        }

        private GameObject ShowInternal(string path, Vector3 position)
        {
            var item = LoadPool(path).Load();
            item.transform.SetParent(null);
            item.SetActive(true);
            item.transform.position = position;
            return item;
        }

        private GameObject ShowInternal(string path, Vector3 position, Quaternion rotation)
        {
            var item = LoadPool(path).Load();
            item.transform.SetParent(null);
            item.SetActive(true);
            item.transform.position = position;
            item.transform.rotation = rotation;
            return item;
        }

        private GameObject ShowInternal(string path, Transform parent)
        {
            var item = LoadPool(path).Load();
            item.transform.SetParent(parent);
            item.SetActive(true);
            item.transform.localScale = Vector3.one;
            item.transform.localPosition = Vector3.zero;
            return item;
        }

        private GameObject ShowInternal(string path, string name, Transform parent)
        {
            var item = LoadPool(path, name).Load();
            item.transform.SetParent(parent);
            item.SetActive(true);
            item.transform.localScale = Vector3.one;
            item.transform.localPosition = Vector3.zero;
            item.name = name;
            return item;
        }

        private void HideInternal<T>(T item) where T : Component
        {
            if (!item) return;
            if (!rootData.TryGetValue(item.name, out var pool))
            {
                pool = new GameObject("Pool - {0}".Format(item.name)).transform;
                pool.SetParent(parent);
                rootData.Add(item.name, pool);
            }

            item.gameObject.SetActive(false);
            item.transform.SetParent(pool);
            LoadPool(item.name).Push(item.gameObject);
        }

        private void HideInternal(GameObject item)
        {
            if (!item) return;
            if (!rootData.TryGetValue(item.name, out var pool))
            {
                pool = new GameObject("Pool - {0}".Format(item.name)).transform;
                pool.SetParent(parent);
                rootData.Add(item.name, pool);
            }

            item.SetActive(false);
            item.transform.SetParent(pool);
            LoadPool(item.name).Push(item);
        }

        private Pool LoadPool(string path)
        {
            if (!poolData.TryGetValue(path, out var pool))
            {
                pool = new Pool(typeof(GameObject), path);
                poolData.Add(path, pool);
            }

            return (Pool)pool;
        }

        private Pool LoadPool(string path, string name)
        {
            if (!poolData.TryGetValue(name, out var pool))
            {
                pool = new Pool(typeof(GameObject), path);
                poolData.Add(name, pool);
            }

            return (Pool)pool;
        }

        [Serializable]
        private class Pool : IPool
        {
            private readonly Queue<GameObject> unused = new Queue<GameObject>();
            private GameObject current;
            private float waitTime;
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

            public GameObject Load(float cooldown)
            {
                if (cooldown != 0)
                {
                    if (waitTime > Time.time)
                    {
                        return current;
                    }

                    waitTime = Time.time + cooldown;
                }

                Dequeue++;
                Acquire++;
                if (unused.TryDequeue(out var item))
                {
                    Release--;
                    if (item)
                    {
                        current = item;
                        return item;
                    }

                    Dequeue++;
                    Enqueue++;
                }

                item = new GameObject(Path);
                current = item;
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
                current = null;
                unused.Clear();
            }
        }
    }
}