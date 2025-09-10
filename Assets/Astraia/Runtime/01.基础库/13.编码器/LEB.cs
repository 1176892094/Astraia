// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2025-01-10 21:01:21
// # Recently: 2025-01-11 14:01:55
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System.Runtime.CompilerServices;

namespace Astraia
{
    public static partial class Service
    {
        public static class LEB
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static int Invoke(ulong value)
            {
                if (value == 0)
                {
                    return 1;
                }

                var count = 0;
                while (value > 0)
                {
                    count++;
                    value >>= 7;
                }

                return count;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static void EncodeULong(MemoryWriter writer, ulong value)
            {
                while (value >= 0x80)
                {
                    writer.Write((byte)((value & 0x7F) | 0x80));
                    value >>= 7;
                }

                writer.Write((byte)value);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static ulong DecodeULong(MemoryReader reader)
            {
                var shift = 0;
                var value = 0UL;
                while (true)
                {
                    var bit = reader.Read<byte>();
                    value |= (ulong)(bit & 0x7F) << shift;
                    if ((bit & 0x80) == 0)
                    {
                        break;
                    }

                    shift += 7;
                }

                return value;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static ulong ZigZagEncode(long n)
            {
                return (ulong)((n << 1) ^ (n >> 63));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static long ZigZagDecode(ulong n)
            {
                return (long)((n >> 1) ^ (ulong)-(long)(n & 1));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static void EncodeUInt(MemoryWriter writer, uint value)
            {
                while (value >= 0x80)
                {
                    writer.Write((byte)((value & 0x7F) | 0x80));
                    value >>= 7;
                }

                writer.Write((byte)value);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static uint DecodeUInt(MemoryReader reader)
            {
                var shift = 0;
                var value = 0U;
                while (true)
                {
                    var bit = reader.Read<byte>();
                    value |= (uint)(bit & 0x7F) << shift;
                    if ((bit & 0x80) == 0)
                    {
                        break;
                    }

                    shift += 7;
                }

                return value;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static uint ZigZagEncode(int n)
            {
                return (uint)((n << 1) ^ (n >> 31));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static int ZigZagDecode(uint n)
            {
                return (int)((n >> 1) ^ -(int)(n & 1));
            }
        }
    }
}