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
        internal static class Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int Invoke(ulong length)
            {
                if (length == 0)
                {
                    return 1;
                }

                var result = 0;
                while (length > 0)
                {
                    result++;
                    length >>= 7;
                }

                return result;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void EncodeULong(MemoryWriter writer, ulong length)
            {
                while (length >= 0x80)
                {
                    writer.Write((byte)((length & 0x7F) | 0x80));
                    length >>= 7;
                }

                writer.Write((byte)length);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ulong DecodeULong(MemoryReader reader)
            {
                var shift = 0;
                var length = 0UL;
                while (true)
                {
                    var bit = reader.Read<byte>();
                    length |= (ulong)(bit & 0x7F) << shift;
                    if ((bit & 0x80) == 0)
                    {
                        break;
                    }

                    shift += 7;
                }

                return length;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ulong ZigZagEncode(long n)
            {
                return (ulong)((n << 1) ^ (n >> 63));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static long ZigZagDecode(ulong n)
            {
                return (long)((n >> 1) ^ (ulong)-(long)(n & 1));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void EncodeUInt(MemoryWriter writer, uint length)
            {
                while (length >= 0x80)
                {
                    writer.Write((byte)((length & 0x7F) | 0x80));
                    length >>= 7;
                }

                writer.Write((byte)length);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static uint DecodeUInt(MemoryReader reader)
            {
                var shift = 0;
                var length = 0U;
                while (true)
                {
                    var bit = reader.Read<byte>();
                    length |= (uint)(bit & 0x7F) << shift;
                    if ((bit & 0x80) == 0)
                    {
                        break;
                    }

                    shift += 7;
                }

                return length;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static uint ZigZagEncode(int n)
            {
                return (uint)((n << 1) ^ (n >> 31));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int ZigZagDecode(uint n)
            {
                return (int)((n >> 1) ^ -(int)(n & 1));
            }
        }
    }
}