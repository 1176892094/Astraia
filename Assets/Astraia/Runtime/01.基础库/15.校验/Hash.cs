// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-08-05 19:08:29
// // # Recently: 2025-08-05 19:08:08
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System.IO;

namespace Astraia
{
    public static partial class Service
    {
        public static class Hash
        {
            private static readonly uint[] Table;

            static Hash()
            {
                Table = new uint[256];
                const uint POLYNOMIAL = 0xEDB88320;
                for (uint i = 0; i < Table.Length; ++i)
                {
                    var result = i;
                    for (var j = 0; j < 8; ++j)
                    {
                        result = (result >> 1) ^ ((result & 1) == 1 ? POLYNOMIAL : 0);
                    }

                    Table[i] = result;
                }
            }

            public static uint Compute(byte[] bytes)
            {
                var result = 0xFFFFFFFF;
                foreach (var b in bytes)
                {
                    var index = (byte)((result & 0xFF) ^ b);
                    result = (result >> 8) ^ Table[index];
                }

                return ~result;
            }

            public static uint Compute(string filePath)
            {
                using var stream = File.OpenRead(filePath);
                var buffer = new byte[8192];
                int bytesRead;
                var result = 0xFFFFFFFF;
                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    for (var i = 0; i < bytesRead; i++)
                    {
                        var index = (byte)((result & 0xFF) ^ buffer[i]);
                        result = (result >> 8) ^ Table[index];
                    }
                }

                return ~result;
            }
        }
    }
}