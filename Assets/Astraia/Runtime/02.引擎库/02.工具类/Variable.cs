// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-09-18 16:09:58
// // # Recently: 2025-09-18 16:09:58
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Astraia.Common
{
    internal static class Variable<T>
    {
        public static readonly Dictionary<Entity, Dictionary<Enum, T>> variables = new Dictionary<Entity, Dictionary<Enum, T>>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Set(Entity owner, Enum id, T value)
        {
            if (!variables.TryGetValue(owner, out var results))
            {
                results = new Dictionary<Enum, T>();
                variables.Add(owner, results);
                owner.OnFaded += () =>
                {
                    results.Clear();
                    variables.Remove(owner);
                };
            }

            results[id] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Get(Entity owner, Enum id, T value)
        {
            if (variables.TryGetValue(owner, out var results))
            {
                if (results.TryGetValue(id, out var result))
                {
                    return result;
                }
            }

            return value;
        }
    }
}