using System;
using System.Collections.Generic;

namespace Astraia
{
    [Serializable]
    public sealed class AStar : Pathfinding
    {
        private PriorityQueue opened;

        private int[] parent;
        private int[] gScore;
        private int[] fScore;
        private bool[] closed;

        public AStar(int width, int height, int[] costs) : base(width, height, costs)
        {
            parent = new int[width * height];
            gScore = new int[width * height];
            fScore = new int[width * height];
            closed = new bool[width * height];
            opened = new PriorityQueue(fScore);
        }

        public IList<Position> Rebuild(int sx, int sy, int ex, int ey)
        {
            if (!Contains(sx, sy) || !Contains(ex, ey))
            {
                return Array.Empty<Position>();
            }

            var s = Index(sx, sy);
            var e = Index(ex, ey);

            if (costs[s] >= INF || costs[e] >= INF)
            {
                return Array.Empty<Position>();
            }

            for (var i = 0; i < gScore.Length; i++)
            {
                parent[i] = -1;
                gScore[i] = INF;
                fScore[i] = INF;
                closed[i] = false;
            }

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

                foreach (var n in Neighbors.Data)
                {
                    if (!CanMove(cx, cy, n))
                    {
                        continue;
                    }

                    var nx = cx + n.x;
                    var ny = cy + n.y;
                    var j = Index(nx, ny);

                    if (closed[j])
                    {
                        continue;
                    }

                    var gCost = gScore[i] + n.cost * costs[j];
                    if (gCost < gScore[j])
                    {
                        parent[j] = i;
                        gScore[j] = gCost;
                        fScore[j] = gCost + Heuristic(j, e);
                        opened.Enqueue(j);
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
            var copied = new List<Position>();

            while (e != -1)
            {
                copied.Add(new Position(e % width, e / width));
                e = parent[e];
            }

            copied.Reverse();
            return copied;
        }
    }
}