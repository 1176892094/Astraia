using System;
using System.Collections.Generic;

namespace Astraia
{
    [Serializable]
    public sealed class FlowField : Pathfinding
    {
        private PriorityQueue opened;

        private int[] nodes;
        private int[] steps;

        public FlowField(int width, int height, int[] costs) : base(width, height, costs)
        {
            nodes = new int[width * height];
            steps = new int[width * height];
            opened = new PriorityQueue(steps);
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
                var x = p.x.FloorToInt();
                var y = p.y.FloorToInt();
                if (!Contains(x, y))
                {
                    continue;
                }

                var i = Index(x, y);
                if (costs[i] >= INF)
                {
                    continue;
                }

                if (steps[i] == 0)
                {
                    continue;
                }

                steps[i] = 0;
                opened.Enqueue(i);
            }

            while (opened.Count > 0)
            {
                var i = opened.Dequeue();

                var cx = i % width;
                var cy = i / width;

                var step = steps[i];

                foreach (var n in Neighbors.Data)
                {
                    if (!CanMove(cx, cy, n))
                    {
                        continue;
                    }

                    var nx = cx + n.x;
                    var ny = cy + n.y;
                    var j = Index(nx, ny);

                    var cost = step + n.cost * costs[j];
                    if (cost < steps[j])
                    {
                        steps[j] = cost;
                        opened.Enqueue(j);
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

                for (var k = 0; k < Neighbors.Data.Length; k++)
                {
                    var n = Neighbors.Data[k];
                    var nx = cx + n.x;
                    var ny = cy + n.y;

                    if (CanMove(cx, cy, n))
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
            var cx = d.x.FloorToInt();
            var cy = d.y.FloorToInt();

            if (!Contains(cx, cy))
            {
                return Position.Zero;
            }

            var i = Index(cx, cy);

            if (nodes[i] != -1)
            {
                var n = Neighbors.Data[nodes[i]];
                return new Position(n.x, n.y);
            }

            var best = -1;
            var step = INF;

            for (var k = 0; k < Neighbors.Data.Length; k++)
            {
                var n = Neighbors.Data[k];
                var nx = cx + n.x;
                var ny = cy + n.y;

                if (CanMove(cx, cy, n))
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
                var n = Neighbors.Data[best];
                return new Position(n.x, n.y);
            }

            return Position.Zero;
        }
    }
}