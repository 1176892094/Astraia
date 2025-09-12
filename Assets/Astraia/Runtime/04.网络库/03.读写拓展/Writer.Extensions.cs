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
        public static void WriteByte(this MemoryWriter writer, byte value)
        {
            writer.Write(value);
        }

        public static void WriteByteNullable(this MemoryWriter writer, byte? value)
        {
            writer.WriteNullable(value);
        }

        public static void WriteSByte(this MemoryWriter writer, sbyte value)
        {
            writer.Write(value);
        }

        public static void WriteSByteNullable(this MemoryWriter writer, sbyte? value)
        {
            writer.WriteNullable(value);
        }

        public static void WriteChar(this MemoryWriter writer, char value)
        {
            writer.Write((ushort)value);
        }

        public static void WriteCharNullable(this MemoryWriter writer, char? value)
        {
            writer.WriteNullable((ushort?)value);
        }

        public static void WriteBool(this MemoryWriter writer, bool value)
        {
            writer.Write((byte)(value ? 1 : 0));
        }

        public static void WriteBoolNullable(this MemoryWriter writer, bool? value)
        {
            writer.WriteNullable(value.HasValue ? (byte)(value.Value ? 1 : 0) : new byte?());
        }

        public static void WriteShort(this MemoryWriter writer, short value)
        {
            writer.Write(value);
        }

        public static void WriteShortNullable(this MemoryWriter writer, short? value)
        {
            writer.WriteNullable(value);
        }

        public static void WriteUShort(this MemoryWriter writer, ushort value)
        {
            writer.Write(value);
        }

        public static void WriteUShortNullable(this MemoryWriter writer, ushort? value)
        {
            writer.WriteNullable(value);
        }

        public static void WriteInt(this MemoryWriter writer, int value)
        {
            writer.WriteUInt(Service.Bit.ZigZagEncode(value));
        }

        public static void WriteIntNullable(this MemoryWriter writer, int? value)
        {
            writer.WriteNullable(value);
        }

        public static void WriteUInt(this MemoryWriter writer, uint value)
        {
            Service.Bit.EncodeUInt(writer, value);
        }

        public static void WriteUIntNullable(this MemoryWriter writer, uint? value)
        {
            writer.WriteNullable(value);
        }

        public static void WriteLong(this MemoryWriter writer, long value)
        {
            writer.WriteULong(Service.Bit.ZigZagEncode(value));
        }

        public static void WriteLongNullable(this MemoryWriter writer, long? value)
        {
            writer.WriteNullable(value);
        }

        public static void WriteULong(this MemoryWriter writer, ulong value)
        {
            Service.Bit.EncodeULong(writer, value);
        }

        public static void WriteULongNullable(this MemoryWriter writer, ulong? value)
        {
            writer.WriteNullable(value);
        }

        public static void WriteFloat(this MemoryWriter writer, float value)
        {
            writer.Write(value);
        }

        public static void WriteFloatNullable(this MemoryWriter writer, float? value)
        {
            writer.WriteNullable(value);
        }

        public static void WriteDouble(this MemoryWriter writer, double value)
        {
            writer.Write(value);
        }

        public static void WriteDoubleNullable(this MemoryWriter writer, double? value)
        {
            writer.WriteNullable(value);
        }

        public static void WriteDecimal(this MemoryWriter writer, decimal value)
        {
            writer.Write(value);
        }

        public static void WriteDecimalNullable(this MemoryWriter writer, decimal? value)
        {
            writer.WriteNullable(value);
        }

        public static void WriteString(this MemoryWriter writer, string value)
        {
            if (value == null)
            {
                writer.WriteUShort(0);
                return;
            }

            writer.Resize(writer.position + 2 + Service.Text.GetByteCount(value.Length));
            var count = Service.Text.GetByteCount(value, value.Length, writer.buffer, writer.position + 2);
            if (count > ushort.MaxValue - 1)
            {
                throw new EndOfStreamException("写入字符串过长!");
            }

            writer.WriteUShort(checked((ushort)(count + 1)));
            writer.position += count;
        }

        public static void WriteBytes(this MemoryWriter writer, byte[] value)
        {
            if (value == null)
            {
                Service.Bit.EncodeULong(writer, 0);
                return;
            }

            Service.Bit.EncodeULong(writer, checked((uint)value.Length) + 1);
            writer.WriteBytes(value, 0, value.Length);
        }

        public static void WriteArraySegment(this MemoryWriter writer, ArraySegment<byte> value)
        {
            if (value == default)
            {
                Service.Bit.EncodeULong(writer, 0);
                return;
            }

            Service.Bit.EncodeULong(writer, checked((uint)value.Count) + 1);
            writer.WriteBytes(value.Array, value.Offset, value.Count);
        }

        public static void WriteDateTime(this MemoryWriter writer, DateTime value)
        {
            writer.WriteDouble(value.ToOADate());
        }

        public static void WriteList<T>(this MemoryWriter writer, List<T> values)
        {
            if (values == null)
            {
                Service.Bit.EncodeULong(writer, 0);
                return;
            }

            Service.Bit.EncodeULong(writer, checked((uint)values.Count) + 1);
            foreach (var value in values)
            {
                writer.Invoke(value);
            }
        }

        public static void WriteHashSet<T>(this MemoryWriter writer, HashSet<T> values)
        {
            if (values == null)
            {
                Service.Bit.EncodeULong(writer, 0);
                return;
            }

            Service.Bit.EncodeULong(writer, checked((uint)values.Count) + 1);
            foreach (var value in values)
            {
                writer.Invoke(value);
            }
        }

        public static void WriteArray<T>(this MemoryWriter writer, T[] values)
        {
            if (values == null)
            {
                Service.Bit.EncodeULong(writer, 0);
                return;
            }

            Service.Bit.EncodeULong(writer, checked((uint)values.Length) + 1);
            foreach (var value in values)
            {
                writer.Invoke(value);
            }
        }

        public static void WriteUri(this MemoryWriter writer, Uri value)
        {
            if (value == null)
            {
                writer.WriteString(null);
                return;
            }

            writer.WriteString(value.ToString());
        }

        public static void WriteType(this MemoryWriter writer, Type value)
        {
            if (value == null)
            {
                writer.WriteString(null);
                return;
            }

            writer.WriteString(value.FullName + "," + value.Assembly.GetName().Name);
        }
    }
}