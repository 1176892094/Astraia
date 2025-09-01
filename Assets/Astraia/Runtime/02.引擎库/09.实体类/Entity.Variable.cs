// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-09-01 23:09:26
// // # Recently: 2025-09-01 23:09:26
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System.Collections.Generic;

namespace Astraia
{
    internal static class Variable<T>
    {
        private static readonly Dictionary<Entity, Dictionary<int, T>> fields = new Dictionary<Entity, Dictionary<int, T>>();

        public static void SetValue(Entity owner, int id, T value)
        {
            if (!fields.TryGetValue(owner, out var caches))
            {
                caches = new Dictionary<int, T>();
                fields[owner] = caches;
                owner.OnFade += Enqueue;

                void Enqueue()
                {
                    caches.Clear();
                    fields.Remove(owner);
                }
            }

            caches[id] = value;
        }

        public static T GetValue(Entity owner, int id)
        {
            if (fields.TryGetValue(owner, out var properties))
            {
                if (properties.TryGetValue(id, out var value))
                {
                    return value;
                }
            }

            return default;
        }
    }

    public static partial class Extensions
    {
        public static T GetValue<T>(this Entity owner, int id)
        {
            return Variable<T>.GetValue(owner, id);
        }

        public static void SetValue<T>(this Entity owner, int id, T value)
        {
            Variable<T>.SetValue(owner, id, value);
        }
    }
}