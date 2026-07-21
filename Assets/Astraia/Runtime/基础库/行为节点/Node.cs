using System;
using System.Collections.Generic;
using System.Linq;

namespace Astraia.Node
{
    public interface INode
    {
        Root.State OnTick(int[] indices, Whiteboard<int> root);
    }

    [Serializable]
    public struct Sequence : INode
    {
        private int Index;
        private INode[] Nodes;

        public Sequence(int index, INode[] nodes)
        {
            Index = index;
            Nodes = nodes ?? Array.Empty<INode>();
        }

        public Root.State OnTick(int[] indices, Whiteboard<int> root)
        {
            var current = indices[Index];
            while (current < Nodes.Length)
            {
                var result = Nodes[current].OnTick(indices, root);
                if (result == Root.State.Running)
                {
                    return Root.State.Running;
                }

                if (result == Root.State.Failure)
                {
                    indices[Index] = 0;
                    return Root.State.Failure;
                }

                current++;
                indices[Index] = current;
            }

            indices[Index] = 0;
            return Root.State.Success;
        }
    }

    [Serializable]
    public struct Selector : INode
    {
        private int Index;
        private INode[] Nodes;

        public Selector(int index, INode[] nodes)
        {
            Index = index;
            Nodes = nodes ?? Array.Empty<INode>();
        }

        public Root.State OnTick(int[] indices, Whiteboard<int> root)
        {
            var current = indices[Index];
            while (current < Nodes.Length)
            {
                var result = Nodes[current].OnTick(indices, root);
                if (result == Root.State.Running)
                {
                    return Root.State.Running;
                }

                if (result == Root.State.Success)
                {
                    indices[Index] = 0;
                    return Root.State.Success;
                }

                current++;
                indices[Index] = current;
            }

            indices[Index] = 0;
            return Root.State.Failure;
        }
    }

    [Serializable]
    public struct Parallel : INode
    {
        private bool IsAny;
        private INode[] Nodes;

        public Parallel(string isAny, INode[] nodes)
        {
            IsAny = isAny == "Any";
            Nodes = nodes ?? Array.Empty<INode>();
        }

        public Root.State OnTick(int[] indices, Whiteboard<int> root)
        {
            if (IsAny)
            {
                foreach (var node in Nodes)
                {
                    var result = node.OnTick(indices, root);
                    if (result == Root.State.Success)
                    {
                        return Root.State.Success;
                    }

                    if (result == Root.State.Failure)
                    {
                        return Root.State.Failure;
                    }
                }

                return Root.State.Running;
            }

            var isAll = true;
            foreach (var node in Nodes)
            {
                var result = node.OnTick(indices, root);
                if (result == Root.State.Failure)
                {
                    return Root.State.Failure;
                }

                if (result == Root.State.Running)
                {
                    isAll = false;
                }
            }

            return isAll ? Root.State.Success : Root.State.Running;
        }
    }

    [Serializable]
    public struct Randomer : INode
    {
        private int Index;
        private INode[] Nodes;

        public Randomer(int index, INode[] nodes)
        {
            Index = index;
            Nodes = nodes ?? Array.Empty<INode>();
        }

        public Root.State OnTick(int[] indices, Whiteboard<int> root)
        {
            if (indices[Index] == 0)
            {
                indices[Index] = Seed.Next(Nodes.Length) + 1;
            }

            var result = Nodes[indices[Index] - 1].OnTick(indices, root);
            if (result == Root.State.Running)
            {
                return Root.State.Running;
            }

            indices[Index] = 0;
            return result;
        }
    }

    [Serializable]
    public struct Repeater : INode
    {
        private int Index;
        private int Count;
        private INode Node;

        public Repeater(int index, int count, INode node)
        {
            Node = node;
            Index = index;
            Count = count;
        }

        public Root.State OnTick(int[] indices, Whiteboard<int> root)
        {
            var result = Node.OnTick(indices, root);
            if (result == Root.State.Running)
            {
                return Root.State.Running;
            }

            indices[Index]++;
            if (Count < 0 || indices[Index] < Count)
            {
                return Root.State.Running;
            }

            indices[Index] = 0;
            return Root.State.Success;
        }
    }

    [Serializable]
    public struct Inverter : INode
    {
        private INode Node;

        public Inverter(INode node)
        {
            Node = node;
        }

        public Root.State OnTick(int[] indices, Whiteboard<int> root)
        {
            switch (Node.OnTick(indices, root))
            {
                case Root.State.Success: return Root.State.Failure;
                case Root.State.Failure: return Root.State.Success;
            }

            return Root.State.Running;
        }
    }

    [Serializable]
    public struct Success : INode
    {
        private INode Node;

        public Success(INode node)
        {
            Node = node;
        }

        public Root.State OnTick(int[] indices, Whiteboard<int> root)
        {
            return Node.OnTick(indices, root) == Root.State.Running ? Root.State.Running : Root.State.Success;
        }
    }

    [Serializable]
    public struct Failure : INode
    {
        private INode Node;

        public Failure(INode node)
        {
            Node = node;
        }

        public Root.State OnTick(int[] indices, Whiteboard<int> root)
        {
            return Node.OnTick(indices, root) == Root.State.Running ? Root.State.Running : Root.State.Failure;
        }
    }

    public static class Root
    {
        private static readonly Dictionary<Type, Func<Node, Func<Node, Type>, INode>> Func = new();

        public enum State
        {
            Running,
            Success,
            Failure
        }

        static Root()
        {
            Func[typeof(Sequence)] = Sequence;
            Func[typeof(Selector)] = Selector;
            Func[typeof(Parallel)] = Parallel;
            Func[typeof(Randomer)] = Randomer;
            Func[typeof(Repeater)] = Repeater;
            Func[typeof(Inverter)] = Inverter;
            Func[typeof(Success)] = Success;
            Func[typeof(Failure)] = Failure;
        }

        private static INode Sequence(Node node, Func<Node, Type> func)
        {
            return new Sequence(node.Index, node.Nodes.Select(i => i.Build(func)).ToArray());
        }

        private static INode Selector(Node node, Func<Node, Type> func)
        {
            return new Selector(node.Index, node.Nodes.Select(i => i.Build(func)).ToArray());
        }

        private static INode Parallel(Node node, Func<Node, Type> func)
        {
            return new Parallel(node.Data, node.Nodes.Select(i => i.Build(func)).ToArray());
        }

        private static INode Randomer(Node node, Func<Node, Type> func)
        {
            return new Randomer(node.Index, node.Nodes.Select(i => i.Build(func)).ToArray());
        }

        private static INode Repeater(Node node, Func<Node, Type> func)
        {
            return new Repeater(node.Index, int.Parse(node.Data), node.Nodes.Select(i => i.Build(func)).First());
        }

        private static INode Inverter(Node node, Func<Node, Type> func)
        {
            return new Inverter(node.Nodes.Select(i => i.Build(func)).First());
        }

        private static INode Success(Node node, Func<Node, Type> func)
        {
            return new Success(node.Nodes.Select(i => i.Build(func)).First());
        }

        private static INode Failure(Node node, Func<Node, Type> func)
        {
            return new Failure(node.Nodes.Select(i => i.Build(func)).First());
        }

        public static Node Load(string reason, ref int i)
        {
            if (string.IsNullOrEmpty(reason))
            {
                return default;
            }

            var index = FindFirstBracket(reason);
            if (index < 0)
            {
                return new Node(reason, i++);
            }

            var result = new Node(reason.Substring(0, index).Trim(), i++);
            foreach (var child in LoadNode(Checked(reason, index)))
            {
                result.Nodes.Add(Load(child, ref i));
            }

            return result;
        }

        private static string Checked(string reason, int index)
        {
            var depth = 0;
            var count = index;
            while (count < reason.Length)
            {
                if (IsLeftBracket(reason[count]))
                {
                    depth++;
                }
                else if (IsRightBracket(reason[count]))
                {
                    depth--;
                }

                if (depth == 0)
                {
                    break;
                }

                count++;
            }

            return reason.Substring(index + 1, count - index - 1);
        }

        private static List<string> LoadNode(string reason)
        {
            var result = new List<string>();
            var depth = 0;
            var index = 0;

            for (var i = 0; i < reason.Length; i++)
            {
                var c = reason[i];
                if (IsLeftBracket(c))
                {
                    depth++;
                }
                else if (IsRightBracket(c))
                {
                    depth--;
                }
                else if (depth == 0 && IsSeparator(c))
                {
                    result.Add(reason.Substring(index, i - index).Trim());
                    index = i + 1;
                }
            }

            result.Add(reason.Substring(index).Trim());
            return result;
        }

        private static int FindFirstBracket(string text)
        {
            var englishIndex = text.IndexOf('(');
            var chineseIndex = text.IndexOf('（');

            if (englishIndex < 0) return chineseIndex;
            if (chineseIndex < 0) return englishIndex;

            return Math.Min(englishIndex, chineseIndex);
        }

        private static int FindColon(string text)
        {
            var englishIndex = text.IndexOf(':');
            var chineseIndex = text.IndexOf('：');

            if (englishIndex < 0) return chineseIndex;
            if (chineseIndex < 0) return englishIndex;

            return Math.Min(englishIndex, chineseIndex);
        }

        private static bool IsLeftBracket(char c)
        {
            return c is '(' or '（';
        }

        private static bool IsRightBracket(char c)
        {
            return c is ')' or '）';
        }

        private static bool IsSeparator(char c)
        {
            return c is ',' or '，';
        }

        [Serializable]
        public struct Node
        {
            public int Index;
            public string Name;
            public string Data;
            public List<Node> Nodes;

            public Node(string name, int index)
            {
                var i = FindColon(name);
                if (i < 0)
                {
                    Name = name;
                    Data = null;
                }
                else
                {
                    Name = name.Substring(0, i);
                    Data = name.Substring(i + 1);
                }

                Index = index;
                Nodes = new List<Node>();
            }

            public INode Build(Func<Node, Type> func)
            {
                if (Name.IsNullOrEmpty())
                {
                    throw new NullReferenceException();
                }

                var reason = func.Invoke(this);
                if (Func.TryGetValue(reason, out var result))
                {
                    return result.Invoke(this, func);
                }

                return (INode)Activator.CreateInstance(reason);
            }
        }
    }

    [Serializable]
    internal readonly struct Grid
    {
        public static readonly Grid[] Neighbors;

        public readonly int x;
        public readonly int y;
        public readonly int cost;

        public Grid(int x, int y, int cost)
        {
            this.x = x;
            this.y = y;
            this.cost = cost;
        }

        static Grid()
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
    }

    [Serializable]
    internal sealed class MinHeap
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

    [Serializable]
    public sealed class AStar
    {
        private const int INF = int.MaxValue;

        private int width;
        private int height;

        private int[] parent;
        private int[] gScore;
        private int[] fScore;
        private bool[] closed;
        private bool[] walkable;

        private MinHeap opened;
        private List<Position> copied = new();

        public AStar(int width, int height, bool[] map)
        {
            this.width = width;
            this.height = height;

            walkable = map;
            parent = new int[width * height];
            gScore = new int[width * height];
            fScore = new int[width * height];
            closed = new bool[width * height];
            opened = new MinHeap(fScore);
        }

        private int Index(int x, int y)
        {
            return y * width + x;
        }

        private bool Contains(int x, int y)
        {
            return x >= 0 && y >= 0 && x < width && y < height;
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

                foreach (var n in Grid.Neighbors)
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
    public class FlowField
    {
        private const int INF = int.MaxValue;

        private int width;
        private int height;

        private int[] nodes;
        private int[] costs;
        private int[] steps;
        private MinHeap opened;

        public FlowField(int width, int height, bool[] walkable)
        {
            this.width = width;
            this.height = height;

            costs = new int[width * height];
            nodes = new int[width * height];
            steps = new int[width * height];
            opened = new MinHeap(steps);
            for (var i = 0; i < costs.Length; i++)
            {
                costs[i] = walkable[i] ? 1 : INF;
            }
        }

        private int Index(int x, int y)
        {
            return y * width + x;
        }

        private bool Contains(int x, int y)
        {
            return x >= 0 && y >= 0 && x < width && y < height;
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

                foreach (var n in Grid.Neighbors)
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

                for (var k = 0; k < Grid.Neighbors.Length; k++)
                {
                    var n = Grid.Neighbors[k];
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
            var cx = d.X;
            var cy = d.Y;

            if (!Contains(cx, cy))
            {
                return default;
            }

            var i = Index(cx, cy);

            if (nodes[i] != -1)
            {
                var neighbor = Grid.Neighbors[nodes[i]];
                return new Position(neighbor.x, neighbor.y);
            }

            var best = -1;
            var step = INF;

            for (var k = 0; k < Grid.Neighbors.Length; k++)
            {
                var n = Grid.Neighbors[k];
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
                var neighbor = Grid.Neighbors[best];
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