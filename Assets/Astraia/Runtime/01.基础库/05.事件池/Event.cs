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
using System.Runtime.CompilerServices;

namespace Astraia.Common
{
    public static class EventManager
    {
        internal static readonly Dictionary<Type, IPool> poolData = new Dictionary<Type, IPool>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Listen<T>(IEvent<T> data) where T : IEvent
        {
            LoadPool<T>().Listen(data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Remove<T>(IEvent<T> data) where T : IEvent
        {
            LoadPool<T>().Remove(data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Invoke<T>(T data) where T : IEvent
        {
            LoadPool<T>().Invoke(data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Pool<T> LoadPool<T>() where T : IEvent
        {
            if (poolData.TryGetValue(typeof(T), out var pool))
            {
                return (Pool<T>)pool;
            }

            pool = Pool<T>.Create(typeof(T), typeof(T).Name);
            poolData.Add(typeof(T), pool);
            return (Pool<T>)pool;
        }

        internal static void Dispose()
        {
            foreach (var item in poolData.Values)
            {
                item.Dispose();
            }

            poolData.Clear();
        }

        private class Pool<T> : IPool where T : IEvent
        {
            private readonly HashSet<IEvent<T>> cached = new HashSet<IEvent<T>>();
            private event Action<T> OnExecute;

            public Type Type { get; private set; }
            public string Path { get; private set; }
            public int Acquire { get; private set; }
            public int Release { get; private set; }
            public int Dequeue { get; private set; }
            public int Enqueue { get; private set; }

            public void Listen(IEvent<T> obj)
            {
                if (cached.Add(obj))
                {
                    Dequeue++;
                    Acquire++;
                    OnExecute += obj.Execute;
                }
            }

            public void Remove(IEvent<T> obj)
            {
                if (cached.Remove(obj))
                {
                    Enqueue++;
                    Acquire--;
                    OnExecute -= obj.Execute;
                }
            }

            public void Invoke(T message)
            {
                Release++;
                OnExecute?.Invoke(message);
            }

            void IDisposable.Dispose()
            {
                cached.Clear();
                OnExecute = null;
            }

            public static Pool<T> Create(Type type, string path)
            {
                var instance = new Pool<T>();
                instance.Type = type;
                instance.Path = path;
                return instance;
            }
        }
    }

    public interface IEvent
    {
    }

    public interface IEvent<in T> where T : IEvent
    {
        void Execute(T message);

        void Listen() => EventManager.Listen(this);

        void Remove() => EventManager.Remove(this);
    }
}