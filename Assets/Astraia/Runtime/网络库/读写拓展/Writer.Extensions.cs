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

        public static void WriteInt16(this MemoryWriter writer, short value)
        {
            writer.Write(value);
        }

        public static void WriteInt16Nullable(this MemoryWriter writer, short? value)
        {
            writer.WriteNullable(value);
        }

        public static void WriteUInt16(this MemoryWriter writer, ushort value)
        {
            writer.Write(value);
        }

        public static void WriteUInt16Nullable(this MemoryWriter writer, ushort? value)
        {
            writer.WriteNullable(value);
        }

        public static void WriteInt32(this MemoryWriter writer, int value)
        {
            writer.WriteUInt32(Compress.ZigZagEncode(value));
        }

        public static void WriteInt32Nullable(this MemoryWriter writer, int? value)
        {
            writer.WriteNullable(value);
        }

        public static void WriteUInt32(this MemoryWriter writer, uint value)
        {
            Compress.EncodeUInt32(writer, value);
        }

        public static void WriteUInt32Nullable(this MemoryWriter writer, uint? value)
        {
            writer.WriteNullable(value);
        }

        public static void WriteInt64(this MemoryWriter writer, long value)
        {
            writer.WriteUInt64(Compress.ZigZagEncode(value));
        }

        public static void WriteInt64Nullable(this MemoryWriter writer, long? value)
        {
            writer.WriteNullable(value);
        }

        public static void WriteUInt64(this MemoryWriter writer, ulong value)
        {
            Compress.EncodeUInt64(writer, value);
        }

        public static void WriteUInt64Nullable(this MemoryWriter writer, ulong? value)
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
                writer.WriteUInt16(0);
                return;
            }

            writer.Resize(writer.position + 2 + Text.GetMaxByteCount(value.Length));
            var count = Text.GetBytes(value, value.Length, writer.buffer, writer.position + 2);
            if (count > ushort.MaxValue - 1)
            {
                throw new EndOfStreamException("写入字符串过长!");
            }

            writer.WriteUInt16(checked((ushort)(count + 1)));
            writer.position += count;
        }

        public static void WriteBytes(this MemoryWriter writer, byte[] value)
        {
            if (value == null)
            {
                Compress.EncodeUInt32(writer, 0);
                return;
            }

            Compress.EncodeUInt32(writer, checked((uint)value.Length + 1));
            writer.WriteBytes(value, 0, value.Length);
        }

        public static void WriteArraySegment(this MemoryWriter writer, ArraySegment<byte> value)
        {
            if (value == default)
            {
                Compress.EncodeUInt32(writer, 0);
                return;
            }

            Compress.EncodeUInt32(writer, checked((uint)value.Count + 1));
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
                Compress.EncodeUInt32(writer, 0);
                return;
            }

            Compress.EncodeUInt32(writer, checked((uint)values.Count + 1));
            foreach (var value in values)
            {
                writer.Invoke(value);
            }
        }

        public static void WriteHashSet<T>(this MemoryWriter writer, HashSet<T> values)
        {
            if (values == null)
            {
                Compress.EncodeUInt32(writer, 0);
                return;
            }

            Compress.EncodeUInt32(writer, checked((uint)values.Count + 1));
            foreach (var value in values)
            {
                writer.Invoke(value);
            }
        }

        public static void WriteArray<T>(this MemoryWriter writer, T[] values)
        {
            if (values == null)
            {
                Compress.EncodeUInt32(writer, 0);
                return;
            }

            Compress.EncodeUInt32(writer, checked((uint)values.Length + 1));
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
    }
}