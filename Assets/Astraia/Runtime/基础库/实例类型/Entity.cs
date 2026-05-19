using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Astraia
{
    public record OnVariableEvent : IEvent;

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
        public Xor32 SetBit(int shift, int bits, int value)
        {
            var mask = ((1 << bits) - 1) << shift;
            return (Value & ~mask) | ((value << shift) & mask);
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
        public Xor64 SetBit(int shift, int bits, int value)
        {
            var mask = ((1L << bits) - 1L) << shift;
            return (Value & ~mask) | ((value << shift) & mask);
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
}

namespace Astraia
{
    [Serializable]
    public class Enumerable<T> : IEnumerable<T>
    {
        public readonly T[] items;
        public int count;

        public Enumerable(int count)
        {
            items = new T[count];
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(items, count);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public struct Enumerator : IEnumerator<T>
        {
            private readonly T[] items;
            private readonly int count;
            private int index;
            public T Current => items[index];
            object IEnumerator.Current => items[index];

            public Enumerator(T[] items, int count)
            {
                this.items = items;
                this.count = count;
                index = -1;
            }

            public bool MoveNext()
            {
                index++;
                return index < count;
            }

            public void Reset()
            {
                index = -1;
            }

            public void Dispose()
            {
                index = -1;
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
    }
}

namespace Astraia
{
    internal static class TimeManager
    {
        public static float TimeStep;
        public static event Action OnUpdate;

        public static void Update(float value)
        {
            TimeStep = value;
            OnUpdate?.Invoke();
        }

        public static void Dispose()
        {
            TimeStep = 0;
            OnUpdate = null;
        }
    }
}

namespace Astraia
{
    public interface IState
    {
        void Acquire(object item);
        void OnEnter();
        void OnUpdate();
        void OnExit();
    }

    public abstract class State<T> : IState
    {
        public T owner;

        void IState.Acquire(object item)
        {
            owner = (T)item;
        }

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

    public sealed class StateMachine<T>
    {
        private readonly Dictionary<T, IState> states = new Dictionary<T, IState>();
        private IState state;

        public void Create<TState>(object owner, T key) where TState : IState
        {
            var item = HeapManager.Dequeue<IState>(typeof(TState));
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
                HeapManager.Enqueue(item, item.GetType());
            }

            state = null;
            states.Clear();
        }
    }

    public sealed class Blackboard<T>
    {
        private readonly Dictionary<T, int> properties = new Dictionary<T, int>();

        public int GetInt(T key)
        {
            return properties[key] / 100;
        }

        public void SetInt(T key, int value)
        {
            properties[key] = value * 100;
        }

        public void AddInt(T key, int value)
        {
            properties[key] += value * 100;
        }

        public void SubInt(T key, int value)
        {
            properties[key] -= value * 100;
        }

        public float GetFloat(T key)
        {
            return properties[key] / 100F;
        }

        public void SetFloat(T key, float value)
        {
            properties[key] = (int)Math.Round(value * 100);
        }

        public void AddFloat(T key, float value)
        {
            properties[key] += (int)Math.Round(value * 100);
        }

        public void SubFloat(T key, float value)
        {
            properties[key] -= (int)Math.Round(value * 100);
        }

        public void Clear()
        {
            properties.Clear();
        }
    }

    public sealed class Dictionary<T>
    {
        private readonly Dictionary<Type, IDictionary> Items = new Dictionary<Type, IDictionary>();

        public void Set<TValue>(T key, TValue value)
        {
            if (!Items.TryGetValue(typeof(TValue), out var items))
            {
                items = new Dictionary<T, TValue>();
                Items.Add(typeof(TValue), items);
            }

            ((Dictionary<T, TValue>)items)[key] = value;
        }

        public bool TryGet<TValue>(T key, out TValue value)
        {
            if (!Items.TryGetValue(typeof(TValue), out var items))
            {
                items = new Dictionary<T, TValue>();
                Items.Add(typeof(TValue), items);
            }

            return ((Dictionary<T, TValue>)items).TryGetValue(key, out value);
        }

        public void Clear()
        {
            foreach (var item in Items.Values)
            {
                item.Clear();
            }

            Items.Clear();
        }
    }
}

namespace Astraia
{
    public abstract class Tick : INotifyCompletion
    {
        public interface IAdaptor
        {
            bool isActive { get; }
        }

        protected IAdaptor owner;
        protected short complete;
        protected float nextTime;
        protected float duration;
        protected Action onNext;
        protected Action onComplete;
        public bool IsCompleted => complete != 0;

        protected void Update()
        {
            try
            {
                if (owner.isActive)
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

        protected abstract void OnComplete();
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

        public Tick GetAwaiter()
        {
            return this;
        }

        void INotifyCompletion.OnCompleted(Action onNext)
        {
            if (!owner.isActive)
            {
                Break();
                return;
            }

            this.onNext = onNext;
        }
    }

    public sealed class Timer : Tick
    {
        private int progress;
        private Action onUpdate;

        internal static Timer Create(IAdaptor owner, float duration)
        {
            var item = HeapManager.Dequeue<Timer>();
            TimeManager.OnUpdate += item.Update;
            item.owner = owner;
            item.progress = 1;
            item.complete = 0;
            item.duration = duration;
            item.nextTime = TimeManager.TimeStep + duration;
            item.onComplete = item.OnComplete;
            return item;
        }

        protected override void OnComplete()
        {
            complete = 1;
            owner = null;
            onNext = null;
            onUpdate = null;
            TimeManager.OnUpdate -= Update;
            HeapManager.Enqueue(this);
        }

        protected override void OnTick()
        {
            if (nextTime < TimeManager.TimeStep)
            {
                nextTime = TimeManager.TimeStep + duration;
                if (onUpdate != null)
                {
                    onUpdate.Invoke();
                }

                progress--;
                if (progress == 0)
                {
                    complete = 2;
                    onComplete += onNext;
                    onComplete.Invoke();
                }
            }
        }

        public Timer OnUpdate(Action onUpdate)
        {
            this.onUpdate += onUpdate;
            return this;
        }

        public Timer Set(float duration)
        {
            this.duration = duration;
            nextTime = TimeManager.TimeStep + duration;
            return this;
        }

        public Timer Add(float duration)
        {
            nextTime += duration;
            return this;
        }

        public Timer Loops(int progress = 0)
        {
            this.progress = progress;
            return this;
        }
    }

    public sealed class Tween : Tick
    {
        private float progress;
        private Action<float> onUpdate;

        internal static Tween Create(IAdaptor owner, float duration)
        {
            var item = HeapManager.Dequeue<Tween>();
            TimeManager.OnUpdate += item.Update;
            item.owner = owner;
            item.progress = 0;
            item.complete = 0;
            item.duration = duration;
            item.nextTime = TimeManager.TimeStep;
            item.onComplete = item.OnComplete;
            return item;
        }

        protected override void OnComplete()
        {
            complete = 1;
            owner = null;
            onNext = null;
            onUpdate = null;
            TimeManager.OnUpdate -= Update;
            HeapManager.Enqueue(this);
        }

        protected override void OnTick()
        {
            if (nextTime < TimeManager.TimeStep)
            {
                progress = (TimeManager.TimeStep - nextTime) / duration;
                if (progress > 1)
                {
                    progress = 1;
                }

                onUpdate.Invoke(progress);
                if (progress >= 1)
                {
                    complete = 2;
                    onComplete += onNext;
                    onComplete.Invoke();
                }
            }
        }

        public Tween OnUpdate(Action<float> onUpdate)
        {
            this.onUpdate += onUpdate;
            return this;
        }
    }
}

namespace Astraia
{
    public enum BTState
    {
        Running,
        Success,
        Failure
    }

    public interface INode
    {
        BTState OnTick(Dictionary<int> root);
    }

    public readonly struct Sequence : INode
    {
        private readonly INode[] Nodes;
        private readonly int Index;

        public Sequence(int index, INode[] nodes)
        {
            Index = index;
            Nodes = nodes ?? Array.Empty<INode>();
        }

        BTState INode.OnTick(Dictionary<int> root)
        {
            if (!root.TryGet(0, out int[] reason))
            {
                return BTState.Failure;
            }

            while (reason[Index] < Nodes.Length)
            {
                var result = Nodes[reason[Index]].OnTick(root);
                if (result == BTState.Running)
                {
                    return BTState.Running;
                }

                if (result == BTState.Failure)
                {
                    reason[Index] = 0;
                    return BTState.Failure;
                }

                reason[Index]++;
            }

            reason[Index] = 0;
            return BTState.Success;
        }
    }

    public readonly struct Selector : INode
    {
        private readonly INode[] Nodes;
        private readonly int Index;

        public Selector(int index, INode[] nodes)
        {
            Index = index;
            Nodes = nodes ?? Array.Empty<INode>();
        }

        BTState INode.OnTick(Dictionary<int> root)
        {
            if (!root.TryGet(0, out int[] reason))
            {
                return BTState.Failure;
            }

            while (reason[Index] < Nodes.Length)
            {
                var result = Nodes[reason[Index]].OnTick(root);
                if (result == BTState.Running)
                {
                    return BTState.Running;
                }

                if (result == BTState.Success)
                {
                    reason[Index] = 0;
                    return BTState.Success;
                }

                reason[Index]++;
            }

            reason[Index] = 0;
            return BTState.Failure;
        }
    }

    public readonly struct Parallel : INode
    {
        private readonly INode[] Nodes;
        private readonly bool IsAny;

        public Parallel(int index, INode[] nodes)
        {
            IsAny = index != 0;
            Nodes = nodes ?? Array.Empty<INode>();
        }

        BTState INode.OnTick(Dictionary<int> root)
        {
            var isAll = true;
            var isAny = false;

            foreach (var reason in Nodes)
            {
                var result = reason.OnTick(root);
                if (IsAny)
                {
                    if (result == BTState.Success)
                    {
                        return BTState.Success;
                    }

                    if (result == BTState.Failure)
                    {
                        return BTState.Failure;
                    }
                }
                else
                {
                    if (result == BTState.Failure)
                    {
                        return BTState.Failure;
                    }

                    if (result != BTState.Success)
                    {
                        isAll = false;
                    }

                    if (result == BTState.Success)
                    {
                        isAny = true;
                    }
                }
            }

            if (IsAny)
            {
                return isAny ? BTState.Success : BTState.Running;
            }

            return isAll ? BTState.Success : BTState.Running;
        }
    }

    public readonly struct Actuator : INode
    {
        private readonly INode[] Nodes;
        private readonly int Index;

        public Actuator(int index, INode[] nodes)
        {
            Index = index;
            Nodes = nodes ?? Array.Empty<INode>();
        }

        BTState INode.OnTick(Dictionary<int> root)
        {
            if (!root.TryGet(0, out int[] reason))
            {
                return BTState.Failure;
            }

            if (reason[Index] == 0)
            {
                reason[Index] = Seed.Next(Nodes.Length) + 1;
            }

            var result = Nodes[reason[Index] - 1].OnTick(root);
            if (result == BTState.Running)
            {
                return BTState.Running;
            }

            reason[Index] = 0;
            return result;
        }
    }

    public readonly struct Repeater : INode
    {
        private readonly INode Node;
        private readonly int Index;
        private readonly int Count;

        public Repeater(int index, int count, INode node)
        {
            Node = node;
            Index = index;
            Count = count;
        }

        BTState INode.OnTick(Dictionary<int> root)
        {
            if (!root.TryGet(0, out int[] reason))
            {
                return BTState.Failure;
            }

            var result = Node.OnTick(root);
            if (result == BTState.Running)
            {
                return BTState.Running;
            }

            reason[Index]++;
            if (Count < 0 || reason[Index] < Count)
            {
                return BTState.Running;
            }

            reason[Index] = 0;
            return BTState.Success;
        }
    }

    public readonly struct Inverter : INode
    {
        private readonly INode Node;

        public Inverter(INode node)
        {
            Node = node;
        }

        BTState INode.OnTick(Dictionary<int> root)
        {
            var result = Node.OnTick(root);
            if (result == BTState.Success)
            {
                return BTState.Failure;
            }

            if (result == BTState.Failure)
            {
                return BTState.Success;
            }

            return BTState.Running;
        }
    }

    public readonly struct Success : INode
    {
        private readonly INode Node;

        public Success(INode node)
        {
            Node = node;
        }

        BTState INode.OnTick(Dictionary<int> root)
        {
            return Node.OnTick(root) == BTState.Running ? BTState.Running : BTState.Success;
        }
    }

    public readonly struct Failure : INode
    {
        private readonly INode Node;

        public Failure(INode node)
        {
            Node = node;
        }

        BTState INode.OnTick(Dictionary<int> root)
        {
            return Node.OnTick(root) == BTState.Running ? BTState.Running : BTState.Failure;
        }
    }

    public static class Nodes
    {
        public static INode LoadNode(Node node, Func<Node, string> func)
        {
            if (node.Name.IsNullOrEmpty())
            {
                throw new NullReferenceException();
            }

            var root = Search.GetType(func.Invoke(node));
            if (root == typeof(Sequence))
            {
                return new Sequence(node.Index, LoadNodes(node, func));
            }

            if (root == typeof(Selector))
            {
                return new Selector(node.Index, LoadNodes(node, func));
            }

            if (root == typeof(Parallel))
            {
                return new Parallel(int.Parse(node.Data), LoadNodes(node, func));
            }

            if (root == typeof(Actuator))
            {
                return new Actuator(node.Index, LoadNodes(node, func));
            }

            if (root == typeof(Repeater))
            {
                return new Repeater(node.Index, int.Parse(node.Data), LoadNode(node.Nodes[0], func));
            }

            if (root == typeof(Inverter))
            {
                return new Inverter(LoadNode(node.Nodes[0], func));
            }

            if (root == typeof(Success))
            {
                return new Success(LoadNode(node.Nodes[0], func));
            }

            if (root == typeof(Failure))
            {
                return new Failure(LoadNode(node.Nodes[0], func));
            }

            return (INode)Activator.CreateInstance(root);
        }

        private static INode[] LoadNodes(Node node, Func<Node, string> func)
        {
            var nodes = new INode[node.Nodes.Count];
            for (var i = 0; i < node.Nodes.Count; i++)
            {
                nodes[i] = LoadNode(node.Nodes[i], func);
            }

            return nodes;
        }

        public static Node GetNode(string reason, ref int i)
        {
            if (string.IsNullOrEmpty(reason))
            {
                return default;
            }

            var index = reason.IndexOf('(');
            if (index < 0)
            {
                return new Node(reason, i++);
            }

            var result = new Node(reason.Substring(0, index).Trim(), i++);
            foreach (var child in LoadNode(Checked(reason, index)))
            {
                result.Nodes.Add(GetNode(child, ref i));
            }

            return result;
        }

        private static string Checked(string reason, int index)
        {
            var depth = 0;
            var count = index;
            while (count < reason.Length)
            {
                if (reason[count] == '(')
                {
                    depth++;
                }
                else if (reason[count] == ')')
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
                if (c == '(')
                {
                    depth++;
                }
                else if (c == ')')
                {
                    depth--;
                }
                else if (depth == 0 && c == '，')
                {
                    result.Add(reason.Substring(index, i - index).Trim());
                    index = i + 1;
                }
            }

            result.Add(reason.Substring(index).Trim());
            return result;
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
                var i = name.IndexOf(':');
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
        }
    }
}