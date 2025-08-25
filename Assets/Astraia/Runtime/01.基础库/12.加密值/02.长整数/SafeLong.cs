// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-07-19 11:07:38
// // # Recently: 2025-07-19 11:07:38
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using System.Runtime.CompilerServices;

namespace Astraia.Common
{
    public static partial class Safe
    {
        [Serializable]
        public struct Long : IEquatable<Long>
        {
            private static readonly long Ticks = DateTime.Now.Ticks;
            public long origin;
            public long buffer;
            public long offset;

            public long Value
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    var value = origin ^ offset;
                    if (buffer != ((offset >> 8) ^ value))
                    {
                        throw new InvalidOperationException();
                    }

                    return value;
                }
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set
                {
                    offset = Ticks;
                    origin = value ^ offset;
                    buffer = (offset >> 8) ^ value;
                }
            }

            public Long(long value = 0)
            {
                offset = Ticks;
                origin = value ^ offset;
                buffer = (offset >> 8) ^ value;
            }

            public static implicit operator long(Long data)
            {
                return data.Value;
            }

            public static implicit operator Long(long data)
            {
                return new Long(data);
            }

            public bool Equals(Long other)
            {
                return Value == other.Value;
            }

            public override bool Equals(object obj)
            {
                return obj is Long other && Equals(other);
            }

            public override string ToString()
            {
                return Value.ToString();
            }

            public override int GetHashCode()
            {
                return Value.GetHashCode();
            }
        }
    }
}