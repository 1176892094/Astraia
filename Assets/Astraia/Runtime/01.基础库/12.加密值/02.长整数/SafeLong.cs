// // *********************************************************************************
// // # Project: JFramework
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-07-19 11:07:38
// // # Recently: 2025-07-19 11:07:38
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using Astraia.Common;

namespace Astraia
{
    [Serializable]
    public struct SafeLong : IEquatable<SafeLong>
    {
        public long origin;
        public long buffer;
        public long offset;

        public long Value
        {
            get
            {
                if (offset == 0 && buffer == 0)
                {
                    Value = origin;
                }

                if (buffer == (origin ^ offset))
                {
                    return origin ^ long.MaxValue;
                }

                EventManager.Invoke(new VariableEvent());
                return 0;
            }
            set
            {
                origin = value ^ long.MaxValue;
                offset = Service.Random.Next(1, int.MaxValue);
                buffer = origin ^ offset;
            }
        }

        public SafeLong(long value = 0)
        {
            origin = value ^ long.MaxValue;
            offset = Service.Random.Next(1, int.MaxValue);
            buffer = origin ^ offset;
        }

        public static implicit operator long(SafeLong variable)
        {
            return variable.Value;
        }

        public static implicit operator SafeLong(long value)
        {
            return new SafeLong(value);
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public bool Equals(SafeLong other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is SafeLong other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(origin);
        }
    }
}