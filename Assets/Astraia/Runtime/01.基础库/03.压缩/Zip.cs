// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-04-09 21:04:47
// // # Recently: 2025-04-09 21:04:47
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using System.IO;
using System.IO.Compression;

namespace Astraia
{
    public static partial class Service
    {
        public static class Zip
        {
            public static string Compress(string data)
            {
                var bytes = Text.GetBytes(data);
                using var buffer = new MemoryStream();
                using (var stream = new GZipStream(buffer, CompressionMode.Compress, true))
                {
                    stream.Write(bytes, 0, bytes.Length);
                }

                return Convert.ToBase64String(buffer.ToArray());
            }

            public static string Decompress(string data)
            {
                var bytes = Convert.FromBase64String(data);
                using var buffer = new MemoryStream(bytes);
                using var stream = new GZipStream(buffer, CompressionMode.Decompress);
                using var reader = new StreamReader(stream, Text.Encoding);
                return reader.ReadToEnd();
            }
        }
    }
}