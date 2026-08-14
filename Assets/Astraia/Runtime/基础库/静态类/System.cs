// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2025-04-09 21:04:23
// # Recently: 2025-04-09 21:04:23
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace Astraia
{
    public static class Utils
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

    public static class Seed
    {
        private static readonly Random random = new(Environment.TickCount);

        public static float value => (float)random.NextDouble();

        public static int Next()
        {
            return random.Next();
        }

        public static int Next(int max)
        {
            return random.Next(max);
        }

        public static int Next(int min, int max)
        {
            return random.Next(min, max);
        }

        public static float Next(float max)
        {
            return Next(0, max);
        }

        public static float Next(float min, float max)
        {
            return value * (max - min) + min;
        }

        public static void NextBytes(byte[] bytes)
        {
            random.NextBytes(bytes);
        }

        public static float NextDouble(float min, float max)
        {
            return random.NextDouble() < 0.5F ? Next(min, max) : -Next(min, max);
        }

        private static class Enum<T> where T : unmanaged, Enum
        {
            public static readonly Dictionary<T, int> Indices;
            public static readonly T[] Values;

            static Enum()
            {
                Values = (T[])Enum.GetValues(typeof(T));
                Indices = new Dictionary<T, int>(Values.Length);
                for (var i = 0; i < Values.Length; i++)
                {
                    Indices[Values[i]] = i;
                }
            }
        }

        public static T Next<T>(T minValue, T maxValue) where T : unmanaged, Enum
        {
            return Next(minValue.ToInt(), maxValue.ToInt() + 1).ToEnum<T>();
        }

        public static T Next<T>(T maxValue) where T : unmanaged, Enum
        {
            return Next(maxValue.ToInt() + 1).ToEnum<T>();
        }

        public static T Next<T>() where T : unmanaged, Enum
        {
            return Next(Enum<T>.Values.Length).ToEnum<T>();
        }

        public static T[] Array<T>() where T : unmanaged, Enum
        {
            return Enum<T>.Values;
        }

        public static int Count<T>() where T : unmanaged, Enum
        {
            return Enum<T>.Values.Length;
        }

        public static int Index<T>(this T value) where T : unmanaged, Enum
        {
            return Enum<T>.Indices[value];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe T ToEnum<T>(this int value) where T : unmanaged, Enum
        {
            if (sizeof(T) == sizeof(int))
            {
                return *(T*)&value;
            }

            throw new InvalidOperationException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe int ToInt<T>(this T value) where T : unmanaged, Enum
        {
            if (sizeof(T) == sizeof(int))
            {
                return *(int*)&value;
            }

            throw new InvalidOperationException();
        }
    }

    internal static class Text
    {
        [ThreadStatic] private static UTF8Encoding encoding;

        private static UTF8Encoding Encoding => encoding ??= new UTF8Encoding(false, true);

        public static byte[] GetBytes(string message)
        {
            return Encoding.GetBytes(message);
        }

        public static int GetBytes(string message, int count, byte[] buffer, int index)
        {
            return Encoding.GetBytes(message, 0, count, buffer, index);
        }

        public static string GetString(byte[] bytes)
        {
            return Encoding.GetString(bytes);
        }

        public static string GetString(byte[] bytes, int index, int count)
        {
            return Encoding.GetString(bytes, index, count);
        }

        public static int GetMaxByteCount(int count)
        {
            return Encoding.GetMaxByteCount(count);
        }
    }

    public static class Search
    {
        private static readonly Dictionary<string, Assembly> assemblies = new();
        private static readonly Dictionary<string, Type> cacheTypes = new();

        public const BindingFlags Static = (BindingFlags)56;
        public const BindingFlags Instance = (BindingFlags)52;

        public static event Action<Type> OnLoad;

        public static void LoadData(params string[] args)
        {
            var assemblyData = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblyData)
            {
                var name = assembly.GetName().Name;
                assemblies[name] = assembly;
                if (args.Contains(name) || name.StartsWith("Assembly-CSharp"))
                {
                    foreach (var result in assembly.GetTypes())
                    {
                        cacheTypes["{0},{1}".Format(result.FullName, name)] = result;
                        OnLoad?.Invoke(result);
                    }
                }
            }
        }

        public static Assembly GetAssembly(string name)
        {
            if (assemblies.TryGetValue(name, out var result))
            {
                return result;
            }

            var assemblyData = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblyData)
            {
                if (assembly.GetName().Name == name)
                {
                    result = assembly;
                    break;
                }
            }

            if (result != null)
            {
                assemblies[name] = result;
            }

            return result;
        }

        public static Type GetType(string name)
        {
            if (cacheTypes.TryGetValue(name, out var result))
            {
                return result;
            }

            var index = name.LastIndexOf(',');
            if (index < 0)
            {
                var assemblyData = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in assemblyData)
                {
                    result = assembly.GetType(name);
                    if (result != null)
                    {
                        cacheTypes[name] = result;
                        assemblies[assembly.GetName().Name] = assembly;
                        break;
                    }
                }
            }
            else
            {
                var assembly = GetAssembly(name.Substring(index + 1).Trim());
                if (assembly != null)
                {
                    result = assembly.GetType(name.Substring(0, index));
                    if (result != null)
                    {
                        cacheTypes[name] = result;
                    }
                }
            }

            return result;
        }
    }

    public static class Host
    {
        public static readonly HttpClient Http = new();

        public static string Ip()
        {
            try
            {
                var interfaces = NetworkInterface.GetAllNetworkInterfaces();
                foreach (var @interface in interfaces)
                {
                    if (@interface.OperationalStatus == OperationalStatus.Up && @interface.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    {
                        var properties = @interface.GetIPProperties();
                        foreach (var ip in properties.UnicastAddresses)
                        {
                            if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                            {
                                return ip.Address.ToString();
                            }
                        }
                    }
                }

                var addresses = Dns.GetHostEntry(Dns.GetHostName()).AddressList; // 虚拟机无法解析网络接口 因此额外解析主机地址
                foreach (var ip in addresses)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return ip.ToString();
                    }
                }

                return IPAddress.Loopback.ToString();
            }
            catch
            {
                return IPAddress.Loopback.ToString();
            }
        }

        public static void Start(string address, Func<HttpListenerRequest, HttpListenerResponse, Task> request)
        {
            var reason = new HttpListener();
            reason.Prefixes.Add(address);
            reason.Start();
            Task.Run(HttpThread);
            return;

            async Task HttpThread()
            {
                while (true)
                {
                    try
                    {
                        var context = await reason.GetContextAsync(); // 异步等待请求
                        _ = Task.Run(HttpRequest); // 每个请求单独处理

                        async Task HttpRequest()
                        {
                            try
                            {
                                await request.Invoke(context.Request, context.Response);
                            }
                            catch (Exception e)
                            {
                                Log.Warn(e.ToString());
                                context.Response.StatusCode = 500;
                            }
                            finally
                            {
                                try
                                {
                                    context.Response.Close();
                                }
                                catch
                                {
                                    // ignored
                                }
                            }
                        }
                    }
                    catch (ArgumentException)
                    {
                        return;
                    }
                    catch (Exception e)
                    {
                        Log.Warn(e.ToString());
                    }
                }
            }
        }
    }

    internal static class Bad
    {
        private class Node
        {
            public readonly Dictionary<char, Node> nodes = new();
            public bool finish;
        }

        private static readonly Node root = new();

        public static void SetUp(string text)
        {
            var splits = Utils.Decompress(text).Split('\n');
            foreach (var chars in splits)
            {
                var current = root;
                foreach (var c in chars)
                {
                    if (!current.nodes.TryGetValue(c, out var node))
                    {
                        node = new Node();
                        current.nodes[c] = node;
                    }

                    current = node;
                }

                current.finish = true;
            }
        }

        public static string Invoke(string text, char mask)
        {
            var chars = text.ToCharArray();
            for (var i = 0; i < chars.Length; i++)
            {
                var current = root;
                var j = i;
                while (j < chars.Length && current.nodes.TryGetValue(chars[j], out var next))
                {
                    if (next.finish)
                    {
                        for (var k = i; k <= j; k++)
                        {
                            chars[k] = mask;
                        }

                        break;
                    }

                    current = next;
                    j++;
                }
            }

            return new string(chars);
        }
    }

    internal static class String
    {
        [ThreadStatic] private static StringBuilder stringBuilder;

        private static StringBuilder StringBuilder => stringBuilder ??= new StringBuilder(1024);

        internal static string Format<T>(string format, T arg1)
        {
            StringBuilder.Length = 0;
            StringBuilder.AppendFormat(format, arg1);
            return StringBuilder.ToString();
        }

        internal static string Format<T1, T2>(string format, T1 arg1, T2 arg2)
        {
            StringBuilder.Length = 0;
            StringBuilder.AppendFormat(format, arg1, arg2);
            return StringBuilder.ToString();
        }

        internal static string Format<T1, T2, T3>(string format, T1 arg1, T2 arg2, T3 arg3)
        {
            StringBuilder.Length = 0;
            StringBuilder.AppendFormat(format, arg1, arg2, arg3);
            return StringBuilder.ToString();
        }

        internal static string Format<T1, T2, T3, T4>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            StringBuilder.Length = 0;
            StringBuilder.AppendFormat(format, arg1, arg2, arg3, arg4);
            return StringBuilder.ToString();
        }
    }

    public static class StringExtensions
    {
        public static string Format<T>(this string format, T arg1)
        {
            return String.Format(format, arg1);
        }

        public static string Format<T1, T2>(this string format, T1 arg1, T2 arg2)
        {
            return String.Format(format, arg1, arg2);
        }

        public static string Format<T1, T2, T3>(this string format, T1 arg1, T2 arg2, T3 arg3)
        {
            return String.Format(format, arg1, arg2, arg3);
        }

        public static string Format<T1, T2, T3, T4>(this string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            return String.Format(format, arg1, arg2, arg3, arg4);
        }

        public static bool IsNullOrEmpty(this string result)
        {
            return string.IsNullOrEmpty(result);
        }

        public static string Mask(this string result, char mask = '*')
        {
            return Bad.Invoke(result, mask);
        }

        public static string Limit(this string result, int count)
        {
            var value = string.Empty;
            var input = 0;

            foreach (var c in result)
            {
                var width = c > 255 ? 2 : 1;
                if (input + width > count)
                {
                    break;
                }

                input += width;
                value += c;
            }

            return value;
        }

        public static string Align(this string str, int count, string mask = "")
        {
            var width = 0;
            var i1 = str.Length;

            for (var i = 0; i < i1; i++)
            {
                width += str[i] > 255 ? 2 : 1;
            }

            if (width <= count)
            {
                return str + new string(' ', count - width);
            }

            var cur = 0;
            var i2 = 0;
            while (i2 < i1)
            {
                var w = str[i2] > 255 ? 2 : 1;

                if (cur + w + mask.Length > count)
                {
                    break;
                }

                cur += w;
                i2++;
            }

            return str.Substring(0, i2) + mask;
        }
    }
}