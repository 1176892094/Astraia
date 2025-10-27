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
using UnityEngine;

namespace Astraia.Common
{
    public static class DataManager
    {
        public static bool isLoaded;

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
                    var assetData = (IDataTable)AssetManager.Load<ScriptableObject>(GlobalSetting.Table.Format(nickName));
                    var assetType = assembly.GetType(assetName.Substring(0, assetName.Length - 5));
                    var properties = assetType.GetProperties(Service.Ref.Instance);
                    foreach (var property in properties)
                    {
                        if (Service.Ref<PrimaryAttribute>.GetAttribute(property) != null)
                        {
                            if (property.PropertyType == typeof(int))
                            {
                                assetData.AddData<int>(property.Name);
                            }
                            else if (property.PropertyType.IsEnum)
                            {
                                assetData.AddData<Enum>(property.Name);
                            }
                            else if (property.PropertyType == typeof(string))
                            {
                                assetData.AddData<string>(property.Name);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Service.Log.Error("加载 {0} 数据失败!\n{1}".Format(nickName, e));
                }
            }

            isLoaded = true;
            EventManager.Invoke(new OnDataComplete());
        }

        public static T Get<T>(int key) where T : IData
        {
            return DataTable<T>.By<int>.Get(key);
        }

        public static T Get<T>(Enum key) where T : IData
        {
            return DataTable<T>.By<Enum>.Get(key);
        }

        public static T Get<T>(string key) where T : IData
        {
            return DataTable<T>.By<string>.Get(key);
        }

        public static IReadOnlyList<T> GetTable<T>() where T : IData
        {
            if (DataTable<T>.Instance)
            {
                return DataTable<T>.Instance.items;
            }

            Service.Log.Warn("获取 {0} 失败!".Format(typeof(T).Name));
            return null;
        }
    }
}