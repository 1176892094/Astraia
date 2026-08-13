// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-08-13 22:08:00
// # Recently: 2026-08-13 22:45:00
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Collections;
using System.Collections.Generic;

namespace Astraia
{
    [Serializable]
    public class Blackboard<T>
    {
        private Dictionary<Type, IDictionary> properties = new();

        public void Set<TValue>(T key, TValue value)
        {
            if (!properties.TryGetValue(typeof(TValue), out var items))
            {
                items = new Dictionary<T, TValue>();
                properties.Add(typeof(TValue), items);
            }

            ((Dictionary<T, TValue>)items)[key] = value;
        }

        public TValue Get<TValue>(T key)
        {
            if (!properties.TryGetValue(typeof(TValue), out var items))
            {
                items = new Dictionary<T, TValue>();
                properties.Add(typeof(TValue), items);
            }

            return ((Dictionary<T, TValue>)items).GetValueOrDefault(key);
        }

        public void Clear()
        {
            foreach (var child in properties.Values)
            {
                child.Clear();
            }

            properties.Clear();
        }
    }
}