// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-08-26 04:08:34
// // # Recently: 2025-08-26 04:08:34
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using System.Collections;
using System.Collections.Generic;

namespace Astraia
{
    [Serializable]
    public sealed partial class List<Key, Value> : IList<List<Key, Value>.Node>
    {
        private readonly IDictionary<Key, int> keyIndex = new Dictionary<Key, int>();
        private readonly IDictionary<Value, int> valueIndex = new Dictionary<Value, int>();
        
        public List<Node> Nodes = new List<Node>();
        public bool Sorted;
        public int Count => Nodes.Count;

        public List(bool sorted)
        {
            Sorted = sorted;
        }

        public Value this[Key key]
        {
            get => TryGetValue(key, out var value) ? value : throw new KeyNotFoundException();
            set => Add(key, value);
        }

        public void Add(Key key, Value value)
        {
            if (keyIndex.TryGetValue(key, out var index))
            {
                var replace = Nodes[index].Value;
                Nodes[index] = new Node(key, value);
                valueIndex.Remove(replace);
                valueIndex[value] = index;
                return;
            }

            index = Nodes.Count;
            Nodes.Add(new Node(key, value));
            keyIndex[key] = index;
            valueIndex[value] = index;
        }

        public void AddRange(IEnumerable<Node> nodes, Predicate<Value> match = null)
        {
            if (nodes == null)
            {
                throw new ArgumentNullException(nameof(nodes));
            }

            foreach (var node in nodes)
            {
                if (match == null || match(node.Value))
                {
                    Add(node.Key, node.Value);
                }
            }
        }

        public void Insert(Key key, Value value, int index)
        {
            if (keyIndex.ContainsKey(key) || valueIndex.ContainsKey(value))
            {
                throw new ArgumentException("Key or Value already exists");
            }

            Nodes.Insert(index, new Node(key, value));
            Rebuild(index);
        }

        public bool ContainsKey(Key key)
        {
            return keyIndex.ContainsKey(key);
        }

        public bool ContainsValue(Value value)
        {
            return valueIndex.ContainsKey(value);
        }

        public bool TryGetKey(Value value, out Key key)
        {
            if (valueIndex.TryGetValue(value, out var index))
            {
                key = Nodes[index].Key;
                return true;
            }

            key = default;
            return false;
        }

        public bool TryGetValue(Key key, out Value value)
        {
            if (keyIndex.TryGetValue(key, out var index))
            {
                value = Nodes[index].Value;
                return true;
            }

            value = default;
            return false;
        }

        public bool RemoveByKey(Key key)
        {
            if (keyIndex.TryGetValue(key, out var index))
            {
                RemoveAt(index);
                return true;
            }

            return false;
        }

        public bool RemoveByValue(Value value)
        {
            if (valueIndex.TryGetValue(value, out var index))
            {
                RemoveAt(index);
                return true;
            }

            return false;
        }

        public int RemoveByKey(Predicate<Key> match)
        {
            var count = 0;
            if (Sorted)
            {
                var shift = 0;
                for (var i = 0; i < Nodes.Count; i++)
                {
                    if (match(Nodes[i].Key))
                    {
                        Dispose(i);
                        count++;
                        shift++;
                    }
                    else if (shift > 0)
                    {
                        Nodes[i - shift] = Nodes[i];
                        keyIndex[Nodes[i].Key] = i - shift;
                        valueIndex[Nodes[i].Value] = i - shift;
                    }
                }

                Remove(count);
            }
            else
            {
                for (var i = Nodes.Count - 1; i >= 0; i--)
                {
                    if (match(Nodes[i].Key))
                    {
                        Release(i);
                        count++;
                    }
                }
            }

            return count;
        }

        public int RemoveByValue(Predicate<Value> match)
        {
            var count = 0;
            if (Sorted)
            {
                var shift = 0;
                for (var i = 0; i < Nodes.Count; i++)
                {
                    if (match(Nodes[i].Value))
                    {
                        Dispose(i);
                        count++;
                        shift++;
                    }
                    else if (shift > 0)
                    {
                        Nodes[i - shift] = Nodes[i];
                        keyIndex[Nodes[i].Key] = i - shift;
                        valueIndex[Nodes[i].Value] = i - shift;
                    }
                }

                Remove(count);
            }
            else
            {
                for (var i = Nodes.Count - 1; i >= 0; i--)
                {
                    if (match(Nodes[i].Value))
                    {
                        Release(i);
                        count++;
                    }
                }
            }

            return count;
        }

        public void SortByKey(IComparer<Key> comparer = null)
        {
            Nodes.Sort((a, b) => (comparer ?? Comparer<Key>.Default).Compare(a.Key, b.Key));
            keyIndex.Clear();
            valueIndex.Clear();
            Rebuild();
        }

        public void SortByValue(IComparer<Value> comparer = null)
        {
            Nodes.Sort((a, b) => (comparer ?? Comparer<Value>.Default).Compare(a.Value, b.Value));
            keyIndex.Clear();
            valueIndex.Clear();
            Rebuild();
        }

        public void Rebuild(int index = 0)
        {
            for (var i = index; i < Nodes.Count; i++)
            {
                var node = Nodes[i];
                keyIndex[node.Key] = i;
                valueIndex[node.Value] = i;
            }
        }

        public void RemoveAt(int index)
        {
            if (Sorted)
            {
                Dispose(index);
                Rebuild(index);
                return;
            }

            Release(index);
        }

        private void Remove(int count)
        {
            var index = Nodes.Count - count;
            for (var i = index; i < Nodes.Count; i++)
            {
                var node = Nodes[i];
                keyIndex.Remove(node.Key);
                valueIndex.Remove(node.Value);
            }

            Nodes.RemoveRange(index, count);
        }

        private void Release(int index)
        {
            var maxIndex = Nodes.Count - 1;
            if (index != maxIndex)
            {
                var node = Nodes[maxIndex];
                Nodes[index] = node;
                keyIndex[node.Key] = index;
                valueIndex[node.Value] = index;
            }

            Dispose(maxIndex);
            Nodes.RemoveAt(maxIndex);
        }

        private void Dispose(int index)
        {
            var node = Nodes[index];
            keyIndex.Remove(node.Key);
            valueIndex.Remove(node.Value);
        }

        public void Clear()
        {
            Nodes.Clear();
            keyIndex.Clear();
            valueIndex.Clear();
        }

        public IEnumerable<Key> Keys
        {
            get
            {
                for (var i = 0; i < Nodes.Count; i++)
                {
                    yield return Nodes[i].Key;
                }
            }
        }

        public IEnumerable<Value> Values
        {
            get
            {
                for (var i = 0; i < Nodes.Count; i++)
                {
                    yield return Nodes[i].Value;
                }
            }
        }

        public IEnumerator<Node> GetEnumerator()
        {
            for (var i = Nodes.Count - 1; i >= 0; i--)
            {
                yield return Nodes[i];
            }
        }

        bool ICollection<Node>.IsReadOnly => false;
        void ICollection<Node>.Add(Node node) => Add(node.Key, node.Value);
        bool ICollection<Node>.Contains(Node node) => Nodes.Contains(node);
        void ICollection<Node>.CopyTo(Node[] array, int index) => Nodes.CopyTo(array, index);
        bool ICollection<Node>.Remove(Node node) => RemoveByKey(node.Key);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        int IList<Node>.IndexOf(Node node) => Nodes.IndexOf(node);
        void IList<Node>.Insert(int index, Node node) => Insert(node.Key, node.Value, index);

        Node IList<Node>.this[int index]
        {
            get => Nodes[index];
            set => Nodes[index] = value;
        }
    }
}