// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-08-14 19:08:29
// # Recently: 2026-08-14 19:24:29
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Astraia
{
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
}