// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-04-09 21:04:10
// // # Recently: 2025-04-09 21:04:10
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using System.Collections.Generic;

namespace Astraia
{
    public static partial class Service
    {
        public static class Xor
        {
            private static readonly Dictionary<byte, byte[]> KeyMap = new Dictionary<byte, byte[]>();
            private const int LENGTH = 16;

            static Xor()
            {
                Register(0, "A1B2C3D4E5F6G7H8");
            }

            public static void Register(byte version, string data)
            {
                var item = Text.GetBytes(data);
                if (item.Length != LENGTH)
                {
                    Array.Resize(ref item, LENGTH);
                }

                KeyMap[version] = item;
            }

            public static unsafe byte[] Encrypt(byte[] data, byte version = 0)
            {
                var iv = new byte[LENGTH];
                Random.NextBytes(iv);
                iv[0] = version;

                var key = KeyMap[iv[0]];
                var buffer = new byte[LENGTH + data.Length];
                Buffer.BlockCopy(iv, 0, buffer, 0, LENGTH);

                fixed (byte* pData = data, pBuffer = buffer, pKey = key, pIv = iv)
                {
                    var pOutput = pBuffer + LENGTH;
                    for (var i = 0; i < data.Length; i++)
                    {
                        pOutput[i] = (byte)(pData[i] ^ pKey[i % key.Length] ^ pIv[i % LENGTH]);
                    }
                }

                return buffer;
            }

            public static unsafe byte[] Decrypt(byte[] data)
            {
                var iv = new byte[LENGTH];

                Buffer.BlockCopy(data, 0, iv, 0, LENGTH);
                var buffer = new byte[data.Length - LENGTH];
                var key = KeyMap[iv[0]];

                fixed (byte* pData = data, pBuffer = buffer, pKey = key, pIv = iv)
                {
                    var pInput = pData + LENGTH;
                    for (var i = 0; i < buffer.Length; i++)
                    {
                        pBuffer[i] = (byte)(pInput[i] ^ pKey[i % key.Length] ^ pIv[i % LENGTH]);
                    }
                }

                return buffer;
            }
        }
    }
}