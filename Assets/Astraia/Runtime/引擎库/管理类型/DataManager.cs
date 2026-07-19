// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2024-12-23 18:12:21
// # Recently: 2025-01-08 17:01:43
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Astraia
{
    using static DataManager;

    public static class DataManager
    {
        internal static readonly Dictionary<Type, IDataTable> DataTable = new Dictionary<Type, IDataTable>();
        internal static readonly Dictionary<Type, IDictionary> Database1 = new Dictionary<Type, IDictionary>();
        internal static readonly Dictionary<Type, IDictionary> Database2 = new Dictionary<Type, IDictionary>();
        internal static readonly Dictionary<Type, IDictionary> Database3 = new Dictionary<Type, IDictionary>();

        public static void LoadDataTable()
        {
            var assembly = Search.GetAssembly(GlobalSetting.DEFINE);
            if (assembly != null)
            {
                foreach (var assetName in assembly.GetTypes().Where(type => typeof(IDataTable).IsAssignableFrom(type)).Select(type => type.Name))
                {
                    var dataTable = (IDataTable)AssetManager.Load<ScriptableObject>(GlobalSetting.SHEETS.Format(assetName));
                    if (dataTable != null)
                    {
                        var properties = dataTable.Type.GetProperties(Search.Instance);
                        foreach (var property in properties)
                        {
                            if (property.HasAttribute<PrimaryAttribute>())
                            {
                                dataTable.AddData(property.Name, property.PropertyType);
                            }
                        }
                    }
                }
            }
        }

        public static T Get<T>(int key) where T : struct, IData
        {
            if (Database1.TryGetValue(typeof(T), out var result))
            {
                return ((Dictionary<int, T>)result).GetValueOrDefault(key);
            }

            return default;
        }

        public static T Get<T>(Enum key) where T : struct, IData
        {
            if (Database2.TryGetValue(typeof(T), out var result))
            {
                return ((Dictionary<Enum, T>)result).GetValueOrDefault(key);
            }

            return default;
        }

        public static T Get<T>(string key) where T : struct, IData
        {
            if (Database3.TryGetValue(typeof(T), out var result))
            {
                return ((Dictionary<string, T>)result).GetValueOrDefault(key);
            }

            return default;
        }

        public static List<T> GetTable<T>() where T : struct, IData
        {
            if (DataTable.TryGetValue(typeof(T), out var items))
            {
                return (DataTable<T>)items;
            }

            return null;
        }

        public static void Dispose()
        {
            Database1.Clear();
            Database2.Clear();
            Database3.Clear();
            DataTable.Clear();
        }
    }

    [Serializable]
    internal abstract class DataTable<TData> : ScriptableObject, IDataTable where TData : struct, IData
    {
        [SerializeField] private List<TData> items = new List<TData>();

        Type IDataTable.Type => typeof(TData);

        void IDataTable.AddData(IData data) => items.Add((TData)data);

        void IDataTable.AddData(string name, Type value)
        {
            if (!Database1.ContainsKey(typeof(TData)) && value == typeof(int))
            {
                Database1[typeof(TData)] = GetData<int>(name);
            }

            if (!Database2.ContainsKey(typeof(TData)) && value.IsEnum)
            {
                Database2[typeof(TData)] = GetData<Enum>(name);
            }

            if (!Database3.ContainsKey(typeof(TData)) && value == typeof(string))
            {
                Database3[typeof(TData)] = GetData<string>(name);
            }

            if (!DataTable.ContainsKey(typeof(TData)))
            {
                DataTable[typeof(TData)] = this;
            }
        }

        private IDictionary GetData<T>(string name)
        {
            var result = new Dictionary<T, TData>();
            foreach (var item in items)
            {
                var index = (T)typeof(TData).GetProperty(name, Search.Instance)!.GetValue(item);
                if (result.ContainsKey(index))
                {
                    Log.Warn("加载数据 {0} 失败。键值重复: {1}".Format(item, index));
                }

                result[index] = item;
            }

            return result;
        }

        public static implicit operator List<TData>(DataTable<TData> dataTable)
        {
            return dataTable.items;
        }
    }

    internal interface IDataTable
    {
        Type Type { get; }

        void AddData(IData data);

        void AddData(string name, Type type);
    }

    public interface IData
    {
    }
}