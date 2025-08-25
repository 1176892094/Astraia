// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-07-19 13:07:52
// // # Recently: 2025-07-19 13:07:52
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;

namespace Astraia.Common
{
    public static partial class Safe
    {
        [Serializable]
        public struct Bytes : IEquatable<Bytes>
        {
            public byte[] origin;
            public int buffer;
            public int offset;

            public byte[] Value
            {
                get
                {
                    if (buffer != GetHashCode())
                    {
                        throw new InvalidOperationException();
                    }

                    return origin;
                }
                set
                {
                    offset = Environment.TickCount;
                    origin = value;
                    buffer = GetHashCode();
                }
            }

            public Bytes(byte[] value)
            {
                offset = Environment.TickCount;
                origin = value;
                buffer = 0;
                buffer = GetHashCode();
            }

            public static implicit operator byte[](Bytes variable)
            {
                return variable.Value;
            }

            public static implicit operator Bytes(byte[] value)
            {
                return new Bytes(value);
            }

            public bool Equals(Bytes other)
            {
                return buffer - offset == other.buffer - other.offset;
            }

            public override bool Equals(object obj)
            {
                return obj is Bytes other && Equals(other);
            }

            public override string ToString()
            {
                return BitConverter.ToString(Value, 0, origin.Length);
            }

            public override int GetHashCode()
            {
                var result = offset;
                unchecked
                {
                    foreach (var b in origin)
                    {
                        result = (result * 31) ^ b;
                    }

                    return result;
                }
            }
        }
    }
}