// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2025-01-10 21:01:21
// # Recently: 2025-01-11 18:01:33
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Astraia.Core;

// ReSharper disable All

namespace Astraia
{
    public static class Writer<T>
    {
        public static Action<MemoryWriter, T> writer;
    }

    public static class Reader<T>
    {
        public static Func<MemoryReader, T> reader;
    }

    public class MemoryWriter : IDisposable
    {
        public byte[] buffer = new byte[1500];
        public int position;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write<T>(T value) where T : unmanaged
        {
            Resize(position + sizeof(T));
            fixed (byte* ptr = &buffer[position])
            {
                *(T*)ptr = value;
            }

            position += sizeof(T);
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
            Writer<T>.writer?.Invoke(this, value);
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
            HeapManager.Enqueue(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Resize(int length)
        {
            if (buffer.Length < length)
            {
                Array.Resize(ref buffer, Math.Max(length, buffer.Length * 2));
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

    public class MemoryReader : IDisposable
    {
        public ArraySegment<byte> buffer;
        public int position;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe T Read<T>() where T : unmanaged
        {
            T value;
            fixed (byte* ptr = &buffer.Array[buffer.Offset + position])
            {
                value = *(T*)ptr;
            }

            position += sizeof(T);
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T? ReadNullable<T>() where T : unmanaged
        {
            return Read<byte>() != 0 ? Read<T>() : default(T?);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Invoke<T>()
        {
            return Reader<T>.reader != null ? Reader<T>.reader.Invoke(this) : default;
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
            return BitConverter.ToString(buffer.Array, buffer.Offset, buffer.Count);
        }

        void IDisposable.Dispose()
        {
            HeapManager.Enqueue(this);
        }

        public byte[] ReadBytes(byte[] bytes, int count)
        {
            if (buffer.Count - position < count)
            {
                throw new OverflowException("读取器剩余容量不够!");
            }

            Buffer.BlockCopy(buffer.Array, buffer.Offset + position, bytes, 0, count);
            position += count;
            return bytes;
        }

        public ArraySegment<byte> ReadArraySegment(int count)
        {
            if (buffer.Count - position < count)
            {
                throw new OverflowException("读取器剩余容量不够!");
            }

            var segment = new ArraySegment<byte>(buffer.Array, buffer.Offset + position, count);
            position += count;
            return segment;
        }
    }

    internal class PacketWriter
    {
        private readonly Queue<MemoryWriter> writers = new Queue<MemoryWriter>();
        private readonly uint maxLength;
        private MemoryWriter writer;

        public PacketWriter(uint maxLength)
        {
            this.maxLength = maxLength;
        }

        public void AddMessage(ArraySegment<byte> segment)
        {
            var length = Service.Bit.Invoke((ulong)segment.Count);
            if (writer != null && writer.position + length + segment.Count > maxLength)
            {
                writers.Enqueue(writer);
                writer = null;
            }

            writer ??= MemoryWriter.Pop();
            Service.Bit.EncodeULong(writer, (ulong)segment.Count);
            writer.WriteBytes(segment.Array, segment.Offset, segment.Count);
        }

        public bool GetPacket(MemoryWriter target)
        {
            if (writers.Count > 0)
            {
                var cached = writers.Dequeue();
                if (target.position != 0)
                {
                    throw new ArgumentException("拷贝目标不是空的！");
                }

                ArraySegment<byte> segment = cached;
                target.WriteBytes(segment.Array, segment.Offset, segment.Count);
                MemoryWriter.Push(cached);
                return true;
            }

            if (writer != null)
            {
                if (target.position != 0)
                {
                    throw new ArgumentException("拷贝目标不是空的！");
                }

                ArraySegment<byte> segment = writer;
                target.WriteBytes(segment.Array, segment.Offset, segment.Count);
                MemoryWriter.Push(writer);
                writer = null;
                return true;
            }

            return false;
        }
    }

    internal class PacketReader
    {
        private readonly Queue<MemoryWriter> writers = new Queue<MemoryWriter>();
        private readonly MemoryReader reader = new MemoryReader();
        public int Count => writers.Count;

        public bool AddPacket(ArraySegment<byte> segment)
        {
            if (segment.Count < sizeof(ushort))
            {
                return false;
            }

            var writer = MemoryWriter.Pop();
            writer.WriteBytes(segment.Array, segment.Offset, segment.Count);
            if (writers.Count == 0)
            {
                reader.Reset(writer);
            }

            writers.Enqueue(writer);
            return true;
        }

        public bool GetMessage(out ArraySegment<byte> segment)
        {
            segment = default;
            if (writers.Count == 0)
            {
                return false;
            }

            if (reader.buffer.Count == 0)
            {
                return false;
            }

            if (reader.buffer.Count - reader.position == 0)
            {
                var writer = writers.Dequeue();
                MemoryWriter.Push(writer);
                if (writers.Count > 0)
                {
                    writer = writers.Peek();
                    reader.Reset(writer);
                }
                else
                {
                    return false;
                }
            }

            if (reader.buffer.Count - reader.position == 0)
            {
                return false;
            }

            var length = (int)Service.Bit.DecodeULong(reader);

            if (reader.buffer.Count - reader.position < length)
            {
                return false;
            }

            segment = reader.ReadArraySegment(length);
            return true;
        }
    }
}