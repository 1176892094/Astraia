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
        public static GameObject Show(string path)
        {
            if (!Instance) return null;
            var item = LoadPool(path, path).Load();
            item.transform.SetParent(null);
            item.SetActive(true);
            return item;
        }

        public static GameObject Show(string path, Vector3 position)
        {
            if (!Instance) return null;
            var item = LoadPool(path, path).Load();
            item.transform.SetParent(null);
            item.transform.position = position;
            item.SetActive(true);
            return item;
        }

        public static GameObject Show(string path, Vector3 position, Quaternion rotation)
        {
            if (!Instance) return null;
            var item = LoadPool(path, path).Load();
            item.transform.SetParent(null);
            item.transform.position = position;
            item.transform.rotation = rotation;
            item.SetActive(true);
            return item;
        }

        public static GameObject Show(string path, Transform parent)
        {
            if (!Instance) return null;
            var item = LoadPool(path, path).Load();
            item.transform.SetParent(parent);
            item.transform.localPosition = Vector3.zero;
            item.SetActive(true);
            return item;
        }

        public static GameObject Show(string path, Transform parent, Vector3 localScale)
        {
            if (!Instance) return null;
            var item = LoadPool(path, path).Load();
            item.transform.SetParent(parent);
            item.transform.localScale = localScale;
            item.transform.localPosition = Vector3.zero;
            item.SetActive(true);
            return item;
        }

        public static GameObject Show(string path, string name, Transform parent)
        {
            if (!Instance) return null;
            var item = LoadPool(path, name).Load();
            item.transform.SetParent(parent);
            item.transform.localPosition = Vector3.zero;
            item.SetActive(true);
            item.name = name;
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
            LoadPool(item.name, item.name).Push(item);
        }

        internal static AudioSource Play(string path)
        {
            if (!Instance) return null;
            var item = LoadPool(path).Load();
            item.transform.SetParent(null);
            item.gameObject.SetActive(true);
            return item.GetComponent<AudioSource>();
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

        private static Pool LoadPool(string path)
        {
            if (poolData.TryGetValue(path, out var pool))
            {
                return (Pool)pool;
            }

            pool = Pool.Create(typeof(AudioSource), path, Create, 3, 3, 1);
            poolData.Add(path, pool);
            return (Pool)pool;
        }

        private static GameObject Load(string path)
        {
            return AssetManager.Load<GameObject>(path);
        }

        private static GameObject Create(string path)
        {
            return new GameObject(path, typeof(AudioSource));
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