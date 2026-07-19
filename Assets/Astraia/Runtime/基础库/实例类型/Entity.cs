using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public interface IEvent
    {
    }

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
        void Acquire(object owner);
        void Dequeue();
        void Enqueue();
        void OnShow();
        void OnHide();
    }

    internal interface IState
    {
        void Acquire(object owner);
        void OnEnter();
        void OnUpdate();
        void OnExit();
    }

    [Serializable]
    public abstract class Module<T> : IModule
    {
        public T owner { get; internal set; }
        void IModule.Acquire(object owner) => this.owner = (T)owner;
        void IModule.Dequeue() => Dequeue();
        void IModule.Enqueue() => Enqueue();
        void IModule.OnShow() => OnShow();
        void IModule.OnHide() => OnHide();

        protected virtual void Dequeue()
        {
        }

        protected virtual void Enqueue()
        {
        }

        protected virtual void OnShow()
        {
        }

        protected virtual void OnHide()
        {
        }
    }

    [Serializable]
    public abstract class State<T> : IState
    {
        public T owner { get; internal set; }
        void IState.Acquire(object owner) => this.owner = (T)owner;
        void IState.OnEnter() => OnEnter();
        void IState.OnUpdate() => OnUpdate();
        void IState.OnExit() => OnExit();

        protected virtual void OnEnter()
        {
        }

        protected virtual void OnUpdate()
        {
        }

        protected virtual void OnExit()
        {
        }
    }

    [Serializable]
    public class StateMachine<T>
    {
        private Dictionary<T, IState> states = new Dictionary<T, IState>();
        private IState state;

        public void Create<TState>(object owner, T key)
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

        public void Dispose()
        {
            foreach (var item in states.Values)
            {
                HeapManager.Enqueue(item, item.GetType());
            }

            state = null;
            states.Clear();
        }
    }

    [Serializable]
    public class Blackboard<T> : Dictionary<T, int>
    {
        public int GetInt(T key)
        {
            TryGetValue(key, out var value);
            return value / 100;
        }

        public void SetInt(T key, int value)
        {
            this[key] = value * 100;
        }

        public void AddInt(T key, int value)
        {
            TryGetValue(key, out var v);
            this[key] = v + value * 100;
        }

        public void SubInt(T key, int value)
        {
            TryGetValue(key, out var v);
            this[key] = v - value * 100;
        }

        public float GetFloat(T key)
        {
            TryGetValue(key, out var value);
            return value / 100F;
        }

        public void SetFloat(T key, float value)
        {
            this[key] = (int)Math.Round(value * 100);
        }

        public void AddFloat(T key, float value)
        {
            TryGetValue(key, out var v);
            this[key] = v + (int)Math.Round(value * 100);
        }

        public void SubFloat(T key, float value)
        {
            TryGetValue(key, out var v);
            this[key] = v - (int)Math.Round(value * 100);
        }
    }

    [Serializable]
    public class Dictionary<T> : Dictionary<Type, IDictionary>
    {
        public void Set<TValue>(T key, TValue value)
        {
            if (!TryGetValue(typeof(TValue), out var items))
            {
                items = new Dictionary<T, TValue>();
                Add(typeof(TValue), items);
            }

            ((Dictionary<T, TValue>)items)[key] = value;
        }

        public TValue Get<TValue>(T key)
        {
            if (!TryGetValue(typeof(TValue), out var items))
            {
                items = new Dictionary<T, TValue>();
                Add(typeof(TValue), items);
            }

            return ((Dictionary<T, TValue>)items).GetValueOrDefault(key);
        }

        public bool TryGet<TValue>(T key, out TValue value)
        {
            if (!TryGetValue(typeof(TValue), out var items))
            {
                items = new Dictionary<T, TValue>();
                Add(typeof(TValue), items);
            }

            return ((Dictionary<T, TValue>)items).TryGetValue(key, out value);
        }
    }

    public struct OnEarlyUpdate : IEvent
    {
    }

    public struct OnAfterUpdate : IEvent
    {
    }

    public struct OnFixedUpdate : IEvent
    {
    }

    public struct OnGizmoUpdate : IEvent
    {
    }

    internal interface IAsync
    {
        bool isActive { get; }
    }

    [Serializable]
    public abstract class Async : INotifyCompletion, IEvent<OnEarlyUpdate>
    {
        internal IAsync owner;

        protected short state;
        protected float waitTime;
        protected float duration;
        protected Action onWaitable;
        protected Action onComplete;
        protected Func<float> onTime;
        public bool IsCompleted => state != 0;

        void IEvent<OnEarlyUpdate>.Execute(OnEarlyUpdate message)
        {
            try
            {
                if (owner.isActive)
                {
                    Update();
                    return;
                }

                Break();
            }
            catch (Exception e)
            {
                Log.Info("打断异步方法：\n{0}".Format(e));
                Break();
            }
        }

        protected abstract void Update();

        public void Break()
        {
            onComplete.Invoke();
        }

        public void OnComplete(Action complete)
        {
            onComplete += complete;
        }

        public int GetResult()
        {
            return state;
        }

        public Async GetAwaiter()
        {
            return this;
        }

        void INotifyCompletion.OnCompleted(Action waitable)
        {
            if (owner.isActive)
            {
                onWaitable = waitable;
            }
            else
            {
                Break();
            }
        }
    }

    [Serializable]
    public sealed class Timer : Async
    {
        private int progress;
        private Action onUpdate;

        internal static Timer Create(IAsync owner, Func<float> onTime, float duration)
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
            onWaitable = null;
            EventManager.Remove(this);
            HeapManager.Enqueue(this);
        }

        protected override void Update()
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
                    state |= 1 << 1;
                    onComplete += onWaitable;
                    onComplete.Invoke();
                }
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

        public Timer SetTime(Func<float> onTime)
        {
            this.onTime = onTime;
            waitTime = duration + onTime();
            return this;
        }
    }

    [Serializable]
    public sealed class Tween : Async
    {
        private float progress;
        private Action<float> onUpdate;

        internal static Tween Create(IAsync owner, Func<float> onTime, float duration)
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
            onWaitable = null;
            EventManager.Remove(this);
            HeapManager.Enqueue(this);
        }

        protected override void Update()
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
                    state |= 1 << 1;
                    onComplete += onWaitable;
                    onComplete.Invoke();
                }
            }
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
    }

    public enum BTState
    {
        Running,
        Success,
        Failure
    }

    public interface INode
    {
        BTState OnTick(int index, Dictionary<int> root);
    }

    [Serializable]
    public struct Sequence : INode
    {
        private INode[] Nodes;
        private int Index;

        public Sequence(int index, INode[] nodes)
        {
            Index = index;
            Nodes = nodes ?? Array.Empty<INode>();
        }

        public BTState OnTick(int index, Dictionary<int> root)
        {
            var reason = root.Get<int[]>(index);
            while (reason[Index] < Nodes.Length)
            {
                var result = Nodes[reason[Index]].OnTick(index, root);
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

    [Serializable]
    public struct Selector : INode
    {
        private INode[] Nodes;
        private int Index;

        public Selector(int index, INode[] nodes)
        {
            Index = index;
            Nodes = nodes ?? Array.Empty<INode>();
        }

        public BTState OnTick(int index, Dictionary<int> root)
        {
            var reason = root.Get<int[]>(index);
            while (reason[Index] < Nodes.Length)
            {
                var result = Nodes[reason[Index]].OnTick(index, root);
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

    [Serializable]
    public struct Parallel : INode
    {
        private INode[] Nodes;
        private bool IsAny;

        public Parallel(int index, INode[] nodes)
        {
            IsAny = index != 0;
            Nodes = nodes ?? Array.Empty<INode>();
        }

        public BTState OnTick(int index, Dictionary<int> root)
        {
            if (IsAny)
            {
                foreach (var node in Nodes)
                {
                    var result = node.OnTick(index, root);
                    if (result == BTState.Success)
                    {
                        return BTState.Success;
                    }

                    if (result == BTState.Failure)
                    {
                        return BTState.Failure;
                    }
                }

                return BTState.Running;
            }

            var IsAll = true;
            foreach (var node in Nodes)
            {
                var result = node.OnTick(index, root);
                if (result == BTState.Failure)
                {
                    return BTState.Failure;
                }

                if (result == BTState.Running)
                {
                    IsAll = false;
                }
            }

            return IsAll ? BTState.Success : BTState.Running;
        }
    }

    [Serializable]
    public struct Randomer : INode
    {
        private INode[] Nodes;
        private int Index;

        public Randomer(int index, INode[] nodes)
        {
            Index = index;
            Nodes = nodes ?? Array.Empty<INode>();
        }

        public BTState OnTick(int index, Dictionary<int> root)
        {
            var reason = root.Get<int[]>(index);
            if (reason[Index] == 0)
            {
                reason[Index] = Seed.Next(Nodes.Length) + 1;
            }

            var result = Nodes[reason[Index] - 1].OnTick(index, root);
            if (result == BTState.Running)
            {
                return BTState.Running;
            }

            reason[Index] = 0;
            return result;
        }
    }

    [Serializable]
    public struct Repeater : INode
    {
        private INode Node;
        private int Index;
        private int Count;

        public Repeater(int index, int count, INode node)
        {
            Node = node;
            Index = index;
            Count = count;
        }

        public BTState OnTick(int index, Dictionary<int> root)
        {
            var reason = root.Get<int[]>(index);
            var result = Node.OnTick(index, root);
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

    [Serializable]
    public struct Inverter : INode
    {
        private INode Node;

        public Inverter(INode node)
        {
            Node = node;
        }

        public BTState OnTick(int index, Dictionary<int> root)
        {
            var result = Node.OnTick(index, root);
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

    [Serializable]
    public struct Success : INode
    {
        private INode Node;

        public Success(INode node)
        {
            Node = node;
        }

        public BTState OnTick(int index, Dictionary<int> root)
        {
            return Node.OnTick(index, root) == BTState.Running ? BTState.Running : BTState.Success;
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

        public BTState OnTick(int index, Dictionary<int> root)
        {
            return Node.OnTick(index, root) == BTState.Running ? BTState.Running : BTState.Failure;
        }
    }

    public static class BTNode
    {
        private static readonly Dictionary<Type, Func<Node, INode>> NodeFunc = new();

        static BTNode()
        {
            NodeFunc[typeof(Sequence)] = Sequence;
            NodeFunc[typeof(Selector)] = Selector;
            NodeFunc[typeof(Parallel)] = Parallel;
            NodeFunc[typeof(Randomer)] = Randomer;
            NodeFunc[typeof(Repeater)] = Repeater;
            NodeFunc[typeof(Inverter)] = Inverter;
            NodeFunc[typeof(Success)] = Success;
            NodeFunc[typeof(Failure)] = Failure;
        }

        private static INode Sequence(Node node)
        {
            return new Sequence(node.Index, node.Nodes.Select(i => i.Build()).ToArray());
        }

        private static INode Selector(Node node)
        {
            return new Selector(node.Index, node.Nodes.Select(i => i.Build()).ToArray());
        }

        private static INode Parallel(Node node)
        {
            return new Parallel(int.Parse(node.Data), node.Nodes.Select(i => i.Build()).ToArray());
        }

        private static INode Randomer(Node node)
        {
            return new Randomer(node.Index, node.Nodes.Select(i => i.Build()).ToArray());
        }

        private static INode Repeater(Node node)
        {
            return new Repeater(node.Index, int.Parse(node.Data), node.Nodes[0].Build());
        }

        private static INode Inverter(Node node)
        {
            return new Inverter(node.Nodes[0].Build());
        }

        private static INode Success(Node node)
        {
            return new Success(node.Nodes[0].Build());
        }

        private static INode Failure(Node node)
        {
            return new Failure(node.Nodes[0].Build());
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

        [Serializable]
        public struct Node
        {
            public static Func<Node, Type> OnLoad;

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

            public INode Build()
            {
                if (Name.IsNullOrEmpty())
                {
                    throw new NullReferenceException();
                }

                if (NodeFunc.TryGetValue(OnLoad(this), out var result))
                {
                    return result.Invoke(this);
                }

                return (INode)Activator.CreateInstance(OnLoad(this));
            }
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
    }
}