// // *********************************************************************************
// // # Project: JFramework
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-05-01 17:05:24
// // # Recently: 2025-05-01 17:05:24
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using System.Collections.Generic;

namespace Astraia
{
    [Serializable]
    public sealed class BiDictionary<TKey, TValue>
    {
        private readonly IDictionary<TKey, TValue> forward = new Dictionary<TKey, TValue>();
        private readonly IDictionary<TValue, TKey> reverse = new Dictionary<TValue, TKey>();

        public IEnumerable<TKey> Keys => forward.Keys;
        public IEnumerable<TValue> Values => forward.Values;
        public int Count => forward.Count;

        public void Add(TKey key, TValue value)
        {
            forward.Add(key, value);
            reverse.Add(value, key);
        }

        public bool TryGetByKey(TKey key, out TValue value)
        {
            return forward.TryGetValue(key, out value);
        }

        public bool TryGetByValue(TValue value, out TKey key)
        {
            return reverse.TryGetValue(value, out key);
        }

        public bool RemoveByKey(TKey key)
        {
            if (forward.Remove(key, out var value))
            {
                reverse.Remove(value);
                return true;
            }

            return false;
        }

        public bool RemoveByValue(TValue value)
        {
            if (reverse.Remove(value, out var key))
            {
                forward.Remove(key);
                return true;
            }

            return false;
        }
        
        public void Clear()
        {
            forward.Clear();
            reverse.Clear();
        }
    }
}