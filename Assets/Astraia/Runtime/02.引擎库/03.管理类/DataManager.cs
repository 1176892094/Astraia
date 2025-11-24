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
using System.Linq;
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

    public static class DataSetting
    {
        private static readonly Dictionary<Type, Delegate> parsers = new Dictionary<Type, Delegate>();

        static DataSetting()
        {
            parsers[typeof(Vector2)] = new Func<string, Vector2>(InputVector2);
            parsers[typeof(Vector3)] = new Func<string, Vector3>(InputVector3);
            parsers[typeof(Vector4)] = new Func<string, Vector4>(InputVector4);
            parsers[typeof(Vector2Int)] = new Func<string, Vector2Int>(InputVector2Int);
            parsers[typeof(Vector3Int)] = new Func<string, Vector3Int>(InputVector3Int);
            parsers[typeof(Vector2[])] = new Func<string, Vector2[]>(InputVector2Array);
            parsers[typeof(Vector3[])] = new Func<string, Vector3[]>(InputVector3Array);
            parsers[typeof(Vector4[])] = new Func<string, Vector4[]>(InputVector4Array);
            parsers[typeof(Vector2Int[])] = new Func<string, Vector2Int[]>(InputVector2IntArray);
            parsers[typeof(Vector3Int[])] = new Func<string, Vector3Int[]>(InputVector3IntArray);
        }

        public static T Parse<T>(this byte[] reason)
        {
            if (reason == null) return default;
            var value = Service.Text.GetString(reason);
            if (parsers.TryGetValue(typeof(T), out var func))
            {
                return ((Func<string, T>)func).Invoke(value);
            }

            return value.InputGeneric(typeof(T)) is T result ? result : default;
        }

        private static Vector2 InputVector2(this string reason)
        {
            var points = reason.Split(',');
            var x = float.Parse(points[0]);
            var y = float.Parse(points[1]);
            return new Vector2(x, y);
        }

        private static Vector3 InputVector3(this string reason)
        {
            var points = reason.Split(',');
            var x = float.Parse(points[0]);
            var y = float.Parse(points[1]);
            var z = float.Parse(points[2]);
            return new Vector3(x, y, z);
        }

        private static Vector4 InputVector4(this string reason)
        {
            var points = reason.Split(',');
            var x = float.Parse(points[0]);
            var y = float.Parse(points[1]);
            var z = float.Parse(points[2]);
            var a = float.Parse(points[3]);
            return new Vector4(x, y, z, a);
        }

        private static Vector2Int InputVector2Int(this string reason)
        {
            var points = reason.Split(',');
            var x = int.Parse(points[0]);
            var y = int.Parse(points[1]);
            return new Vector2Int(x, y);
        }

        private static Vector3Int InputVector3Int(this string reason)
        {
            var points = reason.Split(',');
            var x = int.Parse(points[0]);
            var y = int.Parse(points[1]);
            var z = int.Parse(points[2]);
            return new Vector3Int(x, y, z);
        }

        private static Vector2[] InputVector2Array(this string reason)
        {
            return reason.InputArray().Select(InputVector2).ToArray();
        }

        private static Vector3[] InputVector3Array(this string reason)
        {
            return reason.InputArray().Select(InputVector3).ToArray();
        }

        private static Vector4[] InputVector4Array(this string reason)
        {
            return reason.InputArray().Select(InputVector4).ToArray();
        }

        private static Vector2Int[] InputVector2IntArray(this string reason)
        {
            return reason.InputArray().Select(InputVector2Int).ToArray();
        }

        private static Vector3Int[] InputVector3IntArray(this string reason)
        {
            return reason.InputArray().Select(InputVector3Int).ToArray();
        }

        private static List<string> InputArray(this string reason)
        {
            var result = new List<string>();
            if (!string.IsNullOrEmpty(reason))
            {
                if (reason.EndsWith(";"))
                {
                    reason = reason.Substring(0, reason.Length - 1);
                }

                result.AddRange(reason.Split(';'));
            }

            return result;
        }

        private static object InputGeneric(this string reason, Type target)
        {
            if (string.IsNullOrEmpty(reason) || target == typeof(string))
            {
                return reason;
            }

            if (target.IsEnum)
            {
                return Enum.Parse(target, reason);
            }

            if (target.IsPrimitive)
            {
                return Convert.ChangeType(reason, target);
            }

            if (target.IsArray)
            {
                if (reason.EndsWith(";"))
                {
                    reason = reason.Substring(0, reason.Length - 1);
                }

                var element = target.GetElementType();
                var members = reason.Split(';');
                var instance = Array.CreateInstance(element!, members.Length);
                for (var i = 0; i < members.Length; ++i)
                {
                    instance.SetValue(InputGeneric(members[i], element), i);
                }

                return instance;
            }

            var member = reason.Split(',');
            var result = Activator.CreateInstance(target);
            var fields = target.GetFields(Service.Ref.Instance);
            for (var i = 0; i < fields.Length; i++)
            {
                fields[i].SetValue(result, Service.Text.GetBytes(member[i]));
            }

            return result;
        }
    }

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
                        Service.Log.Warn("加载数据 {0} 失败。键值重复: {1}".Format(value, index));
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

    public interface IData
    {
        void Create(string[] sheet, int column);
    }

    internal interface IDataTable
    {
        void AddData(IData data);

        void AddData<T>(string name);
    }
}