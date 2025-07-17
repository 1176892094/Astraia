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

        public static void WriteByteNull(this MemoryWriter writer, byte? value)
        {
            writer.Writable(value);
        }

        public static void WriteSByte(this MemoryWriter writer, sbyte value)
        {
            writer.Write(value);
        }

        public static void WriteSByteNull(this MemoryWriter writer, sbyte? value)
        {
            writer.Writable(value);
        }

        public static void WriteChar(this MemoryWriter writer, char value)
        {
            writer.Write((ushort)value);
        }

        public static void WriteCharNull(this MemoryWriter writer, char? value)
        {
            writer.Writable((ushort?)value);
        }

        public static void WriteBool(this MemoryWriter writer, bool value)
        {
            writer.Write((byte)(value ? 1 : 0));
        }

        public static void WriteBoolNull(this MemoryWriter writer, bool? value)
        {
            writer.Writable(value.HasValue ? (byte)(value.Value ? 1 : 0) : new byte?());
        }

        public static void WriteShort(this MemoryWriter writer, short value)
        {
            writer.Write(value);
        }

        public static void WriteShortNull(this MemoryWriter writer, short? value)
        {
            writer.Writable(value);
        }

        public static void WriteUShort(this MemoryWriter writer, ushort value)
        {
            writer.Write(value);
        }

        public static void WriteUShortNull(this MemoryWriter writer, ushort? value)
        {
            writer.Writable(value);
        }

        public static void WriteInt(this MemoryWriter writer, int value)
        {
            writer.Write(value);
        }

        public static void WriteIntNull(this MemoryWriter writer, int? value)
        {
            writer.Writable(value);
        }

        public static void WriteUInt(this MemoryWriter writer, uint value)
        {
            writer.Write(value);
        }

        public static void WriteUIntNull(this MemoryWriter writer, uint? value)
        {
            writer.Writable(value);
        }

        public static void WriteLong(this MemoryWriter writer, long value)
        {
            writer.Write(value);
        }

        public static void WriteLongNull(this MemoryWriter writer, long? value)
        {
            writer.Writable(value);
        }

        public static void WriteULong(this MemoryWriter writer, ulong value)
        {
            writer.Write(value);
        }

        public static void WriteULongNull(this MemoryWriter writer, ulong? value)
        {
            writer.Writable(value);
        }

        public static void WriteFloat(this MemoryWriter writer, float value)
        {
            writer.Write(value);
        }

        public static void WriteFloatNull(this MemoryWriter writer, float? value)
        {
            writer.Writable(value);
        }

        public static void WriteDouble(this MemoryWriter writer, double value)
        {
            writer.Write(value);
        }

        public static void WriteDoubleNull(this MemoryWriter writer, double? value)
        {
            writer.Writable(value);
        }

        public static void WriteDecimal(this MemoryWriter writer, decimal value)
        {
            writer.Write(value);
        }

        public static void WriteDecimalNull(this MemoryWriter writer, decimal? value)
        {
            writer.Writable(value);
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
                writer.WriteUInt(0);
                return;
            }

            writer.WriteUInt(checked((uint)value.Length) + 1);
            writer.WriteBytes(value, 0, value.Length);
        }

        public static void WriteArraySegment(this MemoryWriter writer, ArraySegment<byte> value)
        {
            if (value == default)
            {
                writer.WriteUInt(0);
                return;
            }

            writer.WriteUInt(checked((uint)value.Count) + 1);
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
                writer.WriteInt(-1);
                return;
            }

            writer.WriteInt(values.Count);
            foreach (var value in values)
            {
                writer.Invoke(value);
            }
        }

        public static void WriteArray<T>(this MemoryWriter writer, T[] values)
        {
            if (values == null)
            {
                writer.WriteInt(-1);
                return;
            }

            writer.WriteInt(values.Length);
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

            writer.WriteString(value.FullName);
        }
    }
}