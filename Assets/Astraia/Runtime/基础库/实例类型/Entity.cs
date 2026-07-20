using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Astraia
{
    [Serializable]
    public struct Xor32 : IEquatable<Xor32>
    {
        private static readonly int Ticks = (int)DateTime.Now.Ticks;
        public int origin;
        public int buffer;
        public int offset;

        public int Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var value = origin ^ offset;
                if (buffer != ((offset >> 8) ^ value))
                {
                    throw new InvalidOperationException();
                }

                return value;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                offset = Ticks;
                origin = value ^ offset;
                buffer = (offset >> 8) ^ value;
            }
        }

        public Xor32(int value = 0)
        {
            offset = Ticks;
            origin = value ^ offset;
            buffer = (offset >> 8) ^ value;
        }

        public static implicit operator int(Xor32 data)
        {
            return data.Value;
        }

        public static implicit operator Xor32(int data)
        {
            return new Xor32(data);
        }

        public bool Equals(Xor32 other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is Xor32 other && Equals(other);
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetBit(int shift, int bits)
        {
            return (Value >> shift) & (1 << bits) - 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetBit(int shift, int bits, int value)
        {
            Value = (Value & ~((1 << bits) - 1 << shift)) | ((value & (1 << bits) - 1) << shift);
        }
    }

    [Serializable]
    public struct Xor64 : IEquatable<Xor64>
    {
        private static readonly long Ticks = DateTime.Now.Ticks;
        public long origin;
        public long buffer;
        public long offset;

        public long Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var value = origin ^ offset;
                if (buffer != ((offset >> 8) ^ value))
                {
                    throw new InvalidOperationException();
                }

                return value;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                offset = Ticks;
                origin = value ^ offset;
                buffer = (offset >> 8) ^ value;
            }
        }

        public Xor64(long value = 0)
        {
            offset = Ticks;
            origin = value ^ offset;
            buffer = (offset >> 8) ^ value;
        }

        public static implicit operator long(Xor64 data)
        {
            return data.Value;
        }

        public static implicit operator Xor64(long data)
        {
            return new Xor64(data);
        }

        public bool Equals(Xor64 other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is Xor64 other && Equals(other);
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetBit(int shift, int bits)
        {
            return (int)((Value >> shift) & (1L << bits) - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetBit(int shift, int bits, int value)
        {
            Value = (Value & ~((1L << bits) - 1 << shift)) | ((value & (1L << bits) - 1) << shift);
        }
    }

    [Serializable]
    public struct Bytes : IEquatable<Bytes>
    {
        private static readonly int Ticks = (int)DateTime.Now.Ticks;
        public byte[] origin;
        public int buffer;
        public int offset;

        public byte[] Value
        {
            get
            {
                if (origin == null)
                {
                    return null;
                }

                if (buffer != GetHashCode())
                {
                    throw new InvalidOperationException();
                }

                return origin;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                offset = Ticks;
                origin = value;
                buffer = GetHashCode();
            }
        }

        public Bytes(byte[] value)
        {
            buffer = 0;
            offset = Ticks;
            origin = value;
            buffer = GetHashCode();
        }

        public static implicit operator byte[](Bytes variable)
        {
            return variable.Value;
        }

        public static implicit operator Bytes(byte[] value)
        {
            return new Bytes(value);
        }

        public bool Equals(Bytes other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is Bytes other && Equals(other);
        }

        public override string ToString()
        {
            return BitConverter.ToString(Value, 0, origin.Length);
        }

        public override unsafe int GetHashCode()
        {
            var result = offset;
            unchecked
            {
                fixed (byte* ptr = origin)
                {
                    var count = origin.Length / 4;
                    var ip = (int*)ptr;
                    for (var i = 0; i < count; i++)
                    {
                        result = (result * 31) ^ ip[i];
                    }

                    var bp = ptr + count * 4;
                    for (var i = count * 4; i < origin.Length; i++)
                    {
                        result = (result * 31) ^ *bp;
                        bp++;
                    }
                }

                return result;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe int GetBit(int shift, int bits)
        {
            fixed (byte* ptr = origin)
            {
                var byteIndex = shift >> 3;
                var bitOffset = shift & 7;

                var result = 0;
                var read = 0;

                var p = ptr + byteIndex;

                while (read < bits)
                {
                    var take = 8 - bitOffset;
                    var remain = bits - read;

                    if (take > remain)
                        take = remain;

                    var mask = (1 << take) - 1;

                    var part = (*p >> bitOffset) & mask;

                    result |= part << read;

                    read += take;
                    bitOffset = 0;
                    p++;
                }

                return result;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void SetBit(int shift, int bits, int value)
        {
            fixed (byte* ptr = origin)
            {
                var byteIndex = shift >> 3;
                var bitOffset = shift & 7;

                var written = 0;

                var p = ptr + byteIndex;

                while (written < bits)
                {
                    var take = 8 - bitOffset;
                    var remain = bits - written;

                    if (take > remain)
                        take = remain;

                    var mask = (1 << take) - 1;

                    var part = (value >> written) & mask;

                    var clearMask = ~(mask << bitOffset);

                    *p = (byte)((*p & clearMask) | (part << bitOffset));

                    written += take;
                    bitOffset = 0;
                    p++;
                }
            }
        }
    }

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
        internal static readonly Dictionary<Type, IPool> poolData = new Dictionary<Type, IPool>();

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
            private readonly Queue<T> unused = new Queue<T>();
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
        internal static readonly Dictionary<Type, IPool> poolData = new Dictionary<Type, IPool>();

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
        private Dictionary<T, IState> states = new Dictionary<T, IState>();
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
    public sealed class Blackboard<T>
    {
        private Dictionary<T, int> properties = new Dictionary<T, int>();
        private int percent;

        public Blackboard(int percent = 100)
        {
            this.percent = percent;
        }

        public int GetInt(T key)
        {
            properties.TryGetValue(key, out var result);
            return result / percent;
        }

        public void SetInt(T key, int value)
        {
            properties[key] = value * percent;
        }

        public void AddInt(T key, int value)
        {
            properties.TryGetValue(key, out var result);
            properties[key] = result + value * percent;
        }

        public void SubInt(T key, int value)
        {
            properties.TryGetValue(key, out var result);
            properties[key] = result - value * percent;
        }

        public float GetFloat(T key)
        {
            properties.TryGetValue(key, out var result);
            return result / (float)percent;
        }

        public void SetFloat(T key, float value)
        {
            properties[key] = (int)Math.Round(value * percent);
        }

        public void AddFloat(T key, float value)
        {
            properties.TryGetValue(key, out var result);
            properties[key] = result + (int)Math.Round(value * percent);
        }

        public void SubFloat(T key, float value)
        {
            properties.TryGetValue(key, out var result);
            properties[key] = result - (int)Math.Round(value * percent);
        }

        public void Clear()
        {
            properties.Clear();
        }
    }

    [Serializable]
    public class Whiteboard<T>
    {
        private Dictionary<Type, IDictionary> properties = new Dictionary<Type, IDictionary>();

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

        public bool TryGet<TValue>(T key, out TValue value)
        {
            if (!properties.TryGetValue(typeof(TValue), out var items))
            {
                items = new Dictionary<T, TValue>();
                properties.Add(typeof(TValue), items);
            }

            return ((Dictionary<T, TValue>)items).TryGetValue(key, out value);
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
                Log.Info("打断异步方法：\n{0}".Format(e));
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
                Log.Info("打断异步方法：\n{0}".Format(e));
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
}