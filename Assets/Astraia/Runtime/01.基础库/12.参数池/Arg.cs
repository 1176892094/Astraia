// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-09-10 21:09:24
// // # Recently: 2025-09-10 21:09:24
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Astraia.Common
{
    internal static partial class ArgManager
    {
        private static readonly Dictionary<int, Dictionary<Type, IPool>> poolData = new Dictionary<int, Dictionary<Type, IPool>>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Set<T>(int owner, Enum id, T value)
        {
            LoadPool<T>(owner).Set(id, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Get<T>(int owner, Enum id)
        {
            return LoadPool<T>(owner).Get(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Pool<T> LoadPool<T>(int owner)
        {
            if (!poolData.TryGetValue(owner, out var pools))
            {
                pools = new Dictionary<Type, IPool>();
                poolData[owner] = pools;
            }

            if (!pools.TryGetValue(typeof(T), out var pool))
            {
                pool = new Pool<T>();
                pools[typeof(T)] = pool;
            }

            return (Pool<T>)pool;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Dispose(int owner)
        {
            if (poolData.TryGetValue(owner, out var pools))
            {
                foreach (var pool in pools.Values)
                {
                    pool.Dispose();
                }

                pools.Clear();
                poolData.Remove(owner);
            }
        }

        public static void Dispose()
        {
            var copies = new List<int>(poolData.Keys);
            foreach (var owner in copies)
            {
                Dispose(owner);
            }

            poolData.Clear();
        }
    }
}