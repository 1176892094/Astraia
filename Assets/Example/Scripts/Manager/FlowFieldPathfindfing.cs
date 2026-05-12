using System.Collections.Generic;
using Astraia;
using UnityEngine;

public class FlowFieldPathfinding
{
    private const int INF = ushort.MaxValue;

    private readonly int[] cost;
    private readonly Vector2Int[] flow;
    public readonly int Height;
    private readonly int[] integration;
    public readonly int Width;

    public FlowFieldPathfinding(int width, int height)
    {
        Width = width;
        Height = height;
        cost = new int[width * height];
        integration = new int[width * height];
        flow = new Vector2Int[width * height];

        for (var i = 0; i < cost.Length; i++)
        {
            cost[i] = 1;
        }
    }

    private static Vector2Int[] Neighbors8 => Direction.GetD8();

    public void SetObstacle(int x, int y, bool blocked)
    {
        if (!InBounds(x, y))
        {
            return;
        }

        cost[Index(x, y)] = blocked ? INF : 1;
    }

    public void SetCost(int x, int y, int moveCost)
    {
        if (!InBounds(x, y))
        {
            return;
        }

        cost[Index(x, y)] = Mathf.Max(1, moveCost);
    }

    public void Build(Vector2Int goal)
    {
        BuildIntegration(goal);
        BuildFlowField();
    }

    public Vector2 GetDirection(Vector3 worldPos, float cellSize, Vector3 origin)
    {
        var x = Mathf.FloorToInt((worldPos.x - origin.x) / cellSize);
        var y = Mathf.FloorToInt((worldPos.z - origin.z) / cellSize);

        if (!InBounds(x, y))
        {
            return Vector2.zero;
        }

        var dir = flow[Index(x, y)];
        if (dir == Vector2Int.zero)
        {
            return Vector2.zero;
        }

        return new Vector2(dir.x, dir.y).normalized;
    }

    private void BuildIntegration(Vector2Int goal)
    {
        for (var i = 0; i < integration.Length; i++)
        {
            integration[i] = INF;
        }

        if (!InBounds(goal.x, goal.y))
        {
            return;
        }

        var goalIndex = Index(goal.x, goal.y);
        if (cost[goalIndex] >= INF)
        {
            return;
        }
        
        var pq = new PriorityQueue<int>();
        integration[goalIndex] = 0;
        pq.Enqueue(goalIndex, 0);

        while (pq.Count > 0)
        {
            int current = pq.Dequeue();
            var cx = current % Width;
            var cy = current / Width;

            var baseCost = integration[current];

            for (var i = 0; i < Neighbors8.Length; i++)
            {
                var n = Neighbors8[i];
                var nx = cx + n.x;
                var ny = cy + n.y;

                if (!InBounds(nx, ny))
                {
                    continue;
                }

                var ni = Index(nx, ny);
                if (cost[ni] >= INF)
                {
                    continue;
                }

                var stepCost = Mathf.Abs(n.x) + Mathf.Abs(n.y) == 2 ? 14 : 10;
                var tileCost = cost[ni] * 10;
                var newCost = baseCost + stepCost + tileCost;

                if (newCost < integration[ni])
                {
                    integration[ni] = newCost;
                    pq.Enqueue(ni, newCost);
                }
            }
        }
    }

    private void BuildFlowField()
    {
        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                var i = Index(x, y);

                if (integration[i] >= INF || cost[i] >= INF)
                {
                    flow[i] = Vector2Int.zero;
                    continue;
                }

                var best = integration[i];
                var bestDir = Vector2Int.zero;

                for (var n = 0; n < Neighbors8.Length; n++)
                {
                    var d = Neighbors8[n];
                    var nx = x + d.x;
                    var ny = y + d.y;

                    if (!InBounds(nx, ny))
                    {
                        continue;
                    }

                    var ni = Index(nx, ny);
                    if (integration[ni] < best)
                    {
                        best = integration[ni];
                        bestDir = d;
                    }
                }

                flow[i] = bestDir;
            }
        }
    }

    public Vector2Int GetFlowCell(int x, int y)
    {
        if (!InBounds(x, y))
        {
            return Vector2Int.zero;
        }

        return flow[Index(x, y)];
    }

    private bool InBounds(int x, int y)
    {
        return x >= 0 && y >= 0 && x < Width && y < Height;
    }

    private int Index(int x, int y)
    {
        return y * Width + x;
    }
}

public class PriorityQueue<T>
{
    private readonly List<Node> heap = new List<Node>(128);

    public int Count => heap.Count;

    public void Enqueue(T item, int priority)
    {
        heap.Add(new Node(item, priority));
        SiftUp(heap.Count - 1);
    }

    public T Dequeue()
    {
        var result = heap[0].Item;
        var last = heap.Count - 1;

        heap[0] = heap[last];
        heap.RemoveAt(last);

        if (heap.Count > 0)
        {
            SiftDown(0);
        }

        return result;
    }

    private void SiftUp(int index)
    {
        while (index > 0)
        {
            var parent = (index - 1) >> 1;
            if (heap[parent].Priority <= heap[index].Priority)
            {
                break;
            }

            Swap(parent, index);
            index = parent;
        }
    }

    private void SiftDown(int index)
    {
        var count = heap.Count;

        while (true)
        {
            var left = (index << 1) + 1;
            var right = left + 1;
            var smallest = index;

            if (left < count && heap[left].Priority < heap[smallest].Priority)
            {
                smallest = left;
            }

            if (right < count && heap[right].Priority < heap[smallest].Priority)
            {
                smallest = right;
            }

            if (smallest == index)
            {
                break;
            }

            Swap(index, smallest);
            index = smallest;
        }
    }

    private void Swap(int a, int b)
    {
        (heap[a], heap[b]) = (heap[b], heap[a]);
    }

    private struct Node
    {
        public readonly T Item;
        public readonly int Priority;

        public Node(T item, int priority)
        {
            Item = item;
            Priority = priority;
        }
    }
}