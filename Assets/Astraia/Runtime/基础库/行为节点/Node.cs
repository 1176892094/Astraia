using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Astraia
{
    [Serializable]
    public readonly struct Properties<T> where T : unmanaged, Enum
    {
        private readonly Fixation[] properties;

        public Properties(int value)
        {
            properties = value != 0 ? new Fixation[value] : new Fixation[Seed.Count<T>()];
        }

        public float Get(T key)
        {
            return properties[key.Index()];
        }

        public void Set(T key, float value)
        {
            properties[key.Index()] = value;
        }

        public void Add(T key, float value)
        {
            properties[key.Index()] += value;
        }

        public void Sub(T key, float value)
        {
            properties[key.Index()] -= value;
        }

        public void Clear()
        {
            Array.Clear(properties, 0, properties.Length);
        }
    }

    [Serializable]
    public readonly struct Fixation : IEquatable<Fixation>
    {
        private const int BIT = 12;
        private const float FIX = 1 << BIT;

        public readonly int value;

        public Fixation(int value)
        {
            this.value = value;
        }

        public bool Equals(Fixation other)
        {
            return value == other.value;
        }

        public override bool Equals(object obj)
        {
            return obj is Fixation other && Equals(other);
        }

        public override int GetHashCode()
        {
            return value;
        }

        public override string ToString()
        {
            return (value / FIX).ToString("R");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(Fixation a, Fixation b)
        {
            return a.value < b.value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(Fixation a, Fixation b)
        {
            return a.value > b.value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(Fixation a, Fixation b)
        {
            return a.value <= b.value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(Fixation a, Fixation b)
        {
            return a.value >= b.value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Fixation a, Fixation b)
        {
            return a.value == b.value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Fixation a, Fixation b)
        {
            return a.value != b.value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixation operator +(Fixation a, Fixation b)
        {
            return new Fixation(a.value + b.value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixation operator -(Fixation a, Fixation b)
        {
            return new Fixation(a.value - b.value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixation operator *(Fixation a, Fixation b)
        {
            return new Fixation((int)(((long)a.value * b.value) >> BIT));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixation operator /(Fixation a, Fixation b)
        {
            return new Fixation((int)(((long)a.value << BIT) / b.value));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator int(Fixation value)
        {
            return value.value >> BIT;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Fixation(int value)
        {
            return new Fixation(value << BIT);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator float(Fixation value)
        {
            return value.value / FIX;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Fixation(float value)
        {
            return new Fixation((int)(value * FIX));
        }

        public static int Sign(Fixation value)
        {
            return value > 0 ? 1 : value < 0 ? -1 : 0;
        }

        public static Fixation Sqrt(Fixation value)
        {
            if (value.value <= 0)
            {
                return 0;
            }

            var number = 0L + value.value << 16;
            var result = 1L << ((BitLength(number) + 1) >> 1);

            while (true)
            {
                var next = (result + number / result) >> 1;
                if (next >= result)
                {
                    break;
                }

                result = next;
            }

            return new Fixation((int)result);
        }

        private static int BitLength(long value)
        {
            var length = 0;

            while (value > 0)
            {
                value >>= 1;
                length++;
            }

            return length;
        }
    }

    [Serializable]
    public readonly struct Position : IEquatable<Position>
    {
        public static readonly Position Zero = new Position(0, 0);

        public readonly Fixation x;
        public readonly Fixation y;

        internal int X => (int)x;
        internal int Y => (int)y;

        public Fixation sqrMagnitude => x * x + y * y;
        public Fixation magnitude => Fixation.Sqrt(sqrMagnitude);
        public Position normalize => x == 0 && y == 0 ? Zero : this / magnitude;

        public Position(Fixation x, Fixation y)
        {
            this.x = x;
            this.y = y;
        }

        public bool Equals(Position other)
        {
            return x.Equals(other.x) && y.Equals(other.y);
        }

        public override bool Equals(object obj)
        {
            return obj is Position other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (X << 16) ^ Y;
        }

        public override string ToString()
        {
            return "({0}, {1})".Format(x, y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Position a, Position b)
        {
            return a.x == b.x && a.y == b.y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Position a, Position b)
        {
            return a.x != b.x || a.y != b.y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Position operator +(Position a, Position b)
        {
            return new Position(a.x + b.x, a.y + b.y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Position operator -(Position a, Position b)
        {
            return new Position(a.x - b.x, a.y - b.y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Position operator *(Position a, Fixation b)
        {
            return new Position(a.x * b, a.y * b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Position operator /(Position a, Fixation b)
        {
            return new Position(a.x / b, a.y / b);
        }

        public static Fixation Dot(Position a, Position b)
        {
            return a.x * b.x + a.y * b.y;
        }

        public static Fixation Cross(Position a, Position b)
        {
            return a.x * b.y - a.y * b.x;
        }

        public static Fixation Distance(Position a, Position b)
        {
            return Fixation.Sqrt((a - b).sqrMagnitude);
        }

        public static Position MoveTowards(Position current, Position target, Fixation maxDistanceDelta)
        {
            var delta = target - current;

            var sqrDistance = delta.sqrMagnitude;

            if (sqrDistance == 0)
            {
                return target;
            }

            var maxSqrDistance = maxDistanceDelta * maxDistanceDelta;

            if (sqrDistance <= maxSqrDistance)
            {
                return target;
            }

            return current + delta.normalize * maxDistanceDelta;
        }
    }

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
            return (Value >> shift) & ((1 << bits) - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetBit(int shift, int bits, int value)
        {
            Value = (Value & ~(((1 << bits) - 1) << shift)) | ((value & ((1 << bits) - 1)) << shift);
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
            return (int)((Value >> shift) & ((1L << bits) - 1));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetBit(int shift, int bits, int value)
        {
            Value = (Value & ~(((1L << bits) - 1) << shift)) | ((value & ((1L << bits) - 1)) << shift);
        }
    }

    [Serializable]
    public struct XorEx : IEquatable<XorEx>
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

        public XorEx(byte[] value)
        {
            buffer = 0;
            offset = Ticks;
            origin = value;
            buffer = GetHashCode();
        }

        public static implicit operator byte[](XorEx variable)
        {
            return variable.Value;
        }

        public static implicit operator XorEx(byte[] value)
        {
            return new XorEx(value);
        }

        public bool Equals(XorEx other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is XorEx other && Equals(other);
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
                    {
                        take = remain;
                    }

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
                    {
                        take = remain;
                    }

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
    public readonly struct Sequence : INode
    {
        private readonly int Index;
        private readonly INode[] Nodes;

        public Sequence(int index, INode[] nodes)
        {
            Index = index;
            Nodes = nodes ?? Array.Empty<INode>();
        }

        public async Task<Awaiter.State> OnTick(int[] indices, Blackboard<int> root)
        {
            var current = indices[Index];
            while (current < Nodes.Length)
            {
                var state = await Nodes[current].OnTick(indices, root);
                if (state == Awaiter.State.Running)
                {
                    return Awaiter.State.Running;
                }

                if (state == Awaiter.State.Failure)
                {
                    indices[Index] = 0;
                    return Awaiter.State.Failure;
                }

                current++;
                indices[Index] = current;
            }

            indices[Index] = 0;
            return Awaiter.State.Success;
        }
    }

    [Serializable]
    public readonly struct Selector : INode
    {
        private readonly int Index;
        private readonly INode[] Nodes;

        public Selector(int index, INode[] nodes)
        {
            Index = index;
            Nodes = nodes ?? Array.Empty<INode>();
        }

        public async Task<Awaiter.State> OnTick(int[] indices, Blackboard<int> root)
        {
            var current = indices[Index];
            while (current < Nodes.Length)
            {
                var state = await Nodes[current].OnTick(indices, root);
                if (state == Awaiter.State.Running)
                {
                    return Awaiter.State.Running;
                }

                if (state == Awaiter.State.Success)
                {
                    indices[Index] = 0;
                    return Awaiter.State.Success;
                }

                current++;
                indices[Index] = current;
            }

            indices[Index] = 0;
            return Awaiter.State.Failure;
        }
    }

    [Serializable]
    public readonly struct Parallel : INode
    {
        private readonly bool IsAny;
        private readonly INode[] Nodes;

        public Parallel(string isAny, INode[] nodes)
        {
            IsAny = isAny == "Any";
            Nodes = nodes ?? Array.Empty<INode>();
        }

        public async Task<Awaiter.State> OnTick(int[] indices, Blackboard<int> root)
        {
            if (IsAny)
            {
                foreach (var node in Nodes)
                {
                    var state = await node.OnTick(indices, root);
                    if (state == Awaiter.State.Success)
                    {
                        return Awaiter.State.Success;
                    }

                    if (state == Awaiter.State.Failure)
                    {
                        return Awaiter.State.Failure;
                    }
                }

                return Awaiter.State.Running;
            }

            var isAll = true;
            foreach (var node in Nodes)
            {
                var state = await node.OnTick(indices, root);
                if (state == Awaiter.State.Failure)
                {
                    return Awaiter.State.Failure;
                }

                if (state == Awaiter.State.Running)
                {
                    isAll = false;
                }
            }

            return isAll ? Awaiter.State.Success : Awaiter.State.Running;
        }
    }

    [Serializable]
    public readonly struct Randomer : INode
    {
        private readonly int Index;
        private readonly INode[] Nodes;

        public Randomer(int index, INode[] nodes)
        {
            Index = index;
            Nodes = nodes ?? Array.Empty<INode>();
        }

        public async Task<Awaiter.State> OnTick(int[] indices, Blackboard<int> root)
        {
            if (indices[Index] == 0)
            {
                indices[Index] = Seed.Next(Nodes.Length) + 1;
            }

            var state = await Nodes[indices[Index] - 1].OnTick(indices, root);
            if (state == Awaiter.State.Running)
            {
                return Awaiter.State.Running;
            }

            indices[Index] = 0;
            return state;
        }
    }

    [Serializable]
    public readonly struct Repeater : INode
    {
        private readonly int Index;
        private readonly int Count;
        private readonly INode Node;

        public Repeater(int index, int count, INode node)
        {
            Node = node;
            Index = index;
            Count = count;
        }

        public async Task<Awaiter.State> OnTick(int[] indices, Blackboard<int> root)
        {
            var state = await Node.OnTick(indices, root);
            if (state == Awaiter.State.Running)
            {
                return Awaiter.State.Running;
            }

            indices[Index]++;
            if (Count < 0 || indices[Index] < Count)
            {
                return Awaiter.State.Running;
            }

            indices[Index] = 0;
            return Awaiter.State.Success;
        }
    }

    [Serializable]
    public readonly struct Inverter : INode
    {
        private readonly INode Node;

        public Inverter(INode node)
        {
            Node = node;
        }

        public async Task<Awaiter.State> OnTick(int[] indices, Blackboard<int> root)
        {
            var state = await Node.OnTick(indices, root);
            switch (state)
            {
                case Awaiter.State.Success: return Awaiter.State.Failure;
                case Awaiter.State.Failure: return Awaiter.State.Success;
            }

            return Awaiter.State.Running;
        }
    }

    [Serializable]
    public readonly struct Success : INode
    {
        private readonly INode Node;

        public Success(INode node)
        {
            Node = node;
        }

        public async Task<Awaiter.State> OnTick(int[] indices, Blackboard<int> root)
        {
            var state = await Node.OnTick(indices, root);
            return state == Awaiter.State.Running ? Awaiter.State.Running : Awaiter.State.Success;
        }
    }

    [Serializable]
    public readonly struct Failure : INode
    {
        private readonly INode Node;

        public Failure(INode node)
        {
            Node = node;
        }

        public async Task<Awaiter.State> OnTick(int[] indices, Blackboard<int> root)
        {
            var state = await Node.OnTick(indices, root);
            return state == Awaiter.State.Running ? Awaiter.State.Running : Awaiter.State.Failure;
        }
    }

    public interface INode
    {
        Task<Awaiter.State> OnTick(int[] indices, Blackboard<int> root);
    }

    public static class Nodes
    {
        private static readonly Dictionary<Type, Func<Node, Func<Node, Type>, INode>> Func = new();

        static Nodes()
        {
            Func[typeof(Sequence)] = SequenceInternal;
            Func[typeof(Selector)] = SelectorInternal;
            Func[typeof(Parallel)] = ParallelInternal;
            Func[typeof(Randomer)] = RandomerInternal;
            Func[typeof(Repeater)] = RepeaterInternal;
            Func[typeof(Inverter)] = InverterInternal;
            Func[typeof(Success)] = SuccessInternal;
            Func[typeof(Failure)] = FailureInternal;
        }

        private static INode SequenceInternal(Node node, Func<Node, Type> func)
        {
            return new Sequence(node.Index, node.Nodes.Select(i => i.Build(func)).ToArray());
        }

        private static INode SelectorInternal(Node node, Func<Node, Type> func)
        {
            return new Selector(node.Index, node.Nodes.Select(i => i.Build(func)).ToArray());
        }

        private static INode ParallelInternal(Node node, Func<Node, Type> func)
        {
            return new Parallel(node.Data, node.Nodes.Select(i => i.Build(func)).ToArray());
        }

        private static INode RandomerInternal(Node node, Func<Node, Type> func)
        {
            return new Randomer(node.Index, node.Nodes.Select(i => i.Build(func)).ToArray());
        }

        private static INode RepeaterInternal(Node node, Func<Node, Type> func)
        {
            return new Repeater(node.Index, int.Parse(node.Data), node.Nodes.Select(i => i.Build(func)).First());
        }

        private static INode InverterInternal(Node node, Func<Node, Type> func)
        {
            return new Inverter(node.Nodes.Select(i => i.Build(func)).First());
        }

        private static INode SuccessInternal(Node node, Func<Node, Type> func)
        {
            return new Success(node.Nodes.Select(i => i.Build(func)).First());
        }

        private static INode FailureInternal(Node node, Func<Node, Type> func)
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
}