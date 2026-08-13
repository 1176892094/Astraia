// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-08-13 23:08:10
// # Recently: 2026-08-13 23:06:10
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System.Runtime.CompilerServices;

namespace Astraia
{
    internal static class Compress
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Length(ulong value)
        {
            return value switch
            {
                < 1UL << 07 => 1,
                < 1UL << 14 => 2,
                < 1UL << 21 => 3,
                < 1UL << 28 => 4,
                < 1UL << 35 => 5,
                < 1UL << 42 => 6,
                < 1UL << 49 => 7,
                < 1UL << 56 => 8,
                < 1UL << 63 => 9,
                _ => 10
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EncodeUInt16(MemoryWriter writer, ushort value)
        {
            while (value >= 0x80)
            {
                writer.Write((byte)((value & 0x7F) | 0x80));
                value >>= 7;
            }

            writer.Write((byte)value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EncodeUInt32(MemoryWriter writer, uint value)
        {
            while (value >= 0x80)
            {
                writer.Write((byte)((value & 0x7F) | 0x80));
                value >>= 7;
            }

            writer.Write((byte)value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EncodeUInt64(MemoryWriter writer, ulong value)
        {
            while (value >= 0x80)
            {
                writer.Write((byte)((value & 0x7F) | 0x80));
                value >>= 7;
            }

            writer.Write((byte)value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort DecodeUInt16(MemoryReader reader)
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

            return (ushort)value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint DecodeUInt32(MemoryReader reader)
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
        public static ulong DecodeUInt64(MemoryReader reader)
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
        public static ushort ZigZagEncode(short n)
        {
            return (ushort)((n << 1) ^ (n >> 15));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ZigZagEncode(int n)
        {
            return (uint)((n << 1) ^ (n >> 31));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong ZigZagEncode(long n)
        {
            return (ulong)((n << 1) ^ (n >> 63));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short ZigZagDecode(ushort n)
        {
            return (short)((n >> 1) ^ (ushort)-(short)(n & 1));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ZigZagDecode(uint n)
        {
            return (int)((n >> 1) ^ (uint)-(int)(n & 1));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long ZigZagDecode(ulong n)
        {
            return (long)((n >> 1) ^ (ulong)-(long)(n & 1));
        }
    }
}