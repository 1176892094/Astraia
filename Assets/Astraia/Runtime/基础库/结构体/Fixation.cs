// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-08-13 22:08:15
// # Recently: 2026-08-13 22:59:15
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Runtime.CompilerServices;

namespace Astraia
{
    [Serializable]
    public struct Fixation : IEquatable<Fixation>
    {
        private const int BIT = 12;
        private const int FIX = 1 << BIT;

        public static readonly Fixation One = new Fixation(FIX);
        public static readonly Fixation Zero = new Fixation(0);
        public static readonly Fixation Epsilon = new Fixation(1);
        public static readonly Fixation MaxValue = new Fixation(int.MaxValue);
        public static readonly Fixation MinValue = new Fixation(int.MinValue);

        public int value;

        public Fixation(int value)
        {
            this.value = value;
        }

        public bool Equals(Fixation other)
        {
            return value == other.value;
        }

        public override bool Equals(object obj)
        {
            return obj is Fixation other && Equals(other);
        }

        public override int GetHashCode()
        {
            return value;
        }

        public override string ToString()
        {
            return ((float)value / FIX).ToString("R");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <(Fixation a, Fixation b)
        {
            return a.value < b.value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >(Fixation a, Fixation b)
        {
            return a.value > b.value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator <=(Fixation a, Fixation b)
        {
            return a.value <= b.value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator >=(Fixation a, Fixation b)
        {
            return a.value >= b.value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Fixation a, Fixation b)
        {
            return a.value == b.value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Fixation a, Fixation b)
        {
            return a.value != b.value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixation operator +(Fixation a, Fixation b)
        {
            return new Fixation(a.value + b.value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixation operator -(Fixation a, Fixation b)
        {
            return new Fixation(a.value - b.value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixation operator *(Fixation a, Fixation b)
        {
            return new Fixation((int)(((long)a.value * b.value) >> BIT));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Fixation operator /(Fixation a, Fixation b)
        {
            return new Fixation((int)(((long)a.value << BIT) / b.value));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator int(Fixation value)
        {
            return value.value >> BIT;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Fixation(int value)
        {
            return new Fixation(value << BIT);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator float(Fixation value)
        {
            return (float)value.value / FIX;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Fixation(float value)
        {
            return new Fixation((int)(value * FIX));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int FloorToInt()
        {
            return value >> BIT;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int CeilToInt()
        {
            return value >= 0 ? (value + FIX - 1) >> BIT : -(-value >> BIT);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly int RoundToInt()
        {
            return value >= 0 ? (value + (1 << (BIT - 1))) >> BIT : -((-value + (1 << (BIT - 1))) >> BIT);
        }

        public static Fixation Max(Fixation a, Fixation b)
        {
            return a > b ? a : b;
        }

        public static Fixation Min(Fixation a, Fixation b)
        {
            return a < b ? a : b;
        }

        public static Fixation Abs(Fixation a)
        {
            return a < 0 ? -a : a;
        }

        public static int Sign(Fixation value)
        {
            return value > 0 ? 1 : value < 0 ? -1 : 0;
        }

        public static Fixation Sqrt(Fixation value)
        {
            if (value.value <= 0)
            {
                return Zero;
            }

            var x = (long)value.value << BIT;

            var count = 0;
            var index = x;

            while (index > 0)
            {
                index >>= 1;
                count++;
            }

            var guess = 1L << ((count + 1) >> 1);
            while (true)
            {
                var next = (guess + x / guess) >> 1;
                if (next >= guess)
                {
                    break;
                }

                guess = next;
            }

            return new Fixation((int)guess);
        }
    }
}