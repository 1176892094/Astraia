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
            item.transform.localPosition = Vector3.zero;
            return item;
        }

        public static GameObject Show(string path, Transform parent, Vector3 scale)
        {
            if (!Instance) return null;
            var item = LoadPool(path).Load();
            item.transform.SetParent(parent);
            item.SetActive(true);
            item.transform.localScale = scale;
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
    }
}