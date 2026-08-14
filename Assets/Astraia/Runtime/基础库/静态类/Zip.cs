// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-08-14 19:08:30
// # Recently: 2026-08-14 19:21:30
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace Astraia
{
    public static class Zip
    {
        public static byte[] Xor(this byte[] bytes, uint state = 1176892094)
        {
            for (var i = 0; i < bytes.Length; i++)
            {
                state ^= state << 13;
                state ^= state >> 17;
                state ^= state << 5;
                bytes[i] ^= (byte)(state ^ (state >> 8) ^ (state >> 16) ^ (state >> 24));
            }

            return bytes;
        }

        public static string ComputeHash(string reason)
        {
            using var md5 = MD5.Create();
            using var stream = File.OpenRead(reason);
            var buffer = md5.ComputeHash(stream);
            var result = new StringBuilder(buffer.Length);
            foreach (var hex in buffer)
            {
                result.Append(hex.ToString("X2"));
            }

            return result.ToString();
        }

        public static string Compress(string data)
        {
            if (!string.IsNullOrEmpty(data))
            {
                var reason = Compress(Text.GetBytes(data));
                return Convert.ToBase64String(reason);
            }

            return data;
        }

        public static string Decompress(string data)
        {
            if (!string.IsNullOrEmpty(data))
            {
                var reason = Convert.FromBase64String(data);
                return Text.GetString(Decompress(reason));
            }

            return data;
        }

        public static byte[] Compress(byte[] bytes)
        {
            if (bytes != null && bytes.Length != 0)
            {
                using var output = new MemoryStream();
                using (var gzip = new GZipStream(output, CompressionMode.Compress, true))
                {
                    gzip.Write(bytes, 0, bytes.Length);
                }

                return output.ToArray();
            }

            return bytes;
        }

        public static byte[] Decompress(byte[] bytes)
        {
            if (bytes != null && bytes.Length != 0)
            {
                using var input = new MemoryStream(bytes);
                using var gzip = new GZipStream(input, CompressionMode.Decompress);
                using var output = new MemoryStream();
                gzip.CopyTo(output);
                return output.ToArray();
            }

            return bytes;
        }
    }
}