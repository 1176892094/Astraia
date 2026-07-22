using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Astraia
{
    [Serializable]
    public readonly struct Fixation : IEquatable<Fixation>
    {
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
            return (value / 65536F).ToString("R");
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
            return new Fixation((int)(((long)a.value * b.value) >> 16));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixation operator /(Fixation a, Fixation b)
        {
            return new Fixation((int)(((long)a.value << 16) / b.value));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator int(Fixation value)
        {
            return value.value >> 16;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Fixation(int value)
        {
            return new Fixation(value << 16);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator float(Fixation value)
        {
            return value.value / 65536F;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Fixation(float value)
        {
            return new Fixation((int)(value * 65536));
        }

        public static int Sign(Fixation value)
        {
            return value > 0 ? 1 : value < 0 ? -1 : 0;
        }

        public static Fixation Min(Fixation a, Fixation b)
        {
            return a < b ? a : b;
        }

        public static Fixation Max(Fixation a, Fixation b)
        {
            return a > b ? a : b;
        }

        public static Fixation Abs(Fixation value)
        {
            return value.value < 0 ? new Fixation(-value.value) : value;
        }

        public static Fixation Lerp(Fixation a, Fixation b, Fixation t)
        {
            return a + (b - a) * t;
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

        public static Position Lerp(Position a, Position b, Fixation t)
        {
            return new Position(Fixation.Lerp(a.x, b.x, t), Fixation.Lerp(a.y, b.y, t));
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

        public Nodes.State OnTick(int[] indices, Whiteboard<int> root)
        {
            var current = indices[Index];
            while (current < Nodes.Length)
            {
                var result = Nodes[current].OnTick(indices, root);
                if (result == Astraia.Nodes.State.Running)
                {
                    return Astraia.Nodes.State.Running;
                }

                if (result == Astraia.Nodes.State.Failure)
                {
                    indices[Index] = 0;
                    return Astraia.Nodes.State.Failure;
                }

                current++;
                indices[Index] = current;
            }

            indices[Index] = 0;
            return Astraia.Nodes.State.Success;
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

        public Nodes.State OnTick(int[] indices, Whiteboard<int> root)
        {
            var current = indices[Index];
            while (current < Nodes.Length)
            {
                var result = Nodes[current].OnTick(indices, root);
                if (result == Astraia.Nodes.State.Running)
                {
                    return Astraia.Nodes.State.Running;
                }

                if (result == Astraia.Nodes.State.Success)
                {
                    indices[Index] = 0;
                    return Astraia.Nodes.State.Success;
                }

                current++;
                indices[Index] = current;
            }

            indices[Index] = 0;
            return Astraia.Nodes.State.Failure;
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

        public Nodes.State OnTick(int[] indices, Whiteboard<int> root)
        {
            if (IsAny)
            {
                foreach (var node in Nodes)
                {
                    var result = node.OnTick(indices, root);
                    if (result == Astraia.Nodes.State.Success)
                    {
                        return Astraia.Nodes.State.Success;
                    }

                    if (result == Astraia.Nodes.State.Failure)
                    {
                        return Astraia.Nodes.State.Failure;
                    }
                }

                return Astraia.Nodes.State.Running;
            }

            var isAll = true;
            foreach (var node in Nodes)
            {
                var result = node.OnTick(indices, root);
                if (result == Astraia.Nodes.State.Failure)
                {
                    return Astraia.Nodes.State.Failure;
                }

                if (result == Astraia.Nodes.State.Running)
                {
                    isAll = false;
                }
            }

            return isAll ? Astraia.Nodes.State.Success : Astraia.Nodes.State.Running;
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

        public Nodes.State OnTick(int[] indices, Whiteboard<int> root)
        {
            if (indices[Index] == 0)
            {
                indices[Index] = Seed.Next(Nodes.Length) + 1;
            }

            var result = Nodes[indices[Index] - 1].OnTick(indices, root);
            if (result == Astraia.Nodes.State.Running)
            {
                return Astraia.Nodes.State.Running;
            }

            indices[Index] = 0;
            return result;
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

        public Nodes.State OnTick(int[] indices, Whiteboard<int> root)
        {
            var result = Node.OnTick(indices, root);
            if (result == Nodes.State.Running)
            {
                return Nodes.State.Running;
            }

            indices[Index]++;
            if (Count < 0 || indices[Index] < Count)
            {
                return Nodes.State.Running;
            }

            indices[Index] = 0;
            return Nodes.State.Success;
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

        public Nodes.State OnTick(int[] indices, Whiteboard<int> root)
        {
            switch (Node.OnTick(indices, root))
            {
                case Nodes.State.Success: return Nodes.State.Failure;
                case Nodes.State.Failure: return Nodes.State.Success;
            }

            return Nodes.State.Running;
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

        public Nodes.State OnTick(int[] indices, Whiteboard<int> root)
        {
            return Node.OnTick(indices, root) == Nodes.State.Running ? Nodes.State.Running : Nodes.State.Success;
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

        public Nodes.State OnTick(int[] indices, Whiteboard<int> root)
        {
            return Node.OnTick(indices, root) == Nodes.State.Running ? Nodes.State.Running : Nodes.State.Failure;
        }
    }

    public interface INode
    {
        Nodes.State OnTick(int[] indices, Whiteboard<int> root);
    }

    public static class Nodes
    {
        private static readonly Dictionary<Type, Func<Node, Func<Node, Type>, INode>> Func = new();

        public enum State
        {
            Running,
            Success,
            Failure
        }

        static Nodes()
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
}