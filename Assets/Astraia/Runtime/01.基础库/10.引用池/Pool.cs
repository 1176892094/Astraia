// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2025-01-09 16:01:50
// # Recently: 2025-01-11 18:01:35
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Collections.Generic;


namespace Astraia.Common
{
    public static partial class HeapManager
    {
        internal static readonly Dictionary<Type, IPool> poolData = new Dictionary<Type, IPool>();

        public static T Dequeue<T>()
        {
            return LoadPool<T>(typeof(T)).Dequeue();
        }

        public static T Dequeue<T>(Type type)
        {
            return LoadPool<T>(type).Dequeue();
        }

        public static void Enqueue<T>(T item)
        {
            LoadPool<T>(typeof(T)).Enqueue(item);
        }

        public static void Enqueue<T>(T item, Type type)
        {
            LoadPool<T>(type).Enqueue(item);
        }

        private static HeapPool<T> LoadPool<T>(Type type)
        {
            if (poolData.TryGetValue(type, out var item))
            {
                return (HeapPool<T>)item;
            }

            item = new HeapPool<T>(type);
            poolData.Add(type, item);
            return (HeapPool<T>)item;
        }
        
        internal static void Dispose()
        {
            foreach (var item in poolData.Values)
            {
                item.Dispose();
            }

            poolData.Clear();
        }
    }
}