// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-08-13 23:08:07
// # Recently: 2026-08-13 23:07:07
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Collections.Generic;

namespace Astraia
{
    internal sealed class NetworkWriter
    {
        private readonly Queue<MemoryWriter> writers = new();
        private readonly int capacity;
        private MemoryWriter writer;

        public NetworkWriter(int capacity)
        {
            this.capacity = capacity;
        }

        public void AddMessage(ArraySegment<byte> segment)
        {
            var count = Compress.Length((ulong)segment.Count);
            if (writer == null)
            {
                writer = MemoryWriter.Pop();
            }
            else if (writer.position + count + segment.Count > capacity)
            {
                writers.Enqueue(writer);
                writer = MemoryWriter.Pop();
            }

            Compress.EncodeUInt32(writer, (uint)segment.Count);
            writer.WriteBytes(segment.Array, segment.Offset, segment.Count);
        }

        public bool GetBatch(MemoryWriter result)
        {
            if (result.position != 0)
            {
                throw new ArgumentException("拷贝目标不是空的!", nameof(result));
            }

            MemoryWriter copied;
            if (writers.Count > 0)
            {
                copied = writers.Dequeue();
            }
            else if (writer != null)
            {
                copied = writer;
                writer = null;
            }
            else
            {
                return false;
            }

            result.WriteBytes(copied.buffer, 0, copied.position);
            MemoryWriter.Push(copied);
            return true;
        }
    }
}