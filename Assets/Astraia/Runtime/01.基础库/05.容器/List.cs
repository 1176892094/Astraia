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
    public sealed partial class List<Key, Value> : ICollection<List<Key, Value>.Node>
    {
        private Dictionary<Key, LinkedListNode<Node>> items = new Dictionary<Key, LinkedListNode<Node>>();
        private LinkedList<Node> nodes = new LinkedList<Node>();
        public int Count => nodes.Count;

        public Value this[Key key]
        {
            get => TryGetValue(key, out var value) ? value : throw new KeyNotFoundException(nameof(key));
            set => Add(key, value);
        }

        public void Add(Key key, Value value)
        {
            if (items.TryGetValue(key, out var item))
            {
                var node = item.Value;
                node.Value = value;
                item.Value = node;
            }
            else
            {
                items[key] = nodes.AddLast(new Node(key, value));
            }
        }

        public void AddFirst(Key key, Value value)
        {
            if (items.TryGetValue(key, out var item))
            {
                var node = item.Value;
                node.Value = value;
                item.Value = node;
            }
            else
            {
                items[key] = nodes.AddFirst(new Node(key, value));
            }
        }

        public void AddRange(IEnumerable<Node> items, Predicate<Node> match = null)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            foreach (var node in items)
            {
                if (match == null || match(node))
                {
                    Add(node.Key, node.Value);
                }
            }
        }

        public void Insert(Key origin, Key key, Value value)
        {
            if (!items.TryGetValue(origin, out var target))
            {
                throw new KeyNotFoundException(nameof(origin));
            }

            if (items.TryGetValue(key, out var item))
            {
                var node = item.Value;
                node.Value = value;
                item.Value = node;
            }
            else
            {
                items[key] = nodes.AddBefore(target, new Node(key, value));
            }
        }

        public bool Remove(Key key)
        {
            if (items.TryGetValue(key, out var node))
            {
                nodes.Remove(node);
                items.Remove(key);
                return true;
            }

            return false;
        }

        public int Remove(Predicate<Node> match)
        {
            if (match == null)
            {
                throw new ArgumentNullException(nameof(match));
            }

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

        public void Reset(IList<Node> data)
        {
            Clear();
            foreach (var item in data)
            {
                Add(item.Key, item.Value);
            }
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

        bool ICollection<Node>.IsReadOnly => false;
        void ICollection<Node>.Add(Node node) => Add(node.Key, node.Value);
        bool ICollection<Node>.Remove(Node node) => Remove(node.Key);
        bool ICollection<Node>.Contains(Node node) => nodes.Contains(node);
        void ICollection<Node>.CopyTo(Node[] array, int index) => nodes.CopyTo(array, index);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}