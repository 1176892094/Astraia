// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-01-20 14:01:05
// # Recently: 2026-01-20 14:01:05
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System.Collections.Generic;
using UnityEngine;

namespace Astraia.Core
{
    public interface IVisible
    {
        Transform transform { get; }
        GameObject gameObject { get; }
    }

    public sealed class Visible<T>
    {
        private readonly Dictionary<Vector2Int, HashSet<T>> grids = new Dictionary<Vector2Int, HashSet<T>>();
        private readonly Dictionary<T, Vector2Int> nodes = new Dictionary<T, Vector2Int>();
        private readonly int rangeX;
        private readonly int rangeY;
        private readonly int scaleX;
        private readonly int scaleY;

        public Visible(int rangeX, int rangeY, int scaleX, int scaleY)
        {
            this.rangeX = rangeX;
            this.rangeY = rangeY;
            this.scaleX = scaleX;
            this.scaleY = scaleY;
        }

        public void Add(T item, Vector2 position)
        {
            var node = Position(position);

            if (!grids.TryGetValue(node, out var items))
            {
                items = new HashSet<T>();
                grids.Add(node, items);
            }

            items.Add(item);
            nodes[item] = node;
        }

        public void Remove(T item)
        {
            if (nodes.TryGetValue(item, out var node))
            {
                if (grids.TryGetValue(node, out var items))
                {
                    items.Remove(item);
                }

                nodes.Remove(item);
            }
        }

        public void Update(T item, Vector2 position)
        {
            if (nodes.TryGetValue(item, out var oldNode))
            {
                var newNode = Position(position);
                if (oldNode != newNode)
                {
                    grids[oldNode].Remove(item);
                    if (!grids.TryGetValue(newNode, out var items))
                    {
                        items = new HashSet<T>();
                        grids.Add(newNode, items);
                    }

                    items.Add(item);
                    nodes[item] = newNode;
                }
            }
        }

        public void Find(Vector2Int center, HashSet<T> items)
        {
            items.Clear();
            for (var x = -rangeX; x <= rangeX; x++)
            {
                for (var y = -rangeY; y <= rangeY; y++)
                {
                    var node = center + new Vector2Int(x, y);
                    if (grids.TryGetValue(node, out var copies))
                    {
                        foreach (var item in copies)
                        {
                            items.Add(item);
                        }
                    }
                }
            }
        }

        public Vector2Int Position(Vector2 position)
        {
            return new Vector2Int(
                Mathf.FloorToInt(position.x / scaleX),
                Mathf.FloorToInt(position.y / scaleY)
            );
        }

        public void Clear()
        {
            grids.Clear();
            nodes.Clear();
        }
    }
}