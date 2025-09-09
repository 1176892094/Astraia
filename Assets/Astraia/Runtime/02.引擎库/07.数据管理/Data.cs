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
        public static async void LoadDataTable()
        {
            var assembly = Service.Ref.GetAssembly(GlobalSetting.Instance.assemblyName);
            if (assembly == null)
            {
                EventManager.Invoke(new DataComplete());
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
                EventManager.Invoke(new DataComplete());
                return;
            }

            EventManager.Invoke(new DataAwake(assetNames.ToArray()));
            foreach (var assetName in assetNames)
            {
                var nickName = assetName.Substring(assetName.LastIndexOf('.') + 1);
                try
                {
                    ScriptableObject assetData = null;
                    if (Application.isPlaying)
                    {
                        assetData = await AssetManager.Load<ScriptableObject>(GlobalSetting.GetTablePath(nickName));
                    }
#if UNITY_EDITOR
                    assetData ??= UnityEditor.AssetDatabase.LoadAssetAtPath<ScriptableObject>(GlobalSetting.GetEditorPath(nickName));
#endif
                    var assetType = assembly.GetType(assetName.Substring(0, assetName.Length - 5));
                    var properties = assetType.GetProperties(Service.Ref.Instance);
                    foreach (var property in properties)
                    {
                        if (property.GetCustomAttribute(typeof(PrimaryAttribute)) != null)
                        {
                            if (property.PropertyType == typeof(int))
                            {
                                typeof(DataTable<,>).MakeGenericType(typeof(int), assetType).Invoke("Add", assetData, property.Name, nickName);
                            }
                            else if (property.PropertyType == typeof(string))
                            {
                                typeof(DataTable<,>).MakeGenericType(typeof(string), assetType).Invoke("Add", assetData, property.Name, nickName);
                            }
                            else if (property.PropertyType.IsEnum)
                            {
                                typeof(DataTable<,>).MakeGenericType(typeof(Enum), assetType).Invoke("Add", assetData, property.Name, nickName);
                            }
                        }
                    }

                    EventManager.Invoke(new DataUpdate(nickName));
                }
                catch (Exception e)
                {
                    Debug.LogError("加载 {0} 数据失败!\n{1}".Format(nickName, e));
                }
            }

            EventManager.Invoke(new DataComplete());
        }

        public static T Get<T>(int key) where T : IData
        {
            return DataTable<int, T>.Get(key);
        }

        public static T Get<T>(string key) where T : IData
        {
            return DataTable<string, T>.Get(key);
        }

        public static T Get<T>(Enum key) where T : IData
        {
            return DataTable<Enum, T>.Get(key);
        }

        public static IReadOnlyList<T> GetTable<T>() where T : IData
        {
            if (DataTable<int, T>.items != null)
            {
                return DataTable<int, T>.items;
            }

            if (DataTable<Enum, T>.items != null)
            {
                return DataTable<Enum, T>.items;
            }

            if (DataTable<string, T>.items != null)
            {
                return DataTable<string, T>.items;
            }

            Debug.LogError("获取 {0} 失败!".Format(typeof(T).Name));
            return null;
        }
    }
}