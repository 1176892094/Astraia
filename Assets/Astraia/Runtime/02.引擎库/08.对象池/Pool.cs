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
using System.Threading.Tasks;
using UnityEngine;

namespace Astraia.Common
{
    using static GlobalManager;

    public static partial class PoolManager
    {
        public static async Task<GameObject> Show(string path)
        {
            if (!Instance) return null;
            var item = await LoadPool(path).Load();
            item.transform.SetParent(null);
            item.SetActive(true);
            return item;
        }

        public static async void Show(string path, Action<GameObject> action)
        {
            if (!Instance) return;
            var item = await LoadPool(path).Load();
            item.transform.SetParent(null);
            item.SetActive(true);
            action.Invoke(item);
        }

        public static void Hide(GameObject item)
        {
            if (!Instance) return;
            if (!RootData.TryGetValue(item.name, out var pool))
            {
                pool = new GameObject(Service.Text.Format("Pool - {0}", item.name));
                pool.transform.SetParent(Instance.transform);
                RootData.Add(item.name, pool);
            }

            item.SetActive(false);
            item.transform.SetParent(pool.transform);
            LoadPool(item.name).Push(item);
        }

        private static Pool LoadPool(string path)
        {
            if (PoolData.TryGetValue(path, out var pool))
            {
                return (Pool)pool;
            }

            pool = Pool.Create(typeof(GameObject), path);
            PoolData.Add(path, pool);
            return (Pool)pool;
        }

        internal static void Dispose()
        {
            foreach (var item in PoolData.Values)
            {
                item.Dispose();
            }

            PoolData.Clear();
            RootData.Clear();
        }
    }
}