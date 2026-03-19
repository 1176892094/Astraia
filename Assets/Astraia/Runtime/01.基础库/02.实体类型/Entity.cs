using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Astraia
{
    public record OnVariableEvent : IEvent;

    [Serializable]
    public struct XorInt32 : IEquatable<XorInt32>
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
                    EventManager.Invoke(new OnVariableEvent());
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

        public XorInt32(int value = 0)
        {
            offset = Ticks;
            origin = value ^ offset;
            buffer = (offset >> 8) ^ value;
        }

        public static implicit operator int(XorInt32 data)
        {
            return data.Value;
        }

        public static implicit operator XorInt32(int data)
        {
            return new XorInt32(data);
        }

        public bool Equals(XorInt32 other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is XorInt32 other && Equals(other);
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
        public int GetBit(int shift, int mask)
        {
            return (Value >> shift) & (1 << mask) - 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public XorInt32 SetBit(int shift, int mask, int value)
        {
            return (Value & ~((1 << mask) - 1 << shift)) | ((value & (1 << mask) - 1) << shift);
        }
    }

    [Serializable]
    public struct XorInt64 : IEquatable<XorInt64>
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
                    EventManager.Invoke(new OnVariableEvent());
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

        public XorInt64(long value = 0)
        {
            offset = Ticks;
            origin = value ^ offset;
            buffer = (offset >> 8) ^ value;
        }

        public static implicit operator long(XorInt64 data)
        {
            return data.Value;
        }

        public static implicit operator XorInt64(long data)
        {
            return new XorInt64(data);
        }

        public bool Equals(XorInt64 other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is XorInt64 other && Equals(other);
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
        public int GetBit(int shift, int mask)
        {
            return (int)((Value >> shift) & (1L << mask) - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public XorInt64 SetBit(int shift, int mask, int value)
        {
            return (Value & ~((1L << mask) - 1 << shift)) | ((value & (1L << mask) - 1) << shift);
        }
    }

    [Serializable]
    public struct XorBytes : IEquatable<XorBytes>
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
                    EventManager.Invoke(new OnVariableEvent());
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

        public XorBytes(byte[] value)
        {
            buffer = 0;
            offset = Ticks;
            origin = value;
            buffer = GetHashCode();
        }

        public static implicit operator byte[](XorBytes variable)
        {
            return variable.Value;
        }

        public static implicit operator XorBytes(byte[] value)
        {
            return new XorBytes(value);
        }

        public bool Equals(XorBytes other)
        {
            return buffer - offset == other.buffer - other.offset;
        }

        public override bool Equals(object obj)
        {
            return obj is XorBytes other && Equals(other);
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
    }
}

namespace Astraia
{
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
            if (poolData.TryGetValue(type, out var item))
            {
                return (Pool<T>)item;
            }

            item = new Pool<T>(type, type.Name);
            poolData.Add(type, item);
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
            private readonly HashSet<T> cached = new HashSet<T>();
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
                T item;
                if (unused.Count > 0)
                {
                    item = unused.Dequeue();
                    cached.Remove(item);
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
                if (cached.Add(item))
                {
                    Acquire--;
                    Release++;
                    unused.Enqueue(item);
                }
            }

            void IDisposable.Dispose()
            {
                cached.Clear();
                unused.Clear();
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

    internal readonly struct Pool : IPool
    {
        public Type Type { get; }
        public string Path { get; }
        public int Acquire { get; }
        public int Release { get; }
        public int Dequeue { get; }
        public int Enqueue { get; }

        public Pool(IPool pool)
        {
            Type = pool.Type;
            Path = pool.Path;
            Acquire = pool.Acquire;
            Release = pool.Release;
            Dequeue = pool.Dequeue;
            Enqueue = pool.Enqueue;
        }

        public override string ToString()
        {
            var result = string.Empty;
            result += Release.ToString().Align(10);
            result += Acquire.ToString().Align(10);
            result += Dequeue.ToString().Align(10);
            result += Enqueue.ToString().Align(10);
            return result;
        }

        public void Dispose()
        {
        }
    }
}

namespace Astraia
{
    public static class EventManager
    {
        internal static readonly Dictionary<Type, IPool> poolData = new Dictionary<Type, IPool>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Listen<T>(IEvent<T> data) where T : IEvent
        {
            LoadPool<T>().Listen(data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Remove<T>(IEvent<T> data) where T : IEvent
        {
            LoadPool<T>().Remove(data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Invoke<T>(T data) where T : IEvent
        {
            LoadPool<T>().Invoke(data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Pool<T> LoadPool<T>() where T : IEvent
        {
            if (poolData.TryGetValue(typeof(T), out var pool))
            {
                return (Pool<T>)pool;
            }

            pool = new Pool<T>(typeof(T), typeof(T).Name);
            poolData.Add(typeof(T), pool);
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

        private class Pool<T> : IPool where T : IEvent
        {
            private readonly HashSet<IEvent<T>> cached = new HashSet<IEvent<T>>();
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
                if (cached.Add(obj))
                {
                    Dequeue++;
                    Acquire++;
                    OnExecute += obj.Execute;
                }
            }

            public void Remove(IEvent<T> obj)
            {
                if (cached.Remove(obj))
                {
                    Enqueue++;
                    Acquire--;
                    OnExecute -= obj.Execute;
                }
            }

            public void Invoke(T message)
            {
                Release++;
                OnExecute?.Invoke(message);
            }

            void IDisposable.Dispose()
            {
                cached.Clear();
                OnExecute = null;
            }
        }
    }

    public interface IEvent
    {
    }

    public interface IEvent<in T> where T : IEvent
    {
        void Execute(T message);

        void Listen() => EventManager.Listen(this);

        void Remove() => EventManager.Remove(this);
    }
}

namespace Astraia
{
    internal static class TimeManager
    {
        public static float Time;
        public static event Action OnUpdate;

        public static void Update(float value)
        {
            Time = value;
            OnUpdate?.Invoke();
        }

        public static void Dispose()
        {
            Time = 0;
            OnUpdate = null;
        }
    }
}

namespace Astraia
{
    public interface ISystem
    {
        void Update();
        void AddEvent() => TimeManager.OnUpdate += Update;
        void SubEvent() => TimeManager.OnUpdate -= Update;
    }

    public interface IActive
    {
        void OnShow();
        void OnHide();
    }

    public interface IAcquire
    {
        void Acquire(object item);
    }

    public interface IState : IAcquire
    {
        void OnEnter();
        void OnUpdate();
        void OnExit();
    }

    public interface IModule : IAcquire
    {
        void Dequeue();
        void Enqueue();
    }

    public abstract class Acquire<T> : IAcquire
    {
        [NonSerialized] public T owner;
        void IAcquire.Acquire(object item) => owner = (T)item;
    }

    public abstract class State<T> : Acquire<T>, IState
    {
        public virtual void OnEnter()
        {
        }

        public virtual void OnUpdate()
        {
        }

        public virtual void OnExit()
        {
        }
    }

    public abstract class StateMachine<TKey, T> : Acquire<T>, IModule
    {
        private readonly Dictionary<TKey, IState> states = new Dictionary<TKey, IState>();
        private IState state;

        public void Create<TState>(TKey key) where TState : IState
        {
            var item = HeapManager.Dequeue<IState>(typeof(TState));
            item.Acquire(owner);
            states[key] = item;
        }

        public void Switch(TKey key)
        {
            state?.OnExit();
            states.TryGetValue(key, out state);
            state?.OnEnter();
        }

        public void Update()
        {
            state?.OnUpdate();
        }

        public virtual void Dequeue()
        {
        }

        public virtual void Enqueue()
        {
            foreach (var item in states.Values)
            {
                HeapManager.Enqueue(item, item.GetType());
            }

            state = null;
            states.Clear();
        }
    }

    public abstract class Blackboard<TKey, T> : Acquire<T>, IModule
    {
        private readonly Dictionary<TKey, int> properties = new Dictionary<TKey, int>();
        private const int SCALE = 100;

        public int GetInt(TKey key)
        {
            properties.TryAdd(key, 0);
            return properties[key] / SCALE;
        }

        public void SetInt(TKey key, int value)
        {
            properties[key] = value * SCALE;
        }

        public void AddInt(TKey key, int value)
        {
            properties.TryAdd(key, 0);
            properties[key] += value * SCALE;
        }

        public void SubInt(TKey key, int value)
        {
            properties.TryAdd(key, 0);
            properties[key] -= value * SCALE;
        }

        public float GetFloat(TKey key)
        {
            properties.TryAdd(key, 0);
            return properties[key] / (float)SCALE;
        }

        public void SetFloat(TKey key, float value)
        {
            properties[key] = (int)Math.Round(value * SCALE);
        }

        public void AddFloat(TKey key, float value)
        {
            properties.TryAdd(key, 0);
            properties[key] += (int)Math.Round(value * SCALE);
        }

        public void SubFloat(TKey key, float value)
        {
            properties.TryAdd(key, 0);
            properties[key] -= (int)Math.Round(value * SCALE);
        }

        public virtual void Dequeue()
        {
        }

        public virtual void Enqueue()
        {
            properties.Clear();
        }
    }

    public class Dictionary<TKey> : IDisposable
    {
        private readonly Dictionary<Type, IDictionary> Items = new Dictionary<Type, IDictionary>();

        public void Set<T>(TKey key, T value)
        {
            GetDict<T>()[key] = value;
        }

        public T Get<T>(TKey key)
        {
            return GetDict<T>().GetValueOrDefault(key);
        }

        public void Add<T>(TKey key, T value)
        {
            GetDict<T>().Add(key, value);
        }

        public void Remove<T>(TKey key)
        {
            GetDict<T>().Remove(key);
        }

        public bool ContainsKey<T>(TKey key)
        {
            return GetDict<T>().ContainsKey(key);
        }

        public ICollection<TKey> GetKeys<T>()
        {
            return GetDict<T>().Keys;
        }

        public ICollection<T> GetValues<T>()
        {
            return GetDict<T>().Values;
        }

        public void Clear<T>()
        {
            GetDict<T>().Clear();
        }

        public void Dispose()
        {
            foreach (var item in Items.Values)
            {
                item.Clear();
            }

            Items.Clear();
        }

        private Dictionary<TKey, T> GetDict<T>()
        {
            if (!Items.TryGetValue(typeof(T), out var items))
            {
                items = new Dictionary<TKey, T>();
                Items.Add(typeof(T), items);
            }

            return (Dictionary<TKey, T>)items;
        }
    }
}

namespace Astraia
{
    public abstract class Tick<T> : ISystem, INotifyCompletion
    {
        protected T owner;
        protected int complete;
        protected float nextTime;
        protected float duration;
        protected Action onNext;
        protected Action onComplete;
        public bool IsCompleted => complete > 0;

        void ISystem.Update()
        {
            try
            {
                if (IsActive())
                {
                    OnTick();
                }
                else
                {
                    Break();
                }
            }
            catch (Exception e)
            {
                Break();
                Log.Info("无法执行异步方法：\n{0}".Format(e));
            }
        }

        protected abstract bool IsActive();
        protected abstract void OnTick();

        public void Break()
        {
            onComplete.Invoke();
        }

        public void OnComplete(Action onComplete)
        {
            this.onComplete += onComplete;
        }

        public bool GetResult()
        {
            return complete == 2;
        }

        public Tick<T> GetAwaiter()
        {
            return this;
        }

        void INotifyCompletion.OnCompleted(Action onNext)
        {
            if (!IsActive())
            {
                Break();
                return;
            }

            this.onNext = onNext;
        }
    }
}

namespace Astraia
{
    public enum Node : byte
    {
        Running,
        Success,
        Failure
    }

    public interface INode
    {
        Node OnTick();
    }

    public abstract class CompositeNode : INode
    {
        protected IList<INode> nodes;
        public void Create(params INode[] nodes) => this.nodes = nodes;
        public abstract Node OnTick();
    }

    public abstract class DecoratorNode : INode
    {
        protected INode node;
        public void Create(INode node) => this.node = node;
        public abstract Node OnTick();
    }

    [Serializable]
    public sealed class Interval : INode
    {
        private float interval;
        public void Create(float duration) => interval = TimeManager.Time + duration;
        public Node OnTick() => interval < TimeManager.Time ? Node.Success : Node.Running;
    }

    [Serializable]
    public sealed class Sequence : CompositeNode
    {
        private int index;

        public override Node OnTick()
        {
            while (index < nodes.Count)
            {
                var result = nodes[index].OnTick();
                if (result == Node.Running)
                {
                    return Node.Running;
                }

                if (result == Node.Failure)
                {
                    index = 0;
                    return Node.Failure;
                }

                index++;
            }

            index = 0;
            return Node.Success;
        }
    }

    [Serializable]
    public sealed class Selector : CompositeNode
    {
        private int index;

        public override Node OnTick()
        {
            while (index < nodes.Count)
            {
                var result = nodes[index].OnTick();
                if (result == Node.Running)
                {
                    return Node.Running;
                }

                if (result == Node.Success)
                {
                    index = 0;
                    return Node.Success;
                }

                index++;
            }

            index = 0;
            return Node.Failure;
        }
    }

    [Serializable]
    public sealed class Parallel : CompositeNode
    {
        private Mode mode;

        public override Node OnTick()
        {
            var isAll = true;
            var isAny = false;

            foreach (var child in nodes)
            {
                var result = child.OnTick();
                switch (mode)
                {
                    case Mode.Any:
                        if (result == Node.Success)
                        {
                            return Node.Success;
                        }

                        if (result == Node.Failure)
                        {
                            return Node.Failure;
                        }

                        break;

                    case Mode.All:
                        if (result == Node.Failure)
                        {
                            return Node.Failure;
                        }

                        if (result != Node.Success)
                        {
                            isAll = false;
                        }

                        if (result == Node.Success)
                        {
                            isAny = true;
                        }

                        break;
                }
            }

            if (mode == Mode.All)
            {
                return isAll ? Node.Success : Node.Running;
            }

            return isAny ? Node.Success : Node.Running;
        }

        public enum Mode
        {
            All,
            Any
        }
    }

    [Serializable]
    public sealed class Actuator : CompositeNode
    {
        private int index = -1;

        public override Node OnTick()
        {
            if (index == -1)
            {
                index = Seed.Next(nodes.Count);
            }

            var result = nodes[index].OnTick();
            if (result == Node.Running)
            {
                return Node.Running;
            }

            index = -1;
            return result;
        }
    }

    [Serializable]
    public sealed class Repeater : DecoratorNode
    {
        private int count = -1;
        private int index;

        public override Node OnTick()
        {
            var result = node.OnTick();
            if (result == Node.Running)
            {
                return Node.Running;
            }

            index++;
            if (count < 0 || index < count)
            {
                return Node.Running;
            }

            index = 0;
            return Node.Success;
        }
    }

    [Serializable]
    public sealed class Inverter : DecoratorNode
    {
        public override Node OnTick()
        {
            var result = node.OnTick();
            if (result == Node.Success)
            {
                return Node.Failure;
            }

            if (result == Node.Failure)
            {
                return Node.Success;
            }

            return Node.Running;
        }
    }

    [Serializable]
    public sealed class Success : DecoratorNode
    {
        public override Node OnTick()
        {
            return node.OnTick() == Node.Running ? Node.Running : Node.Success;
        }
    }

    [Serializable]
    public sealed class Failure : DecoratorNode
    {
        public override Node OnTick()
        {
            return node.OnTick() == Node.Running ? Node.Running : Node.Failure;
        }
    }
}