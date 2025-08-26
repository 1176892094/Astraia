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

namespace Astraia.Common
{
    public static partial class EventManager
    {
        internal static readonly List<Type, IPool> poolData = new List<Type, IPool>();

        public static void Listen<T>(IEvent<T> data) where T : struct, IEvent
        {
            LoadPool<T>().Listen(data);
        }

        public static void Remove<T>(IEvent<T> data) where T : struct, IEvent
        {
            LoadPool<T>().Remove(data);
        }
        
        public static void Invoke<T>(T data) where T : struct, IEvent
        {
            LoadPool<T>().Invoke(data);
        }

        private static EventPool<T> LoadPool<T>() where T : struct, IEvent
        {
            if (poolData.TryGetValue(typeof(T), out var pool))
            {
                return (EventPool<T>)pool;
            }

            pool = new EventPool<T>(typeof(T));
            poolData.Add(typeof(T), pool);
            return (EventPool<T>)pool;
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