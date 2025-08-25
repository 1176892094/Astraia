// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-08-25 23:08:12
// // # Recently: 2025-08-25 23:08:12
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;

namespace Astraia
{
    [Serializable]
    public class BitStream
    {
        public byte[] source;
        public byte[] buffer;

        public BitStream(int count = 32)
        {
            count = (count + 7) / 8;
            source = new byte[count];
            buffer = new byte[count];
        }

        public BitStream(byte[] target)
        {
            source = new byte[target.Length];
            buffer = new byte[target.Length];
            unsafe
            {
                fixed (byte* srcPtr = target, dstPtr = source)
                {
                    Buffer.MemoryCopy(srcPtr, dstPtr, source.Length, target.Length);
                }
            }
        }

        public unsafe int GetBit(int offset, int count)
        {
            if (count % 8 != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            count /= 8;
            var result = 0;
            fixed (byte* ptr = source, buf = buffer)
            {
                var p = ptr + offset / 8;
                var b = buf + offset / 8;
                for (var i = 0; i < count; i++)
                {
                    if ((p[i] ^ 0xAA) != b[i])
                    {
                        throw new InvalidOperationException();
                    }

                    result |= p[i] << (i * 8);
                }
            }

            return result;
        }

        public unsafe void SetBit(int offset, int count, int value)
        {
            if (count % 8 != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            count /= 8;
            EnsureCapacity(offset + count);
            fixed (byte* ptr = source, buf = buffer)
            {
                var p = ptr + offset / 8;
                var b = buf + offset / 8;
                for (var i = 0; i < count; i++)
                {
                    p[i] = (byte)((value >> (i * 8)) & 0xFF);
                    b[i] = (byte)(p[i] ^ 0xAA);
                }
            }
        }

        private void EnsureCapacity(int count)
        {
            var required = (count + 7) / 8;
            if (required <= source.Length)
            {
                return;
            }

            var newSize = source.Length;
            if (newSize == 0)
            {
                newSize = 8;
            }

            while (newSize < required)
            {
                newSize += 8;
            }

            Resize(ref source, newSize);
            Resize(ref buffer, newSize);
        }

        private unsafe void Resize(ref byte[] array, int count)
        {
            var newArray = new byte[count];
            fixed (byte* srcPtr = array, dstPtr = newArray)
            {
                Buffer.MemoryCopy(srcPtr, dstPtr, count, array.Length);
            }

            array = newArray;
        }
    }
}