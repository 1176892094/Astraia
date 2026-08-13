// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-08-13 23:08:13
// # Recently: 2026-08-13 23:00:13
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Runtime.CompilerServices;

namespace Astraia
{
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
            long result = offset;

            fixed (byte* ptr = origin)
            {
                var count = origin.Length / 8;

                var lpt = (long*)ptr;
                for (var i = 0; i < count; i++)
                {
                    result = (result * 31) ^ lpt[i];
                }

                for (var i = count * 8; i < origin.Length; i++)
                {
                    result = (result * 31) ^ ptr[i];
                }
            }

            return (int)(result ^ (result >> 32));
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