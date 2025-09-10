// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2024-12-23 18:12:21
// # Recently: 2025-01-08 17:01:30
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Collections.Generic;

namespace Astraia
{
    internal class WriterBatch
    {
        private readonly Queue<MemoryWriter> writers = new Queue<MemoryWriter>();
        private readonly int maxLength;
        private MemoryWriter writer;

        public WriterBatch(int maxLength)
        {
            this.maxLength = maxLength;
        }

        public void AddMessage(ArraySegment<byte> segment)
        {
            var length = Service.LEB.Invoke((ulong)segment.Count);
            if (writer != null && writer.position + length + segment.Count > maxLength)
            {
                writers.Enqueue(writer);
                writer = null;
            }

            writer ??= MemoryWriter.Pop();
            Service.LEB.EncodeULong(writer, (ulong)segment.Count);
            writer.WriteBytes(segment.Array, segment.Offset, segment.Count);
        }

        public bool GetBatch(MemoryWriter target)
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
}