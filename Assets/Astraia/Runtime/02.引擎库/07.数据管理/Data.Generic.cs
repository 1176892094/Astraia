// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2025-01-08 17:01:14
// # Recently: 2025-01-08 17:01:30
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Collections.Generic;
using Astraia.Common;
using UnityEngine;

namespace Astraia
{
    [Serializable]
    public abstract class DataTable<TData> : ScriptableObject, IDataTable where TData : IData
    {
        [SerializeField] internal List<TData> items = new List<TData>();

        internal static DataTable<TData> Instance;

        void IDataTable.AddData(IData data)
        {
            items.Add((TData)data);
        }

        void IDataTable.AddData<TKey>(string name)
        {
            Instance = this;
            By<TKey>.Add(items, name);
        }

        internal static class By<TKey>
        {
            private static readonly Dictionary<TKey, TData> itemData = new Dictionary<TKey, TData>();

            public static void Add(List<TData> items, string name)
            {
                if (DataManager.isLoaded)
                {
                    return;
                }

                itemData.Clear();
                foreach (var item in items)
                {
                    var index = item.GetValue<TKey>(name);
                    if (itemData.TryGetValue(index, out var value))
                    {
                        Debug.LogWarning("加载数据 {0} 失败。键值重复: {1}".Format(value, index));
                        continue;
                    }

                    itemData.Add(index, item);
                }
            }

            public static TData Get(TKey key)
            {
                if (itemData.TryGetValue(key, out var item))
                {
                    return item;
                }

                return default;
            }
        }
    }
}