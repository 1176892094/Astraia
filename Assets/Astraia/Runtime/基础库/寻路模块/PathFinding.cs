using System;
using System.Collections.Generic;

namespace Astraia
{
    [Serializable]
    public struct Position
    {
        public int x;
        public int y;

        public Position(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public static Position operator +(Position a, Position b)
        {
            return new Position(a.x + b.x, a.y + b.y);
        }

        public static Position operator -(Position a, Position b)
        {
            return new Position(a.x - b.x, a.y - b.y);
        }

        public ulong Hash => ((ulong)x << 32) ^ (ulong)y;
    }

    [Serializable]
    public sealed class PriorityQueue<T> where T : IComparable<T>
    {
        private List<T> heap = new List<T>();

        public int Count => heap.Count;

        public T this[int index]
        {
            get => heap[index];
            set => heap[index] = value;
        }

        public void Enqueue(T item)
        {
            heap.Add(item);

            var index = heap.Count - 1;

            while (index > 0)
            {
                var parent = (index - 1) >> 1;

                if (heap[parent].CompareTo(item) <= 0)
                {
                    break;
                }

                heap[index] = heap[parent];
                index = parent;
            }

            heap[index] = item;
        }

        public T Dequeue()
        {
            var result = heap[0];

            var last = heap.Count - 1;

            var node = heap[last];
            heap.RemoveAt(last);

            if (heap.Count > 0)
            {
                var index = 0;

                while (true)
                {
                    var left = index * 2 + 1;

                    if (left >= heap.Count)
                    {
                        break;
                    }

                    var right = left + 1;

                    var child = left;

                    if (right < heap.Count && heap[right].CompareTo(heap[left]) < 0)
                    {
                        child = right;
                    }

                    if (heap[child].CompareTo(node) >= 0)
                    {
                        break;
                    }

                    heap[index] = heap[child];
                    index = child;
                }

                heap[index] = node;
            }

            return result;
        }

        public T Peek()
        {
            return heap[0];
        }

        public void Clear()
        {
            heap.Clear();
        }
    }

    [Serializable]
    public sealed class SpatialHash<T>
    {
        private readonly Dictionary<ulong, HashSet<T>> buckets = new Dictionary<ulong, HashSet<T>>();
        private readonly Dictionary<T, ulong> objects = new Dictionary<T, ulong>();

        public void Insert(T item, Position center)
        {
            var node = center.Hash;
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
                var newNode = center.Hash;
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
            var minX = center.x - extentX;
            var maxX = center.x + extentX;
            var minY = center.y - extentY;
            var maxY = center.y + extentY;

            for (var x = minX; x <= maxX; x++)
            {
                for (var y = minY; y <= maxY; y++)
                {
                    var node = new Position(x, y).Hash;
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

    [Serializable]
    public abstract class PathFinding
    {
        protected const int INF = int.MaxValue;

        protected int width;
        protected int height;

        protected static readonly Grid[] Neighbors;

        static PathFinding()
        {
            Neighbors = new Grid[8];

            Neighbors[0] = new Grid(0, 1, 10);
            Neighbors[1] = new Grid(1, 1, 14);
            Neighbors[2] = new Grid(-1, 1, 14);

            Neighbors[3] = new Grid(0, -1, 10);
            Neighbors[4] = new Grid(1, -1, 14);
            Neighbors[5] = new Grid(-1, -1, 14);

            Neighbors[6] = new Grid(1, 0, 10);
            Neighbors[7] = new Grid(-1, 0, 10);
        }

        protected PathFinding(int width, int height)
        {
            this.width = width;
            this.height = height;
        }

        protected int Index(int x, int y)
        {
            return y * width + x;
        }

        protected bool Contains(int x, int y)
        {
            return x >= 0 && y >= 0 && x < width && y < height;
        }

        protected readonly struct Grid
        {
            public readonly int x;
            public readonly int y;
            public readonly int cost;

            public Grid(int x, int y, int cost)
            {
                this.x = x;
                this.y = y;
                this.cost = cost;
            }
        }

        protected sealed class MinHeap
        {
            private readonly List<int> heap = new();
            private readonly int[] cost;

            public MinHeap(int[] cost)
            {
                this.cost = cost;
            }

            public int Count => heap.Count;

            public void Clear()
            {
                heap.Clear();
            }

            public void Enqueue(int index)
            {
                heap.Add(index);

                var i = heap.Count - 1;

                while (i > 0)
                {
                    var parent = (i - 1) >> 1;

                    if (cost[heap[parent]] <= cost[heap[i]])
                    {
                        break;
                    }

                    (heap[parent], heap[i]) = (heap[i], heap[parent]);
                    i = parent;
                }
            }

            public int Dequeue()
            {
                var root = heap[0];

                var last = heap[^1];
                heap.RemoveAt(heap.Count - 1);

                if (heap.Count == 0)
                {
                    return root;
                }

                heap[0] = last;

                var i = 0;

                while (true)
                {
                    var left = i * 2 + 1;

                    if (left >= heap.Count)
                    {
                        break;
                    }

                    var right = left + 1;

                    var smallest = left;

                    if (right < heap.Count && cost[heap[right]] < cost[heap[left]])
                    {
                        smallest = right;
                    }

                    if (cost[heap[i]] <= cost[heap[smallest]])
                    {
                        break;
                    }

                    (heap[i], heap[smallest]) = (heap[smallest], heap[i]);
                    i = smallest;
                }

                return root;
            }
        }
    }

    [Serializable]
    public class AStar : PathFinding
    {
        private int[] parent;
        private int[] gScore;
        private int[] fScore;
        private bool[] closed;
        private bool[] walkable;

        private MinHeap opened;
        private List<Position> copied = new();

        public AStar(int width, int height, bool[] map) : base(width, height)
        {
            walkable = map;
            parent = new int[width * height];
            gScore = new int[width * height];
            fScore = new int[width * height];
            closed = new bool[width * height];
            opened = new MinHeap(fScore);
        }

        public IList<Position> Rebuild(int sx, int sy, int ex, int ey)
        {
            for (var i = 0; i < gScore.Length; i++)
            {
                parent[i] = -1;
                gScore[i] = INF;
                fScore[i] = INF;
                closed[i] = false;
            }

            var s = Index(sx, sy);
            var e = Index(ex, ey);
            gScore[s] = 0;
            fScore[s] = Heuristic(s, e);

            opened.Clear();
            opened.Enqueue(s);

            while (opened.Count > 0)
            {
                var i = opened.Dequeue();

                if (closed[i])
                {
                    continue;
                }

                closed[i] = true;

                if (i == e)
                {
                    return Reconstruct(e);
                }

                var cx = i % width;
                var cy = i / width;

                foreach (var n in Neighbors)
                {
                    var nx = cx + n.x;
                    var ny = cy + n.y;

                    if (Contains(nx, ny))
                    {
                        var j = Index(nx, ny);

                        if (!walkable[j] || closed[j])
                        {
                            continue;
                        }

                        if (n.cost == 14)
                        {
                            var wx = Index(cx, ny);
                            var wy = Index(nx, cy);

                            if (!walkable[wx] || !walkable[wy])
                            {
                                continue;
                            }
                        }

                        var gCost = gScore[i] + n.cost;
                        if (gCost < gScore[j])
                        {
                            parent[j] = i;
                            gScore[j] = gCost;
                            fScore[j] = gCost + Heuristic(j, e);
                            opened.Enqueue(j);
                        }
                    }
                }
            }

            return Array.Empty<Position>();
        }

        private int Heuristic(int a, int b)
        {
            var sx = a % width;
            var sy = a / width;

            var ex = b % width;
            var ey = b / width;

            var nx = Math.Abs(sx - ex);
            var ny = Math.Abs(sy - ey);

            var min = Math.Min(nx, ny);
            var max = Math.Max(nx, ny);

            return 14 * min + 10 * (max - min);
        }

        private List<Position> Reconstruct(int e)
        {
            copied.Clear();

            while (e != -1)
            {
                copied.Add(new Position(e % width, e / width));
                e = parent[e];
            }

            copied.Reverse();
            return copied;
        }

        public void Dispose()
        {
            opened.Clear();
            copied.Clear();
            opened = null;
            copied = null;
            parent = null;
            fScore = null;
            gScore = null;
            closed = null;
            walkable = null;
        }
    }

    [Serializable]
    public class FlowField : PathFinding
    {
        private int[] nodes;
        private int[] costs;
        private int[] steps;
        private MinHeap opened;

        public FlowField(int width, int height, bool[] walkable) : base(width, height)
        {
            costs = new int[width * height];
            nodes = new int[width * height];
            steps = new int[width * height];
            opened = new MinHeap(steps);
            for (var i = 0; i < costs.Length; i++)
            {
                costs[i] = walkable[i] ? 1 : INF;
            }
        }

        public void SetCost(int x, int y, int cost)
        {
            if (Contains(x, y))
            {
                costs[Index(x, y)] = Math.Max(1, cost);
            }
        }

        public void SetObstacle(int x, int y, bool walkable)
        {
            if (Contains(x, y))
            {
                costs[Index(x, y)] = walkable ? 1 : INF;
            }
        }

        private bool CanMove(int x, int y, Grid g)
        {
            return g.cost != 14 || costs[Index(x, y + g.y)] < INF && costs[Index(x + g.x, y)] < INF;
        }

        public void Rebuild(IList<Position> points)
        {
            BuildIntegration(points);
            BuildFlowField();
        }

        private void BuildIntegration(IList<Position> points)
        {
            for (var i = 0; i < steps.Length; i++)
            {
                steps[i] = INF;
            }

            opened.Clear();
            foreach (var p in points)
            {
                if (Contains(p.x, p.y))
                {
                    var i = Index(p.x, p.y);
                    steps[i] = 0;
                    opened.Enqueue(i);
                }
            }

            while (opened.Count > 0)
            {
                var i = opened.Dequeue();

                var cx = i % width;
                var cy = i / width;

                var step = steps[i];

                foreach (var n in Neighbors)
                {
                    var nx = cx + n.x;
                    var ny = cy + n.y;

                    if (Contains(nx, ny) && CanMove(cx, cy, n))
                    {
                        var j = Index(nx, ny);

                        if (costs[j] < INF)
                        {
                            var cost = step + n.cost + costs[j] * 10;
                            if (cost < steps[j])
                            {
                                steps[j] = cost;
                                opened.Enqueue(j);
                            }
                        }
                    }
                }
            }
        }

        private void BuildFlowField()
        {
            for (var i = 0; i < steps.Length; i++)
            {
                var cx = i % width;
                var cy = i / width;

                if (costs[i] >= INF || steps[i] >= INF)
                {
                    nodes[i] = -1;
                    continue;
                }

                var best = -1;
                var step = steps[i];

                for (var k = 0; k < Neighbors.Length; k++)
                {
                    var n = Neighbors[k];
                    var nx = cx + n.x;
                    var ny = cy + n.y;

                    if (Contains(nx, ny) && CanMove(cx, cy, n))
                    {
                        var j = Index(nx, ny);

                        if (steps[j] < step)
                        {
                            best = k;
                            step = steps[j];
                        }
                    }
                }

                nodes[i] = best;
            }
        }

        public Position GetDirection(Position d)
        {
            var cx = d.x;
            var cy = d.y;

            if (!Contains(cx, cy))
            {
                return default;
            }

            var i = Index(cx, cy);

            if (nodes[i] != -1)
            {
                var neighbor = Neighbors[nodes[i]];
                return new Position(neighbor.x, neighbor.y);
            }

            var best = -1;
            var step = INF;

            for (var k = 0; k < Neighbors.Length; k++)
            {
                var n = Neighbors[k];
                var nx = cx + n.x;
                var ny = cy + n.y;

                if (Contains(nx, ny) && CanMove(cx, cy, n))
                {
                    var j = Index(nx, ny);

                    if (steps[j] < step)
                    {
                        best = k;
                        step = steps[j];
                    }
                }
            }

            if (best != -1)
            {
                var neighbor = Neighbors[best];
                return new Position(neighbor.x, neighbor.y);
            }

            return default;
        }

        public void Dispose()
        {
            opened.Clear();
            opened = null;
            costs = null;
            nodes = null;
            steps = null;
        }
    }
}