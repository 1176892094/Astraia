using System;
using System.Collections.Generic;
using UnityEngine;

namespace Astraia
{
    [Serializable]
    public class VisibleManager : Singleton<VisibleManager>, IEvent<OnAfterUpdate>, IEvent<OnGizmoUpdate>
    {
        private readonly SpatialHash<IVisible> visibles = new SpatialHash<IVisible>();
        private readonly HashSet<IVisible> forwards = new HashSet<IVisible>();
        private readonly HashSet<IVisible> previous = new HashSet<IVisible>();
        private readonly List<IVisible> dynamics = new List<IVisible>();

        private Vector2 position;
        private Transform observer;

        [SerializeField] private int extentX = 1;
        [SerializeField] private int extentY = 1;
        [SerializeField] private int cellSize = 1;

        protected override void Enqueue()
        {
            forwards.Clear();
            previous.Clear();
            dynamics.Clear();
            visibles.Clear();
        }

        protected override void OnShow()
        {
            EventManager.Listen<OnAfterUpdate>(this);
            EventManager.Listen<OnGizmoUpdate>(this);
        }

        protected override void OnHide()
        {
            EventManager.Remove<OnAfterUpdate>(this);
            EventManager.Remove<OnGizmoUpdate>(this);
        }

        public void Execute(OnAfterUpdate message)
        {
            if (observer)
            {
                foreach (var dynamic in dynamics)
                {
                    visibles.Update(dynamic, WorldToNode(dynamic.transform.position));
                }

                position = observer.position;
                visibles.Query(WorldToNode(position), extentX, extentY, forwards);

                foreach (var visible in forwards)
                {
                    if (previous.Add(visible))
                    {
                        SetActive(visible, true);
                    }
                }

                foreach (var visible in previous)
                {
                    if (!forwards.Contains(visible))
                    {
                        SetActive(visible, false);
                    }
                }

                previous.Clear();
                foreach (var visible in forwards)
                {
                    previous.Add(visible);
                }
            }
        }

        public void Execute(OnGizmoUpdate message)
        {
            Gizmos.color = Color.cyan;
            if (observer)
            {
                var center = WorldToNode(observer.transform.position);
                var minX = center.X - extentX;
                var maxX = center.X + extentX;
                var minY = center.Y - extentY;
                var maxY = center.Y + extentY;

                for (var x = minX; x <= maxX; x++)
                {
                    for (var y = minY; y <= maxY; y++)
                    {
                        Gizmos.DrawWireCube(new Vector2(x + 0.5F, y + 0.5F) * cellSize, Vector2.one * cellSize);
                    }
                }
            }
        }

        public void Register(IVisible visible)
        {
            if (visible.IsCulling)
            {
                return;
            }

            if (visible.IsDynamic)
            {
                dynamics.Add(visible);
            }

            visibles.Insert(visible, WorldToNode(visible.transform.position));
            SetActive(visible, IsVisible(visible));
        }

        public void UnRegister(IVisible visible)
        {
            if (visible.IsCulling)
            {
                return;
            }

            if (visible.IsDynamic)
            {
                dynamics.Remove(visible);
            }

            visibles.Remove(visible);
            previous.Remove(visible);
        }

        public void SetObserver(Transform observer)
        {
            this.observer = observer;
        }

        public void SetPosition(Vector2 position)
        {
            this.position = position;
        }

        public void SetPosition(IVisible visible)
        {
            visibles.Update(visible, WorldToNode(visible.transform.position));
            SetActive(visible, false);
        }

        private bool IsVisible(IVisible visible)
        {
            var node = WorldToNode(visible.transform.position) - WorldToNode(position);
            return Mathf.Abs(node.X) <= extentX && Mathf.Abs(node.Y) <= extentY;
        }

        private static void SetActive(IVisible visible, bool enabled)
        {
            foreach (var render in visible.transform.GetComponentsInChildren<Renderer>())
            {
                render.enabled = enabled;
            }

            foreach (var render in visible.transform.GetComponentsInChildren<Animator>())
            {
                render.enabled = enabled;
            }

            visible.enabled = enabled;
        }

        private Position WorldToNode(Vector2 position)
        {
            var x = Mathf.FloorToInt(position.x / cellSize);
            var y = Mathf.FloorToInt(position.y / cellSize);
            return new Position(x, y);
        }
    }

    public interface IVisible
    {
        bool enabled { set; }
        bool IsDynamic { get; }
        bool IsCulling { get; }
        Transform transform { get; }
    }

    public enum Visible
    {
        Static,
        Dynamic,
        Culling,
    }
}