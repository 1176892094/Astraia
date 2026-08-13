// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-08-13 23:08:58
// # Recently: 2026-08-13 23:09:58
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Runtime.CompilerServices;

namespace Astraia
{
    public static class Reader<T>
    {
        // ReSharper disable once UnassignedField.Global
        public static Func<MemoryReader, T> reader;
    }

    [Serializable]
    public class MemoryReader : IDisposable
    {
        public ArraySegment<byte> buffer;
        public int position;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe T Read<T>() where T : unmanaged
        {
            var count = sizeof(T);
            var value = Unsafe.ReadUnaligned<T>(ref buffer.Array![buffer.Offset + position]);
            position += count;
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T? ReadNullable<T>() where T : unmanaged
        {
            return Read<byte>() != 0 ? Read<T>() : null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Invoke<T>()
        {
            var reader = Reader<T>.reader;
            if (reader == null)
            {
                throw new NullReferenceException($"没有找到读取器: {typeof(T)}");
            }

            return reader(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset(ArraySegment<byte> segment)
        {
            buffer = segment;
            position = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MemoryReader Pop(ArraySegment<byte> segment)
        {
            var reader = HeapManager.Dequeue<MemoryReader>();
            reader.Reset(segment);
            return reader;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Push(MemoryReader reader)
        {
            HeapManager.Enqueue(reader);
        }

        public override string ToString()
        {
            return BitConverter.ToString(buffer.Array!, buffer.Offset, buffer.Count);
        }

        void IDisposable.Dispose()
        {
            Push(this);
        }

        public byte[] ReadBytes(byte[] bytes, int count)
        {
            if (buffer.Count - position < count)
            {
                throw new OverflowException("读取器剩余容量不够!");
            }

            Buffer.BlockCopy(buffer.Array!, buffer.Offset + position, bytes, 0, count);
            position += count;
            return bytes;
        }

        public ArraySegment<byte> ReadArraySegment(int count)
        {
            if (buffer.Count - position < count)
            {
                throw new OverflowException("读取器剩余容量不够!");
            }

            var segment = new ArraySegment<byte>(buffer.Array!, buffer.Offset + position, count);
            position += count;
            return segment;
        }
    }
}