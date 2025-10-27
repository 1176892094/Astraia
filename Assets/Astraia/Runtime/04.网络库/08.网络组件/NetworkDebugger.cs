// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2024-12-16 01:12:26
// # Recently: 2024-12-22 20:12:26
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using Astraia.Net;
using UnityEngine;
using UnityEngine.Profiling;

namespace Astraia.Common
{
    public abstract class DebugManager : MonoBehaviour, IEvent<PingUpdate>
    {
        private static bool isRunning;
        protected string address;
        protected double framePing;
        private double waitTime;

        private int clientIntervalReceivedPackets;
        private long clientIntervalReceivedBytes;
        private int clientIntervalSentPackets;
        private long clientIntervalSentBytes;

        private int clientReceivedPacketsPerSecond;
        private long clientReceivedBytesPerSecond;
        private int clientSentPacketsPerSecond;
        private long clientSentBytesPerSecond;

        private int serverIntervalReceivedPackets;
        private long serverIntervalReceivedBytes;
        private int serverIntervalSentPackets;
        private long serverIntervalSentBytes;

        private int serverReceivedPacketsPerSecond;
        private long serverReceivedBytesPerSecond;
        private int serverSentPacketsPerSecond;
        private long serverSentBytesPerSecond;

        private static readonly Items sendItems = new Items();
        private static readonly Items receiveItems = new Items();

        protected virtual void Awake()
        {
            isRunning = true;
            address = Service.Host.Ip();
        }

        protected virtual void Start()
        {
            if (Transport.Instance)
            {
                Transport.Instance.OnClientSend += OnClientSend;
                Transport.Instance.OnServerSend += OnServerSend;
                Transport.Instance.OnClientReceive += OnClientReceive;
                Transport.Instance.OnServerReceive += OnServerReceive;
            }
        }

        protected virtual void OnEnable()
        {
            EventManager.Listen(this);
        }

        protected virtual void OnDisable()
        {
            EventManager.Remove(this);
        }

        protected virtual void Update()
        {
            if (waitTime < Time.unscaledTimeAsDouble)
            {
                if (NetworkManager.isClient)
                {
                    UpdateClient();
                }

                if (NetworkManager.isServer)
                {
                    UpdateServer();
                }

                waitTime = Time.unscaledTimeAsDouble + 1;
            }
        }

        private void OnDestroy()
        {
            if (Transport.Instance)
            {
                Transport.Instance.OnClientSend -= OnClientSend;
                Transport.Instance.OnServerSend -= OnServerSend;
                Transport.Instance.OnClientReceive -= OnClientReceive;
                Transport.Instance.OnServerReceive -= OnServerReceive;
            }

            sendItems.Clear();
            receiveItems.Clear();
        }

        public void Execute(PingUpdate message)
        {
            framePing = message.pingTime;
        }

        private void OnClientReceive(ArraySegment<byte> data, int channel)
        {
            clientIntervalReceivedPackets++;
            clientIntervalReceivedBytes += data.Count;
        }

        private void OnClientSend(ArraySegment<byte> data, int channel)
        {
            clientIntervalSentPackets++;
            clientIntervalSentBytes += data.Count;
        }

        private void OnServerReceive(int connectionId, ArraySegment<byte> data, int channel)
        {
            serverIntervalReceivedPackets++;
            serverIntervalReceivedBytes += data.Count;
        }

        private void OnServerSend(int connectionId, ArraySegment<byte> data, int channel)
        {
            serverIntervalSentPackets++;
            serverIntervalSentBytes += data.Count;
        }

        private void UpdateClient()
        {
            clientReceivedPacketsPerSecond = clientIntervalReceivedPackets;
            clientReceivedBytesPerSecond = clientIntervalReceivedBytes;
            clientSentPacketsPerSecond = clientIntervalSentPackets;
            clientSentBytesPerSecond = clientIntervalSentBytes;

            clientIntervalReceivedPackets = 0;
            clientIntervalReceivedBytes = 0;
            clientIntervalSentPackets = 0;
            clientIntervalSentBytes = 0;
        }

        private void UpdateServer()
        {
            serverReceivedPacketsPerSecond = serverIntervalReceivedPackets;
            serverReceivedBytesPerSecond = serverIntervalReceivedBytes;
            serverSentPacketsPerSecond = serverIntervalSentPackets;
            serverSentBytesPerSecond = serverIntervalSentBytes;

            serverIntervalReceivedPackets = 0;
            serverIntervalReceivedBytes = 0;
            serverIntervalSentPackets = 0;
            serverIntervalSentBytes = 0;
        }

        protected void OnGUIServer()
        {
            GUILayout.Label("向服务器发送数量:\t\t{0}".Format(clientSentPacketsPerSecond));
            GUILayout.Label("向服务器发送大小:\t\t{0}/s".Format(PrettyBytes(clientSentBytesPerSecond)));
            GUILayout.Label("从服务器接收数量:\t\t{0}".Format(clientReceivedPacketsPerSecond));
            GUILayout.Label("从服务器接收大小:\t\t{0}/s".Format(PrettyBytes(clientReceivedBytesPerSecond)));
        }

        protected void OnGUIClient()
        {
            GUILayout.Label("向客户端发送数量:\t\t{0}".Format(serverSentPacketsPerSecond));
            GUILayout.Label("向客户端发送大小:\t\t{0}/s".Format(PrettyBytes(serverSentBytesPerSecond)));
            GUILayout.Label("从客户端接收数量:\t\t{0}".Format(serverReceivedPacketsPerSecond));
            GUILayout.Label("从客户端接收大小:\t\t{0}/s".Format(PrettyBytes(serverReceivedBytesPerSecond)));
        }

        protected static void ItemReset()
        {
            sendItems.Reset();
            receiveItems.Reset();
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

        internal static IList<Pool> SendReference()
        {
            var pools = sendItems.messages.Values.Select(item => new Pool(item)).ToList();
            pools.AddRange(sendItems.function.Values.Select(item => new Pool(item)));
            return pools;
        }

        internal static IList<Pool> ReceiveReference()
        {
            var pools = receiveItems.messages.Values.Select(item => new Pool(item)).ToList();
            pools.AddRange(receiveItems.function.Values.Select(item => new Pool(item)));
            return pools;
        }

        internal static void OnSend<T>(T message, int bytes) where T : struct, IMessage
        {
            if (!isRunning) return;
            sendItems.Record(message, Service.Bit.Invoke((uint)bytes) + bytes);
        }

        internal static void OnReceive<T>(T message, int bytes) where T : struct, IMessage
        {
            if (!isRunning) return;
            receiveItems.Record(message, Service.Bit.Invoke((uint)bytes + 2) + bytes + 2);
        }

        private class Items
        {
            public readonly Dictionary<Type, Item> messages = new Dictionary<Type, Item>();
            public readonly Dictionary<ushort, Item> function = new Dictionary<ushort, Item>();

            public void Record<T>(T message, int bytes) where T : struct, IMessage
            {
                if (!messages.TryGetValue(typeof(T), out var item))
                {
                    item = HeapManager.Dequeue<Item>();
                    item.Type = typeof(T);
                    messages[typeof(T)] = item;
                }

                switch (message)
                {
                    case ServerRpcMessage server:
                        Record(server.methodHash, bytes, typeof(T));
                        break;
                    case ClientRpcMessage client:
                        Record(client.methodHash, bytes, typeof(T));
                        break;
                }

                item.Add(bytes);
            }

            private void Record(ushort method, int bytes, Type type)
            {
                if (!function.TryGetValue(method, out var item))
                {
                    item = HeapManager.Dequeue<Item>();
                    var data = NetworkAttribute.GetInvoke(method);
                    if (data != null)
                    {
                        var name = data.Method.Name;
                        if (name.EndsWith("_0"))
                        {
                            name = name.Substring(0, name.Length - 2);
                        }

                        item.Path = "{0}.{1}".Format(data.Method.DeclaringType, name);
                    }

                    item.Type = type;
                    function[method] = item;
                }

                item.Add(bytes);
            }

            public void Reset()
            {
                foreach (var item in function.Values)
                {
                    item.Dispose();
                }

                foreach (var item in messages.Values)
                {
                    item.Dispose();
                }
            }

            public void Clear()
            {
                foreach (var item in messages.Values)
                {
                    item.Dispose();
                    HeapManager.Enqueue(item);
                }

                messages.Clear();

                foreach (var item in function.Values)
                {
                    item.Dispose();
                    HeapManager.Enqueue(item);
                }

                function.Clear();
            }

            [Serializable]
            public class Item : IPool
            {
                public Type Type { get; set; }
                public string Path { get; set; }
                public int Acquire { get; set; }
                public int Release { get; set; }
                public int Dequeue { get; set; }
                public int Enqueue { get; set; }

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
            }
        }
    }

    [DefaultExecutionOrder(-100)]
    public partial class NetworkDebugger : DebugManager
    {
        private Font font;
        private bool maximized;
        private float frameData;
        private double frameTime;

        private Window window;
        private Rect windowRect;
        private Rect screenRect;
        private Color screenColor;
        private Vector2 screenView;
        private Vector2 windowView;
        private Vector2 screenRate;

        private float screenWidth => Screen.width / screenSize;
        private float screenHeight => Screen.height / screenSize;
        private float screenSize => Screen.width / screenRate.x + Screen.height / screenRate.y;

        protected override void Awake()
        {
            base.Awake();
            screenColor = Color.white;
            screenRate = new Vector2(2560, 1440);
            screenRect = new Rect(10, 20, 100, 60);
            font = Resources.Load<Font>("Sarasa Mono SC");
        }

        protected override void Start()
        {
            base.Start();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var sceneTypes = assembly.GetTypes();
                foreach (var sceneType in sceneTypes)
                {
                    if (!sceneType.IsSubclassOf(typeof(Component)))
                    {
                        continue;
                    }

                    if (sceneType.IsAbstract)
                    {
                        continue;
                    }

                    if (sceneType.IsGenericType)
                    {
                        continue;
                    }

                    cachedTypes.Add(sceneType);
                }
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            Application.logMessageReceived += LogMessageReceived;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            Application.logMessageReceived -= LogMessageReceived;
        }

        protected override void Update()
        {
            base.Update();
            if (frameTime > Time.realtimeSinceStartup)
            {
                return;
            }

            frameData = (int)(1.0 / Time.deltaTime);
            frameTime = Time.realtimeSinceStartup + 1;
        }

        private void OnGUI()
        {
            var matrix = GUI.matrix;
            var labelAlignment = GUI.skin.label.alignment;
            var fieldAlignment = GUI.skin.textField.alignment;

            GUI.matrix = Matrix4x4.Scale(new Vector3(screenSize, screenSize, 1));
            GUI.skin.label.alignment = TextAnchor.MiddleLeft;
            GUI.skin.textField.alignment = TextAnchor.MiddleLeft;

            if (font != null)
            {
                GUI.skin.font = font;
            }

            if (maximized)
            {
                var maxRect = new Rect(0, 0, screenWidth, screenHeight);
                GUI.Window(0, maxRect, MaxWindow, "调试器");
            }
            else
            {
                windowRect.size = screenRect.size;
                windowRect = GUI.Window(0, windowRect, MinWindow, "调试器");
            }

            GUI.matrix = matrix;
            GUI.skin.label.alignment = labelAlignment;
            GUI.skin.textField.alignment = fieldAlignment;
        }

        private void MaxWindow(int id)
        {
            GUILayout.BeginHorizontal();
            GUI.contentColor = screenColor;
            if (GUILayout.Button("FPS: {0}".Format(frameData), GUILayout.Height(30), GUILayout.Width(80)))
            {
                maximized = false;
            }

            GUI.contentColor = window == Window.Console ? Color.white : Color.gray;
            if (GUILayout.Button(Window.Console.ToString(), GUILayout.Height(30)))
            {
                window = Window.Console;
            }

            GUI.contentColor = window == Window.Scene ? Color.white : Color.gray;
            if (GUILayout.Button(Window.Scene.ToString(), GUILayout.Height(30)))
            {
                UpdateGameObject();
                UpdateComponent();
                window = Window.Scene;
            }

            GUI.contentColor = window == Window.Reference ? Color.white : Color.gray;
            if (GUILayout.Button(Window.Reference.ToString(), GUILayout.Height(30)))
            {
                window = Window.Reference;
            }

            GUI.contentColor = window == Window.Network ? Color.white : Color.gray;
            if (GUILayout.Button(Window.Network.ToString(), GUILayout.Height(30)))
            {
                window = Window.Network;
            }

            GUILayout.EndHorizontal();

            if (window != Window.Console)
            {
                GUILayout.BeginHorizontal();
                GUI.contentColor = window == Window.Memory ? Color.white : Color.gray;
                if (GUILayout.Button(Window.Memory.ToString(), GUILayout.Height(30)))
                {
                    window = Window.Memory;
                }

                GUI.contentColor = window == Window.Time ? Color.white : Color.gray;
                if (GUILayout.Button(Window.Time.ToString(), GUILayout.Height(30)))
                {
                    window = Window.Time;
                }

                GUI.contentColor = window == Window.System ? Color.white : Color.gray;
                if (GUILayout.Button(Window.System.ToString(), GUILayout.Height(30)))
                {
                    window = Window.System;
                }

                GUI.contentColor = window == Window.Screen ? Color.white : Color.gray;
                if (GUILayout.Button(Window.Screen.ToString(), GUILayout.Height(30)))
                {
                    window = Window.Screen;
                }

                GUI.contentColor = window == Window.Project ? Color.white : Color.gray;
                if (GUILayout.Button(Window.Project.ToString(), GUILayout.Height(30)))
                {
                    window = Window.Project;
                }

                GUILayout.EndHorizontal();
            }

            GUI.contentColor = Color.white;
            switch (window)
            {
                case Window.Console:
                    ConsoleWindow();
                    break;
                case Window.Scene:
                    SceneWindow();
                    break;
                case Window.Reference:
                    ReferenceWindow();
                    break;
                case Window.Network:
                    NetworkWindow();
                    break;
                case Window.System:
                    SystemWindow();
                    break;
                case Window.Project:
                    ProjectWindow();
                    break;
                case Window.Memory:
                    MemoryWindow();
                    break;
                case Window.Screen:
                    ScreenWindow();
                    break;
                case Window.Time:
                    TimeWindow();
                    break;
            }
        }

        private void MinWindow(int id)
        {
            GUI.DragWindow(new Rect(0, 0, screenRect.width, 20f));

            GUILayout.BeginHorizontal();
            GUI.contentColor = screenColor;
            if (GUILayout.Button("FPS: {0}".Format(frameData), GUILayout.Height(30), GUILayout.Width(80)))
            {
                maximized = true;
            }

            GUI.contentColor = Color.white;
            GUILayout.EndHorizontal();
        }

        private enum Window
        {
            Console,
            Scene,
            Reference,
            Network,
            System,
            Project,
            Memory,
            Screen,
            Time,
        }
    }

    public partial class NetworkDebugger
    {
        private readonly Dictionary<LogType, LogData> logData = new Dictionary<LogType, LogData>
        {
            { LogType.Log, new LogData(Color.white) },
            { LogType.Warning, new LogData(Color.yellow) },
            { LogType.Exception, new LogData(Color.magenta) },
            { LogType.Error, new LogData(Color.red) },
            { LogType.Assert, new LogData(Color.green) }
        };

        private readonly List<LogMessage> messages = new List<LogMessage>();
        private int selectMessage = -1;

        private void ConsoleWindow()
        {
            ConsoleOption();
            ConsoleButton();
            ConsoleScroll();
        }

        private void LogMessageReceived(string message, string stackTrace, LogType logType)
        {
            if (!logData.TryGetValue(logType, out var data))
            {
                return;
            }

            if (messages.Count >= 300)
            {
                if (logData.TryGetValue(messages[0].logType, out var log))
                {
                    log.count--;
                }

                messages.RemoveAt(0);
            }

            messages.Add(new LogMessage(message, stackTrace, logType));

            data.count++;
            var logs = logData.Values.Reverse();
            foreach (var log in logs)
            {
                if (log.count > 0)
                {
                    screenColor = log.color;
                    break;
                }
            }
        }

        private void ConsoleButton()
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Clear", GUILayout.Width((screenWidth - 20) / 2), GUILayout.Height(30)))
            {
                selectMessage = -1;
                foreach (var data in logData.Values)
                {
                    data.count = 0;
                }

                messages.Clear();
                screenColor = Color.white;
            }

            if (GUILayout.Button("Report", GUILayout.Height(30)))
            {
                // var mailBody = new StringBuilder(1024);
                // foreach (var message in messages)
                // {
                //     mailBody.Append(message + "\n\n" + message.stackTrace + "\n\n");
                // }
                //
                // Service.Mail.Send(new MailData
                // {
                //     smtpServer = GlobalSetting.Instance.smtpServer,
                //     smtpPort = GlobalSetting.Instance.smtpPort,
                //     senderName = "Astraia",
                //     senderAddress = GlobalSetting.Instance.smtpUsername,
                //     senderPassword = GlobalSetting.Instance.smtpPassword,
                //     targetAddress = GlobalSetting.Instance.smtpUsername,
                //     mailName = "来自《Astraia》的调试日志:",
                //     mailBody = mailBody.ToString()
                // });
            }

            GUILayout.EndHorizontal();
        }

        private void ConsoleOption()
        {
            GUILayout.BeginHorizontal();
            foreach (var type in logData.Keys)
            {
                if (logData.TryGetValue(type, out var data))
                {
                    GUI.contentColor = data.status ? Color.white : Color.gray;
                    if (GUILayout.Button("{0} [{1}]".Format(type, data.count), GUILayout.Height(30)))
                    {
                        data.status = !data.status;
                    }
                }
            }

            GUILayout.EndHorizontal();
        }

        private void ConsoleScroll()
        {
            screenView = GUILayout.BeginScrollView(screenView, "Box", GUILayout.Height(screenHeight * 0.4f));

            for (var i = 0; i < messages.Count; i++)
            {
                if (logData.TryGetValue(messages[i].logType, out var data) && data.status)
                {
                    GUILayout.BeginHorizontal();
                    GUI.contentColor = data.color;
                    if (GUILayout.Toggle(selectMessage == i, messages[i].ToString(), GUILayout.Height(20)))
                    {
                        selectMessage = i;
                    }

                    GUI.contentColor = Color.white;
                    GUILayout.EndHorizontal();
                }
            }

            GUILayout.EndScrollView();

            windowView = GUILayout.BeginScrollView(windowView, "Box");
            if (selectMessage != -1)
            {
                GUILayout.Label(messages[selectMessage].message + "\n\n" + messages[selectMessage].stackTrace);
            }

            GUILayout.EndScrollView();
        }

        [Serializable]
        private class LogData
        {
            public int count;
            public bool status;
            public Color color;

            public LogData(Color color)
            {
                this.color = color;
                status = true;
            }
        }

        [Serializable]
        private struct LogMessage
        {
            public string message;
            public string stackTrace;
            public LogType logType;
            public DateTime dateTime;

            public LogMessage(string message, string stackTrace, LogType logType)
            {
                this.logType = logType;
                this.message = message;
                this.stackTrace = stackTrace;
                dateTime = DateTime.Now;
            }

            public override string ToString()
            {
                return "[{0}] [{1}] {2}".Format(dateTime.ToString("HH:mm:ss"), logType, message);
            }
        }
    }

    public partial class NetworkDebugger
    {
        private readonly List<Type> cachedTypes = new List<Type>();
        private bool cachedComponent;

        private int componentIndex = -1;
        private string componentFilter = "";
        private readonly List<Component> components = new List<Component>();

        private int gameObjectIndex = -1;
        private string gameObjectFilter = "";
        private readonly List<GameObject> gameObjects = new List<GameObject>();

        private void SceneWindow()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("GameObject [{0}]".Format(gameObjects.Count), "Button", GUILayout.Width((screenWidth - 20) / 2), GUILayout.Height(30));
            if (GUILayout.Button("Refresh", GUILayout.Height(30)))
            {
                UpdateGameObject();
                UpdateComponent();
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical("Box", GUILayout.Width((screenWidth - 20) / 2));
            ShowGameObject();
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Box");
            ShowComponent();
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
        }

        private void UpdateGameObject()
        {
            gameObjects.Clear();
            var objects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            foreach (var component in objects)
            {
                gameObjects.Add(component);
            }

            gameObjects.Sort(Comparison);
            gameObjectIndex = -1;
        }

        private static int Comparison(GameObject origin, GameObject target)
        {
            return string.Compare(origin.name, target.name, StringComparison.Ordinal);
        }

        private void UpdateComponent()
        {
            components.Clear();
            if (gameObjectIndex != -1 && gameObjectIndex < gameObjects.Count)
            {
                var objects = gameObjects[gameObjectIndex].GetComponents<Component>();
                foreach (var component in objects)
                {
                    components.Add(component);
                }
            }

            componentIndex = -1;
            cachedComponent = false;
        }

        private void ShowGameObject()
        {
            GUILayout.BeginHorizontal();
            gameObjectFilter = GUILayout.TextField(gameObjectFilter, GUILayout.Height(25));
            GUILayout.EndHorizontal();

            screenView = GUILayout.BeginScrollView(screenView);
            for (var i = 0; i < gameObjects.Count; i++)
            {
                var target = gameObjects[i];
                if (target && target.name.Contains(gameObjectFilter))
                {
                    GUILayout.BeginHorizontal();
                    GUI.contentColor = target.activeSelf ? Color.white : Color.gray;
                    var selected = gameObjectIndex == i;
                    if (GUILayout.Toggle(selected, " " + target.name) != selected)
                    {
                        gameObjectIndex = gameObjectIndex != i ? i : -1;
                        UpdateComponent();
                    }

                    GUILayout.EndHorizontal();

                    if (gameObjectIndex == i)
                    {
                        GUILayout.BeginVertical("Box");

                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Tag: " + target.tag, GUILayout.Width(160));
                        GUILayout.Label("Layer: " + LayerMask.LayerToName(target.layer));
                        GUILayout.EndHorizontal();

                        GUILayout.EndVertical();
                    }
                }
            }

            GUILayout.EndScrollView();
        }

        private void ShowComponent()
        {
            if (gameObjectIndex != -1)
            {
                GUILayout.BeginHorizontal();
                if (cachedComponent)
                {
                    componentFilter = GUILayout.TextField(componentFilter, GUILayout.Height(25));
                }
                else
                {
                    if (componentIndex != -1 && componentIndex < components.Count && components[componentIndex])
                    {
                        if (GUILayout.Button("Remove Component", GUILayout.Height(25)))
                        {
                            var component = components[componentIndex];
                            if (component is NetworkDebugger || component is Transform)
                            {
                                Service.Log.Warn("无法销毁组件: " + component.GetType().Name);
                            }
                            else
                            {
                                Destroy(component);
                                UpdateComponent();
                                return;
                            }
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("Add Component", GUILayout.Height(25)))
                        {
                            cachedComponent = !cachedComponent;
                        }
                    }
                }

                GUILayout.EndHorizontal();
            }

            windowView = GUILayout.BeginScrollView(windowView);

            if (gameObjectIndex != -1)
            {
                if (cachedComponent)
                {
                    foreach (var cachedType in cachedTypes)
                    {
                        if (cachedType.FullName == null)
                        {
                            continue;
                        }

                        if (!cachedType.FullName.Contains(componentFilter))
                        {
                            continue;
                        }

                        if (GUILayout.Button(cachedType.FullName, GUILayout.Height(25)))
                        {
                            gameObjects[gameObjectIndex].gameObject.AddComponent(cachedType);
                            cachedComponent = false;
                            UpdateComponent();
                            break;
                        }
                    }
                }
                else
                {
                    for (var i = 0; i < components.Count; ++i)
                    {
                        var component = components[i];
                        if (component == null)
                        {
                            continue;
                        }

                        GUILayout.BeginHorizontal();
                        var selected = componentIndex == i;
                        if (GUILayout.Toggle(selected, " " + component.GetType().Name) != selected)
                        {
                            componentIndex = componentIndex != i ? i : -1;
                        }

                        GUILayout.EndHorizontal();
                    }
                }
            }

            GUILayout.EndScrollView();
        }
    }

    public partial class NetworkDebugger
    {
        private readonly Dictionary<string, List<Pool>> poolData = new Dictionary<string, List<Pool>>();
        private PoolMode windowOption = PoolMode.Heap;

        private void ReferenceWindow()
        {
            GUILayout.BeginHorizontal();
            GUI.contentColor = windowOption == PoolMode.Heap ? Color.white : Color.gray;
            if (GUILayout.Button("Heap", GUILayout.Height(30)))
            {
                windowOption = PoolMode.Heap;
            }

            GUI.contentColor = windowOption == PoolMode.Event ? Color.white : Color.gray;
            if (GUILayout.Button("Event", GUILayout.Height(30)))
            {
                windowOption = PoolMode.Event;
            }

            GUI.contentColor = windowOption == PoolMode.Pool ? Color.white : Color.gray;
            if (GUILayout.Button("Pool", GUILayout.Height(30)))
            {
                windowOption = PoolMode.Pool;
            }

            GUI.contentColor = Color.white;
            GUILayout.EndHorizontal();
            switch (windowOption)
            {
                case PoolMode.Heap:
                    Draw(HeapManager.poolData.Values, "引用池", "未使用\t\t使用中\t\t使用次数\t\t释放次数");
                    break;
                case PoolMode.Event:
                    Draw(EventManager.poolData.Values, "事件池", "触发数\t\t事件数\t\t添加次数\t\t移除次数");
                    break;
                case PoolMode.Pool:
                    Draw(GlobalManager.poolData.Values, "对象池", "未激活\t\t激活中\t\t出队次数\t\t入队次数");
                    break;
            }
        }

        private void Draw(IEnumerable<IPool> items, string message, string module)
        {
            poolData.Clear();
            foreach (var item in items)
            {
                var assembly = "{0} - {1}".Format(item.Type.Assembly.GetName().Name, message);
                if (!poolData.TryGetValue(assembly, out var pool))
                {
                    pool = new List<Pool>();
                    poolData.Add(assembly, pool);
                }

                pool.Add(new Pool(item));
            }

            screenView = GUILayout.BeginScrollView(screenView, "Box");
            foreach (var poolPair in poolData)
            {
                poolPair.Value.Sort(Comparison);
                GUILayout.BeginHorizontal();

                GUILayout.BeginVertical("Box", GUILayout.Width((screenWidth - 28) / 2));
                GUILayout.Label(poolPair.Key, GUILayout.Height(20));
                foreach (var data in poolPair.Value)
                {
                    var assetName = data.Type.Name;
                    if (!string.IsNullOrEmpty(data.Path))
                    {
                        assetName = "{0} - {1}".Format(GetFriendlyName(data.Type), data.Path);
                    }

                    GUILayout.Label(assetName, GUILayout.Height(20));
                }

                GUILayout.EndVertical();

                GUILayout.BeginVertical("Box");
                GUILayout.Label(module, GUILayout.Height(20));
                foreach (var data in poolPair.Value)
                {
                    GUILayout.Label(data.ToString(), GUILayout.Height(20));
                }

                GUILayout.EndVertical();

                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
        }

        private static int Comparison(Pool origin, Pool target)
        {
            return string.Compare(origin.Type.Name, target.Type.Name, StringComparison.Ordinal);
        }

        public static string GetFriendlyName(Type type)
        {
            if (!type.IsGenericType)
            {
                return type.Name;
            }

            var name = type.Name;
            var index = name.IndexOf('`');
            if (index > 0)
            {
                name = name.Substring(0, index);
            }

            var args = string.Join(", ", Array.ConvertAll(type.GetGenericArguments(), GetFriendlyName));
            return "{0}<{1}>".Format(name, args);
        }

        private enum PoolMode
        {
            Heap,
            Event,
            Pool,
        }
    }

    public partial class NetworkDebugger
    {
        private IList<Pool> sendList = new List<Pool>();
        private IList<Pool> receiveList = new List<Pool>();
        private float duration;

        private void NetworkWindow()
        {
            GUILayout.BeginHorizontal();
            string peer;
            ushort port;
            if (Transport.Instance)
            {
                peer = Transport.Instance.address;
                if (peer == "localhost")
                {
                    peer = address;
                }

                port = Transport.Instance.port;
            }
            else
            {
                peer = "127.0.0.1";
                port = 20974;
            }

            GUILayout.Label("{0} : {1}".Format(peer, port), "Button", GUILayout.Width((screenWidth - 20) / 2), GUILayout.Height(30));
            var ping = NetworkManager.isClient ? "Ping: {0} ms".Format(Math.Min((int)(framePing * 1000), 999)) : "Client is not active!";
            GUILayout.Label(ping, "Button", GUILayout.Height(30));

            GUILayout.EndHorizontal();

            screenView = GUILayout.BeginScrollView(screenView, "Box");
            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical("Box", GUILayout.Width((screenWidth - 28) / 2));
            OnGUIServer();
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Box");
            OnGUIClient();
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            if (duration < Time.unscaledTime)
            {
                duration = Time.unscaledTime + 1;
                sendList = SendReference();
                receiveList = ReceiveReference();
                ItemReset();
            }

            NetworkMessage(sendList, "发送队列", "每秒发送\t\t每秒发送\t\t全局发送\t\t全局发送");
            NetworkMessage(receiveList, "接收队列", "每秒接收\t\t每秒接收\t\t全局接收\t\t全局接收");


            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            if (!NetworkManager.Client.isActive && !NetworkManager.isServer)
            {
                if (!NetworkManager.isClient)
                {
                    if (GUILayout.Button("Host (Server + Client)", GUILayout.Height(30)))
                    {
                        NetworkManager.StartHost();
                    }

                    if (GUILayout.Button("Server", GUILayout.Height(30)))
                    {
                        NetworkManager.StartServer();
                    }

                    if (GUILayout.Button("Client", GUILayout.Height(30)))
                    {
                        NetworkManager.StartClient();
                    }
                }
                else
                {
                    if (GUILayout.Button("Stop Client", GUILayout.Height(30)))
                    {
                        NetworkManager.StopClient();
                    }
                }
            }

            if (NetworkManager.Client.isActive && !NetworkManager.Client.isReady)
            {
                if (GUILayout.Button("Ready", GUILayout.Height(30)))
                {
                    NetworkManager.Client.Ready();
                }
            }

            if (NetworkManager.isServer && NetworkManager.Client.isActive)
            {
                if (GUILayout.Button("Stop Host", GUILayout.Height(30)))
                {
                    NetworkManager.StopHost();
                }
            }
            else if (NetworkManager.Client.isActive)
            {
                if (GUILayout.Button("Stop Client", GUILayout.Height(30)))
                {
                    NetworkManager.StopClient();
                }
            }
            else if (NetworkManager.isServer)
            {
                if (GUILayout.Button("Stop Server", GUILayout.Height(30)))
                {
                    NetworkManager.StopServer();
                }
            }

            GUILayout.EndHorizontal();
        }

        private void NetworkMessage(IList<Pool> references, string message, string module)
        {
            poolData.Clear();
            foreach (var reference in references)
            {
                var assemblyName = "{0} - {1}".Format(reference.Type.Assembly.GetName().Name, message);
                if (!poolData.TryGetValue(assemblyName, out var results))
                {
                    results = new List<Pool>();
                    poolData.Add(assemblyName, results);
                }

                results.Add(reference);
            }


            foreach (var poolPair in poolData)
            {
                poolPair.Value.Sort(Comparison);
                GUILayout.BeginHorizontal();

                GUILayout.BeginVertical("Box", GUILayout.Width((screenWidth - 28) / 2));
                GUILayout.Label(poolPair.Key, GUILayout.Height(20));
                foreach (var data in poolPair.Value)
                {
                    var assetName = data.Type.Name;
                    if (!string.IsNullOrEmpty(data.Path))
                    {
                        assetName = "{0} - {1}".Format(data.Type.Name, data.Path);
                    }

                    GUILayout.Label(assetName, GUILayout.Height(20));
                }

                GUILayout.EndVertical();

                GUILayout.BeginVertical("Box");
                GUILayout.Label(module, GUILayout.Height(20));
                foreach (var data in poolPair.Value)
                {
                    var result = "{0}\t\t{1}\t\t{2}\t\t{3}".Format(data.Release, PrettyBytes(data.Acquire), data.Dequeue, PrettyBytes(data.Enqueue));
                    GUILayout.Label(result, GUILayout.Height(20));
                }

                GUILayout.EndVertical();

                GUILayout.EndHorizontal();
            }
        }
    }

    public partial class NetworkDebugger
    {
        private void TimeWindow()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(" 时间信息", GUILayout.Height(25));
            GUILayout.EndHorizontal();

            screenView = GUILayout.BeginScrollView(screenView, "Box");
            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical("Box", GUILayout.Width(300));
            GUILayout.Label("DataTime:");
            GUILayout.Label("Time.frameCount:");
            GUILayout.Label("Time.realtimeSinceStartup:");
            GUILayout.Label("Time.timeScale:");
            GUILayout.Label("Time.time:");
            GUILayout.Label("Time.deltaTime:");
            GUILayout.Label("Time.unscaledTime:");
            GUILayout.Label("Time.unscaledDeltaTime:");
            GUILayout.Label("Time.fixedTime:");
            GUILayout.Label("Time.fixedDeltaTime:");
            GUILayout.Label("Time.fixedUnscaledTime:");
            GUILayout.Label("Time.fixedUnscaledDeltaTime:");

            GUILayout.EndVertical();

            GUILayout.BeginVertical("Box");
            GUILayout.Label(DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss"));
            GUILayout.Label(Time.frameCount.ToString());
            GUILayout.Label(Time.realtimeSinceStartup.ToString("F"));
            GUILayout.Label(Time.timeScale.ToString("F"));
            GUILayout.Label(Time.time.ToString("F"));
            GUILayout.Label(Time.deltaTime.ToString("F"));
            GUILayout.Label(Time.unscaledTime.ToString("F"));
            GUILayout.Label(Time.unscaledDeltaTime.ToString("F"));
            GUILayout.Label(Time.fixedTime.ToString("F"));
            GUILayout.Label(Time.fixedDeltaTime.ToString("F"));
            GUILayout.Label(Time.fixedUnscaledTime.ToString("F"));
            GUILayout.Label(Time.fixedUnscaledDeltaTime.ToString("F"));
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("0.0x", GUILayout.Height(30)))
            {
                Time.timeScale = 0.0f;
            }

            if (GUILayout.Button("0.1x", GUILayout.Height(30)))
            {
                Time.timeScale = 0.1f;
            }

            if (GUILayout.Button("0.2x", GUILayout.Height(30)))
            {
                Time.timeScale = 0.2f;
            }

            if (GUILayout.Button("0.5x", GUILayout.Height(30)))
            {
                Time.timeScale = 0.5f;
            }

            if (GUILayout.Button("1x", GUILayout.Height(30)))
            {
                Time.timeScale = 1f;
            }

            if (GUILayout.Button("2x", GUILayout.Height(30)))
            {
                Time.timeScale = 2f;
            }

            if (GUILayout.Button("5x ", GUILayout.Height(30)))
            {
                Time.timeScale = 5f;
            }

            if (GUILayout.Button("10x", GUILayout.Height(30)))
            {
                Time.timeScale = 10f;
            }

            GUILayout.EndHorizontal();
        }
    }

    public partial class NetworkDebugger
    {
        private readonly Dictionary<int, float> minMemory = new Dictionary<int, float>();
        private readonly Dictionary<int, float> maxMemory = new Dictionary<int, float>();

        private void MemoryWindow()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(" 内存信息", GUILayout.Height(25));
            GUILayout.EndHorizontal();

            var pair1 = Calculate(1, Profiler.GetTotalReservedMemoryLong());
            var pair2 = Calculate(2, Profiler.GetTotalAllocatedMemoryLong());
            var pair3 = Calculate(3, Profiler.GetTotalUnusedReservedMemoryLong());
            var pair4 = Calculate(4, Profiler.GetAllocatedMemoryForGraphicsDriver());
            var pair5 = Calculate(5, Profiler.GetMonoHeapSizeLong());
            var pair6 = Calculate(6, Profiler.GetMonoUsedSizeLong());

            screenView = GUILayout.BeginScrollView(screenView, "Box");

            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical("Box", GUILayout.Width(300));
            GUILayout.Label("已保留的内存总量: " + pair1.Item1);
            GUILayout.Label("已分配的内存总量: " + pair2.Item1);
            GUILayout.Label("未使用的内存总量: " + pair3.Item1);
            GUILayout.Label("图形资源使用内存: " + pair4.Item1);
            GUILayout.Label("Mono分配的托管堆: " + pair5.Item1);
            GUILayout.Label("Mono使用的托管堆: " + pair6.Item1);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Box");
            GUILayout.Label(pair1.Item2);
            GUILayout.Label(pair2.Item2);
            GUILayout.Label(pair3.Item2);
            GUILayout.Label(pair4.Item2);
            GUILayout.Label(pair5.Item2);
            GUILayout.Label(pair6.Item2);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("垃圾回收", GUILayout.Height(30)))
            {
                GC.Collect();
            }

            GUILayout.EndHorizontal();
        }

        private (string, string) Calculate(int key, long memory)
        {
            var value = memory / 1024F / 1024F;
            if (!minMemory.TryGetValue(key, out var minValue))
            {
                minValue = 1024 * 1024;
                minMemory.Add(key, minValue);
            }

            if (!maxMemory.TryGetValue(key, out var maxValue))
            {
                maxValue = 0;
                maxMemory.Add(key, maxValue);
            }

            if (value > maxValue)
            {
                maxMemory[key] = value;
            }
            else if (value < minValue)
            {
                minMemory[key] = value;
            }

            var item1 = "{0:F2} MB".Format(value);
            var item2 = "[ 最小值: {0:F2} MB  \t最大值: {1:F2} MB]".Format(minMemory[key], maxMemory[key]);
            return (item1, item2);
        }
    }

    public partial class NetworkDebugger
    {
        private void SystemWindow()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(" 系统信息", GUILayout.Height(25));
            GUILayout.EndHorizontal();

            screenView = GUILayout.BeginScrollView(screenView, "Box");
            GUILayout.Label("操作系统: " + SystemInfo.operatingSystem);
            GUILayout.Label("系统内存: " + SystemInfo.systemMemorySize + "MB");
            GUILayout.Label("处理器: " + SystemInfo.processorType);
            GUILayout.Label("处理器数量: " + SystemInfo.processorCount);
            GUILayout.Label("显卡名称: " + SystemInfo.graphicsDeviceName);
            GUILayout.Label("显卡类型: " + SystemInfo.graphicsDeviceType);
            GUILayout.Label("显卡内存: " + SystemInfo.graphicsMemorySize + "MB");
            GUILayout.Label("显卡标识: " + SystemInfo.graphicsDeviceID);
            GUILayout.Label("显卡供应商: " + SystemInfo.graphicsDeviceVendor);
            GUILayout.Label("显卡供应商标识: " + SystemInfo.graphicsDeviceVendorID);
            GUILayout.Label("设备模式: " + SystemInfo.deviceModel);
            GUILayout.Label("设备名称: " + SystemInfo.deviceName);
            GUILayout.Label("设备类型: " + SystemInfo.deviceType);
            GUILayout.Label("设备唯一标识符: " + SystemInfo.deviceUniqueIdentifier);
            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("退出游戏", GUILayout.Height(30)))
            {
                Application.Quit();
            }

            GUILayout.EndHorizontal();
        }
    }

    public partial class NetworkDebugger
    {
        private void ScreenWindow()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(" 屏幕信息", GUILayout.Height(25));
            GUILayout.EndHorizontal();

            screenView = GUILayout.BeginScrollView(screenView, "Box");
            GUILayout.Label("像素密度: " + Screen.dpi);
            GUILayout.Label("启用全屏: " + Screen.fullScreen);
            GUILayout.Label("屏幕模式: " + Screen.fullScreenMode);
            GUILayout.Label("程序分辨率: " + "{0} x {1}".Format(Screen.width, Screen.height));
            GUILayout.Label("设备分辨率: " + Screen.currentResolution);
            GUILayout.Label("显示区域: " + Screen.safeArea);
            GUILayout.Label("质量等级: " + QualitySettings.names[QualitySettings.GetQualityLevel()]);
            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            for (var i = 0; i < QualitySettings.names.Length; i++)
            {
                var label = QualitySettings.names[i];
                if (GUILayout.Button(label, GUILayout.Height(30)))
                {
                    QualitySettings.SetQualityLevel(i);
                }
            }

            GUILayout.EndHorizontal();
        }
    }

    public partial class NetworkDebugger
    {
        private void ProjectWindow()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(" 环境配置", GUILayout.Height(25));
            GUILayout.EndHorizontal();

            screenView = GUILayout.BeginScrollView(screenView, "Box");
            GUILayout.Label("项目名称: " + Application.productName);
            GUILayout.Label("项目版本: " + Application.version);
            GUILayout.Label("运行平台: " + Application.platform);
            GUILayout.Label("项目标识: " + Application.identifier);
            GUILayout.Label("公司名称: " + Application.companyName);
            GUILayout.Label("Unity版本: " + Application.unityVersion);
            GUILayout.Label("Unity专业版: " + Application.HasProLicense());
            var message = "";
            switch (Application.internetReachability)
            {
                case NetworkReachability.NotReachable:
                    message = "当前设备无法访问互联网";
                    break;
                case NetworkReachability.ReachableViaCarrierDataNetwork:
                    message = "当前设备通过 蜂窝移动网络 连接到互联网";
                    break;
                case NetworkReachability.ReachableViaLocalAreaNetwork:
                    message = "当前设备通过 WiFi 或有线网络连接到互联网";
                    break;
            }

            GUILayout.Label("网络状态: " + message);
            GUILayout.Label("项目路径: " + Application.dataPath);
            GUILayout.Label("存储路径: " + Application.persistentDataPath);
            GUILayout.Label("流动资源路径: " + Application.streamingAssetsPath);
            GUILayout.Label("临时缓存路径: " + Application.temporaryCachePath);

            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("0.5x", GUILayout.Height(30)))
            {
                screenRate = new Vector2(3200, 1800);
            }

            if (GUILayout.Button("1.0x", GUILayout.Height(30)))
            {
                screenRate = new Vector2(2560, 1440);
            }

            if (GUILayout.Button("1.5x", GUILayout.Height(30)))
            {
                screenRate = new Vector2(1920, 1080);
            }

            if (GUILayout.Button("2.0x", GUILayout.Height(30)))
            {
                screenRate = new Vector2(1280, 720);
            }

            GUILayout.EndHorizontal();
        }
    }
}