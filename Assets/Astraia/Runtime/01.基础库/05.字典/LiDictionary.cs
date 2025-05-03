// // *********************************************************************************
// // # Project: JFramework
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-05-01 17:05:33
// // # Recently: 2025-05-01 17:05:33
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using System.Collections.Generic;

namespace Astraia
{
    [Serializable]
    public sealed class LiDictionary<TKey, TValue>
    {
        private readonly IDictionary<TKey, TValue> forward = new Dictionary<TKey, TValue>();
        private readonly IList<TValue> reverse = new List<TValue>();

        public IEnumerable<TKey> Keys => forward.Keys;
        public IList<TValue> Values => reverse;
        public int Count => forward.Count;

        public TValue this[TKey key]
        {
            get => forward[key];
            set => forward[key] = value;
        }

        public void Add(TKey key, TValue value)
        {
            reverse.Add(value);
            forward.Add(key, value);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return forward.TryGetValue(key, out value);
        }

        public bool Remove(TKey key)
        {
            if (forward.Remove(key, out var value))
            {
                reverse.Remove(value);
                return true;
            }

            return false;
        }

        public bool Contains(TKey key)
        {
            return forward.ContainsKey(key);
        }

        public void Clear()
        {
            reverse.Clear();
            forward.Clear();
        }
    }
}