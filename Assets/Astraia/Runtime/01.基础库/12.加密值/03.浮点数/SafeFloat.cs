// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-07-19 11:07:03
// // # Recently: 2025-07-19 11:07:04
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Astraia.Common
{
    public static partial class Safe
    {
        [Serializable]
        public struct Float : IEquatable<Float>
        {
            private static readonly int Ticks = (int)DateTime.Now.Ticks;
            public int origin;
            public int buffer;
            public int offset;

            public unsafe float Value
            {
                get
                {
                    var value = origin ^ offset;
                    if (buffer != ((offset >> 8) ^ value))
                    {
                        throw new InvalidOperationException();
                    }

                    return *(float*)&value;
                }
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set
                {
                    var ptr = *(int*)&value;
                    offset = Ticks;
                    origin = ptr ^ offset;
                    buffer = (offset >> 8) ^ ptr;
                }
            }

            public unsafe Float(float value = 0)
            {
                var ptr = *(int*)&value;
                offset = Ticks;
                origin = ptr ^ offset;
                buffer = (offset >> 8) ^ ptr;
            }

            public static implicit operator float(Float variable)
            {
                return variable.Value;
            }

            public static implicit operator Float(float value)
            {
                return new Float(value);
            }

            public bool Equals(Float other)
            {
                return origin == other.origin;
            }

            public override bool Equals(object obj)
            {
                return obj is Float other && Equals(other);
            }

            public override string ToString()
            {
                return Value.ToString(CultureInfo.InvariantCulture);
            }

            public override int GetHashCode()
            {
                return Value.GetHashCode();
            }
        }
    }
}