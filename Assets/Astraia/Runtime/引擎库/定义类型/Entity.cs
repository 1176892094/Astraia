using System;
using System.Collections.Generic;
using UnityEngine;

namespace Astraia
{
    [Serializable]
    public class Entity : MonoBehaviour
    {
        [SerializeReference] public List<IModule> moduleList = new List<IModule>();

        protected virtual void Awake()
        {
            foreach (var module in moduleList)
            {
                module.Acquire(this);
            }

            foreach (var module in moduleList)
            {
                module.Dequeue();
            }
        }

        protected virtual void OnEnable()
        {
            foreach (var module in moduleList)
            {
                module.OnShow();
            }
        }

        protected virtual void OnDisable()
        {
            foreach (var module in moduleList)
            {
                module.OnHide();
            }
        }

        protected virtual void OnDestroy()
        {
            foreach (var module in moduleList)
            {
                module.Enqueue();
            }

            moduleList.Clear();
        }

        public T AddComponent<T>() where T : IModule
        {
            var module = Activator.CreateInstance<T>();
            moduleList.Add(module);
            module.Acquire(this);
            return module;
        }

        public T AddComponent<T>(Type item) where T : IModule
        {
            var module = (T)Activator.CreateInstance(item);
            moduleList.Add(module);
            module.Acquire(this);
            return module;
        }

        public T GetComponent<T>(int index) where T : IModule
        {
            return index < moduleList.Count ? (T)moduleList[index] : default;
        }
    }

    public abstract class Singleton<T> : Module<Entity>, IModule where T : Singleton<T>
    {
        public static T Instance;

        void IModule.Acquire(object owner)
        {
            Instance = (T)this;
            this.owner = (Entity)owner;
        }
    }

    public class SpatialHash<T>
    {
        private readonly Dictionary<ulong, HashSet<T>> buckets = new Dictionary<ulong, HashSet<T>>();
        private readonly Dictionary<T, ulong> objects = new Dictionary<T, ulong>();
        private readonly float cellSize;

        public SpatialHash(float cellSize)
        {
            this.cellSize = cellSize;
        }

        public void Insert(T item, Vector2 position)
        {
            var node = Hash(WorldToNode(position));
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

        public void Update(T item, Vector2 position)
        {
            if (objects.TryGetValue(item, out var oldNode))
            {
                var newNode = Hash(WorldToNode(position));
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

        public void Query(Vector2 center, Vector2Int extents, HashSet<T> items)
        {
            items.Clear();
            var pos = WorldToNode(center);
            var min = pos - extents;
            var max = pos + extents;

            for (var x = min.x; x <= max.x; x++)
            {
                for (var y = min.y; y <= max.y; y++)
                {
                    var node = Hash(new Vector2Int(x, y));
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

        public Vector2Int WorldToNode(Vector2 position)
        {
            var x = Mathf.FloorToInt(position.x / cellSize);
            var y = Mathf.FloorToInt(position.y / cellSize);
            return new Vector2Int(x, y);
        }

        public void Clear()
        {
            buckets.Clear();
            objects.Clear();
        }

        private static ulong Hash(Vector2Int grid)
        {
            return ((ulong)grid.x << 32) ^ (uint)grid.y;
        }
    }
}