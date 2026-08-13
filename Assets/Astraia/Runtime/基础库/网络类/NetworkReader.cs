// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-08-13 23:08:02
// # Recently: 2026-08-13 23:07:02
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Collections.Generic;

namespace Astraia
{
    internal sealed class NetworkReader
    {
        private readonly Queue<MemoryWriter> writers = new();
        private readonly MemoryReader reader = new();

        public int Count => writers.Count;

        public bool AddBatch(ArraySegment<byte> segment)
        {
            if (segment.Count < 1 + sizeof(ushort))
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

            while (reader.position >= reader.buffer.Count)
            {
                if (writers.Count == 0)
                {
                    return false;
                }

                var writer = writers.Dequeue();
                MemoryWriter.Push(writer);

                if (writers.Count == 0)
                {
                    return false;
                }

                reader.Reset(writers.Peek());
            }

            var count = (int)Compress.DecodeUInt32(reader);

            if (reader.buffer.Count - reader.position < count)
            {
                return false;
            }

            segment = reader.ReadArraySegment(count);
            return true;
        }
    }
}