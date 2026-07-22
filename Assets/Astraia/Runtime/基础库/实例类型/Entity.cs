using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Astraia
{
    internal interface IPool : IDisposable
    {
        public Type Type { get; }
        public string Path { get; }
        public int Acquire { get; }
        public int Release { get; }
        public int Dequeue { get; }
        public int Enqueue { get; }
    }

    public static class HeapManager
    {
        internal static readonly Dictionary<Type, IPool> poolData = new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Dequeue<T>(params object[] args)
        {
            return LoadPool<T>(typeof(T)).Load(args);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Dequeue<T>(Type type, params object[] args)
        {
            return LoadPool<T>(type).Load(args);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Enqueue<T>(T item)
        {
            LoadPool<T>(typeof(T)).Push(item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Enqueue<T>(T item, Type type)
        {
            LoadPool<T>(type).Push(item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Pool<T> LoadPool<T>(Type type)
        {
            if (!poolData.TryGetValue(type, out var item))
            {
                item = new Pool<T>(type, type.Name);
                poolData.Add(type, item);
            }

            return (Pool<T>)item;
        }

        internal static void Dispose()
        {
            foreach (var item in poolData.Values)
            {
                item.Dispose();
            }

            poolData.Clear();
        }

        private class Pool<T> : IPool
        {
            private readonly Queue<T> unused = new();
            public Type Type { get; private set; }
            public string Path { get; private set; }
            public int Acquire { get; private set; }
            public int Release { get; private set; }
            public int Dequeue { get; private set; }
            public int Enqueue { get; private set; }

            public Pool(Type type, string path)
            {
                Type = type;
                Path = path;
            }

            public T Load(params object[] args)
            {
                Dequeue++;
                Acquire++;
                if (unused.TryDequeue(out var item))
                {
                    Release--;
                }
                else
                {
                    item = (T)Activator.CreateInstance(Type, args);
                }

                return item;
            }

            public void Push(T item)
            {
                Enqueue++;
                Acquire--;
                Release++;
                unused.Enqueue(item);
            }

            void IDisposable.Dispose()
            {
                unused.Clear();
            }
        }
    }

    public interface IEvent { }

    public interface IEvent<in T> where T : struct, IEvent
    {
        void Execute(T message);
    }

    public static class EventManager
    {
        internal static readonly Dictionary<Type, IPool> poolData = new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Listen<T>(IEvent<T> data) where T : struct, IEvent
        {
            LoadPool<T>().Listen(data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Remove<T>(IEvent<T> data) where T : struct, IEvent
        {
            LoadPool<T>().Remove(data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Invoke<T>(T data) where T : struct, IEvent
        {
            LoadPool<T>().Invoke(data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Pool<T> LoadPool<T>() where T : struct, IEvent
        {
            if (!poolData.TryGetValue(typeof(T), out var pool))
            {
                pool = new Pool<T>(typeof(T), typeof(T).Name);
                poolData.Add(typeof(T), pool);
            }

            return (Pool<T>)pool;
        }

        internal static void Dispose()
        {
            foreach (var item in poolData.Values)
            {
                item.Dispose();
            }

            poolData.Clear();
        }

        private class Pool<T> : IPool where T : struct, IEvent
        {
            private event Action<T> OnExecute;
            public Type Type { get; private set; }
            public string Path { get; private set; }
            public int Acquire { get; private set; }
            public int Release { get; private set; }
            public int Dequeue { get; private set; }
            public int Enqueue { get; private set; }

            public Pool(Type type, string path)
            {
                Type = type;
                Path = path;
            }

            public void Listen(IEvent<T> obj)
            {
                Dequeue++;
                Acquire++;
                OnExecute += obj.Execute;
            }

            public void Remove(IEvent<T> obj)
            {
                Enqueue++;
                Acquire--;
                OnExecute -= obj.Execute;
            }

            public void Invoke(T message)
            {
                Release++;
                OnExecute?.Invoke(message);
            }

            void IDisposable.Dispose()
            {
                OnExecute = null;
            }
        }
    }

    internal interface IModule
    {
        void Acquire(object value);
        void Release();
        void Dequeue();
        void Enqueue();
        void OnShow();
        void OnHide();
    }

    internal interface IState
    {
        void Acquire(object value);
        void Release();
        void OnEnter();
        void OnUpdate();
        void OnExit();
    }

    [Serializable]
    public abstract class Module<T> : IModule
    {
        public T owner { get; private set; }
        void IModule.Acquire(object value) => owner = (T)value;
        void IModule.Release() => owner = default;
        void IModule.Dequeue() => Dequeue();
        void IModule.Enqueue() => Enqueue();
        void IModule.OnShow() => OnShow();
        void IModule.OnHide() => OnHide();
        protected virtual void Dequeue() { }
        protected virtual void Enqueue() { }
        protected virtual void OnShow() { }
        protected virtual void OnHide() { }
    }

    [Serializable]
    public abstract class State<T> : IState
    {
        public T owner { get; private set; }
        void IState.Acquire(object value) => owner = (T)value;
        void IState.Release() => owner = default;
        void IState.OnEnter() => OnEnter();
        void IState.OnUpdate() => OnUpdate();
        void IState.OnExit() => OnExit();
        protected virtual void OnEnter() { }
        protected virtual void OnUpdate() { }
        protected virtual void OnExit() { }
    }

    [Serializable]
    public sealed class StateMachine<T>
    {
        private Dictionary<T, IState> states = new();
        private IState state;

        public void Create<TState>(object owner, T key)
        {
            var item = (IState)Activator.CreateInstance<TState>();
            item.Acquire(owner);
            states[key] = item;
        }

        public void Switch(T key)
        {
            state?.OnExit();
            states.TryGetValue(key, out state);
            state?.OnEnter();
        }

        public void Update()
        {
            state?.OnUpdate();
        }

        public void Clear()
        {
            foreach (var item in states.Values)
            {
                item.Release();
            }

            state = null;
            states.Clear();
        }
    }

    [Serializable]
    public class Whiteboard<T>
    {
        private Dictionary<Type, IDictionary> properties = new();

        public void Set<TValue>(T key, TValue value)
        {
            if (!properties.TryGetValue(typeof(TValue), out var items))
            {
                items = new Dictionary<T, TValue>();
                properties.Add(typeof(TValue), items);
            }

            ((Dictionary<T, TValue>)items)[key] = value;
        }

        public TValue Get<TValue>(T key)
        {
            if (!properties.TryGetValue(typeof(TValue), out var items))
            {
                items = new Dictionary<T, TValue>();
                properties.Add(typeof(TValue), items);
            }

            return ((Dictionary<T, TValue>)items).GetValueOrDefault(key);
        }

        public void Clear()
        {
            foreach (var child in properties.Values)
            {
                child.Clear();
            }

            properties.Clear();
        }
    }

    public struct OnEarlyUpdate : IEvent { }

    public struct OnAfterUpdate : IEvent { }

    public struct OnFixedUpdate : IEvent { }

    public struct OnGizmoUpdate : IEvent { }

    [Serializable]
    public sealed class Timer : INotifyCompletion, IEvent<OnEarlyUpdate>
    {
        private int state;
        private object owner;
        private float waitTime;
        private float duration;
        private Action onAwaiter;
        private Action onComplete;
        private Func<float> onTime;

        private int progress;
        private Action onUpdate;
        public bool IsCompleted => state != 0;

        internal static Timer Create(object owner, float duration, Func<float> onTime)
        {
            var item = HeapManager.Dequeue<Timer>();
            EventManager.Listen(item);
            item.owner = owner;
            item.state = 0;
            item.progress = 1;
            item.duration = duration;
            item.onComplete = item.Release;
            item.SetTime(onTime);
            return item;
        }

        private void Release()
        {
            state |= 1 << 0;
            owner = null;
            onTime = null;
            onUpdate = null;
            onAwaiter = null;
            EventManager.Remove(this);
            HeapManager.Enqueue(this);
        }

        void IEvent<OnEarlyUpdate>.Execute(OnEarlyUpdate message)
        {
            try
            {
                if (owner.GetHashCode() == 0)
                {
                    Break();
                    return;
                }

                var stepTime = onTime();
                if (waitTime < stepTime)
                {
                    waitTime = stepTime + duration;
                    if (onUpdate != null)
                    {
                        onUpdate.Invoke();
                    }

                    progress--;
                    if (progress == 0)
                    {
                        state |= 1 << 1;
                        onComplete += onAwaiter;
                        onComplete.Invoke();
                    }
                }
            }
            catch (Exception e)
            {
                Log.Info("打断异步方法：\n{0}", e);
                Break();
            }
        }

        public void Break()
        {
            onComplete.Invoke();
        }

        public void OnComplete(Action complete)
        {
            onComplete += complete;
        }

        public Timer OnUpdate(Action update)
        {
            onUpdate += update;
            return this;
        }

        public Timer SetTime(Func<float> onTime)
        {
            this.onTime = onTime;
            waitTime = duration + onTime();
            return this;
        }

        public Timer Set(float interval)
        {
            duration = interval;
            waitTime = interval + onTime();
            return this;
        }

        public Timer Add(float interval)
        {
            waitTime += interval;
            return this;
        }

        public Timer Loops(int count = 0)
        {
            progress = count;
            return this;
        }

        public int GetResult()
        {
            return state;
        }

        public Timer GetAwaiter()
        {
            return this;
        }

        void INotifyCompletion.OnCompleted(Action awaiter)
        {
            if (owner.GetHashCode() == 0)
            {
                Break();
            }
            else
            {
                onAwaiter = awaiter;
            }
        }
    }

    [Serializable]
    public sealed class Tween : INotifyCompletion, IEvent<OnEarlyUpdate>
    {
        private int state;
        private object owner;
        private float waitTime;
        private float duration;
        private Action onAwaiter;
        private Action onComplete;
        private Func<float> onTime;

        private float progress;
        private Action<float> onUpdate;
        public bool IsCompleted => state != 0;

        internal static Tween Create(object owner, float duration, Func<float> onTime)
        {
            var item = HeapManager.Dequeue<Tween>();
            EventManager.Listen(item);
            item.owner = owner;
            item.state = 0;
            item.progress = 0;
            item.duration = duration;
            item.onComplete = item.Release;
            item.SetTime(onTime);
            return item;
        }

        private void Release()
        {
            state |= 1 << 0;
            owner = null;
            onTime = null;
            onUpdate = null;
            onAwaiter = null;
            EventManager.Remove(this);
            HeapManager.Enqueue(this);
        }

        void IEvent<OnEarlyUpdate>.Execute(OnEarlyUpdate message)
        {
            try
            {
                if (owner.GetHashCode() == 0)
                {
                    Break();
                    return;
                }

                var stepTime = duration + onTime();
                if (waitTime < stepTime)
                {
                    progress = (stepTime - waitTime) / duration;
                    if (progress > 1)
                    {
                        progress = 1;
                    }

                    onUpdate.Invoke(progress);
                    if (progress >= 1)
                    {
                        state |= 1 << 1;
                        onComplete += onAwaiter;
                        onComplete.Invoke();
                    }
                }
            }
            catch (Exception e)
            {
                Log.Info("打断异步方法：\n{0}", e);
                Break();
            }
        }

        public void Break()
        {
            onComplete.Invoke();
        }

        public void OnComplete(Action complete)
        {
            onComplete += complete;
        }

        public Tween OnUpdate(Action<float> update)
        {
            onUpdate += update;
            return this;
        }

        public Tween SetTime(Func<float> onTime)
        {
            this.onTime = onTime;
            waitTime = duration + onTime();
            return this;
        }

        public int GetResult()
        {
            return state;
        }

        public Tween GetAwaiter()
        {
            return this;
        }

        void INotifyCompletion.OnCompleted(Action awaiter)
        {
            if (owner.GetHashCode() == 0)
            {
                Break();
            }
            else
            {
                onAwaiter = awaiter;
            }
        }
    }

    [Serializable]
    public class Enumerable<T> : IEnumerable<T>
    {
        private T[] Items;
        public int Count;

        public Enumerable(int count)
        {
            Items = new T[count];
        }

        public T this[int index]
        {
            get => Items[index];
            set => Items[index] = value;
        }

        public void Clear()
        {
            Count = 0;
            Items = null;
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(Items, Count);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static implicit operator T[](Enumerable<T> value)
        {
            return value.Items;
        }

        public struct Enumerator : IEnumerator<T>
        {
            private readonly T[] Items;
            private readonly int Count;
            private int Index;
            public T Current => Items[Index];
            object IEnumerator.Current => Items[Index];

            public Enumerator(T[] items, int count)
            {
                Index = -1;
                Items = items;
                Count = count;
            }

            public bool MoveNext()
            {
                return ++Index < Count;
            }

            public void Reset()
            {
                Index = -1;
            }

            public void Dispose()
            {
                Index = -1;
            }
        }
    }

    [Serializable]
    public sealed class SpatialHash<T>
    {
        private readonly Dictionary<int, HashSet<T>> buckets = new();
        private readonly Dictionary<T, int> objects = new();

        public void Insert(T item, Position center)
        {
            var node = center.GetHashCode();
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
                var newNode = center.GetHashCode();
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
            var minX = center.X - extentX;
            var maxX = center.X + extentX;
            var minY = center.Y - extentY;
            var maxY = center.Y + extentY;

            for (var x = minX; x <= maxX; x++)
            {
                for (var y = minY; y <= maxY; y++)
                {
                    var node = new Position(x, y).GetHashCode();
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