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
    public abstract class Tick : ISystem, INotifyCompletion
    {
        public interface IAdaptor
        {
            bool isActive { get; }
        }

        protected IAdaptor owner;
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

    [Serializable]
    public sealed class Timer : Tick
    {
        private int progress;
        private Action onUpdate;

        internal static Timer Create(IAdaptor owner, float duration)
        {
            var item = HeapManager.Dequeue<Timer>();
            ((ISystem)item).AddEvent();
            item.owner = owner;
            item.progress = 1;
            item.complete = 0;
            item.duration = duration;
            item.nextTime = TimeManager.Time + duration;
            item.onComplete = OnComplete;
            return item;

            void OnComplete()
            {
                item.complete = 1;
                item.owner = null;
                item.onNext = null;
                item.onUpdate = null;
                ((ISystem)item).SubEvent();
                HeapManager.Enqueue(item);
            }
        }

        protected override void OnTick()
        {
            if (nextTime < TimeManager.Time)
            {
                nextTime = TimeManager.Time + duration;
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
            nextTime = TimeManager.Time + duration;
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

    [Serializable]
    public sealed class Tween : Tick
    {
        private float progress;
        private Action<float> onUpdate;

        internal static Tween Create(IAdaptor owner, float duration)
        {
            var item = HeapManager.Dequeue<Tween>();
            ((ISystem)item).AddEvent();
            item.owner = owner;
            item.progress = 0;
            item.complete = 0;
            item.duration = duration;
            item.nextTime = TimeManager.Time;
            item.onComplete = OnComplete;
            return item;

            void OnComplete()
            {
                item.complete = 1;
                item.owner = null;
                item.onNext = null;
                item.onUpdate = null;
                ((ISystem)item).SubEvent();
                HeapManager.Enqueue(item);
            }
        }

        protected override void OnTick()
        {
            if (nextTime < TimeManager.Time)
            {
                progress = (TimeManager.Time - nextTime) / duration;
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
    public enum Node : byte
    {
        Running,
        Success,
        Failure
    }

    public interface INode
    {
        Node OnTick();
        void Enqueue();
    }

    public abstract class CompositeNode : INode
    {
        public IList<INode> nodes = Array.Empty<INode>();
        public abstract Node OnTick();

        public void Enqueue()
        {
            foreach (var node in nodes)
            {
                node.Enqueue();
            }

            nodes = Array.Empty<INode>();
            HeapManager.Enqueue<INode>(this, GetType());
        }
    }

    public abstract class DecoratorNode : INode
    {
        public INode node;
        public abstract Node OnTick();

        public void Enqueue()
        {
            if (node != null)
            {
                node.Enqueue();
                node = null;
            }

            HeapManager.Enqueue<INode>(this, GetType());
        }
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
    public sealed class Interval : DecoratorNode
    {
        public float waitTime;

        public override Node OnTick()
        {
            return waitTime < TimeManager.Time ? Node.Success : Node.Running;
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


    public static class NodeUtils
    {
        [Serializable]
        public struct Node
        {
            public string Name;
            public List<Node> Item;

            public Node(string name)
            {
                Name = name;
                Item = new List<Node>();
            }
        }

        public static Node GetNode(string reason)
        {
            if (string.IsNullOrEmpty(reason))
            {
                return default;
            }

            var index = reason.IndexOf('(');
            if (index < 0)
            {
                return new Node(reason);
            }

            var result = reason.Substring(0, index).Trim();
            var target = Split(reason, index);

            var node = new Node(result);
            foreach (var child in Split(target))
            {
                node.Item.Add(GetNode(child));
            }

            return node;
        }

        private static string Split(string reason, int index)
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

        private static List<string> Split(string reason)
        {
            var result = new List<string>();
            var depth = 0;
            var index = 0;

            for (var i = 0; i < reason.Length; i++)
            {
                if (reason[i] == '(')
                {
                    depth++;
                }
                else if (reason[i] == ')')
                {
                    depth--;
                }
                else if ((reason[i] == ',' || reason[i] == '，') && depth == 0)
                {
                    result.Add(reason.Substring(index, i - index).Trim());
                    index = i + 1;
                }
            }

            result.Add(reason.Substring(index).Trim());
            return result;
        }
    }


    /// <summary>
    /// 环形缓冲区（队列）
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    public class RingQueue<T> : IEnumerable<T>
    {
        private T[] _buffer;
        private int _head; // 队头索引
        private int _tail; // 队尾索引
        private int _count; // 当前元素数量
        private readonly int _capacity;
        private readonly bool _allowOverwrite; // 是否允许覆盖

        /// <summary>
        /// 获取缓冲区容量
        /// </summary>
        public int Capacity => _capacity;

        /// <summary>
        /// 获取当前元素数量
        /// </summary>
        public int Count => _count;

        /// <summary>
        /// 获取是否为空
        /// </summary>
        public bool IsEmpty => _count == 0;

        /// <summary>
        /// 获取是否已满
        /// </summary>
        public bool IsFull => _count == _capacity;

        /// <summary>
        /// 初始化环形缓冲区
        /// </summary>
        /// <param name="capacity">容量</param>
        /// <param name="allowOverwrite">是否允许覆盖（满时自动覆盖最旧数据）</param>
        public RingQueue(int capacity, bool allowOverwrite = false)
        {
            if (capacity <= 0)
                throw new ArgumentException("容量必须大于0", nameof(capacity));

            _capacity = capacity;
            _allowOverwrite = allowOverwrite;
            _buffer = new T[capacity];
            _head = 0;
            _tail = 0;
            _count = 0;
        }

        /// <summary>
        /// 入队
        /// </summary>
        /// <param name="item">元素</param>
        /// <returns>是否成功入队（不允许覆盖且已满时返回false）</returns>
        public bool Enqueue(T item)
        {
            if (IsFull)
            {
                if (_allowOverwrite)
                {
                    // 覆盖最旧的数据
                    _buffer[_head] = item;
                    _head = (_head + 1) % _capacity;
                    _tail = (_tail + 1) % _capacity;
                    return true;
                }

                return false;
            }

            _buffer[_tail] = item;
            _tail = (_tail + 1) % _capacity;
            _count++;
            return true;
        }

        /// <summary>
        /// 强制入队（满时自动覆盖最旧数据）
        /// </summary>
        public void EnqueueForce(T item)
        {
            if (IsFull)
            {
                _buffer[_head] = item;
                _head = (_head + 1) % _capacity;
                _tail = (_tail + 1) % _capacity;
            }
            else
            {
                _buffer[_tail] = item;
                _tail = (_tail + 1) % _capacity;
                _count++;
            }
        }

        /// <summary>
        /// 出队
        /// </summary>
        /// <param name="item">出队元素</param>
        /// <returns>是否成功出队</returns>
        public bool Dequeue(out T item)
        {
            if (IsEmpty)
            {
                item = default;
                return false;
            }

            item = _buffer[_head];
            _buffer[_head] = default; // 清理引用
            _head = (_head + 1) % _capacity;
            _count--;
            return true;
        }

        /// <summary>
        /// 查看队头元素（不移除）
        /// </summary>
        public T Peek()
        {
            if (IsEmpty)
                throw new InvalidOperationException("队列为空");

            return _buffer[_head];
        }

        /// <summary>
        /// 尝试查看队头元素
        /// </summary>
        public bool TryPeek(out T item)
        {
            if (IsEmpty)
            {
                item = default;
                return false;
            }

            item = _buffer[_head];
            return true;
        }

        /// <summary>
        /// 清空队列
        /// </summary>
        public void Clear()
        {
            Array.Clear(_buffer, 0, _capacity);
            _head = 0;
            _tail = 0;
            _count = 0;
        }

        /// <summary>
        /// 获取指定位置的元素（从队头开始索引）
        /// </summary>
        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= _count)
                    throw new IndexOutOfRangeException("索引超出范围");

                int actualIndex = (_head + index) % _capacity;
                return _buffer[actualIndex];
            }
        }

        /// <summary>
        /// 将数据复制到数组
        /// </summary>
        public T[] ToArray()
        {
            T[] array = new T[_count];
            for (int i = 0; i < _count; i++)
            {
                array[i] = _buffer[(_head + i) % _capacity];
            }

            return array;
        }

        /// <summary>
        /// 获取枚举器
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < _count; i++)
            {
                yield return _buffer[(_head + i) % _capacity];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}