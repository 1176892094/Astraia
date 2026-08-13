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

        private Properties(Fixation[] properties)
        {
            this.properties = properties;
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

        public static Properties<T> Create()
        {
            return new Properties<T>(new Fixation[Seed.Count<T>()]);
        }
    }

    [Serializable]
    public struct Fixation : IEquatable<Fixation>
    {
        private const int BIT = 12;
        private const int FIX = 1 << BIT;

        public static readonly Fixation One = new Fixation(FIX);
        public static readonly Fixation Zero = new Fixation(0);
        public static readonly Fixation Epsilon = new Fixation(1);
        public static readonly Fixation MaxValue = new Fixation(int.MaxValue);
        public static readonly Fixation MinValue = new Fixation(int.MinValue);

        public int value;

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
            return ((float)value / FIX).ToString("R");
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
            return (float)value.value / FIX;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Fixation(float value)
        {
            return new Fixation((int)(value * FIX));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int FloorToInt()
        {
            return value >> BIT;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int CeilToInt()
        {
            return value >= 0 ? (value + FIX - 1) >> BIT : -(-value >> BIT);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int RoundToInt()
        {
            return value >= 0 ? (value + (1 << (BIT - 1))) >> BIT : -((-value + (1 << (BIT - 1))) >> BIT);
        }

        public static Fixation Max(Fixation a, Fixation b)
        {
            return a > b ? a : b;
        }

        public static Fixation Min(Fixation a, Fixation b)
        {
            return a < b ? a : b;
        }

        public static Fixation Abs(Fixation a)
        {
            return a < 0 ? -a : a;
        }

        public static int Sign(Fixation value)
        {
            return value > 0 ? 1 : value < 0 ? -1 : 0;
        }

        public static Fixation Sqrt(Fixation value)
        {
            if (value.value <= 0)
            {
                return Zero;
            }

            var x = (long)value.value << BIT;

            var count = 0;
            var index = x;

            while (index > 0)
            {
                index >>= 1;
                count++;
            }

            var guess = 1L << ((count + 1) >> 1);
            while (true)
            {
                var next = (guess + x / guess) >> 1;
                if (next >= guess)
                {
                    break;
                }

                guess = next;
            }

            return new Fixation((int)guess);
        }
    }

    [Serializable]
    public struct Position : IEquatable<Position>
    {
        public static readonly Position Zero = new Position(0, 0);

        public Fixation x;
        public Fixation y;

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
            return (X << 16) ^ (Y & 0xFFFF);
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

}