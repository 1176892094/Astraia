// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2024-12-21 23:12:50
// # Recently: 2024-12-22 20:12:58
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Collections.Generic;
using System.IO;

namespace Astraia.Net
{
    public static partial class Extensions
    {
        public static void SetByte(this MemorySetter setter, byte value)
        {
            setter.Set(value);
        }

        public static void SetByteNull(this MemorySetter setter, byte? value)
        {
            setter.Setable(value);
        }

        public static void SetSByte(this MemorySetter setter, sbyte value)
        {
            setter.Set(value);
        }

        public static void SetSByteNull(this MemorySetter setter, sbyte? value)
        {
            setter.Setable(value);
        }

        public static void SetChar(this MemorySetter setter, char value)
        {
            setter.Set((ushort)value);
        }

        public static void SetCharNull(this MemorySetter setter, char? value)
        {
            setter.Setable((ushort?)value);
        }

        public static void SetBool(this MemorySetter setter, bool value)
        {
            setter.Set((byte)(value ? 1 : 0));
        }

        public static void SetBoolNull(this MemorySetter setter, bool? value)
        {
            setter.Setable(value.HasValue ? (byte)(value.Value ? 1 : 0) : new byte?());
        }

        public static void SetShort(this MemorySetter setter, short value)
        {
            setter.Set(value);
        }

        public static void SetShortNull(this MemorySetter setter, short? value)
        {
            setter.Setable(value);
        }

        public static void SetUShort(this MemorySetter setter, ushort value)
        {
            setter.Set(value);
        }

        public static void SetUShortNull(this MemorySetter setter, ushort? value)
        {
            setter.Setable(value);
        }

        public static void SetInt(this MemorySetter setter, int value)
        {
            setter.Set(value);
        }

        public static void SetIntNull(this MemorySetter setter, int? value)
        {
            setter.Setable(value);
        }

        public static void SetUInt(this MemorySetter setter, uint value)
        {
            setter.Set(value);
        }

        public static void SetUIntNull(this MemorySetter setter, uint? value)
        {
            setter.Setable(value);
        }

        public static void SetLong(this MemorySetter setter, long value)
        {
            setter.Set(value);
        }

        public static void SetLongNull(this MemorySetter setter, long? value)
        {
            setter.Setable(value);
        }

        public static void SetULong(this MemorySetter setter, ulong value)
        {
            setter.Set(value);
        }

        public static void SetULongNull(this MemorySetter setter, ulong? value)
        {
            setter.Setable(value);
        }

        public static void SetFloat(this MemorySetter setter, float value)
        {
            setter.Set(value);
        }

        public static void SetFloatNull(this MemorySetter setter, float? value)
        {
            setter.Setable(value);
        }

        public static void SetDouble(this MemorySetter setter, double value)
        {
            setter.Set(value);
        }

        public static void SetDoubleNull(this MemorySetter setter, double? value)
        {
            setter.Setable(value);
        }

        public static void SetDecimal(this MemorySetter setter, decimal value)
        {
            setter.Set(value);
        }

        public static void SetDecimalNull(this MemorySetter setter, decimal? value)
        {
            setter.Setable(value);
        }

        public static void SetString(this MemorySetter setter, string value)
        {
            if (value == null)
            {
                setter.SetUShort(0);
                return;
            }

            setter.Resize(setter.position + 2 + Service.Text.GetByteCount(value.Length));
            var count = Service.Text.GetByteCount(value, value.Length, setter.buffer, setter.position + 2);
            if (count > ushort.MaxValue - 1)
            {
                throw new EndOfStreamException("写入字符串过长!");
            }

            setter.SetUShort(checked((ushort)(count + 1)));
            setter.position += count;
        }

        public static void SetBytes(this MemorySetter setter, byte[] value)
        {
            if (value == null)
            {
                setter.SetUInt(0);
                return;
            }

            setter.SetUInt(checked((uint)value.Length) + 1);
            setter.SetBytes(value, 0, value.Length);
        }

        public static void SetArraySegment(this MemorySetter setter, ArraySegment<byte> value)
        {
            if (value == default)
            {
                setter.SetUInt(0);
                return;
            }

            setter.SetUInt(checked((uint)value.Count) + 1);
            setter.SetBytes(value.Array, value.Offset, value.Count);
        }

        public static void SetDateTime(this MemorySetter setter, DateTime value)
        {
            setter.SetDouble(value.ToOADate());
        }

        public static void SetList<T>(this MemorySetter setter, List<T> values)
        {
            if (values == null)
            {
                setter.SetInt(-1);
                return;
            }

            setter.SetInt(values.Count);
            foreach (var value in values)
            {
                setter.Invoke(value);
            }
        }

        public static void SetArray<T>(this MemorySetter setter, T[] values)
        {
            if (values == null)
            {
                setter.SetInt(-1);
                return;
            }

            setter.SetInt(values.Length);
            foreach (var value in values)
            {
                setter.Invoke(value);
            }
        }

        public static void SetUri(this MemorySetter setter, Uri value)
        {
            if (value == null)
            {
                setter.SetString(null);
                return;
            }

            setter.SetString(value.ToString());
        }
        
        public static void SetType(this MemorySetter setter, Type value)
        {
            if (value == null)
            {
                setter.SetString(null);
                return;
            }

            setter.SetString(value.FullName);
        }
    }
}