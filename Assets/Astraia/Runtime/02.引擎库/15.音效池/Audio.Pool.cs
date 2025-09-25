// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-09-22 20:09:52
// // # Recently: 2025-09-22 20:09:52
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using UnityEngine;

namespace Astraia.Common
{
    using static GlobalManager;

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