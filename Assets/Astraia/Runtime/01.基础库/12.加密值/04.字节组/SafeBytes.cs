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
using System.Linq;

namespace Astraia.Common
{
    [Serializable]
    public struct SafeBytes
    {
        public byte[] origin;
        public int buffer;
        public int offset;

        public byte[] Value
        {
            get
            {
                if (origin == null)
                {
                    return null;
                }

                if (buffer == unchecked(origin.Aggregate(offset, (current, t) => (current * 31) ^ t)))
                {
                    return origin;
                }

                EventManager.Invoke(new VariableEvent());
                return null;
            }
            set
            {
                origin = value;
                offset = Service.Random.Next(1, int.MaxValue);
                buffer = unchecked(origin.Aggregate(offset, (current, t) => (current * 31) ^ t));
            }
        }

        public SafeBytes(byte[] value)
        {
            origin = value;
            offset = Service.Random.Next(1, int.MaxValue);
            buffer = unchecked(origin.Aggregate(offset, (current, t) => (current * 31) ^ t));
        }

        public static implicit operator byte[](SafeBytes variable)
        {
            return variable.Value;
        }

        public static implicit operator SafeBytes(byte[] value)
        {
            return new SafeBytes(value);
        }

        public override string ToString()
        {
            return BitConverter.ToString(Value, 0, origin.Length);
        }
    }
}