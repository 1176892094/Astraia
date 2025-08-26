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
    public static partial class PoolManager
    {
        public static async Task<GameObject> Show(string path)
        {
            if (!GlobalManager.Instance) return null;
            var assetData = await LoadPool(path).Dequeue();
            assetData.transform.SetParent(null);
            assetData.SetActive(true);
            return assetData;
        }

        public static async void Show(string path, Action<GameObject> action)
        {
            if (!GlobalManager.Instance) return;
            var assetData = await LoadPool(path).Dequeue();
            assetData.transform.SetParent(null);
            assetData.SetActive(true);
            action.Invoke(assetData);
        }

        public static void Hide(GameObject item)
        {
            if (!GlobalManager.Instance) return;
            if (!GlobalManager.poolGroup.TryGetValue(item.name, out var pool))
            {
                pool = new GameObject(Service.Text.Format("Pool - {0}", item.name));
                pool.transform.SetParent(GlobalManager.Instance.transform);
                GlobalManager.poolGroup.Add(item.name, pool);
            }

            item.SetActive(false);
            item.transform.SetParent(pool.transform);
            LoadPool(item.name).Enqueue(item);
        }

        private static EntityPool LoadPool(string path)
        {
            if (GlobalManager.poolData.TryGetValue(path, out var pool))
            {
                return (EntityPool)pool;
            }

            pool = new EntityPool(typeof(GameObject), path);
            GlobalManager.poolData.Add(path, pool);
            return (EntityPool)pool;
        }
        
        internal static void Dispose()
        {
            foreach (var item in GlobalManager.poolData.Values)
            {
                item.Dispose();
            }

            GlobalManager.poolData.Clear();
            GlobalManager.poolGroup.Clear();
        }
    }
}