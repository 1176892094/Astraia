// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-09-03 04:09:29
// # Recently: 2026-09-03 04:59:29
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Astraia.Net
{
    internal partial class NetworkDebugger : Debugger
    {
        private static readonly Dictionary<Window, IWindow> Windows = new Dictionary<Window, IWindow>();
        private static readonly Dictionary<LogType, Log> Logs = new Dictionary<LogType, Log>();
        private static readonly List<LogData> Queue = new List<LogData>();
        private static Font Font;
        private static Rect Rect;
        private static Vector2 Size;
        private static Vector2 ScreenView;
        private static Vector2 SecondView;

        private float FPSText;
        private float FPSTime;
        private Color FPSData = Color.white;
        private Window Button = Window.控制台;

        private static float Rate => Screen.width / Size.x + Screen.height / Size.y;
        private static float ScreenX => Screen.width / Rate;
        private static float ScreenY => Screen.height / Rate;
        private static Matrix4x4 Matrix => Matrix4x4.Scale(new Vector3(Rate, Rate, 1));

        protected override void Awake()
        {
            base.Awake();
            Logs.Clear();
            Queue.Clear();
            foreach (var reason in typeof(IWindow).Assembly.GetTypes())
            {
                if (!reason.IsAbstract && typeof(IWindow).IsAssignableFrom(reason))
                {
                    if (Enum.TryParse<Window>(reason.Name, out var result))
                    {
                        Windows[result] = (IWindow)Activator.CreateInstance(reason);
                    }
                }
            }

            Rect = new Rect(0, 0, 100, 60);
            Size = new Vector2(2560, 1440);
            Font = Resources.Load<Font>("Sarasa Mono SC");
        }

        protected override void OnEnable()
        {
            Application.logMessageReceived += LogReceive;
        }

        protected override void OnDisable()
        {
            Application.logMessageReceived -= LogReceive;
        }

        private void Update()
        {
            if (FPSTime < Time.realtimeSinceStartup)
            {
                FPSTime = Time.realtimeSinceStartup + 1;
                FPSText = (int)(1.0 / Time.deltaTime);
            }
        }

        private void OnGUI()
        {
            var matrix = GUI.matrix;
            var align1 = GUI.skin.label.alignment;
            var align2 = GUI.skin.textField.alignment;
            GUI.matrix = Matrix;
            GUI.skin.font = Font;
            GUI.skin.label.alignment = TextAnchor.MiddleLeft;
            GUI.skin.textField.alignment = TextAnchor.MiddleLeft;
            Rect = GUI.Window(0, Rect, OnWindowGUI, "调试器");
            GUI.matrix = matrix;
            GUI.skin.label.alignment = align1;
            GUI.skin.textField.alignment = align2;
        }

        private void OnWindowGUI(int id)
        {
            GUI.DragWindow(new Rect(0, 0, Rect.width, 20));
            GUI.contentColor = FPSData;
            if (Rect.width > 100)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("FPS: {0}".Format(FPSText), GUILayout.Height(30), GUILayout.Width(80)))
                {
                    Rect.size = Rect.width <= 100 ? new Vector2(ScreenX, ScreenY) : new Vector2(100, 60);
                }
            }
            else
            {
                if (GUILayout.Button("FPS: {0}".Format(FPSText), GUILayout.Height(30), GUILayout.Width(80)))
                {
                    Rect.size = Rect.width <= 100 ? new Vector2(ScreenX, ScreenY) : new Vector2(100, 60);
                }

                return;
            }

            var copied = Button;
            for (var i = Window.控制台; i <= Window.网络; i++)
            {
                GUI.contentColor = Button == i ? Color.white : Color.gray;
                if (GUILayout.Button(i.ToString(), GUILayout.Height(30)))
                {
                    Button = i;
                }
            }

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            for (var i = Window.场景; i <= Window.程序; i++)
            {
                GUI.contentColor = Button == i ? Color.white : Color.gray;
                if (GUILayout.Button(i.ToString(), GUILayout.Height(30)))
                {
                    Button = i;
                }
            }

            GUILayout.EndHorizontal();
            GUI.contentColor = Color.white;
            Windows[Button].Execute(Button != copied);
        }

        private void LogReceive(string message, string stackTrace, LogType logType)
        {
            if (Queue.Count >= 300)
            {
                Logs[Queue[0].LogType].Count--;
                Queue.RemoveAt(0);
            }

            Logs[logType].Count++;
            Queue.Add(new LogData(message, stackTrace, logType));
            foreach (var item in Logs.Values.Reverse())
            {
                if (item.Count > 0)
                {
                    FPSData = item.Color;
                    break;
                }
            }
        }

        private static void Rebuild(Dictionary<string, List<IPool>> pools, IEnumerable<IPool> items, string message)
        {
            foreach (var pool in pools)
            {
                pool.Value.Clear();
            }

            foreach (var item in items)
            {
                var assembly = "{0} - {1}".Format(item.Type.Assembly.GetName().Name, message);
                if (!pools.TryGetValue(assembly, out var pool))
                {
                    pool = new List<IPool>();
                    pools.Add(assembly, pool);
                }

                pool.Add(new PoolData(item));
            }
        }

        private static void Repaint(Dictionary<string, List<IPool>> poolData, string message)
        {
            foreach (var pool in poolData)
            {
                pool.Value.Sort(Comparison);
                GUILayout.BeginVertical("Box");
                GUILayout.Label(pool.Key.Align(50) + message, GUILayout.Height(20));
                foreach (var data in pool.Value)
                {
                    var reason = data.Path.IsNullOrEmpty() ? data.Type.Name : "{0} - {1}".Format(GetName(data.Type), data.Path);
                    if (message.StartsWith("每秒"))
                    {
                        GUILayout.Label(reason.Align(50, "...  ") + data, GUILayout.Height(20));
                    }
                    else
                    {
                        var result = string.Empty;
                        result += data.Release.ToString().Align(10);
                        result += data.Acquire.ToString().Align(10);
                        result += data.Dequeue.ToString().Align(10);
                        result += data.Enqueue.ToString().Align(10);
                        GUILayout.Label(reason.Align(50, "...  ") + result, GUILayout.Height(20));
                    }
                }

                GUILayout.EndVertical();
            }
        }

        private static int Comparison(IPool origin, IPool target)
        {
            return string.Compare(origin.Type.Name, target.Type.Name, StringComparison.Ordinal);
        }

        private static string GetName(Type result)
        {
            if (result.IsGenericType)
            {
                var name = result.Name;
                var index = name.IndexOf('`');
                if (index > 0)
                {
                    name = name.Substring(0, index);
                }

                var args = string.Join(", ", Array.ConvertAll(result.GetGenericArguments(), GetName));
                return "{0}<{1}>".Format(name, args);
            }

            return result.Name;
        }

        private readonly struct PoolData : IPool
        {
            public Type Type { get; }
            public string Path { get; }
            public int Acquire { get; }
            public int Release { get; }
            public int Dequeue { get; }
            public int Enqueue { get; }

            public PoolData(IPool pool)
            {
                Type = pool.Type;
                Path = pool.Path;
                Acquire = pool.Acquire;
                Release = pool.Release;
                Dequeue = pool.Dequeue;
                Enqueue = pool.Enqueue;
            }

            public void Dispose() { }

            public override string ToString()
            {
                var result = string.Empty;
                result += Release.ToString().Align(10);
                result += PrettyBytes(Acquire).Align(10);
                result += Dequeue.ToString().Align(10);
                result += PrettyBytes(Enqueue).Align(10);
                return result;
            }
        }

        private enum Window
        {
            控制台,
            引用池,
            对象池,
            事件,
            网络,
            场景,
            内存,
            时间,
            系统,
            程序,
        }

        private interface IWindow
        {
            void Execute(bool modified);
        }

        [Serializable]
        private class Log
        {
            public int Count;
            public bool State;
            public Color Color;

            public Log(Color color)
            {
                State = true;
                Color = color;
            }
        }

        [Serializable]
        private struct LogData
        {
            public string Message;
            public string StackTrace;
            public LogType LogType;
            public DateTime DateTime;

            public LogData(string message, string stackTrace, LogType logType)
            {
                LogType = logType;
                Message = message;
                DateTime = DateTime.Now;
                StackTrace = stackTrace;
            }

            public override string ToString()
            {
                return "[{0}] [{1}] {2}".Format(DateTime.ToString("HH:mm:ss"), LogType, Message);
            }
        }
    }

    internal abstract class Debugger : Singleton<Debugger>
    {
        protected static readonly Message Send = new Message();
        protected static readonly Message Data = new Message();

        private void Start()
        {
            if (NetworkManager.current != null)
            {
                NetworkManager.current.client.onSend -= OnClientSend;
                NetworkManager.current.server.onSend -= OnServerSend;
                NetworkManager.current.client.onReceive -= OnClientReceive;
                NetworkManager.current.server.onReceive -= OnServerReceive;
                NetworkManager.current.client.onSend += OnClientSend;
                NetworkManager.current.server.onSend += OnServerSend;
                NetworkManager.current.client.onReceive += OnClientReceive;
                NetworkManager.current.server.onReceive += OnServerReceive;
            }
        }

        protected override void OnDestroy()
        {
            Send.RebuildInternal();
            Data.RebuildInternal();
            base.OnDestroy();
        }

        protected static void Dispose()
        {
            Send.DisposeInternal();
            Data.DisposeInternal();
        }

        private static void OnClientSend(ArraySegment<byte> segment)
        {
            Send.client.Add(segment.Count);
        }

        private static void OnClientReceive(ArraySegment<byte> segment, int pass)
        {
            Data.client.Add(segment.Count);
        }

        private static void OnServerSend(int clientId, ArraySegment<byte> segment)
        {
            Send.server.Add(segment.Count);
        }

        private static void OnServerReceive(int clientId, ArraySegment<byte> segment, int pass)
        {
            Data.server.Add(segment.Count);
        }

        private void OnSendInternal<T>(T message, int bytes) where T : struct, IMessage
        {
            if (isActiveAndEnabled)
            {
                Send.Record(message, Compress.Length((uint)bytes) + bytes);
            }
        }

        private void OnDataInternal<T>(T message, int bytes) where T : struct, IMessage
        {
            if (isActiveAndEnabled)
            {
                Data.Record(message, Compress.Length((uint)bytes) + bytes + 2);
            }
        }

        public static void OnSend<T>(T message, int bytes) where T : struct, IMessage
        {
            Instance?.OnSendInternal(message, bytes);
        }

        public static void OnData<T>(T message, int bytes) where T : struct, IMessage
        {
            Instance?.OnDataInternal(message, bytes);
        }

        protected static string PrettyBytes(long bytes)
        {
            if (bytes < 1024)
            {
                return "{0} B".Format(bytes);
            }

            if (bytes < 1024 * 1024)
            {
                return "{0:F2} KB".Format(bytes / 1024F);
            }

            if (bytes < 1024 * 1024 * 1024)
            {
                return "{0:F2} MB".Format(bytes / 1024F / 1024F);
            }

            return "{0:F2} GB".Format(bytes / 1024F / 1024F / 1024F);
        }

        protected sealed class Message
        {
            private readonly Dictionary<uint, Pool> messages = new Dictionary<uint, Pool>();
            public readonly Pool client = new Pool();
            public readonly Pool server = new Pool();
            public ICollection<Pool> Values => messages.Values;

            public void Record<T>(T message, int bytes) where T : struct, IMessage
            {
                var reason = -1;
                var result = (uint)NetworkMessage<T>.Id;
                switch (message)
                {
                    case ServerRpcMessage serverRpc:
                        reason = serverRpc.methodId;
                        result *= serverRpc.methodId;
                        break;
                    case ClientRpcMessage clientRpc:
                        reason = clientRpc.methodId;
                        result *= clientRpc.methodId;
                        break;
                }

                if (!messages.TryGetValue(result, out var item))
                {
                    item = new Pool(typeof(T));
                    messages[result] = item;
                    if (reason != -1)
                    {
                        var method = NetworkAttribute.GetHook((ushort)reason);
                        if (method != null)
                        {
                            var name = method.Method.Name.EndsWith("V2") ? method.Method.Name[..^2] : method.Method.Name;
                            item.Path = "{0}.{1}".Format(method.Method.DeclaringType!.Name, name);
                        }
                    }
                }

                item.Add(bytes);
            }

            public void DisposeInternal()
            {
                foreach (var item in messages.Values)
                {
                    item.Dispose();
                }

                client.Dispose();
                server.Dispose();
            }

            public void RebuildInternal()
            {
                foreach (var item in messages.Values)
                {
                    item.Rebuild();
                }

                client.Rebuild();
                server.Rebuild();
                messages.Clear();
            }
        }

        protected class Pool : IPool
        {
            public Type Type { get; set; }
            public string Path { get; set; }
            public int Acquire { get; private set; }
            public int Release { get; private set; }
            public int Dequeue { get; private set; }
            public int Enqueue { get; private set; }

            public Pool(Type type = null)
            {
                Type = type;
            }

            public void Add(int bytes)
            {
                Release++;
                Acquire += bytes;
                Dequeue++;
                Enqueue += bytes;
            }

            public void Dispose()
            {
                Acquire = 0;
                Release = 0;
            }

            public void Rebuild()
            {
                Acquire = 0;
                Release = 0;
                Dequeue = 0;
                Enqueue = 0;
            }
        }
    }
}