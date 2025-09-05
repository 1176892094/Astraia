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
        private static readonly Dictionary<Entity, Dictionary<Enum, T>> entities = new Dictionary<Entity, Dictionary<Enum, T>>();

        public static void SetValue(Entity owner, Enum id, T value)
        {
            if (owner.isDestroy) return;
            if (!entities.TryGetValue(owner, out var fields))
            {
                fields = new Dictionary<Enum, T>();
                entities[owner] = fields;
                owner.OnFade += Enqueue;

                void Enqueue()
                {
                    fields.Clear();
                    entities.Remove(owner);
                }
            }

            fields[id] = value;
        }

        public static T GetValue(Entity owner, Enum id, T value)
        {
            if (entities.TryGetValue(owner, out var fields))
            {
                if (fields.TryGetValue(id, out var field))
                {
                    return field;
                }
            }

            return value;
        }
    }

    public static partial class Extensions
    {
        public static T GetValue<T>(this Entity owner, Enum id, T value = default)
        {
            return owner ? Variable<T>.GetValue(owner, id, value) : default;
        }

        public static void SetValue<T>(this Entity owner, Enum id, T value)
        {
            Variable<T>.SetValue(owner, id, value);
        }
    }
}