using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.Utilities;
using UnityEngine;

namespace JFramework.Core
{
    using IntDataDict = Dictionary<int, IData>;
    using StrDataDict = Dictionary<string, IData>;

    /// <summary>
    /// 数据管理器
    /// </summary>
    public sealed class DataManager : Singleton<DataManager>
    {
        /// <summary>
        /// 存储int为主键类型的数据字典
        /// </summary>
        internal Dictionary<Type, IntDataDict> IntDataDict;

        /// <summary>
        /// 存储string为主键的数据字典
        /// </summary>
        internal Dictionary<Type, StrDataDict> StrDataDict;
        
        /// <summary>
        /// 数据表名称
        /// </summary>
        private const string Table = "Table";

        /// <summary>
        /// 资源路径
        /// </summary>
        private const string AssetPath = "DataTable/";

        /// <summary>
        /// 命名空间名称
        /// </summary>
        private const string Namespace = "JFramework.Table";

        /// <summary>
        /// 构造函数初始化数据
        /// </summary>
        internal override void Awake()
        {
            base.Awake();
            IntDataDict = new Dictionary<Type, IntDataDict>();
            StrDataDict = new Dictionary<Type, StrDataDict>();
            var assembly = GetAssembly();
            var types = assembly.GetTypes().Where(type => type.IsSubclassOf(typeof(IDataTable)));
            foreach (var type in types)
            {
                try
                {
                    var keyName = type.Name;
                    AssetManager.Instance.LoadAsync<ScriptableObject>(AssetPath + keyName, obj =>
                    {
                        var dataTable = (IDataTable)obj;
                        var tableType = GetTableType(assembly, type);
                        var keyField = KeyValue(tableType);

                        if (keyField == null)
                        {
                            Debug.LogError($"DataManager没有找到主键:{keyName}!");
                            return;
                        }

                        var keyType = keyField.FieldType;

                        if (keyType == typeof(int))
                        {
                            var dataDict = new IntDataDict();
                            for (var i = 0; i < dataTable.Count; ++i)
                            {
                                var data = (IData)dataTable.GetData(i);
                                int key = (int)keyField.GetValue(data);
                                dataDict.Add(key, data);
                            }

                            IntDataDict.Add(tableType, dataDict);
                        }
                        else if (keyType == typeof(string))
                        {
                            var dataDict = new StrDataDict();
                            for (var i = 0; i < dataTable.Count; ++i)
                            {
                                var data = (IData)dataTable.GetData(i);
                                string key = (string)keyField.GetValue(data);
                                dataDict.Add(key, data);
                            }

                            StrDataDict.Add(tableType, dataDict);
                        }
                        else
                        {
                            Debug.LogError($"DataManager加载{type.Name}失败.这不是有效的主键!");
                        }
                    });
                }
                catch (Exception e)
                {
                    Debug.LogError(e.ToString());
                }
            }

            Debug.Log($"DataManager加载资源完成!");
        }
        
        /// <summary>
        /// 得到数据表对象的类型
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private Type GetTableType(Assembly assembly, Type type) => assembly.GetType(Namespace + "." + type.Name);

        /// <summary>
        /// 获取当前程序集中的数据表对象类
        /// </summary>
        /// <returns></returns>
        private Assembly GetAssembly()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(IDataTable)));
                var collectionTypes = types as Type[] ?? types.ToArray();
                if (collectionTypes.Any()) return assembly;
            }

            return typeof(IDataTable).Assembly;
        }

        /// <summary>
        /// 获取对应类型数据下主键为Key的数据
        /// </summary>
        /// <param name="key">传入的int主键</param>
        /// <typeparam name="T">可以使用所有继承IData类型的对象</typeparam>
        /// <returns>返回一个数据对象</returns>
        public T Get<T>(int key) where T : IData
        {
            IntDataDict.TryGetValue(typeof(T), out IntDataDict soDict);
            if (soDict == null) return default;
            soDict.TryGetValue(key, out IData data);
            return (T)data;
        }

        /// <summary>
        /// 获取对应类型数据下主键为Key的数据
        /// </summary>
        /// <param name="key">传入的string主键</param>
        /// <typeparam name="T">要获取数据的类型,必须继承自JFramework.Data</typeparam>
        /// <returns>返回一个数据对象</returns>
        public T Get<T>(string key) where T : IData
        {
            StrDataDict.TryGetValue(typeof(T), out StrDataDict soDict);
            if (soDict == null) return default;
            soDict.TryGetValue(key, out IData data);
            return (T)data;
        }
        
        /// <summary>
        /// 通过数据管理器得到数据表
        /// </summary>
        /// <typeparam name="T">可以使用所有继承Data类型的对象</typeparam>
        /// <returns>返回泛型列表</returns>
        public List<T> GetTable<T>() where T : IData
        {
            var table = GetTable(typeof(T));
            if (table == null) return null;
            var dataList = new List<T>();
            table.ForEach(data => dataList.Add((T)data));
            return dataList;
        }

        /// <summary>
        /// 通过数据管理器得到数据表
        /// </summary>
        /// <param name="type">传入的类型</param>
        /// <returns>返回一个Data的列表</returns>
        private List<IData> GetTable(Type type)
        {
            IntDataDict.TryGetValue(type, out IntDataDict dictInt);
            if (dictInt != null) return dictInt.Values.ToList();
            StrDataDict.TryGetValue(type, out StrDataDict dictStr);
            return dictStr?.Values.ToList();
        }
        
        /// <summary>
        /// 获取数据的主键
        /// </summary>
        /// <param name="type">传入的数据类型</param>
        /// <returns>返回字段的信息</returns>
        private static FieldInfo KeyValue(Type type)
        {
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return (from field in fields let attrs = field.GetCustomAttributes(typeof(DataAttribute), false) 
                where attrs.Length > 0 select field).FirstOrDefault();
        }
        
        /// <summary>
        /// 获取数据的键值
        /// </summary>
        /// <returns>返回数据的主键</returns>
        public static object KeyValue(IData data)
        {
            var key = KeyValue(data.GetType());
            return key == null ? null : key.GetValue(data);
        }
    }
}