using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

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
    public class Blackboard<T>
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
    public class Awaiter : INotifyCompletion
    {
        public static readonly Task<State> Success = Task.FromResult(State.Success);
        public static readonly Task<State> Failure = Task.FromResult(State.Failure);

        public enum State
        {
            Running,
            Success,
            Failure
        }

        protected State state;
        protected object owner;
        protected float waitTime;
        protected float duration;
        protected Action onAwaiter;
        protected Action onComplete;
        protected Func<float> onTime;

        public bool IsCompleted => state != 0;
        public bool isInterrupt => owner.GetHashCode() == 0;

        public void OnComplete(Action complete)
        {
            onComplete += complete;
        }

        public void Break()
        {
            try
            {
                onComplete.Invoke();
                state = State.Failure;
            }
            finally
            {
                onComplete = null;
            }
        }

        public Awaiter SetTime(Func<float> onTime)
        {
            this.onTime = onTime;
            waitTime = duration + onTime();
            return this;
        }

        public Awaiter GetAwaiter()
        {
            return this;
        }

        public State GetResult()
        {
            return state;
        }

        void INotifyCompletion.OnCompleted(Action awaiter)
        {
            if (isInterrupt)
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
    public sealed class Timer : Awaiter, IEvent<OnEarlyUpdate>
    {
        private int progress;
        private Action onUpdate;

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
            state = State.Running;
            owner = null;
            onTime = null;
            onUpdate = null;
            onAwaiter = null;
            EventManager.Remove(this);
            HeapManager.Enqueue(this);
        }

        void IEvent<OnEarlyUpdate>.Execute(OnEarlyUpdate message)
        {
            if (isInterrupt)
            {
                Break();
                return;
            }

            try
            {
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
                        state = State.Success;
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

        public Timer OnUpdate(Action update)
        {
            onUpdate += update;
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
    }

    [Serializable]
    public sealed class Tween : Awaiter, IEvent<OnEarlyUpdate>
    {
        private float progress;
        private Action<float> onUpdate;

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
            state = State.Running;
            owner = null;
            onTime = null;
            onUpdate = null;
            onAwaiter = null;
            EventManager.Remove(this);
            HeapManager.Enqueue(this);
        }

        void IEvent<OnEarlyUpdate>.Execute(OnEarlyUpdate message)
        {
            if (isInterrupt)
            {
                Break();
                return;
            }

            try
            {
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
                        state = State.Success;
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

        public Tween OnUpdate(Action<float> update)
        {
            onUpdate += update;
            return this;
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
}