// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-08-13 22:08:19
// # Recently: 2026-08-13 22:44:19
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Collections.Generic;

namespace Astraia
{
    [Serializable]
    public sealed class SpatialHash<T>
    {
        private readonly Dictionary<int, HashSet<T>> buckets = new();
        private readonly Dictionary<T, int> objects = new();

        public void Insert(T item, Position center)
        {
            var node = center.GetHashCode();
            if (!buckets.TryGetValue(node, out var items))
            {
                items = new HashSet<T>();
                buckets.Add(node, items);
            }

            items.Add(item);
            objects[item] = node;
        }

        public void Remove(T item)
        {
            if (objects.TryGetValue(item, out var node))
            {
                if (buckets.TryGetValue(node, out var items))
                {
                    items.Remove(item);
                    if (items.Count == 0)
                    {
                        buckets.Remove(node);
                    }
                }

                objects.Remove(item);
            }
        }

        public void Update(T item, Position center)
        {
            if (objects.TryGetValue(item, out var oldNode))
            {
                var newNode = center.GetHashCode();
                if (oldNode != newNode)
                {
                    if (buckets.TryGetValue(oldNode, out var oldItems))
                    {
                        oldItems.Remove(item);
                        if (oldItems.Count == 0)
                        {
                            buckets.Remove(oldNode);
                        }
                    }

                    if (!buckets.TryGetValue(newNode, out var newItems))
                    {
                        newItems = new HashSet<T>();
                        buckets.Add(newNode, newItems);
                    }

                    newItems.Add(item);
                    objects[item] = newNode;
                }
            }
        }

        public void Query(Position center, int extentX, int extentY, HashSet<T> items)
        {
            items.Clear();
            var minX = center.x.FloorToInt() - extentX;
            var maxX = center.x.FloorToInt() + extentX;
            var minY = center.y.FloorToInt() - extentY;
            var maxY = center.y.FloorToInt() + extentY;

            for (var x = minX; x <= maxX; x++)
            {
                for (var y = minY; y <= maxY; y++)
                {
                    var node = new Position(x, y).GetHashCode();
                    if (buckets.TryGetValue(node, out var copies))
                    {
                        foreach (var item in copies)
                        {
                            items.Add(item);
                        }
                    }
                }
            }
        }

        public void Clear()
        {
            foreach (var bucket in buckets.Values)
            {
                bucket.Clear();
            }

            buckets.Clear();
            objects.Clear();
        }
    }
}