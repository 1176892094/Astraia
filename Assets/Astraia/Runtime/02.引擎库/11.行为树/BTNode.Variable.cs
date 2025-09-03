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

using System;
using System.Collections.Generic;

namespace Astraia
{
    internal static class Variable<T>
    {
        private static readonly Dictionary<Entity, Dictionary<Enum, T>> fields = new Dictionary<Entity, Dictionary<Enum, T>>();

        public static void SetValue(Entity owner, Enum id, T value)
        {
            if (!fields.TryGetValue(owner, out var values))
            {
                values = new Dictionary<Enum, T>();
                fields[owner] = values;
                owner.OnFade += Enqueue;

                void Enqueue()
                {
                    values.Clear();
                    fields.Remove(owner);
                }
            }

            values[id] = value;
        }

        public static T GetValue(Entity owner, Enum id)
        {
            if (fields.TryGetValue(owner, out var values))
            {
                if (values.TryGetValue(id, out var value))
                {
                    return value;
                }
            }

            return default;
        }
    }

    public static partial class Extensions
    {
        public static T GetValue<T>(this Entity owner, Enum id)
        {
            return Variable<T>.GetValue(owner, id);
        }

        public static void SetValue<T>(this Entity owner, Enum id, T value)
        {
            Variable<T>.SetValue(owner, id, value);
        }
    }
}