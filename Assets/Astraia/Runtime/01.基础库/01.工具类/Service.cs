// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-04-09 21:04:23
// // # Recently: 2025-04-09 21:04:23
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Reflection.Emit;

namespace Astraia
{
    public static class Service
    {
        public static class Log
        {
            private static Action<string> onInfo = Console.WriteLine;
            private static Action<string> onWarn = Console.WriteLine;
            private static Action<string> onError = Console.Error.WriteLine;

            public static void Setup(Action<string> onInfo, Action<string> onWarn, Action<string> onError)
            {
                Log.onInfo = onInfo;
                Log.onWarn = onWarn;
                Log.onError = onError;
            }

            public static void Info(object message)
            {
                onInfo(message.ToString());
            }

            public static void Warn(object message)
            {
                onWarn(message.ToString());
            }

            public static void Error(object message)
            {
                onError(message.ToString());
            }

            public static void Info<T>(string format, T arg1)
            {
                onInfo(format.Format(arg1));
            }

            public static void Info<T1, T2>(string format, T1 arg1, T2 arg2)
            {
                onInfo(format.Format(arg1, arg2));
            }

            public static void Info<T1, T2, T3>(string format, T1 arg1, T2 arg2, T3 arg3)
            {
                onInfo(format.Format(arg1, arg2, arg3));
            }

            public static void Info<T1, T2, T3, T4>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
            {
                onInfo(format.Format(arg1, arg2, arg3, arg4));
            }

            public static void Warn<T>(string format, T arg1)
            {
                onWarn(format.Format(arg1));
            }

            public static void Warn<T1, T2>(string format, T1 arg1, T2 arg2)
            {
                onWarn(format.Format(arg1, arg2));
            }

            public static void Warn<T1, T2, T3>(string format, T1 arg1, T2 arg2, T3 arg3)
            {
                onWarn(format.Format(arg1, arg2, arg3));
            }

            public static void Warn<T1, T2, T3, T4>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
            {
                onWarn(format.Format(arg1, arg2, arg3, arg4));
            }

            public static void Error<T>(string format, T arg1)
            {
                onError(format.Format(arg1));
            }

            public static void Error<T1, T2>(string format, T1 arg1, T2 arg2)
            {
                onError(format.Format(arg1, arg2));
            }

            public static void Error<T1, T2, T3>(string format, T1 arg1, T2 arg2, T3 arg3)
            {
                onError(format.Format(arg1, arg2, arg3));
            }

            public static void Error<T1, T2, T3, T4>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
            {
                onError(format.Format(arg1, arg2, arg3, arg4));
            }
        }

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

                return Convert.ToBase64String(buffer.GetBuffer(), 0, (int)buffer.Length);
            }

            public static string Decompress(string data)
            {
                var bytes = Convert.FromBase64String(data);
                using var buffer = new MemoryStream(bytes);
                using var stream = new GZipStream(buffer, CompressionMode.Decompress);
                using var output = new MemoryStream();
                stream.CopyTo(output);
                return Text.GetString(output.GetBuffer(), 0, (int)output.Length);
            }
        }

        public static class Xor
        {
            private static readonly Dictionary<byte, byte[]> KeyMap = new Dictionary<byte, byte[]>();
            private const int LENGTH = 16;

            static Xor() => LoadData(0, "A1B2C3D4E5F6G7H8");

            public static void LoadData(byte version, string data)
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
                Seed.NextBytes(iv);
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

        public static class Seed
        {
            private static readonly Random random = new Random(Environment.TickCount);

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

            public static int Next(Enum max)
            {
                return random.Next(Convert.ToInt32(max));
            }

            public static int Next(Enum min, Enum max)
            {
                return random.Next(Convert.ToInt32(min), Convert.ToInt32(max));
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
        }

        public static class Text
        {
            [ThreadStatic] private static UTF8Encoding stringEncode;

            private static UTF8Encoding StringEncode => stringEncode ??= new UTF8Encoding(false, true);

            public static byte[] GetBytes(string message)
            {
                return StringEncode.GetBytes(message);
            }

            public static int GetBytes(string message, int count, byte[] buffer, int index)
            {
                return StringEncode.GetBytes(message, 0, count, buffer, index);
            }

            public static string GetString(byte[] bytes)
            {
                return StringEncode.GetString(bytes);
            }

            public static string GetString(byte[] bytes, int index, int count)
            {
                return StringEncode.GetString(bytes, index, count);
            }

            public static int GetMaxByteCount(int count)
            {
                return StringEncode.GetMaxByteCount(count);
            }
        }

        public static class Host
        {
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
        }

        public static class Http
        {
            public static void Start(int port, Func<HttpListenerRequest, HttpListenerResponse, Task> request)
            {
                var reason = new HttpListener();
                reason.Prefixes.Add("http://*:{0}/".Format(port));
                reason.Start();
                Task.Run(HttpThread);

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

        public static class Ref
        {
            private static readonly Dictionary<string, Assembly> assemblies = new Dictionary<string, Assembly>();
            private static readonly Dictionary<string, Type> cacheTypes = new Dictionary<string, Type>();

            public const BindingFlags Static = (BindingFlags)56;
            public const BindingFlags Instance = (BindingFlags)52;

            public static event Action<Type> OnLoad;
            public static event Action OnLoadComplete;

            public static void LoadData(HashSet<string> assemblyList)
            {
                var assemblyData = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in assemblyData)
                {
                    var name = assembly.GetName().Name;
                    assemblies[name] = assembly;
                    if (assemblyList.Contains(name) || name.StartsWith("Assembly-CSharp"))
                    {
                        foreach (var result in assembly.GetTypes())
                        {
                            cacheTypes["{0},{1}".Format(result.FullName, name)] = result;
                            OnLoad?.Invoke(result);
                        }
                    }
                }

                OnLoadComplete?.Invoke();
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

        public static class Ref<T> where T : Attribute
        {
            private static readonly Dictionary<MemberInfo, Dictionary<Type, T[]>> attributeCache = new();

            public static T GetAttribute(MemberInfo member, bool inherit = true)
            {
                var attrs = GetAttributes(member, inherit);
                return attrs.Length > 0 ? attrs[0] : null;
            }

            public static T[] GetAttributes(MemberInfo member, bool inherit = true)
            {
                if (!attributeCache.TryGetValue(member, out var mapper))
                {
                    mapper = new Dictionary<Type, T[]>();
                    attributeCache[member] = mapper;
                }

                if (!mapper.TryGetValue(typeof(T), out var result))
                {
                    var reason = member.GetCustomAttributes(typeof(T), inherit);
                    result = new T[reason.Length];
                    for (int i = 0; i < reason.Length; i++)
                    {
                        result[i] = (T)reason[i];
                    }

                    mapper.Add(typeof(T), result);
                }

                return result;
            }
        }

        internal static class Emit
        {
#if UNITY_EDITOR
            private static readonly Dictionary<Type, Dictionary<string, Func<object, object>>> getterCache = new();
            private static readonly Dictionary<Type, Dictionary<string, Action<object, object>>> setterCache = new();
            private static readonly Dictionary<Type, Dictionary<string, Func<object, object[], object>>> methodCache = new();

            public static object Invoke(object target, string name, params object[] args)
            {
                var targetType = target as Type ?? target.GetType();
                if (!methodCache.TryGetValue(targetType, out var methods))
                {
                    methods = new Dictionary<string, Func<object, object[], object>>();
                    methodCache[targetType] = methods;
                }

                if (!methods.TryGetValue(name, out var method))
                {
                    var parameters = args.Select(a => a.GetType()).ToArray();
                    for (var current = targetType; current != null; current = current.BaseType)
                    {
                        var result = current.GetMethod(name, (BindingFlags)62, null, parameters, null);
                        if (result != null)
                        {
                            method = LoadFunction(result);
                            methods[name] = method;
                            break;
                        }
                    }

                    var interfaces = targetType.GetInterfaces();
                    foreach (var interfaceType in interfaces)
                    {
                        var result = interfaceType.GetMethod(name, (BindingFlags)62, null, parameters, null);
                        if (result != null)
                        {
                            method = LoadFunction(result);
                            methods[name] = method;
                            break;
                        }
                    }
                }

                if (method == null)
                {
                    throw new MissingMethodException(targetType.FullName, name);
                }

                return method.Invoke(target is Type ? null : target, args);
            }

            public static object GetValue(object target, string name)
            {
                var targetType = target as Type ?? target.GetType();
                if (!getterCache.TryGetValue(targetType, out var getters))
                {
                    getters = new Dictionary<string, Func<object, object>>();
                    getterCache[targetType] = getters;
                }

                if (!getters.TryGetValue(name, out var getter))
                {
                    for (var current = targetType; current != null; current = current.BaseType)
                    {
                        var field = current.GetField(name, (BindingFlags)62);
                        if (field != null)
                        {
                            getter = LoadGetter(field);
                            getters[name] = getter;
                            break;
                        }

                        var property = current.GetProperty(name, (BindingFlags)62);
                        if (property != null && property.CanRead)
                        {
                            getter = LoadGetter(property, property.GetGetMethod(true));
                            getters[name] = getter;
                            break;
                        }
                    }
                }

                if (getter == null)
                {
                    throw new MissingMemberException(targetType.FullName, name);
                }

                return getter.Invoke(target is Type ? null : target);
            }

            public static void SetValue<T>(object target, string name, T value)
            {
                var targetType = target as Type ?? target.GetType();
                if (!setterCache.TryGetValue(targetType, out var setters))
                {
                    setters = new Dictionary<string, Action<object, object>>();
                    setterCache[targetType] = setters;
                }

                if (!setters.TryGetValue(name, out var setter))
                {
                    for (var current = targetType; current != null; current = current.BaseType)
                    {
                        var field = current.GetField(name, (BindingFlags)62);

                        if (field != null)
                        {
                            setter = LoadSetter<T>(field);
                            setters[name] = setter;
                            break;
                        }

                        var property = current.GetProperty(name, (BindingFlags)62);
                        if (property != null && property.CanRead)
                        {
                            setter = LoadSetter<T>(property, property.GetSetMethod(true));
                            setters[name] = setter;
                            break;
                        }
                    }
                }

                if (setter == null)
                {
                    throw new MissingMemberException(targetType.FullName, name);
                }

                setter.Invoke(target is Type ? null : target, value);
            }

            private static Func<object, object[], object> LoadFunction(MethodInfo method)
            {
                var name = "invoke_{0}_{1}".Format(method.DeclaringType!.Name, method.Name);
                var type = method.DeclaringType.IsInterface ? typeof(object) : method.DeclaringType;
                var DM = new DynamicMethod(name, typeof(object), new[] { typeof(object), typeof(object[]) }, type, true);
                var IL = DM.GetILGenerator();

                if (!method.IsStatic)
                {
                    IL.Emit(OpCodes.Ldarg_0);
                    IL.Emit(method.DeclaringType.IsValueType ? OpCodes.Unbox : OpCodes.Castclass, method.DeclaringType);
                }

                var parameters = method.GetParameters();
                for (var i = 0; i < parameters.Length; i++)
                {
                    IL.Emit(OpCodes.Ldarg_1);
                    IL.Emit(OpCodes.Ldc_I4, i);
                    IL.Emit(OpCodes.Ldelem_Ref);

                    var parameter = parameters[i].ParameterType;
                    IL.Emit(parameter.IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass, parameter);
                }

                IL.EmitCall(method.IsVirtual ? OpCodes.Callvirt : OpCodes.Call, method, null);
                if (method.ReturnType == typeof(void))
                {
                    IL.Emit(OpCodes.Ldnull);
                }
                else if (method.ReturnType.IsValueType)
                {
                    IL.Emit(OpCodes.Box, method.ReturnType);
                }

                IL.Emit(OpCodes.Ret);
                return (Func<object, object[], object>)DM.CreateDelegate(typeof(Func<object, object[], object>));
            }

            private static Func<object, object> LoadGetter(FieldInfo field)
            {
                var name = "get_{0}_{1}".Format(field.DeclaringType!.Name, field.Name);
                var DM = new DynamicMethod(name, typeof(object), new[] { typeof(object) }, field.DeclaringType, true);
                var IL = DM.GetILGenerator();

                if (!field.IsStatic)
                {
                    IL.Emit(OpCodes.Ldarg_0);
                    IL.Emit(field.DeclaringType.IsValueType ? OpCodes.Unbox : OpCodes.Castclass, field.DeclaringType);
                }

                IL.Emit(!field.IsStatic ? OpCodes.Ldfld : OpCodes.Ldsfld, field);
                if (field.FieldType.IsValueType)
                {
                    IL.Emit(OpCodes.Box, field.FieldType);
                }

                IL.Emit(OpCodes.Ret);
                return (Func<object, object>)DM.CreateDelegate(typeof(Func<object, object>));
            }

            private static Action<object, object> LoadSetter<T>(FieldInfo field)
            {
                var name = "set_{0}_{1}".Format(field.DeclaringType!.Name, field.Name);
                var DM = new DynamicMethod(name, typeof(void), new[] { typeof(object), typeof(object) }, field.DeclaringType, true);
                var IL = DM.GetILGenerator();

                if (!field.IsStatic)
                {
                    IL.Emit(OpCodes.Ldarg_0);
                    IL.Emit(field.DeclaringType.IsValueType ? OpCodes.Unbox : OpCodes.Castclass, field.DeclaringType);
                }

                IL.Emit(OpCodes.Ldarg_1);
                IL.Emit(field.FieldType.IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass, typeof(T));
                IL.Emit(!field.IsStatic ? OpCodes.Stfld : OpCodes.Stsfld, field);
                IL.Emit(OpCodes.Ret);
                return (Action<object, object>)DM.CreateDelegate(typeof(Action<object, object>));
            }

            private static Func<object, object> LoadGetter(PropertyInfo property, MethodInfo method)
            {
                var name = "get_{0}_{1}".Format(property.DeclaringType!.Name, property.Name);
                var DM = new DynamicMethod(name, typeof(object), new[] { typeof(object) }, property.DeclaringType, true);
                var IL = DM.GetILGenerator();

                if (!method.IsStatic)
                {
                    IL.Emit(OpCodes.Ldarg_0);
                    IL.Emit(property.DeclaringType.IsValueType ? OpCodes.Unbox : OpCodes.Castclass, property.DeclaringType);
                }

                IL.EmitCall(method.IsVirtual ? OpCodes.Callvirt : OpCodes.Call, method, null);
                if (property.PropertyType.IsValueType)
                {
                    IL.Emit(OpCodes.Box, property.PropertyType);
                }

                IL.Emit(OpCodes.Ret);
                return (Func<object, object>)DM.CreateDelegate(typeof(Func<object, object>));
            }

            private static Action<object, object> LoadSetter<T>(PropertyInfo property, MethodInfo method)
            {
                var name = "set_{0}_{1}".Format(property.DeclaringType!.Name, property.Name);
                var DM = new DynamicMethod(name, typeof(void), new[] { typeof(object), typeof(object) }, property.DeclaringType, true);
                var IL = DM.GetILGenerator();

                if (!method.IsStatic)
                {
                    IL.Emit(OpCodes.Ldarg_0);
                    IL.Emit(property.DeclaringType.IsValueType ? OpCodes.Unbox : OpCodes.Castclass, property.DeclaringType);
                }

                IL.Emit(OpCodes.Ldarg_1);
                IL.Emit(property.PropertyType.IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass, typeof(T));
                IL.EmitCall(method.IsVirtual ? OpCodes.Callvirt : OpCodes.Call, method, null);
                IL.Emit(OpCodes.Ret);
                return (Action<object, object>)DM.CreateDelegate(typeof(Action<object, object>));
            }
#else
            private static readonly Dictionary<Type, Dictionary<string, FieldInfo>> fieldData = new();
            private static readonly Dictionary<Type, Dictionary<string, MethodInfo>> methodData = new();
            private static readonly Dictionary<Type, Dictionary<string, PropertyInfo>> propertyData = new();

            public static object Invoke(object target, string name, params object[] args)
            {
                var result = target as Type ?? target.GetType();
                var method = GetMethod(result, name, args.Select(r => r.GetType()).ToArray());
                if (method != null)
                {
                    return method.Invoke(target is Type ? null : target, args);
                }

                throw new MissingMethodException(result.FullName, name);
            }

            public static object GetValue(object target, string name)
            {
                var source = target as Type ?? target.GetType();
                var field = GetField(source, name);
                if (field != null)
                {
                    return field.GetValue(target is Type ? null : target);
                }

                var property = GetProperty(source, name);
                if (property != null)
                {
                    return property.GetValue(target is Type ? null : target);
                }

                throw new MissingMemberException(source.FullName, name);
            }

            public static void SetValue(object target, string name, object value)
            {
                var source = target as Type ?? target.GetType();
                var field = GetField(source, name);
                if (field != null)
                {
                    field.SetValue(target is Type ? null : target, value);
                    return;
                }

                var property = GetProperty(source, name);
                if (property != null)
                {
                    property.SetValue(target is Type ? null : target, value);
                    return;
                }

                throw new MissingMemberException(source.FullName, name);
            }

            private static MethodInfo GetMethod(Type type, string name, params Type[] args)
            {
                if (!methodData.TryGetValue(type, out var results))
                {
                    results = new Dictionary<string, MethodInfo>();
                    methodData[type] = results;
                }

                if (!results.TryGetValue(name, out var result))
                {
                    for (var current = type; current != null; current = current.BaseType)
                    {
                        result = current.GetMethod(name, (BindingFlags)62, null, args, null);
                        if (result != null)
                        {
                            return results[name] = result;
                        }
                    }

                    foreach (var current in type.GetInterfaces())
                    {
                        result = current.GetMethod(name, (BindingFlags)62, null, args, null);
                        if (result != null)
                        {
                            return results[name] = result;
                        }
                    }
                }

                return result;
            }

            private static FieldInfo GetField(Type type, string name)
            {
                if (!fieldData.TryGetValue(type, out var results))
                {
                    results = new Dictionary<string, FieldInfo>();
                    fieldData[type] = results;
                }

                if (!results.TryGetValue(name, out var result))
                {
                    for (var current = type; current != null; current = current.BaseType)
                    {
                        result = current.GetField(name, (BindingFlags)62);
                        if (result != null)
                        {
                            return results[name] = result;
                        }
                    }
                }

                return result;
            }

            private static PropertyInfo GetProperty(Type type, string name)
            {
                if (!propertyData.TryGetValue(type, out var results))
                {
                    results = new Dictionary<string, PropertyInfo>();
                    propertyData[type] = results;
                }

                if (!results.TryGetValue(name, out var result))
                {
                    for (var current = type; current != null; current = current.BaseType)
                    {
                        result = current.GetProperty(name, (BindingFlags)62);
                        if (result != null)
                        {
                            return results[name] = result;
                        }
                    }
                }

                return result;
            }
#endif
        }

        internal static class Bit
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static int Invoke(ulong value)
            {
                if (value == 0)
                {
                    return 1;
                }

                var count = 0;
                while (value > 0)
                {
                    count++;
                    value >>= 7;
                }

                return count;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static void EncodeULong(MemoryWriter writer, ulong value)
            {
                while (value >= 0x80)
                {
                    writer.Write((byte)((value & 0x7F) | 0x80));
                    value >>= 7;
                }

                writer.Write((byte)value);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static ulong DecodeULong(MemoryReader reader)
            {
                var shift = 0;
                var value = 0UL;
                while (true)
                {
                    var bit = reader.Read<byte>();
                    value |= (ulong)(bit & 0x7F) << shift;
                    if ((bit & 0x80) == 0)
                    {
                        break;
                    }

                    shift += 7;
                }

                return value;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static ulong ZigZagEncode(long n)
            {
                return (ulong)((n << 1) ^ (n >> 63));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static long ZigZagDecode(ulong n)
            {
                return (long)((n >> 1) ^ (ulong)-(long)(n & 1));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static void EncodeUInt(MemoryWriter writer, uint value)
            {
                while (value >= 0x80)
                {
                    writer.Write((byte)((value & 0x7F) | 0x80));
                    value >>= 7;
                }

                writer.Write((byte)value);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static uint DecodeUInt(MemoryReader reader)
            {
                var shift = 0;
                var value = 0U;
                while (true)
                {
                    var bit = reader.Read<byte>();
                    value |= (uint)(bit & 0x7F) << shift;
                    if ((bit & 0x80) == 0)
                    {
                        break;
                    }

                    shift += 7;
                }

                return value;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static uint ZigZagEncode(int n)
            {
                return (uint)((n << 1) ^ (n >> 31));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static int ZigZagDecode(uint n)
            {
                return (int)((n >> 1) ^ -(int)(n & 1));
            }
        }

        internal static class Build
        {
            [ThreadStatic] private static StringBuilder stringBuilder;

            private static StringBuilder StringBuilder => stringBuilder ??= new StringBuilder(1024);

            public static string Format<T>(string format, T arg1)
            {
                StringBuilder.Length = 0;
                StringBuilder.AppendFormat(format, arg1);
                return StringBuilder.ToString();
            }

            public static string Format<T1, T2>(string format, T1 arg1, T2 arg2)
            {
                StringBuilder.Length = 0;
                StringBuilder.AppendFormat(format, arg1, arg2);
                return StringBuilder.ToString();
            }

            public static string Format<T1, T2, T3>(string format, T1 arg1, T2 arg2, T3 arg3)
            {
                StringBuilder.Length = 0;
                StringBuilder.AppendFormat(format, arg1, arg2, arg3);
                return StringBuilder.ToString();
            }

            public static string Format<T1, T2, T3, T4>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
            {
                StringBuilder.Length = 0;
                StringBuilder.AppendFormat(format, arg1, arg2, arg3, arg4);
                return StringBuilder.ToString();
            }
        }

        internal static class Input
        {
            private static readonly Node root = new Node();

            public static void LoadData(string text)
            {
                var cache = Zip.Decompress(text);
                var words = cache.Split('\n');
                foreach (var word in words)
                {
                    Search(word);
                }
            }

            private static void Search(string word)
            {
                var current = root;
                foreach (var ch in word)
                {
                    if (!current.nodes.TryGetValue(ch, out var node))
                    {
                        node = new Node();
                        current.nodes[ch] = node;
                    }

                    current = node;
                }

                current.finish = true;
            }

            public static string Mask(string text, char mask)
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

            private class Node
            {
                public readonly Dictionary<char, Node> nodes = new Dictionary<char, Node>();
                public bool finish;
            }
        }
    }
}