using System;
using System.Collections.Generic;

namespace Astraia.Core
{
    [Serializable]
    public class AStar
    {
        private readonly int width;
        private readonly int height;

        private int[] parent;
        private int[] gScore;
        private int[] fScore;
        private bool[] closed;
        private bool[] walkable;

        private MinHeap opened;
        private List<Node> copied = new();

        private static readonly Grid[] Directions;

        static AStar()
        {
            Directions = new Grid[8];

            Directions[0] = new Grid(0, 1, 10);
            Directions[1] = new Grid(1, 1, 14);
            Directions[2] = new Grid(-1, 1, 14);

            Directions[3] = new Grid(0, -1, 10);
            Directions[4] = new Grid(1, -1, 14);
            Directions[5] = new Grid(-1, -1, 14);

            Directions[6] = new Grid(1, 0, 10);
            Directions[7] = new Grid(-1, 0, 10);
        }

        public AStar(int width, int height, bool[] map)
        {
            this.width = width;
            this.height = height;

            parent = new int[width * height];
            gScore = new int[width * height];
            fScore = new int[width * height];
            closed = new bool[width * height];
            opened = new MinHeap(fScore);

            walkable = map;
        }

        private int Index(int x, int y)
        {
            return y * width + x;
        }

        public IList<Node> FindPath(int sx, int sy, int ex, int ey)
        {
            for (var i = 0; i < gScore.Length; i++)
            {
                parent[i] = -1;
                gScore[i] = int.MaxValue;
                fScore[i] = int.MaxValue;
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

                foreach (var d in Directions)
                {
                    var nx = cx + d.X;
                    var ny = cy + d.Y;

                    if (nx < 0 || ny < 0 || nx >= width || ny >= height)
                    {
                        continue;
                    }

                    var j = Index(nx, ny);

                    if (!walkable[j] || closed[j])
                    {
                        continue;
                    }

                    if (d.Cost == 14)
                    {
                        var wx = Index(cx, ny);
                        var wy = Index(nx, cy);

                        if (!walkable[wx] || !walkable[wy])
                        {
                            continue;
                        }
                    }

                    var gCost = gScore[i] + d.Cost;
                    if (gCost < gScore[j])
                    {
                        parent[j] = i;
                        gScore[j] = gCost;
                        fScore[j] = gCost + Heuristic(j, e);
                        opened.Enqueue(j);
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

            var dx = Math.Abs(sx - ex);
            var dy = Math.Abs(sy - ey);

            var min = Math.Min(dx, dy);
            var max = Math.Max(dx, dy);

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

        public void Clear()
        {
            opened.Clear();
            opened = null;
            parent = null;
            gScore = null;
            closed = null;
            walkable = null;
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
        }

        private readonly struct Grid
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

        public sealed class MinHeap
        {
            private readonly List<int> heap = new();
            private readonly int[] fScore;

            public MinHeap(int[] fScore)
            {
                this.fScore = fScore;
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

                    if (fScore[heap[parent]] <= fScore[heap[i]])
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

                    if (right < heap.Count && fScore[heap[right]] < fScore[heap[left]])
                    {
                        smallest = right;
                    }

                    if (fScore[heap[i]] <= fScore[heap[smallest]])
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
}