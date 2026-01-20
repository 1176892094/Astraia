// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2026-01-17 20:01:51
// // # Recently: 2026-01-17 20:01:51
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System.Collections.Generic;
using UnityEngine;

namespace Astraia.Core
{
    public interface IVisible
    {
        Transform transform { get; }
        GameObject gameObject { get; }
    }

    public static class VisibleManager
    {
        private static readonly HashSet<IVisible> items = new HashSet<IVisible>();
        private static readonly HashSet<IVisible> nodes = new HashSet<IVisible>();
        private static Vector2Int point = Vector2Int.zero;
        private static Vector3Int range = Vector3Int.one;
        private static Visible<IVisible> grids;

        public static void Rebuild(int x, int y, int z)
        {
            range = new Vector3Int(x, y, z);
            grids = new Visible<IVisible>(x, y, z);
            EventManager.Invoke(new OnVisibleUpdate(x, y, z));
        }

        public static void Register(IVisible item)
        {
            grids.Add(item, item.transform.position);
            item.gameObject.SetActive(IsVisible(item));
        }

        public static void UnRegister(IVisible item)
        {
            grids.Remove(item);
            nodes.Remove(item);
        }

        private static bool IsVisible(IVisible item)
        {
            var p = grids.Position(item.transform.position) - point;
            return Mathf.Abs(p.x) <= range.x && Mathf.Abs(p.y) <= range.y;
        }

        public static void Tick(Vector2 position)
        {
            foreach (var item in nodes)
            {
                grids.Update(item, item.transform.position);
            }

            var node = grids.Position(position);
            if (node == point)
            {
                return;
            }

            point = node;
            grids.Find(node, items);

            foreach (var item in items)
            {
                if (!nodes.Contains(item))
                {
                    item.gameObject.SetActive(true);
                }
            }

            foreach (var item in nodes)
            {
                if (!items.Contains(item))
                {
                    item.gameObject.SetActive(false);
                }
            }

            nodes.Clear();
            foreach (var item in items)
            {
                nodes.Add(item);
            }
        }

        internal static void Dispose()
        {
            items.Clear();
            nodes.Clear();
            grids?.Clear();
            point = Vector2Int.zero;
        }

        internal static void OnGizmos()
        {
            if (grids != null)
            {
                Gizmos.color = Color.cyan;
                for (var x = -range.x; x <= range.x; x++)
                {
                    for (var y = -range.y; y <= range.y; y++)
                    {
                        var node = point + new Vector2Int(x, y);
                        Gizmos.DrawWireCube(new Vector2(node.x + 0.5f, node.y + 0.5f) * range.z, Vector2.one * range.z);
                    }
                }

                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(new Vector2(point.x + 0.5f, point.y + 0.5f) * range.z, Vector2.one * range.z);
            }
        }
    }

    public sealed class Visible<T>
    {
        private readonly Dictionary<Vector2Int, HashSet<T>> grids = new Dictionary<Vector2Int, HashSet<T>>();
        private readonly Dictionary<T, Vector2Int> nodes = new Dictionary<T, Vector2Int>();
        private readonly Vector3Int range;

        public Visible(int rangeX, int rangeY, int scale)
        {
            range = new Vector3Int(rangeX, rangeY, scale);
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
            for (var x = -range.x; x <= range.x; x++)
            {
                for (var y = -range.y; y <= range.y; y++)
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
            return new Vector2Int(Mathf.FloorToInt(position.x / range.z), Mathf.FloorToInt(position.y / range.z));
        }

        public void Clear()
        {
            grids.Clear();
            nodes.Clear();
        }
    }
}