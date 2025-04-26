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
        public static byte GetByte(this MemoryGetter getter)
        {
            return getter.Get<byte>();
        }

        public static byte? GetByteNull(this MemoryGetter getter)
        {
            return getter.Getable<byte>();
        }

        public static sbyte GetSByte(this MemoryGetter getter)
        {
            return getter.Get<sbyte>();
        }

        public static sbyte? GetSByteNull(this MemoryGetter getter)
        {
            return getter.Getable<sbyte>();
        }

        public static char GetChar(this MemoryGetter getter)
        {
            return (char)getter.Get<ushort>();
        }

        public static char? GetCharNull(this MemoryGetter getter)
        {
            return (char?)getter.Getable<ushort>();
        }

        public static bool GetBool(this MemoryGetter getter)
        {
            return getter.Get<byte>() != 0;
        }

        public static bool? GetBoolNull(this MemoryGetter getter)
        {
            var value = getter.Getable<byte>();
            return value.HasValue ? value.Value != 0 : default(bool?);
        }

        public static short GetShort(this MemoryGetter getter)
        {
            return getter.Get<short>();
        }

        public static short? GetShortNull(this MemoryGetter getter)
        {
            return getter.Getable<short>();
        }

        public static ushort GetUShort(this MemoryGetter getter)
        {
            return getter.Get<ushort>();
        }

        public static ushort? GetUShortNull(this MemoryGetter getter)
        {
            return getter.Getable<ushort>();
        }

        public static int GetInt(this MemoryGetter getter)
        {
            return getter.Get<int>();
        }

        public static int? GetIntNull(this MemoryGetter getter)
        {
            return getter.Getable<int>();
        }

        public static uint GetUInt(this MemoryGetter getter)
        {
            return getter.Get<uint>();
        }

        public static uint? GetUIntNull(this MemoryGetter getter)
        {
            return getter.Getable<uint>();
        }

        public static long GetLong(this MemoryGetter getter)
        {
            return getter.Get<long>();
        }

        public static long? GetLongNull(this MemoryGetter getter)
        {
            return getter.Getable<long>();
        }

        public static ulong GetULong(this MemoryGetter getter)
        {
            return getter.Get<ulong>();
        }

        public static ulong? GetULongNull(this MemoryGetter getter)
        {
            return getter.Getable<ulong>();
        }

        public static float GetFloat(this MemoryGetter getter)
        {
            return getter.Get<float>();
        }

        public static float? GetFloatNull(this MemoryGetter getter)
        {
            return getter.Getable<float>();
        }

        public static double GetDouble(this MemoryGetter getter)
        {
            return getter.Get<double>();
        }

        public static double? GetDoubleNull(this MemoryGetter getter)
        {
            return getter.Getable<double>();
        }

        public static decimal GetDecimal(this MemoryGetter getter)
        {
            return getter.Get<decimal>();
        }

        public static decimal? GetDecimalNull(this MemoryGetter getter)
        {
            return getter.Getable<decimal>();
        }

        public static string GetString(this MemoryGetter getter)
        {
            var count = getter.GetUShort();
            if (count == 0)
            {
                return null;
            }

            count = (ushort)(count - 1);
            if (count > ushort.MaxValue - 1)
            {
                throw new EndOfStreamException("读取字符串过长!");
            }

            var segment = getter.GetArraySegment(count);
            return Service.Text.GetString(segment.Array, segment.Offset, segment.Count);
        }

        public static byte[] GetBytes(this MemoryGetter getter)
        {
            var count = getter.GetUInt();
            if (count == 0)
            {
                return null;
            }

            var bytes = new byte[count];
            getter.GetBytes(bytes, checked((int)(count - 1)));
            return bytes;
        }

        public static ArraySegment<byte> GetArraySegment(this MemoryGetter getter)
        {
            var count = getter.GetUInt();
            return count == 0 ? default : getter.GetArraySegment(checked((int)(count - 1)));
        }

        public static DateTime GetDateTime(this MemoryGetter getter)
        {
            return DateTime.FromOADate(getter.GetDouble());
        }

        public static List<T> GetList<T>(this MemoryGetter getter)
        {
            var length = getter.GetInt();
            if (length < 0)
            {
                return null;
            }

            var result = new List<T>(length);
            for (var i = 0; i < length; i++)
            {
                result.Add(getter.Invoke<T>());
            }

            return result;
        }

        public static T[] GetArray<T>(this MemoryGetter getter)
        {
            var length = getter.GetInt();
            if (length < 0)
            {
                return null;
            }

            var result = new T[length];
            for (var i = 0; i < length; i++)
            {
                result[i] = getter.Invoke<T>();
            }

            return result;
        }

        public static Uri GetUri(this MemoryGetter getter)
        {
            var uri = getter.GetString();
            return string.IsNullOrWhiteSpace(uri) ? null : new Uri(uri);
        }
    }
}