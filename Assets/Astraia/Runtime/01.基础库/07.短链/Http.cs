// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-09-19 23:09:57
// // # Recently: 2025-09-19 23:09:57
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using System.Net;
using System.Threading.Tasks;

// ReSharper disable All

#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法

namespace Astraia
{
    public static class HttpServer
    {
        public static void StartServer(int port, Func<HttpListenerRequest, HttpListenerResponse, Task> request)
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
                        Task.Run(async () => // 每个请求单独处理
                        {
                            try
                            {
                                await request.Invoke(context.Request, context.Response);
                            }
                            catch (Exception e)
                            {
                                Log.Warn(e.ToString());
                                context.Response.StatusCode = 500;
                                context.Response.Close();
                            }
                        });
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