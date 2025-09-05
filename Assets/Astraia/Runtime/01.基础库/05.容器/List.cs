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
    public sealed class List<Key, Value> : ICollection<List<Key, Value>.Node>
    {
        private Dictionary<Key, LinkedListNode<Node>> items = new();
        private LinkedList<Node> nodes = new();
        public int Count => nodes.Count;

        public Value this[Key key]
        {
            get => items[key].Value.Value;
            set
            {
                if (items.TryGetValue(key, out var item))
                {
                    item.Value = new Node(key, value);
                }
                else
                {
                    items[key] = nodes.AddLast(new Node(key, value));
                }
            }
        }

        public void Reset(IEnumerable<Node> data)
        {
            items.Clear();
            nodes.Clear();
            AddRange(data);
        }

        public void AddRange(IEnumerable<Node> data, Predicate<Node> match = null)
        {
            foreach (var node in data)
            {
                if (match == null || match.Invoke(node))
                {
                    Add(node.Key, node.Value);
                }
            }
        }

        public void Add(Key key, Value value)
        {
            items.Add(key, nodes.AddLast(new Node(key, value)));
        }

        public void AddFirst(Key key, Value value)
        {
            items.Add(key, nodes.AddFirst(new Node(key, value)));
        }

        public void Insert(Key origin, Key key, Value value)
        {
            if (!items.TryGetValue(origin, out var target))
            {
                throw new KeyNotFoundException(nameof(origin));
            }

            items.Add(key, nodes.AddBefore(target, new Node(key, value)));
        }

        public bool Remove(Key key)
        {
            if (items.Remove(key, out var node))
            {
                nodes.Remove(node);
                return true;
            }

            return false;
        }

        public int Remove(Predicate<Node> match)
        {
            var count = 0;
            var node = nodes.First;
            while (node != null)
            {
                var next = node.Next;
                if (match.Invoke(node.Value))
                {
                    items.Remove(node.Value.Key);
                    nodes.Remove(node);
                    count++;
                }

                node = next;
            }

            return count;
        }

        public bool ContainsKey(Key key)
        {
            return items.ContainsKey(key);
        }

        public bool TryGetValue(Key key, out Value value)
        {
            if (items.TryGetValue(key, out var node))
            {
                value = node.Value.Value;
                return true;
            }

            value = default;
            return false;
        }

        public void Clear()
        {
            items.Clear();
            nodes.Clear();
        }

        public IEnumerable<Key> Keys
        {
            get
            {
                var node = nodes.First;
                while (node != null)
                {
                    var next = node.Next;
                    yield return node.Value.Key;
                    node = next;
                }
            }
        }

        public IEnumerable<Value> Values
        {
            get
            {
                var node = nodes.First;
                while (node != null)
                {
                    var next = node.Next;
                    yield return node.Value.Value;
                    node = next;
                }
            }
        }

        public IEnumerator<Node> GetEnumerator()
        {
            var node = nodes.First;
            while (node != null)
            {
                var next = node.Next;
                yield return node.Value;
                node = next;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        void ICollection<Node>.Add(Node node)
        {
            Add(node.Key, node.Value);
        }

        bool ICollection<Node>.Remove(Node node)
        {
            return Remove(node.Key);
        }

        bool ICollection<Node>.Contains(Node node)
        {
            return ContainsKey(node.Key);
        }

        void ICollection<Node>.CopyTo(Node[] array, int index)
        {
            nodes.CopyTo(array, index);
        }

        bool ICollection<Node>.IsReadOnly => false;

        [Serializable]
        public struct Node
        {
            public Key Key;
            public Value Value;

            public Node(Key key, Value value)
            {
                Key = key;
                Value = value;
            }
        }
    }
}