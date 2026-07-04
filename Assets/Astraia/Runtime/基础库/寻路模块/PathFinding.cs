using System;
using System.Collections.Generic;

namespace Astraia.Core
{
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

        public readonly struct Node
        {
            public readonly int X;
            public readonly int Y;

            public Node(int x, int y)
            {
                X = x;
                Y = y;
            }

            public Node(Grid grid)
            {
                X = grid.X;
                Y = grid.Y;
            }
        }

        public readonly struct Grid
        {
            public readonly int X;
            public readonly int Y;
            public readonly int Cost;

            public Grid(int x, int y, int cost)
            {
                X = x;
                Y = y;
                Cost = cost;
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
        private List<Node> copied = new();

        public AStar(int width, int height, bool[] map) : base(width, height)
        {
            walkable = map;
            parent = new int[width * height];
            gScore = new int[width * height];
            fScore = new int[width * height];
            closed = new bool[width * height];
            opened = new MinHeap(fScore);
        }

        public IList<Node> FindPath(int sx, int sy, int ex, int ey)
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
                    var nx = cx + n.X;
                    var ny = cy + n.Y;

                    if (Contains(nx, ny))
                    {
                        var j = Index(nx, ny);

                        if (!walkable[j] || closed[j])
                        {
                            continue;
                        }

                        if (n.Cost == 14)
                        {
                            var wx = Index(cx, ny);
                            var wy = Index(nx, cy);

                            if (!walkable[wx] || !walkable[wy])
                            {
                                continue;
                            }
                        }

                        var gCost = gScore[i] + n.Cost;
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

            return Array.Empty<Node>();
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

        private List<Node> Reconstruct(int e)
        {
            copied.Clear();

            while (e != -1)
            {
                copied.Add(new Node(e % width, e / width));
                e = parent[e];
            }

            copied.Reverse();
            return copied;
        }

        public void Dispose()
        {
            opened.Clear();
            opened = null;
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

        private bool CanMove(int cx, int cy, Grid n)
        {
            return n.Cost != 14 || costs[Index(cx, cy + n.Y)] < INF && costs[Index(cx + n.X, cy)] < INF;
        }

        public void Rebuild(IList<Node> points)
        {
            BuildIntegration(points);
            BuildFlowField();
        }

        private void BuildIntegration(IList<Node> points)
        {
            for (var i = 0; i < steps.Length; i++)
            {
                steps[i] = INF;
            }

            opened.Clear();
            foreach (var p in points)
            {
                if (Contains(p.X, p.Y))
                {
                    var i = Index(p.X, p.Y);
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
                    var nx = cx + n.X;
                    var ny = cy + n.Y;

                    if (Contains(nx, ny) && CanMove(cx, cy, n))
                    {
                        var j = Index(nx, ny);

                        if (costs[j] < INF)
                        {
                            var cost = step + n.Cost + costs[j] * 10;
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
            for (int i = 0; i < steps.Length; i++)
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
                    var nx = cx + n.X;
                    var ny = cy + n.Y;

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

        public Node GetDirection(Node d)
        {
            var cx = d.X;
            var cy = d.Y;

            if (!Contains(cx, cy))
            {
                return default;
            }

            var i = Index(cx, cy);

            if (nodes[i] != -1)
            {
                return new Node(Neighbors[nodes[i]]);
            }

            var best = -1;
            var step = INF;

            for (var k = 0; k < Neighbors.Length; k++)
            {
                var n = Neighbors[k];
                var nx = cx + n.X;
                var ny = cy + n.Y;

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
                return new Node(Neighbors[best]);
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