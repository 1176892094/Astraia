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

namespace Astraia.Core
{
    using static GlobalManager;

    public static class DataManager
    {
        public static void LoadDataTable()
        {
            var assembly = Search.GetAssembly(GlobalSetting.Define);
            if (assembly != null)
            {
                foreach (var assetName in assembly.GetTypes().Where(type => typeof(IDataTable).IsAssignableFrom(type)).Select(type => type.Name))
                {
                    var dataTable = (IDataTable)AssetManager.Load<ScriptableObject>(GlobalSetting.Table.Format(assetName));
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

            EventManager.Invoke(new OnDataComplete());
        }

        public static T Get<T>(int key) where T : IData
        {
            if (dataTable1.TryGetValue(typeof(T), out var data))
            {
                if (data.TryGetValue(key, out var result))
                {
                    return (T)result;
                }
            }

            return default;
        }

        public static T Get<T>(Enum key) where T : IData
        {
            if (dataTable2.TryGetValue(typeof(T), out var data))
            {
                if (data.TryGetValue(key, out var result))
                {
                    return (T)result;
                }
            }

            return default;
        }

        public static T Get<T>(string key) where T : IData
        {
            if (dataTable3.TryGetValue(typeof(T), out var data))
            {
                if (data.TryGetValue(key, out var result))
                {
                    return (T)result;
                }
            }

            return default;
        }

        public static List<T> GetTable<T>() where T : IData
        {
            return (DataTable<T>)dataTable[typeof(T)];
        }

        public static void Dispose()
        {
            dataTable1.Clear();
            dataTable2.Clear();
            dataTable3.Clear();
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

        public static T Parse<T>(this XorBytes[] reason, int index)
        {
            if (reason == null) return default;
            var value = Text.GetString(reason[index]);
            if (parsers.TryGetValue(typeof(T), out var func))
            {
                return ((Func<string, T>)func).Invoke(value);
            }

            return value.InputString(typeof(T)) is T result ? result : default;
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

        private static object InputString(this string reason, Type target)
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
                var parts = reason.Split(';');
                var member = target.GetElementType()!;
                var result = Array.CreateInstance(member, parts.Length);
                for (var i = 0; i < parts.Length; i++)
                {
                    result.SetValue(InputString(parts[i], member), i);
                }

                return result;
            }
            else
            {
                var parts = reason.Split(',');
                var member = target.GetFields(Search.Instance);
                var result = Activator.CreateInstance(target);
                for (var i = 0; i < parts.Length; i++)
                {
                    member[i].SetValue(result, InputString(parts[i], member[i].FieldType));
                }

                return result;
            }
        }
    }

    [Serializable]
    public abstract class DataTable<TData> : ScriptableObject, IDataTable where TData : IData
    {
        [SerializeField] private List<TData> items = new List<TData>();

        Type IDataTable.Type => typeof(TData);

        void IDataTable.AddData(IData data) => items.Add((TData)data);

        void IDataTable.AddData(string name, Type type)
        {
            if (type == typeof(int))
            {
                if (!dataTable1.ContainsKey(typeof(TData)))
                {
                    dataTable1[typeof(TData)] = GetData<int>(name);
                }
            }
            else if (type.IsEnum)
            {
                if (!dataTable2.ContainsKey(typeof(TData)))
                {
                    dataTable2[typeof(TData)] = GetData<Enum>(name);
                }
            }
            else if (type == typeof(string))
            {
                if (!dataTable3.ContainsKey(typeof(TData)))
                {
                    dataTable3[typeof(TData)] = GetData<string>(name);
                }
            }

            dataTable[typeof(TData)] = this;
        }

        private Dictionary<TKey, IData> GetData<TKey>(string name)
        {
            var result = new Dictionary<TKey, IData>();
            foreach (var item in items)
            {
                var index = item.GetValue<TKey>(name);
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