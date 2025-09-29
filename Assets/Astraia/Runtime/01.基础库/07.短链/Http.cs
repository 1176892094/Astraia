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
                    catch (Exception e)
                    {
                        Log.Warn(e.ToString());
                    }
                }
            }
        }
    }
}