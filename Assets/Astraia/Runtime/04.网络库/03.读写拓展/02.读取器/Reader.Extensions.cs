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

        public static byte? ReadByteNullable(this MemoryReader reader)
        {
            return reader.ReadNullable<byte>();
        }

        public static sbyte ReadSByte(this MemoryReader reader)
        {
            return reader.Read<sbyte>();
        }

        public static sbyte? ReadSByteNullable(this MemoryReader reader)
        {
            return reader.ReadNullable<sbyte>();
        }

        public static char ReadChar(this MemoryReader reader)
        {
            return (char)reader.Read<ushort>();
        }

        public static char? ReadCharNullable(this MemoryReader reader)
        {
            return (char?)reader.ReadNullable<ushort>();
        }

        public static bool ReadBool(this MemoryReader reader)
        {
            return reader.Read<byte>() != 0;
        }

        public static bool? ReadBoolNullable(this MemoryReader reader)
        {
            var value = reader.ReadNullable<byte>();
            return value.HasValue ? value.Value != 0 : default(bool?);
        }

        public static short ReadShort(this MemoryReader reader)
        {
            return reader.Read<short>();
        }

        public static short? ReadShortNullable(this MemoryReader reader)
        {
            return reader.ReadNullable<short>();
        }

        public static ushort ReadUShort(this MemoryReader reader)
        {
            return reader.Read<ushort>();
        }

        public static ushort? ReadUShortNullable(this MemoryReader reader)
        {
            return reader.ReadNullable<ushort>();
        }

        public static int ReadInt(this MemoryReader reader)
        {
            return reader.Read<int>();
        }

        public static int? ReadIntNullable(this MemoryReader reader)
        {
            return reader.ReadNullable<int>();
        }

        public static uint ReadUInt(this MemoryReader reader)
        {
            return reader.Read<uint>();
        }

        public static uint? ReadUIntNullable(this MemoryReader reader)
        {
            return reader.ReadNullable<uint>();
        }

        public static long ReadLong(this MemoryReader reader)
        {
            return reader.Read<long>();
        }

        public static long? ReadLongNullable(this MemoryReader reader)
        {
            return reader.ReadNullable<long>();
        }

        public static ulong ReadULong(this MemoryReader reader)
        {
            return reader.Read<ulong>();
        }

        public static ulong? ReadULongNullable(this MemoryReader reader)
        {
            return reader.ReadNullable<ulong>();
        }

        public static float ReadFloat(this MemoryReader reader)
        {
            return reader.Read<float>();
        }

        public static float? ReadFloatNullable(this MemoryReader reader)
        {
            return reader.ReadNullable<float>();
        }

        public static double ReadDouble(this MemoryReader reader)
        {
            return reader.Read<double>();
        }

        public static double? ReadDoubleNullable(this MemoryReader reader)
        {
            return reader.ReadNullable<double>();
        }

        public static decimal ReadDecimal(this MemoryReader reader)
        {
            return reader.Read<decimal>();
        }

        public static decimal? ReadDecimalNullable(this MemoryReader reader)
        {
            return reader.ReadNullable<decimal>();
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
            var count = Service.Length.Decode(reader);
            if (count == 0)
            {
                return null;
            }

            var bytes = new byte[count - 1];
            reader.ReadBytes(bytes, checked((int)(count - 1)));
            return bytes;
        }

        public static ArraySegment<byte> ReadArraySegment(this MemoryReader reader)
        {
            var count = Service.Length.Decode(reader);
            return count == 0 ? default : reader.ReadArraySegment(checked((int)(count - 1)));
        }

        public static DateTime ReadDateTime(this MemoryReader reader)
        {
            return DateTime.FromOADate(reader.ReadDouble());
        }

        public static List<T> ReadList<T>(this MemoryReader reader)
        {
            var length = (uint)Service.Length.Decode(reader);
            if (length == 0) return null;

            length--;
            var result = new List<T>(checked((int)length));
            for (var i = 0; i < length; i++)
            {
                result.Add(reader.Invoke<T>());
            }

            return result;
        }

        public static HashSet<T> ReadHashSet<T>(this MemoryReader reader)
        {
            var length = (uint)Service.Length.Decode(reader);
            if (length == 0) return null;

            length--;
            var result = new HashSet<T>(checked((int)length));
            for (var i = 0; i < length; i++)
            {
                result.Add(reader.Invoke<T>());
            }

            return result;
        }

        public static T[] ReadArray<T>(this MemoryReader reader)
        {
            var length = (uint)Service.Length.Decode(reader);
            if (length == 0) return null;

            length--;
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

        public static Type ReadType(this MemoryReader reader)
        {
            var type = reader.ReadString();
            return string.IsNullOrWhiteSpace(type) ? null : Service.Find.Type(type);
        }
    }
}