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
            return (Value & ~((1 << bits) - 1 << shift)) | ((value & (1 << bits) - 1) << shift);
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
            return (Value & ~((1L << bits) - 1 << shift)) | ((value & (1L << bits) - 1) << shift);
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
    public sealed class Logic
    {
        internal interface IInject
        {
            object Inject(object target);
        }

        private readonly Dictionary<Type, IModule> modules = new Dictionary<Type, IModule>();
        private IInject inject;
        public event Action OnShow;
        public event Action OnHide;
        public event Action OnFade;

        public ICollection<IModule> Modules => modules.Values;

        internal Logic(IInject inject, List<string> modules)
        {
            this.inject = inject;
            foreach (var module in modules)
            {
                var result = Search.GetType(module);
                if (result != null)
                {
                    GetComponent(result, result);
                }
            }

            modules.Clear();
        }

        internal void Show()
        {
            OnShow?.Invoke();
        }

        internal void Hide()
        {
            OnHide?.Invoke();
        }

        internal void Clear()
        {
            OnFade?.Invoke();
            OnFade = null;
            OnShow = null;
            OnHide = null;
            inject = null;
            modules.Clear();
        }

        public T AddComponent<T>() where T : IModule
        {
            return (T)GetComponent(typeof(T), typeof(T));
        }

        public T AddComponent<T>(Type result) where T : IModule
        {
            return (T)GetComponent(typeof(T), result);
        }

        public T GetComponent<T>() where T : IModule
        {
            return (T)modules.GetValueOrDefault(typeof(T));
        }

        internal IModule GetComponent(Type source, Type result)
        {
            if (!modules.TryGetValue(source, out var module))
            {
                module = HeapManager.Dequeue<IModule>(result);
                modules.Add(source, module);
                module.Acquire(inject.Inject(module));
                module.Dequeue();
                OnFade += () =>
                {
                    module.Enqueue();
                    modules.Remove(source);
                    HeapManager.Enqueue(module, result);
                };

                var events = module.GetType().GetInterfaces();
                foreach (var @event in events)
                {
                    if (@event.IsGenericType && @event.GetGenericTypeDefinition() == typeof(IEvent<>))
                    {
                        var reason = typeof(IEvent<>).MakeGenericType(@event.GetGenericArguments());
                        OnShow += (Action)Delegate.CreateDelegate(typeof(Action), module, reason.GetMethod("Listen", Search.Instance)!);
                        OnHide += (Action)Delegate.CreateDelegate(typeof(Action), module, reason.GetMethod("Remove", Search.Instance)!);
                    }
                }

                if (module is ISystem system)
                {
                    OnShow += system.AddEvent;
                    OnHide += system.SubEvent;
                }

                if (module is IActive active)
                {
                    OnShow += active.OnShow;
                    OnHide += active.OnHide;
                }
            }

            return module;
        }
    }

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
            item.onComplete = item.OnComplete;
            return item;
        }

        protected override void OnComplete()
        {
            complete = 1;
            owner = null;
            onNext = null;
            onUpdate = null;
            ((ISystem)this).SubEvent();
            HeapManager.Enqueue(this);
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
            item.onComplete = item.OnComplete;
            return item;
        }

        protected override void OnComplete()
        {
            complete = 1;
            owner = null;
            onNext = null;
            onUpdate = null;
            ((ISystem)this).SubEvent();
            HeapManager.Enqueue(this);
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
    public abstract class Node
    {
        public abstract State OnTick();
        public abstract void Dispose();

        public enum State : byte
        {
            Running,
            Success,
            Failure
        }

        public readonly struct Map
        {
            public readonly string Name;
            public readonly List<Map> Root;

            public Map(string name)
            {
                Name = name;
                Root = new List<Map>();
            }
        }

        public static Map GetNode(string reason)
        {
            if (string.IsNullOrEmpty(reason))
            {
                return default;
            }

            var index = reason.IndexOf('(');
            if (index < 0)
            {
                return new Map(reason);
            }

            var result = new Map(reason.Substring(0, index).Trim());
            foreach (var child in LoadRoot(Checked(reason, index)))
            {
                result.Root.Add(GetNode(child));
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

        private static List<string> LoadRoot(string reason)
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

    public abstract class CompositeNode : Node
    {
        public IList<Node> nodes = Array.Empty<Node>();

        public override void Dispose()
        {
            foreach (var node in nodes)
            {
                node.Dispose();
            }

            nodes = Array.Empty<Node>();
            HeapManager.Enqueue<Node>(this, GetType());
        }
    }

    public abstract class DecoratorNode : Node
    {
        public Node node;

        public override void Dispose()
        {
            if (node != null)
            {
                node.Dispose();
                node = null;
            }

            HeapManager.Enqueue<Node>(this, GetType());
        }
    }

    [Serializable]
    public sealed class Sequence : CompositeNode
    {
        private int index;

        public override State OnTick()
        {
            while (index < nodes.Count)
            {
                var result = nodes[index].OnTick();
                if (result == State.Running)
                {
                    return State.Running;
                }

                if (result == State.Failure)
                {
                    index = 0;
                    return State.Failure;
                }

                index++;
            }

            index = 0;
            return State.Success;
        }
    }

    [Serializable]
    public sealed class Selector : CompositeNode
    {
        private int index;

        public override State OnTick()
        {
            while (index < nodes.Count)
            {
                var result = nodes[index].OnTick();
                if (result == State.Running)
                {
                    return State.Running;
                }

                if (result == State.Success)
                {
                    index = 0;
                    return State.Success;
                }

                index++;
            }

            index = 0;
            return State.Failure;
        }
    }

    [Serializable]
    public sealed class Parallel : CompositeNode
    {
        private Mode mode;

        public override State OnTick()
        {
            var isAll = true;
            var isAny = false;

            foreach (var child in nodes)
            {
                var result = child.OnTick();
                switch (mode)
                {
                    case Mode.Any:
                        if (result == State.Success)
                        {
                            return State.Success;
                        }

                        if (result == State.Failure)
                        {
                            return State.Failure;
                        }

                        break;

                    case Mode.All:
                        if (result == State.Failure)
                        {
                            return State.Failure;
                        }

                        if (result != State.Success)
                        {
                            isAll = false;
                        }

                        if (result == State.Success)
                        {
                            isAny = true;
                        }

                        break;
                }
            }

            if (mode == Mode.All)
            {
                return isAll ? State.Success : State.Running;
            }

            return isAny ? State.Success : State.Running;
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

        public override State OnTick()
        {
            if (index == -1)
            {
                index = Seed.Next(nodes.Count);
            }

            var result = nodes[index].OnTick();
            if (result == State.Running)
            {
                return State.Running;
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

        public override State OnTick()
        {
            var result = node.OnTick();
            if (result == State.Running)
            {
                return State.Running;
            }

            index++;
            if (count < 0 || index < count)
            {
                return State.Running;
            }

            index = 0;
            return State.Success;
        }
    }

    [Serializable]
    public sealed class Inverter : DecoratorNode
    {
        public override State OnTick()
        {
            var result = node.OnTick();
            if (result == State.Success)
            {
                return State.Failure;
            }

            if (result == State.Failure)
            {
                return State.Success;
            }

            return State.Running;
        }
    }

    [Serializable]
    public sealed class Interval : DecoratorNode
    {
        public float waitTime;

        public override State OnTick()
        {
            return waitTime < TimeManager.Time ? State.Success : State.Running;
        }
    }

    [Serializable]
    public sealed class Success : DecoratorNode
    {
        public override State OnTick()
        {
            return node.OnTick() == State.Running ? State.Running : State.Success;
        }
    }

    [Serializable]
    public sealed class Failure : DecoratorNode
    {
        public override State OnTick()
        {
            return node.OnTick() == State.Running ? State.Running : State.Failure;
        }
    }
}