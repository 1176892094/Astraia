// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-08-13 23:08:53
// # Recently: 2026-08-13 23:09:53
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Runtime.CompilerServices;

namespace Astraia
{
    public static class Writer<T>
    {
        // ReSharper disable once UnassignedField.Global
        public static Action<MemoryWriter, T> writer;
    }

    [Serializable]
    public class MemoryWriter : IDisposable
    {
        public byte[] buffer = new byte[1500];
        public int position;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write<T>(T value) where T : unmanaged
        {
            var count = sizeof(T);
            Resize(position + count);
            Unsafe.WriteUnaligned(ref buffer[position], value);
            position += count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteNullable<T>(T? value) where T : unmanaged
        {
            if (!value.HasValue)
            {
                Write((byte)0);
                return;
            }

            Write((byte)1);
            Write(value.Value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke<T>(T value)
        {
            var writer = Writer<T>.writer;
            if (writer == null)
            {
                throw new NullReferenceException($"没有找到写入器: {typeof(T)}");
            }

            writer(this, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
        {
            position = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MemoryWriter Pop()
        {
            var writer = HeapManager.Dequeue<MemoryWriter>();
            writer.Reset();
            return writer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Push(MemoryWriter writer)
        {
            HeapManager.Enqueue(writer);
        }

        public override string ToString()
        {
            return BitConverter.ToString(buffer, 0, position);
        }

        void IDisposable.Dispose()
        {
            Push(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Resize(int count)
        {
            if (buffer.Length < count)
            {
                Array.Resize(ref buffer, Math.Max(count, buffer.Length * 2));
            }
        }

        public void WriteBytes(byte[] segment, int offset, int count)
        {
            Resize(position + count);
            Buffer.BlockCopy(segment, offset, buffer, position, count);
            position += count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ArraySegment<byte>(MemoryWriter writer)
        {
            return new ArraySegment<byte>(writer.buffer, 0, writer.position);
        }
    }
}