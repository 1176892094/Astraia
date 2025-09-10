// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2024-12-23 18:12:21
// # Recently: 2025-01-08 17:01:40
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Collections.Generic;

namespace Astraia
{
    internal class ReaderBatch
    {
        private readonly Queue<MemoryWriter> writers = new Queue<MemoryWriter>();
        private readonly MemoryReader reader = new MemoryReader();
        public int Count => writers.Count;


        public bool AddBatch(ArraySegment<byte> segment)
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

            var length = (int)Service.Zip.DecodeULong(reader);
            
            if (reader.buffer.Count - reader.position < length)
            {
                return false;
            }

            segment = reader.ReadArraySegment(length);
            return true;
        }
    }
}