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
    [DefaultExecutionOrder(-100)]
    public sealed class NetworkDebugger : Debugger
    {
        private Font font;
        private bool maximized;
        private float frameData;
        private double frameTime;
        private string address;

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
            address = Service.Host.Ip();
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

        private void OnEnable()
        {
            Application.logMessageReceived += LogMessageReceived;
        }

        private void OnDisable()
        {
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

            GUI.contentColor = window == Window.控制台 ? Color.white : Color.gray;
            if (GUILayout.Button(Window.控制台.ToString(), GUILayout.Height(30)))
            {
                window = Window.控制台;
            }

            GUI.contentColor = window == Window.引用池 ? Color.white : Color.gray;
            if (GUILayout.Button(Window.引用池.ToString(), GUILayout.Height(30)))
            {
                window = Window.引用池;
            }

            GUI.contentColor = window == Window.对象池 ? Color.white : Color.gray;
            if (GUILayout.Button(Window.对象池.ToString(), GUILayout.Height(30)))
            {
                window = Window.对象池;
            }

            GUI.contentColor = window == Window.事件 ? Color.white : Color.gray;
            if (GUILayout.Button(Window.事件.ToString(), GUILayout.Height(30)))
            {
                window = Window.事件;
            }

            GUI.contentColor = window == Window.网络 ? Color.white : Color.gray;
            if (GUILayout.Button(Window.网络.ToString(), GUILayout.Height(30)))
            {
                window = Window.网络;
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            GUI.contentColor = window == Window.场景 ? Color.white : Color.gray;
            if (GUILayout.Button(Window.场景.ToString(), GUILayout.Height(30)))
            {
                UpdateGameObject();
                UpdateComponent();
                window = Window.场景;
            }

            GUI.contentColor = window == Window.内存 ? Color.white : Color.gray;
            if (GUILayout.Button(Window.内存.ToString(), GUILayout.Height(30)))
            {
                window = Window.内存;
            }

            GUI.contentColor = window == Window.时间 ? Color.white : Color.gray;
            if (GUILayout.Button(Window.时间.ToString(), GUILayout.Height(30)))
            {
                window = Window.时间;
            }

            GUI.contentColor = window == Window.系统 ? Color.white : Color.gray;
            if (GUILayout.Button(Window.系统.ToString(), GUILayout.Height(30)))
            {
                window = Window.系统;
            }

            GUI.contentColor = window == Window.程序 ? Color.white : Color.gray;
            if (GUILayout.Button(Window.程序.ToString(), GUILayout.Height(30)))
            {
                window = Window.程序;
            }

            GUILayout.EndHorizontal();

            GUI.contentColor = Color.white;
            switch (window)
            {
                case Window.控制台:
                    ConsoleWindow();
                    break;
                case Window.引用池:
                    HeapWindow();
                    break;
                case Window.对象池:
                    PoolWindow();
                    break;
                case Window.事件:
                    EventWindow();
                    break;
                case Window.网络:
                    NetworkWindow();
                    break;
                case Window.场景:
                    SceneWindow();
                    break;
                case Window.系统:
                    SystemWindow();
                    break;
                case Window.程序:
                    ProjectWindow();
                    break;
                case Window.内存:
                    MemoryWindow();
                    break;
                case Window.时间:
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
            GUILayout.Label("场景对象 [{0}]".Format(gameObjects.Count), "Button", GUILayout.Width((screenWidth - 20) / 2), GUILayout.Height(30));
            if (GUILayout.Button("刷新", GUILayout.Height(30)))
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

        private readonly Dictionary<string, List<Pool>> poolData = new Dictionary<string, List<Pool>>();

        private void HeapWindow()
        {
            Draw(HeapManager.poolData.Values, "引用池", "未使用\t使用中\t使用次数\t释放次数");
        }

        private void PoolWindow()
        {
            Draw(GlobalManager.poolData.Values, "对象池", "未激活\t激活中\t出队次数\t入队次数");
        }

        private void EventWindow()
        {
            Draw(EventManager.poolData.Values, "事件池", "触发数\t事件数\t添加次数\t移除次数");
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

        private IList<Pool> sendList = new List<Pool>();
        private IList<Pool> receiveList = new List<Pool>();
        private float duration;

        private void NetworkWindow()
        {
            GUILayout.BeginHorizontal();
            string peer;
            ushort port;
            if (NetworkManager.Transport)
            {
                peer = NetworkManager.Transport.address;
                if (peer == "localhost")
                {
                    peer = address;
                }

                port = NetworkManager.Transport.port;
            }
            else
            {
                peer = "127.0.0.1";
                port = 20974;
            }

            GUILayout.Label("{0} : {1}".Format(peer, port), "Button", GUILayout.Width((screenWidth - 20) / 2), GUILayout.Height(30));
            var ping = NetworkManager.isClient ? "Ping: {0} ms".Format(Math.Min((int)(NetworkManager.Client.pingTime * 1000), 999)) : "Client is not active!";
            GUILayout.Label(ping, "Button", GUILayout.Height(30));

            GUILayout.EndHorizontal();

            screenView = GUILayout.BeginScrollView(screenView, "Box");
            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical("Box", GUILayout.Width((screenWidth - 28) / 2));
            server.OnGUI();
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Box");
            client.OnGUI();
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            if (duration < Time.unscaledTime)
            {
                duration = Time.unscaledTime + 1;
                sendList = itemSend.Reference();
                receiveList = itemReceive.Reference();
                Reset();
            }

            NetworkMessage(sendList, "发送队列", "每秒发送\t每秒发送\t全局发送\t全局发送");
            NetworkMessage(receiveList, "接收队列", "每秒接收\t每秒接收\t全局接收\t全局接收");


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
                    var result = data.Release.ToString().Align(10);
                    result += PrettyBytes(data.Acquire).Align(10);
                    result += data.Dequeue.ToString().Align(10);
                    result += PrettyBytes(data.Enqueue).Align(10);
                    GUILayout.Label(result, GUILayout.Height(20));
                }

                GUILayout.EndVertical();

                GUILayout.EndHorizontal();
            }
        }

        private void TimeWindow()
        {
            screenView = GUILayout.BeginScrollView(screenView, "Box");
            GUILayout.BeginVertical();

            GUILayout.Label("当前日期:".Align(20) + DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss"));
            GUILayout.Label("游戏总帧:".Align(20) + Time.frameCount);
            GUILayout.Label("时间总长:".Align(20) + Time.realtimeSinceStartup.ToString("F"));
            GUILayout.Label("时间流速:".Align(20) + Time.timeScale.ToString("F"));
            GUILayout.Label("游戏时间:".Align(20) + Time.time.ToString("F"));
            GUILayout.Label("游戏间隔:".Align(20) + Time.deltaTime.ToString("F"));
            GUILayout.Label("游戏时间(U):".Align(20) + Time.unscaledTime.ToString("F"));
            GUILayout.Label("游戏间隔(U):".Align(20) + Time.unscaledDeltaTime.ToString("F"));
            GUILayout.Label("物理时间:".Align(20) + Time.fixedTime.ToString("F"));
            GUILayout.Label("物理间隔:".Align(20) + Time.fixedDeltaTime.ToString("F"));
            GUILayout.Label("物理时间(U):".Align(20) + Time.fixedUnscaledTime.ToString("F"));
            GUILayout.Label("物理间隔(U):".Align(20) + Time.fixedUnscaledDeltaTime.ToString("F"));

            GUILayout.EndVertical();

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

        private readonly Dictionary<int, float> minMemory = new Dictionary<int, float>();
        private readonly Dictionary<int, float> maxMemory = new Dictionary<int, float>();

        private void MemoryWindow()
        {
            screenView = GUILayout.BeginScrollView(screenView, "Box");

            GUILayout.BeginVertical();
            var pair1 = Calculate(1, Profiler.GetTotalReservedMemoryLong());
            var pair2 = Calculate(2, Profiler.GetTotalAllocatedMemoryLong());
            var pair3 = Calculate(3, Profiler.GetTotalUnusedReservedMemoryLong());
            var pair4 = Calculate(4, Profiler.GetAllocatedMemoryForGraphicsDriver());
            var pair5 = Calculate(5, Profiler.GetMonoHeapSizeLong());
            var pair6 = Calculate(6, Profiler.GetMonoUsedSizeLong());
            GUILayout.Label("已保留内存: {0}".Format(pair1.A).Align(26) + pair1.B);
            GUILayout.Label("已分配内存: {0}".Format(pair2.A).Align(26) + pair2.B);
            GUILayout.Label("未使用内存: {0}".Format(pair3.A).Align(26) + pair3.B);
            GUILayout.Label("图形驱动器: {0}".Format(pair4.A).Align(26) + pair4.B);
            GUILayout.Label("分配托管堆: {0}".Format(pair5.A).Align(26) + pair5.B);
            GUILayout.Label("使用托管堆: {0}".Format(pair6.A).Align(26) + pair6.B);
            GUILayout.EndVertical();

            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("垃圾回收", GUILayout.Height(30)))
            {
                GC.Collect();
            }

            GUILayout.EndHorizontal();
        }

        private (string A, string B) Calculate(int key, long memory)
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

            return ("{0:F2} MB".Format(value), "[ 最小值: {0:F2} MB".Format(minMemory[key]).Align(24) + "最大值: {0:F2} MB ]".Format(maxMemory[key]));
        }

        private void SystemWindow()
        {
            screenView = GUILayout.BeginScrollView(screenView, "Box");
            GUILayout.Label("设备标识: ".Align(20) + SystemInfo.deviceUniqueIdentifier);
            GUILayout.Label("操作系统: ".Align(20) + SystemInfo.operatingSystem);
            GUILayout.Label("设备模式: ".Align(20) + SystemInfo.deviceModel);
            GUILayout.Label("设备名称: ".Align(20) + SystemInfo.deviceName);
            GUILayout.Label("设备类型: ".Align(20) + SystemInfo.deviceType);
            GUILayout.Label("设备内存: ".Align(20) + SystemInfo.systemMemorySize + "MB");
            GUILayout.Label("显卡标识: ".Align(20) + SystemInfo.graphicsDeviceID);
            GUILayout.Label("显卡名称: ".Align(20) + SystemInfo.graphicsDeviceName);
            GUILayout.Label("显卡类型: ".Align(20) + SystemInfo.graphicsDeviceType);
            GUILayout.Label("显卡内存: ".Align(20) + SystemInfo.graphicsMemorySize + "MB");
            GUILayout.Label("处理器: ".Align(20) + SystemInfo.processorType);
            GUILayout.Label("处理器数量: ".Align(20) + SystemInfo.processorCount);
            GUILayout.Label("供应商: ".Align(20) + SystemInfo.graphicsDeviceVendor);
            GUILayout.Label("供应商标识: ".Align(20) + SystemInfo.graphicsDeviceVendorID);
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

        private void ProjectWindow()
        {
            screenView = GUILayout.BeginScrollView(screenView, "Box");
            GUILayout.Label("设备分辨率: ".Align(20) + Screen.currentResolution);
            GUILayout.Label("程序分辨率: ".Align(20) + "{0} x {1}".Format(Screen.width, Screen.height));
            GUILayout.Label("屏幕模式: ".Align(20) + Screen.fullScreenMode);
            GUILayout.Label("图形质量: ".Align(20) + QualitySettings.names[QualitySettings.GetQualityLevel()]);
            GUILayout.Label("研发版本: ".Align(20) + Application.unityVersion);
            GUILayout.Label("项目名称: ".Align(20) + Application.productName);
            GUILayout.Label("项目版本: ".Align(20) + Application.version);
            GUILayout.Label("运行平台: ".Align(20) + Application.platform);
            GUILayout.Label("公司名称: ".Align(20) + Application.companyName);
            GUILayout.Label("项目标识: ".Align(20) + Application.identifier);
            GUILayout.Label("网络状态: ".Align(20) + (int)Application.internetReachability switch
            {
                1 => "当前设备通过 蜂窝移动网络 连接到互联网",
                2 => "当前设备通过 WiFi 或有线网络连接到互联网",
                _ => "当前设备无法访问互联网",
            });
            GUILayout.Label("项目路径: ".Align(20) + Application.dataPath);
            GUILayout.Label("存储路径: ".Align(20) + Application.persistentDataPath);
            GUILayout.Label("流动资源路径: ".Align(20) + Application.streamingAssetsPath);
            GUILayout.Label("临时缓存路径: ".Align(20) + Application.temporaryCachePath);

            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("退出游戏", GUILayout.Height(30)))
            {
                Application.Quit();
            }

            GUILayout.EndHorizontal();
        }
    }

    public abstract class Debugger : MonoBehaviour
    {
        protected readonly Messages client = new Messages("服务器");
        protected readonly Messages server = new Messages("客户端");
        protected static Items itemSend;
        protected static Items itemReceive;
        private double waitTime;

        protected virtual void Awake()
        {
            itemSend = new Items();
            itemReceive = new Items();
        }

        protected virtual void Reset()
        {
            itemSend?.Reset();
            itemReceive?.Reset();
        }

        private void OnDestroy()
        {
            itemSend.Clear();
            itemReceive.Clear();
            itemSend = null;
            itemReceive = null;
        }

        protected virtual void Start()
        {
            if (NetworkManager.Transport)
            {
                NetworkManager.Transport.OnClientSend -= OnClientSend;
                NetworkManager.Transport.OnServerSend -= OnServerSend;
                NetworkManager.Transport.OnClientReceive -= OnClientReceive;
                NetworkManager.Transport.OnServerReceive -= OnServerReceive;
                NetworkManager.Transport.OnClientSend += OnClientSend;
                NetworkManager.Transport.OnServerSend += OnServerSend;
                NetworkManager.Transport.OnClientReceive += OnClientReceive;
                NetworkManager.Transport.OnServerReceive += OnServerReceive;
            }
        }

        protected virtual void Update()
        {
            if (waitTime < Time.realtimeSinceStartup)
            {
                if (NetworkManager.isClient)
                {
                    client.Update();
                }

                if (NetworkManager.isServer)
                {
                    server.Update();
                }

                waitTime = Time.realtimeSinceStartup + 1;
            }
        }

        private void OnClientSend(ArraySegment<byte> segment, int channel)
        {
            client.Send.count++;
            client.Send.bytes += segment.Count;
        }

        private void OnClientReceive(ArraySegment<byte> segment, int channel)
        {
            client.Receive.count++;
            client.Receive.bytes += segment.Count;
        }

        private void OnServerSend(int clientId, ArraySegment<byte> segment, int channel)
        {
            server.Send.count++;
            server.Send.bytes += segment.Count;
        }

        private void OnServerReceive(int clientId, ArraySegment<byte> segment, int channel)
        {
            server.Receive.count++;
            server.Receive.bytes += segment.Count;
        }

        internal static void OnSend<T>(T message, int bytes) where T : struct, IMessage
        {
            itemSend?.Record(message, Service.Bit.Invoke((uint)bytes) + bytes);
        }

        internal static void OnReceive<T>(T message, int bytes) where T : struct, IMessage
        {
            itemReceive?.Record(message, Service.Bit.Invoke((uint)bytes + 2) + bytes + 2);
        }

        internal static string PrettyBytes(long bytes)
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


        protected class Messages
        {
            private readonly string Name;
            public readonly Message Send = new Message();
            public readonly Message Receive = new Message();

            public Messages(string name)
            {
                Name = name;
            }

            public void Update()
            {
                Send.Update();
                Receive.Update();
            }

            public void OnGUI()
            {
                Send.OnGUI(Name + "发送");
                Receive.OnGUI(Name + "接收");
            }

            public class Message
            {
                private int Count;
                private int Bytes;
                public int count;
                public int bytes;

                public void Update()
                {
                    Count = count;
                    Bytes = bytes;
                    count = 0;
                    bytes = 0;
                }

                public void OnGUI(string value)
                {
                    GUILayout.Label("{0}数量:\t\t{1}/s".Format(value, Count));
                    GUILayout.Label("{0}大小:\t\t{1}/s".Format(value, PrettyBytes(Bytes)));
                }
            }
        }

        protected class Items
        {
            private readonly Dictionary<Type, Item> messages = new Dictionary<Type, Item>();
            private readonly Dictionary<ushort, Item> function = new Dictionary<ushort, Item>();

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

                        item.Path = "{0}.{1}".Format(data.Method.DeclaringType!.Name, name);
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

            internal IList<Pool> Reference()
            {
                var pools = messages.Values.Select(item => new Pool(item)).ToList();
                pools.AddRange(function.Values.Select(item => new Pool(item)));
                return pools;
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
}