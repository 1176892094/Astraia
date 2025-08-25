// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-07-19 11:07:45
// // # Recently: 2025-07-19 11:07:45
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;

namespace Astraia.Common
{
    public static partial class Safe
    {
        [Serializable]
        public struct Int : IEquatable<Int>
        {
            private static readonly int Ticks = (int)DateTime.Now.Ticks;
            public int origin;
            public int buffer;
            public int offset;

            public int Value
            {
                get
                {
                    var value = origin ^ offset;
                    if (buffer != ((offset >> 8) ^ value))
                    {
                        throw new InvalidOperationException();
                    }

                    return value;
                }
                set
                {
                    offset = Ticks;
                    origin = value ^ offset;
                    buffer = (offset >> 8) ^ value;
                }
            }

            public Int(int value = 0)
            {
                offset = Ticks;
                origin = value ^ offset;
                buffer = (offset >> 8) ^ value;
            }

            public static implicit operator int(Int data)
            {
                return data.Value;
            }

            public static implicit operator Int(int data)
            {
                return new Int(data);
            }

            public bool Equals(Int other)
            {
                return Value == other.Value;
            }

            public override bool Equals(object obj)
            {
                return obj is Int other && Equals(other);
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