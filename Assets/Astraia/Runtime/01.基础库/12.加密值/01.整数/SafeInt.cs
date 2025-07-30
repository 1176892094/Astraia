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
    [Serializable]
    public struct SafeInt: IEquatable<SafeInt>
    {
        public int origin;
        public int buffer;
        public int offset;

        public int Value
        {
            get
            {
                if (offset == 0 && buffer == 0)
                {
                    Value = origin;
                }

                if (buffer == (origin ^ offset))
                {
                    return origin ^ int.MaxValue;
                }

                EventManager.Invoke(new VariableEvent());
                return 0;
            }
            set
            {
                origin = value ^ int.MaxValue;
                offset = Service.Random.Next(1, int.MaxValue);
                buffer = origin ^ offset;
            }
        }

        public SafeInt(int value = 0)
        {
            origin = value ^ int.MaxValue;
            offset = Service.Random.Next(1, int.MaxValue);
            buffer = origin ^ offset;
        }

        public static implicit operator int(SafeInt variable)
        {
            return variable.Value;
        }

        public static implicit operator SafeInt(int value)
        {
            return new SafeInt(value);
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public bool Equals(SafeInt other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is SafeInt other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(origin);
        }
    }
}