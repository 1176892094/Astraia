// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2024-12-21 16:12:11
// # Recently: 2024-12-22 20:12:59
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
        public static byte ReadByte(this MemoryReader reader)
        {
            return reader.Read<byte>();
        }

        public static byte? ReadByteNull(this MemoryReader reader)
        {
            return reader.Readable<byte>();
        }

        public static sbyte ReadSByte(this MemoryReader reader)
        {
            return reader.Read<sbyte>();
        }

        public static sbyte? ReadSByteNull(this MemoryReader reader)
        {
            return reader.Readable<sbyte>();
        }

        public static char ReadChar(this MemoryReader reader)
        {
            return (char)reader.Read<ushort>();
        }

        public static char? ReadCharNull(this MemoryReader reader)
        {
            return (char?)reader.Readable<ushort>();
        }

        public static bool ReadBool(this MemoryReader reader)
        {
            return reader.Read<byte>() != 0;
        }

        public static bool? ReadBoolNull(this MemoryReader reader)
        {
            var value = reader.Readable<byte>();
            return value.HasValue ? value.Value != 0 : default(bool?);
        }

        public static short ReadShort(this MemoryReader reader)
        {
            return reader.Read<short>();
        }

        public static short? ReadShortNull(this MemoryReader reader)
        {
            return reader.Readable<short>();
        }

        public static ushort ReadUShort(this MemoryReader reader)
        {
            return reader.Read<ushort>();
        }

        public static ushort? ReadUShortNull(this MemoryReader reader)
        {
            return reader.Readable<ushort>();
        }

        public static int ReadInt(this MemoryReader reader)
        {
            return reader.Read<int>();
        }

        public static int? ReadIntNull(this MemoryReader reader)
        {
            return reader.Readable<int>();
        }

        public static uint ReadUInt(this MemoryReader reader)
        {
            return reader.Read<uint>();
        }

        public static uint? ReadUIntNull(this MemoryReader reader)
        {
            return reader.Readable<uint>();
        }

        public static long ReadLong(this MemoryReader reader)
        {
            return reader.Read<long>();
        }

        public static long? ReadLongNull(this MemoryReader reader)
        {
            return reader.Readable<long>();
        }

        public static ulong ReadULong(this MemoryReader reader)
        {
            return reader.Read<ulong>();
        }

        public static ulong? ReadULongNull(this MemoryReader reader)
        {
            return reader.Readable<ulong>();
        }

        public static float ReadFloat(this MemoryReader reader)
        {
            return reader.Read<float>();
        }

        public static float? ReadFloatNull(this MemoryReader reader)
        {
            return reader.Readable<float>();
        }

        public static double ReadDouble(this MemoryReader reader)
        {
            return reader.Read<double>();
        }

        public static double? ReadDoubleNull(this MemoryReader reader)
        {
            return reader.Readable<double>();
        }

        public static decimal ReadDecimal(this MemoryReader reader)
        {
            return reader.Read<decimal>();
        }

        public static decimal? ReadDecimalNull(this MemoryReader reader)
        {
            return reader.Readable<decimal>();
        }

        public static string ReadString(this MemoryReader reader)
        {
            var count = reader.ReadUShort();
            if (count == 0)
            {
                return null;
            }

            count = (ushort)(count - 1);
            if (count > ushort.MaxValue - 1)
            {
                throw new EndOfStreamException("读取字符串过长!");
            }

            var segment = reader.ReadArraySegment(count);
            return Service.Text.GetString(segment.Array, segment.Offset, segment.Count);
        }

        public static byte[] ReadBytes(this MemoryReader reader)
        {
            var count = reader.ReadUInt();
            if (count == 0)
            {
                return null;
            }

            var bytes = new byte[count];
            reader.ReadBytes(bytes, checked((int)(count - 1)));
            return bytes;
        }

        public static ArraySegment<byte> ReadArraySegment(this MemoryReader reader)
        {
            var count = reader.ReadUInt();
            return count == 0 ? default : reader.ReadArraySegment(checked((int)(count - 1)));
        }

        public static DateTime ReadDateTime(this MemoryReader reader)
        {
            return DateTime.FromOADate(reader.ReadDouble());
        }

        public static List<T> ReadList<T>(this MemoryReader reader)
        {
            var length = reader.ReadInt();
            if (length < 0)
            {
                return null;
            }

            var result = new List<T>(length);
            for (var i = 0; i < length; i++)
            {
                result.Add(reader.Invoke<T>());
            }

            return result;
        }

        public static T[] ReadArray<T>(this MemoryReader reader)
        {
            var length = reader.ReadInt();
            if (length < 0)
            {
                return null;
            }

            var result = new T[length];
            for (var i = 0; i < length; i++)
            {
                result[i] = reader.Invoke<T>();
            }

            return result;
        }

        public static Uri ReadUri(this MemoryReader reader)
        {
            var uri = reader.ReadString();
            return string.IsNullOrWhiteSpace(uri) ? null : new Uri(uri);
        }
    }
}