// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-08-13 22:08:34
// # Recently: 2026-08-13 22:59:34
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Runtime.CompilerServices;

namespace Astraia
{
    [Serializable]
    public struct Position : IEquatable<Position>
    {
        public static readonly Position Zero = new Position(0, 0);

        public Fixation x;
        public Fixation y;

        internal int X => (int)x;
        internal int Y => (int)y;

        public Fixation sqrMagnitude => x * x + y * y;
        public Fixation magnitude => Fixation.Sqrt(sqrMagnitude);
        public Position normalize => x == 0 && y == 0 ? Zero : this / magnitude;

        public Position(Fixation x, Fixation y)
        {
            this.x = x;
            this.y = y;
        }

        public bool Equals(Position other)
        {
            return x.Equals(other.x) && y.Equals(other.y);
        }

        public override bool Equals(object obj)
        {
            return obj is Position other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (X << 16) ^ (Y & 0xFFFF);
        }

        public override string ToString()
        {
            return "({0}, {1})".Format(x, y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Position a, Position b)
        {
            return a.x == b.x && a.y == b.y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Position a, Position b)
        {
            return a.x != b.x || a.y != b.y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Position operator +(Position a, Position b)
        {
            return new Position(a.x + b.x, a.y + b.y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Position operator -(Position a, Position b)
        {
            return new Position(a.x - b.x, a.y - b.y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Position operator *(Position a, Fixation b)
        {
            return new Position(a.x * b, a.y * b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Position operator /(Position a, Fixation b)
        {
            return new Position(a.x / b, a.y / b);
        }

        public static Fixation Dot(Position a, Position b)
        {
            return a.x * b.x + a.y * b.y;
        }

        public static Fixation Cross(Position a, Position b)
        {
            return a.x * b.y - a.y * b.x;
        }

        public static Fixation Distance(Position a, Position b)
        {
            return Fixation.Sqrt((a - b).sqrMagnitude);
        }

        public static Position MoveTowards(Position current, Position target, Fixation maxDistanceDelta)
        {
            var delta = target - current;

            var sqrDistance = delta.sqrMagnitude;

            if (sqrDistance == 0)
            {
                return target;
            }

            var maxSqrDistance = maxDistanceDelta * maxDistanceDelta;

            if (sqrDistance <= maxSqrDistance)
            {
                return target;
            }

            return current + delta.normalize * maxDistanceDelta;
        }
    }
}