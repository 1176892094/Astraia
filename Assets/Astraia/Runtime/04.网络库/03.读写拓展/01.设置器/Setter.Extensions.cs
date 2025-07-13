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
        public static void SetByte(this MemoryWriter writer, byte value)
        {
            writer.Set(value);
        }

        public static void SetByteNull(this MemoryWriter writer, byte? value)
        {
            writer.Setable(value);
        }

        public static void SetSByte(this MemoryWriter writer, sbyte value)
        {
            writer.Set(value);
        }

        public static void SetSByteNull(this MemoryWriter writer, sbyte? value)
        {
            writer.Setable(value);
        }

        public static void SetChar(this MemoryWriter writer, char value)
        {
            writer.Set((ushort)value);
        }

        public static void SetCharNull(this MemoryWriter writer, char? value)
        {
            writer.Setable((ushort?)value);
        }

        public static void SetBool(this MemoryWriter writer, bool value)
        {
            writer.Set((byte)(value ? 1 : 0));
        }

        public static void SetBoolNull(this MemoryWriter writer, bool? value)
        {
            writer.Setable(value.HasValue ? (byte)(value.Value ? 1 : 0) : new byte?());
        }

        public static void SetShort(this MemoryWriter writer, short value)
        {
            writer.Set(value);
        }

        public static void SetShortNull(this MemoryWriter writer, short? value)
        {
            writer.Setable(value);
        }

        public static void SetUShort(this MemoryWriter writer, ushort value)
        {
            writer.Set(value);
        }

        public static void SetUShortNull(this MemoryWriter writer, ushort? value)
        {
            writer.Setable(value);
        }

        public static void SetInt(this MemoryWriter writer, int value)
        {
            writer.Set(value);
        }

        public static void SetIntNull(this MemoryWriter writer, int? value)
        {
            writer.Setable(value);
        }

        public static void SetUInt(this MemoryWriter writer, uint value)
        {
            writer.Set(value);
        }

        public static void SetUIntNull(this MemoryWriter writer, uint? value)
        {
            writer.Setable(value);
        }

        public static void SetLong(this MemoryWriter writer, long value)
        {
            writer.Set(value);
        }

        public static void SetLongNull(this MemoryWriter writer, long? value)
        {
            writer.Setable(value);
        }

        public static void SetULong(this MemoryWriter writer, ulong value)
        {
            writer.Set(value);
        }

        public static void SetULongNull(this MemoryWriter writer, ulong? value)
        {
            writer.Setable(value);
        }

        public static void SetFloat(this MemoryWriter writer, float value)
        {
            writer.Set(value);
        }

        public static void SetFloatNull(this MemoryWriter writer, float? value)
        {
            writer.Setable(value);
        }

        public static void SetDouble(this MemoryWriter writer, double value)
        {
            writer.Set(value);
        }

        public static void SetDoubleNull(this MemoryWriter writer, double? value)
        {
            writer.Setable(value);
        }

        public static void SetDecimal(this MemoryWriter writer, decimal value)
        {
            writer.Set(value);
        }

        public static void SetDecimalNull(this MemoryWriter writer, decimal? value)
        {
            writer.Setable(value);
        }

        public static void SetString(this MemoryWriter writer, string value)
        {
            if (value == null)
            {
                writer.SetUShort(0);
                return;
            }

            writer.Resize(writer.position + 2 + Service.Text.GetByteCount(value.Length));
            var count = Service.Text.GetByteCount(value, value.Length, writer.buffer, writer.position + 2);
            if (count > ushort.MaxValue - 1)
            {
                throw new EndOfStreamException("写入字符串过长!");
            }

            writer.SetUShort(checked((ushort)(count + 1)));
            writer.position += count;
        }

        public static void SetBytes(this MemoryWriter writer, byte[] value)
        {
            if (value == null)
            {
                writer.SetUInt(0);
                return;
            }

            writer.SetUInt(checked((uint)value.Length) + 1);
            writer.SetBytes(value, 0, value.Length);
        }

        public static void SetArraySegment(this MemoryWriter writer, ArraySegment<byte> value)
        {
            if (value == default)
            {
                writer.SetUInt(0);
                return;
            }

            writer.SetUInt(checked((uint)value.Count) + 1);
            writer.SetBytes(value.Array, value.Offset, value.Count);
        }

        public static void SetDateTime(this MemoryWriter writer, DateTime value)
        {
            writer.SetDouble(value.ToOADate());
        }

        public static void SetList<T>(this MemoryWriter writer, List<T> values)
        {
            if (values == null)
            {
                writer.SetInt(-1);
                return;
            }

            writer.SetInt(values.Count);
            foreach (var value in values)
            {
                writer.Invoke(value);
            }
        }

        public static void SetArray<T>(this MemoryWriter writer, T[] values)
        {
            if (values == null)
            {
                writer.SetInt(-1);
                return;
            }

            writer.SetInt(values.Length);
            foreach (var value in values)
            {
                writer.Invoke(value);
            }
        }

        public static void SetUri(this MemoryWriter writer, Uri value)
        {
            if (value == null)
            {
                writer.SetString(null);
                return;
            }

            writer.SetString(value.ToString());
        }
        
        public static void SetType(this MemoryWriter writer, Type value)
        {
            if (value == null)
            {
                writer.SetString(null);
                return;
            }

            writer.SetString(value.FullName);
        }
    }
}