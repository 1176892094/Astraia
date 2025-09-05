// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-09-04 02:09:24
// // # Recently: 2025-09-04 02:09:24
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System.Collections.Generic;
using UnityEngine;

namespace Astraia.Common
{
    public static partial class DataManager
    {
        private static class DataTable<Key, Data> where Data : IData
        {
            private static Dictionary<Key, Data> itemData;
            public static IReadOnlyList<Data> items;

            public static Data Get(Key key)
            {
                if (itemData != null && itemData.TryGetValue(key, out var data))
                {
                    return data;
                }

                return default;
            }

            private static void Add(IDataTable assetData, string property, string name)
            {
                if (itemData == null)
                {
                    itemData = new Dictionary<Key, Data>();
                    if (assetData is DataTable<Data> dataTable)
                    {
                        items = dataTable.items;
                        foreach (var item in items)
                        {
                            var data = item.GetValue<Key>(property);
                            if (!itemData.TryAdd(data, item))
                            {
                                Debug.LogWarning(Service.Text.Format("加载数据 {0} 失败。键值重复: {1}", name, data));
                            }
                        }
                    }
                }
            }
        }
    }
}