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
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Astraia.Common
{
    public static partial class DataManager
    {
        public static void LoadDataTable()
        {
            var assembly = Service.Ref.GetAssembly(GlobalSetting.Define);
            if (assembly == null)
            {
                EventManager.Invoke(new OnDataComplete());
                return;
            }

            var assetNames = new List<string>();
            foreach (var assetType in assembly.GetTypes())
            {
                if (typeof(IDataTable).IsAssignableFrom(assetType))
                {
                    assetNames.Add(assetType.FullName);
                }
            }

            if (assetNames.Count == 0)
            {
                EventManager.Invoke(new OnDataComplete());
                return;
            }

            foreach (var assetName in assetNames)
            {
                var nickName = assetName.Substring(assetName.LastIndexOf('.') + 1);
                try
                {
                    var assetData = AssetManager.Load<ScriptableObject>(GlobalSetting.Table.Format(nickName));
                    var assetType = assembly.GetType(assetName.Substring(0, assetName.Length - 5));
                    var properties = assetType.GetProperties(Service.Ref.Instance);
                    foreach (var property in properties)
                    {
                        if (property.GetCustomAttribute(typeof(PrimaryAttribute)) != null)
                        {
                            if (property.PropertyType == typeof(int))
                            {
                                Emit.Invoke(typeof(DataTable<,>).MakeGenericType(typeof(int), assetType), "Add", assetData, property.Name, nickName);
                            }
                            else if (property.PropertyType.IsEnum)
                            {
                                Emit.Invoke(typeof(DataTable<,>).MakeGenericType(typeof(Enum), assetType), "Add", assetData, property.Name, nickName);
                            }
                            else if (property.PropertyType == typeof(string))
                            {
                                Emit.Invoke(typeof(DataTable<,>).MakeGenericType(typeof(string), assetType), "Add", assetData, property.Name, nickName);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("加载 {0} 数据失败!\n{1}".Format(nickName, e));
                }
            }

            EventManager.Invoke(new OnDataComplete());
        }

        public static T Get<T>(int key) where T : IData
        {
            return DataTable<int, T>.Get(key);
        }

        public static T Get<T>(Enum key) where T : IData
        {
            return DataTable<Enum, T>.Get(key);
        }

        public static T Get<T>(string key) where T : IData
        {
            return DataTable<string, T>.Get(key);
        }

        public static IReadOnlyList<T> GetTable<T>() where T : IData
        {
            if (DataTable<T>.Items != null)
            {
                return DataTable<T>.Items;
            }

            Debug.LogError("获取 {0} 失败!".Format(typeof(T).Name));
            return null;
        }
    }
}