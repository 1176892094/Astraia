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

                KeyMap.TryGetValue(iv[0], out var key);
                var result = new byte[LENGTH + data.Length];
                Buffer.BlockCopy(iv, 0, result, 0, LENGTH);

                fixed (byte* pData = data, pResult = result, pIv = iv)
                {
                    if (key != null)
                    {
                        fixed (byte* pKey = key)
                        {
                            var pOutput = pResult + LENGTH;
                            for (var i = 0; i < data.Length; i++)
                            {
                                pOutput[i] = (byte)(pData[i] ^ pKey[i % key.Length] ^ pIv[i % LENGTH]);
                            }
                        }
                    }
                    else
                    {
                        var pOutput = pResult + LENGTH;
                        for (var i = 0; i < data.Length; i++)
                        {
                            pOutput[i] = (byte)(pData[i] ^ pIv[i % LENGTH]);
                        }
                    }
                }

                return result;
            }

            public static unsafe byte[] Decrypt(byte[] data)
            {
                var iv = new byte[LENGTH];

                Buffer.BlockCopy(data, 0, iv, 0, LENGTH);
                var result = new byte[data.Length - LENGTH];
                KeyMap.TryGetValue(iv[0], out var key);

                fixed (byte* pData = data, pResult = result, pIv = iv)
                {
                    if (key != null)
                    {
                        fixed (byte* pKey = key)
                        {
                            var pInput = pData + LENGTH;
                            for (var i = 0; i < result.Length; i++)
                            {
                                pResult[i] = (byte)(pInput[i] ^ pKey[i % key.Length] ^ pIv[i % LENGTH]);
                            }
                        }
                    }
                    else
                    {
                        var pInput = pData + LENGTH;
                        for (var i = 0; i < result.Length; i++)
                        {
                            pResult[i] = (byte)(pInput[i] ^ pIv[i % LENGTH]);
                        }
                    }
                }

                return result;
            }
        }
    }
}