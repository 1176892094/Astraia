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

namespace Astraia.Common
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
        private static Vector2Int skipped = Vector2Int.one * int.MinValue;
        private static Vector3Int setting = Vector3Int.one;
        private static Visible<IVisible> grids;

        public static void Rebuild(int rangeX, int rangeY, int scale)
        {
            grids = new Visible<IVisible>(scale);
            setting = new Vector3Int(rangeX, rangeY, scale);
            EventManager.Invoke(new OnVisibleUpdate(rangeX, rangeY, scale));
        }

        public static void Register(IVisible item)
        {
            grids.Add(item, item.transform.position);
            item.gameObject.SetActive(false);
        }

        public static void UnRegister(IVisible item)
        {
            grids.Remove(item);
            nodes.Remove(item);
        }

        public static void Tick(Vector2 position)
        {
            foreach (var item in nodes)
            {
                grids.Update(item, item.transform.position);
            }

            var node = grids.WorldToNode(position);
            if (node == skipped)
            {
                return;
            }

            skipped = node;
            grids.Find(node, items, setting.x, setting.y);

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
            grids.Clear();
            items.Clear();
            nodes.Clear();
            skipped = Vector2Int.one * int.MinValue;
        }

        internal static void OnGizmos()
        {
            if (grids != null)
            {
                Gizmos.color = Color.cyan;
                for (var x = -setting.x; x <= setting.x; x++)
                {
                    for (var y = -setting.y; y <= setting.y; y++)
                    {
                        var node = skipped + new Vector2Int(x, y);
                        Gizmos.DrawWireCube(new Vector2(node.x + 0.5f, node.y + 0.5f) * setting.z, Vector2.one * setting.z);
                    }
                }

                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(new Vector2(skipped.x + 0.5f, skipped.y + 0.5f) * setting.z, Vector2.one * setting.z);
            }
        }
    }

    public sealed class Visible<T>
    {
        private readonly Dictionary<Vector2Int, HashSet<T>> grids = new Dictionary<Vector2Int, HashSet<T>>();
        private readonly Dictionary<T, Vector2Int> nodes = new Dictionary<T, Vector2Int>();
        private readonly int scale;

        public Visible(int scale)
        {
            this.scale = scale;
        }

        public void Add(T item, Vector2 position)
        {
            var node = WorldToNode(position);

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
                var newNode = WorldToNode(position);
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

        public void Find(Vector2Int center, HashSet<T> items, int rangeX, int rangeY)
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

        public Vector2Int WorldToNode(Vector2 position)
        {
            return new Vector2Int(Mathf.FloorToInt(position.x / scale), Mathf.FloorToInt(position.y / scale));
        }

        public void Clear()
        {
            grids.Clear();
            nodes.Clear();
        }
    }
}