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
        public static byte GetByte(this MemoryReader reader)
        {
            return reader.Get<byte>();
        }

        public static byte? GetByteNull(this MemoryReader reader)
        {
            return reader.Getable<byte>();
        }

        public static sbyte GetSByte(this MemoryReader reader)
        {
            return reader.Get<sbyte>();
        }

        public static sbyte? GetSByteNull(this MemoryReader reader)
        {
            return reader.Getable<sbyte>();
        }

        public static char GetChar(this MemoryReader reader)
        {
            return (char)reader.Get<ushort>();
        }

        public static char? GetCharNull(this MemoryReader reader)
        {
            return (char?)reader.Getable<ushort>();
        }

        public static bool GetBool(this MemoryReader reader)
        {
            return reader.Get<byte>() != 0;
        }

        public static bool? GetBoolNull(this MemoryReader reader)
        {
            var value = reader.Getable<byte>();
            return value.HasValue ? value.Value != 0 : default(bool?);
        }

        public static short GetShort(this MemoryReader reader)
        {
            return reader.Get<short>();
        }

        public static short? GetShortNull(this MemoryReader reader)
        {
            return reader.Getable<short>();
        }

        public static ushort GetUShort(this MemoryReader reader)
        {
            return reader.Get<ushort>();
        }

        public static ushort? GetUShortNull(this MemoryReader reader)
        {
            return reader.Getable<ushort>();
        }

        public static int GetInt(this MemoryReader reader)
        {
            return reader.Get<int>();
        }

        public static int? GetIntNull(this MemoryReader reader)
        {
            return reader.Getable<int>();
        }

        public static uint GetUInt(this MemoryReader reader)
        {
            return reader.Get<uint>();
        }

        public static uint? GetUIntNull(this MemoryReader reader)
        {
            return reader.Getable<uint>();
        }

        public static long GetLong(this MemoryReader reader)
        {
            return reader.Get<long>();
        }

        public static long? GetLongNull(this MemoryReader reader)
        {
            return reader.Getable<long>();
        }

        public static ulong GetULong(this MemoryReader reader)
        {
            return reader.Get<ulong>();
        }

        public static ulong? GetULongNull(this MemoryReader reader)
        {
            return reader.Getable<ulong>();
        }

        public static float GetFloat(this MemoryReader reader)
        {
            return reader.Get<float>();
        }

        public static float? GetFloatNull(this MemoryReader reader)
        {
            return reader.Getable<float>();
        }

        public static double GetDouble(this MemoryReader reader)
        {
            return reader.Get<double>();
        }

        public static double? GetDoubleNull(this MemoryReader reader)
        {
            return reader.Getable<double>();
        }

        public static decimal GetDecimal(this MemoryReader reader)
        {
            return reader.Get<decimal>();
        }

        public static decimal? GetDecimalNull(this MemoryReader reader)
        {
            return reader.Getable<decimal>();
        }

        public static string GetString(this MemoryReader reader)
        {
            var count = reader.GetUShort();
            if (count == 0)
            {
                return null;
            }

            count = (ushort)(count - 1);
            if (count > ushort.MaxValue - 1)
            {
                throw new EndOfStreamException("读取字符串过长!");
            }

            var segment = reader.GetArraySegment(count);
            return Service.Text.GetString(segment.Array, segment.Offset, segment.Count);
        }

        public static byte[] GetBytes(this MemoryReader reader)
        {
            var count = reader.GetUInt();
            if (count == 0)
            {
                return null;
            }

            var bytes = new byte[count];
            reader.GetBytes(bytes, checked((int)(count - 1)));
            return bytes;
        }

        public static ArraySegment<byte> GetArraySegment(this MemoryReader reader)
        {
            var count = reader.GetUInt();
            return count == 0 ? default : reader.GetArraySegment(checked((int)(count - 1)));
        }

        public static DateTime GetDateTime(this MemoryReader reader)
        {
            return DateTime.FromOADate(reader.GetDouble());
        }

        public static List<T> GetList<T>(this MemoryReader reader)
        {
            var length = reader.GetInt();
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

        public static T[] GetArray<T>(this MemoryReader reader)
        {
            var length = reader.GetInt();
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

        public static Uri GetUri(this MemoryReader reader)
        {
            var uri = reader.GetString();
            return string.IsNullOrWhiteSpace(uri) ? null : new Uri(uri);
        }
    }
}