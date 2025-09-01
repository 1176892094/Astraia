// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-07-19 11:07:45
// // # Recently: 2025-07-19 11:07:45
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using Astraia.Common;

namespace Astraia
{
    public static partial class Xor
    {
        [Serializable]
        public struct Int : IEquatable<Int>
        {
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
                        EventManager.Invoke(new Exception());
                        throw new InvalidOperationException();
                    }

                    return value;
                }
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set
                {
                    offset = TickInt;
                    origin = value ^ offset;
                    buffer = (offset >> 8) ^ value;
                }
            }

            public Int(int value = 0)
            {
                offset = TickInt;
                origin = value ^ offset;
                buffer = (offset >> 8) ^ value;
            }

            public static implicit operator int(Int data)
            {
                return data.Value;
            }

            public static implicit operator Int(int data)
            {
                return new Int(data);
            }

            public bool Equals(Int other)
            {
                return Value == other.Value;
            }

            public override bool Equals(object obj)
            {
                return obj is Int other && Equals(other);
            }

            public override string ToString()
            {
                return Value.ToString();
            }

            public override int GetHashCode()
            {
                return Value.GetHashCode();
            }
        }

        [Serializable]
        public struct Long : IEquatable<Long>
        {
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
                        EventManager.Invoke(new Exception());
                        throw new InvalidOperationException();
                    }

                    return value;
                }
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set
                {
                    offset = TickLong;
                    origin = value ^ offset;
                    buffer = (offset >> 8) ^ value;
                }
            }

            public Long(long value = 0)
            {
                offset = TickLong;
                origin = value ^ offset;
                buffer = (offset >> 8) ^ value;
            }

            public static implicit operator long(Long data)
            {
                return data.Value;
            }

            public static implicit operator Long(long data)
            {
                return new Long(data);
            }

            public bool Equals(Long other)
            {
                return Value == other.Value;
            }

            public override bool Equals(object obj)
            {
                return obj is Long other && Equals(other);
            }

            public override string ToString()
            {
                return Value.ToString();
            }

            public override int GetHashCode()
            {
                return Value.GetHashCode();
            }
        }

        [Serializable]
        public struct Float : IEquatable<Float>
        {
            public int origin;
            public int buffer;
            public int offset;

            public unsafe float Value
            {
                get
                {
                    var value = origin ^ offset;
                    if (buffer != ((offset >> 8) ^ value))
                    {
                        EventManager.Invoke(new Exception());
                        throw new InvalidOperationException();
                    }

                    return *(float*)&value;
                }
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set
                {
                    var ptr = *(int*)&value;
                    offset = TickInt;
                    origin = ptr ^ offset;
                    buffer = (offset >> 8) ^ ptr;
                }
            }

            public unsafe Float(float value = 0)
            {
                var ptr = *(int*)&value;
                offset = TickInt;
                origin = ptr ^ offset;
                buffer = (offset >> 8) ^ ptr;
            }

            public static implicit operator float(Float variable)
            {
                return variable.Value;
            }

            public static implicit operator Float(float value)
            {
                return new Float(value);
            }

            public bool Equals(Float other)
            {
                return origin == other.origin;
            }

            public override bool Equals(object obj)
            {
                return obj is Float other && Equals(other);
            }

            public override string ToString()
            {
                return Value.ToString(CultureInfo.InvariantCulture);
            }

            public override int GetHashCode()
            {
                return Value.GetHashCode();
            }
        }

        [Serializable]
        public struct Bytes : IEquatable<Bytes>
        {
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
                        EventManager.Invoke(new Exception());
                        throw new InvalidOperationException();
                    }

                    return origin;
                }
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set
                {
                    offset = TickInt;
                    origin = value;
                    buffer = GetHashCode();
                }
            }

            public Bytes(byte[] value)
            {
                buffer = 0;
                offset = TickInt;
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
                return buffer - offset == other.buffer - other.offset;
            }

            public override bool Equals(object obj)
            {
                return obj is Bytes other && Equals(other);
            }

            public override string ToString()
            {
                return BitConverter.ToString(Value, 0, origin.Length);
            }

            public override int GetHashCode()
            {
                var result = offset;
                unchecked
                {
                    foreach (var b in origin)
                    {
                        result = (result * 31) ^ b;
                    }

                    return result;
                }
            }
        }
    }
}