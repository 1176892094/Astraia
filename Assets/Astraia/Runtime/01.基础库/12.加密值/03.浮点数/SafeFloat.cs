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

namespace Astraia.Common
{
    [Serializable]
    public struct SafeFloat
    {
        public int origin;
        public int buffer;
        public int offset;

        public float Value
        {
            get
            {
                if (offset == 0 && buffer == 0)
                {
                    Value = origin;
                }

                if (buffer == (origin ^ offset))
                {
                    return BitConverter.Int32BitsToSingle(origin ^ int.MaxValue);
                }

                EventManager.Invoke(new VariableEvent());
                return 0;
            }
            set
            {
                origin = BitConverter.SingleToInt32Bits(value) ^ int.MaxValue;
                offset = Service.Random.Next(1, int.MaxValue);
                buffer = origin ^ offset;
            }
        }

        public SafeFloat(float value = 0)
        {
            origin = BitConverter.SingleToInt32Bits(value) ^ int.MaxValue;
            offset = Service.Random.Next(1, int.MaxValue);
            buffer = origin ^ offset;
        }

        public static implicit operator float(SafeFloat variable)
        {
            return variable.Value;
        }

        public static implicit operator SafeFloat(float value)
        {
            return new SafeFloat(value);
        }

        public override string ToString()
        {
            return Value.ToString(CultureInfo.InvariantCulture);
        }
    }
}