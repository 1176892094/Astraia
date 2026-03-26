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
using UnityEngine;
using UnityEngine.Profiling;
using Astraia.Core;

namespace Astraia.Net
{
    [DefaultExecutionOrder(-100)]
    public sealed class NetworkDebugger : MonoBehaviour
    {
        private static readonly Dictionary<Window, IWindow> Windows = new Dictionary<Window, IWindow>();
        private static readonly Dictionary<LogType, Log> Logs = new Dictionary<LogType, Log>();
        private static readonly List<LogData> Queue = new List<LogData>();
        private static Font Font;
        private static Rect Rect = new Rect(0, 0, 100, 60);
        private static string Host;
        private static Vector2 Size = new Vector2(2560, 1440);
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

        private void Awake()
        {
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

            Host = Astraia.Host.Ip();
            Font = Resources.Load<Font>("Sarasa Mono SC");
        }

        private void Start()
        {
            Debugger.Enable();
        }

        private void OnEnable()
        {
            Application.logMessageReceived += LogReceive;
        }

        private void OnDisable()
        {
            Application.logMessageReceived -= LogReceive;
        }

        private void OnDestroy()
        {
            Debugger.Clear();
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

        private static void Rebuild(Dictionary<string, List<IPool>> pools, ICollection<IPool> items, Pools message, bool dispose)
        {
            if (dispose)
            {
                foreach (var pool in pools)
                {
                    pool.Value.Clear();
                }
            }

            foreach (var item in items)
            {
                var assembly = "{0} - {1}".Format(item.Type.Assembly.GetName().Name, message);
                if (!pools.TryGetValue(assembly, out var pool))
                {
                    pool = new List<IPool>();
                    pools.Add(assembly, pool);
                }

                pool.Add(new Debugger.Pool(item));
            }
        }

        private static void Repaint(Dictionary<string, List<IPool>> poolData, Pools message)
        {
            foreach (var pool in poolData)
            {
                pool.Value.Sort(Comparison);
                GUILayout.BeginVertical("Box");
                var reason = message switch
                {
                    Pools.对象池 => "未激活\t激活中\t出队次数\t入队次数",
                    Pools.引用池 => "未使用\t使用中\t使用次数\t释放次数",
                    Pools.事件池 => "触发数\t事件数\t添加次数\t移除次数",
                    Pools.发送队列 => "每秒接收\t每秒接收\t累计接收\t累计接收",
                    Pools.接收队列 => "每秒发送\t每秒发送\t累计发送\t累计发送",
                    _ => string.Empty
                };

                GUILayout.Label(pool.Key.Align(50) + reason, GUILayout.Height(20));
                foreach (var data in pool.Value)
                {
                    var result = string.Empty;
                    if (!string.IsNullOrEmpty(data.Path))
                    {
                        result += "{0} - {1}".Format(GetName(data.Type), data.Path);
                    }
                    else
                    {
                        result += data.Type.Name;
                    }

                    result = result.Align(50, "...  ");
                    if (message is Pools.发送队列 or Pools.接收队列)
                    {
                        GUILayout.Label(result + data, GUILayout.Height(20));
                    }
                    else
                    {
                        result += data.Release.ToString().Align(10);
                        result += data.Acquire.ToString().Align(10);
                        result += data.Dequeue.ToString().Align(10);
                        result += data.Enqueue.ToString().Align(10);
                        GUILayout.Label(result, GUILayout.Height(20));
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

        [Serializable]
        private class 控制台 : IWindow
        {
            private int index = -1;

            public 控制台()
            {
                Logs[LogType.Log] = new Log(Color.white);
                Logs[LogType.Error] = new Log(Color.red);
                Logs[LogType.Assert] = new Log(Color.green);
                Logs[LogType.Warning] = new Log(Color.yellow);
                Logs[LogType.Exception] = new Log(Color.magenta);
            }

            public void Execute(bool modified)
            {
                GUILayout.BeginHorizontal();
                foreach (var key in Logs.Keys)
                {
                    var value = Logs[key];
                    GUI.contentColor = value.State ? Color.white : Color.gray;
                    if (GUILayout.Button("{0} [{1}]".Format(key, value.Count), GUILayout.Height(30)))
                    {
                        value.State = !value.State;
                    }
                }

                GUILayout.EndHorizontal();

                ScreenView = GUILayout.BeginScrollView(ScreenView, "Box", GUILayout.Height(ScreenY * 0.4f));
                for (var i = 0; i < Queue.Count; i++)
                {
                    if (Logs.TryGetValue(Queue[i].LogType, out var data) && data.State)
                    {
                        GUILayout.BeginHorizontal();
                        GUI.contentColor = data.Color;
                        if (GUILayout.Toggle(index == i, Queue[i].ToString(), GUILayout.Height(20)))
                        {
                            index = i;
                        }

                        GUI.contentColor = Color.white;
                        GUILayout.EndHorizontal();
                    }
                }

                GUILayout.EndScrollView();

                SecondView = GUILayout.BeginScrollView(SecondView, "Box");
                if (index != -1)
                {
                    GUILayout.Label("{0}\n\n{1}".Format(Queue[index].Message, Queue[index].StackTrace));
                }

                GUILayout.EndScrollView();
            }
        }

        [Serializable]
        private class 引用池 : IWindow
        {
            private Dictionary<string, List<IPool>> poolData = new Dictionary<string, List<IPool>>();

            public void Execute(bool modified)
            {
                Rebuild(poolData, HeapManager.poolData.Values, Pools.引用池, true);
                ScreenView = GUILayout.BeginScrollView(ScreenView, "Box");
                Repaint(poolData, Pools.引用池);
                GUILayout.EndScrollView();
            }
        }

        [Serializable]
        private class 对象池 : IWindow
        {
            private Dictionary<string, List<IPool>> poolData = new Dictionary<string, List<IPool>>();

            public void Execute(bool modified)
            {
                Rebuild(poolData, GlobalManager.poolData.Values, Pools.对象池, true);
                ScreenView = GUILayout.BeginScrollView(ScreenView, "Box");
                Repaint(poolData, Pools.对象池);
                GUILayout.EndScrollView();
            }
        }

        [Serializable]
        private class 事件 : IWindow
        {
            private Dictionary<string, List<IPool>> poolData = new Dictionary<string, List<IPool>>();

            public void Execute(bool modified)
            {
                Rebuild(poolData, EventManager.poolData.Values, Pools.事件池, true);
                ScreenView = GUILayout.BeginScrollView(ScreenView, "Box");
                Repaint(poolData, Pools.事件池);
                GUILayout.EndScrollView();
            }
        }

        [Serializable]
        private class 网络 : IWindow
        {
            private Dictionary<string, List<IPool>> itemSend = new Dictionary<string, List<IPool>>();
            private Dictionary<string, List<IPool>> itemData = new Dictionary<string, List<IPool>>();
            private float waitTime;
            private IPool clientSend;
            private IPool serverSend;
            private IPool clientData;
            private IPool serverData;

            public void Execute(bool modified)
            {
                GUILayout.BeginHorizontal();
                var peer = NetworkManager.Transport ? NetworkManager.Transport.address == "localhost" ? Host : NetworkManager.Transport.address : "127.0.0.1";
                var port = NetworkManager.Transport ? NetworkManager.Transport.port : (ushort)20974;
                var ping = NetworkManager.isClient ? "Ping: {0} ms".Format(Math.Min((int)(NetworkManager.Client.pingTime * 1000), 999)) : "Client is not active!";
                GUILayout.Label("{0} : {1}".Format(peer, port), "Button", GUILayout.Width((ScreenX - 20) / 2), GUILayout.Height(30));
                GUILayout.Label(ping, "Button", GUILayout.Width((ScreenX - 20) / 2), GUILayout.Height(30));
                GUILayout.EndHorizontal();

                if (waitTime < Time.realtimeSinceStartup)
                {
                    waitTime = Time.realtimeSinceStartup + 1;
                    clientSend = new Debugger.Pool(Debugger.clientSend);
                    serverSend = new Debugger.Pool(Debugger.serverSend);
                    clientData = new Debugger.Pool(Debugger.clientData);
                    serverData = new Debugger.Pool(Debugger.serverData);
                    Rebuild(itemSend, Debugger.itemSend.messages.Values, Pools.发送队列, true);
                    Rebuild(itemSend, Debugger.itemSend.function.Values, Pools.发送队列, false);
                    Rebuild(itemData, Debugger.itemData.messages.Values, Pools.接收队列, true);
                    Rebuild(itemData, Debugger.itemData.function.Values, Pools.接收队列, false);
                    Debugger.Reset();
                }

                ScreenView = GUILayout.BeginScrollView(ScreenView, "Box");
                GUILayout.BeginVertical("Box");
                GUILayout.Label("Astraia.Net - 网络信息".Align(50) + "每秒数量\t每秒大小\t累计数量\t累计大小", GUILayout.Height(20));
                GUILayout.Label("NetworkSend".Align(50) + (NetworkManager.isServer ? serverSend : clientSend), GUILayout.Height(20));
                GUILayout.Label("NetworkReceive".Align(50) + (NetworkManager.isServer ? serverData : clientData), GUILayout.Height(20));
                GUILayout.EndVertical();
                Repaint(itemSend, Pools.发送队列);
                Repaint(itemData, Pools.接收队列);
                GUILayout.EndScrollView();

                GUILayout.BeginHorizontal();
                DrawButton();
                GUILayout.EndHorizontal();
            }

            private void DrawButton()
            {
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
            }
        }

        [Serializable]
        private class 场景 : IWindow
        {
            private readonly List<Type> cachedTypes = new List<Type>();
            private bool cachedInput;

            private int componentIndex = -1;
            private string componentName = string.Empty;
            private readonly List<Component> components = new List<Component>();

            private int transformIndex = -1;
            private string transformName = string.Empty;
            private readonly List<Transform> transforms = new List<Transform>();

            public 场景()
            {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (!assembly.GetName().Name.Contains("UnityEngine"))
                    {
                        foreach (var result in assembly.GetTypes())
                        {
                            if (!result.IsAbstract && !result.IsGenericType && result.IsSubclassOf(typeof(MonoBehaviour)))
                            {
                                cachedTypes.Add(result);
                            }
                        }
                    }
                }
            }

            public void Execute(bool modified)
            {
                if (modified)
                {
                    UpdateTransform();
                    UpdateComponent();
                }

                GUILayout.BeginHorizontal();
                GUILayout.Label("场景对象 [{0}]".Format(transforms.Count), "Button", GUILayout.Width((ScreenX - 20) / 2), GUILayout.Height(30));
                if (GUILayout.Button("刷新", GUILayout.Height(30)))
                {
                    UpdateTransform();
                    UpdateComponent();
                }

                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical("Box", GUILayout.Width((ScreenX - 20) / 2));
                ShowTransform();
                GUILayout.EndVertical();
                GUILayout.BeginVertical("Box");
                ShowComponent();
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }

            public void UpdateTransform()
            {
                transforms.Clear();
#if UNITY_6000_4_OR_NEWER
                var copies = FindObjectsByType<Transform>();
#else
                var copies = FindObjectsByType<Transform>(FindObjectsSortMode.None);
#endif
                foreach (var transform in copies)
                {
                    transforms.Add(transform);
                }

                transformIndex = -1;
                transforms.Sort(Comparison);
            }

            private static int Comparison(Transform a, Transform b)
            {
                return string.Compare(a.name, b.name, StringComparison.Ordinal);
            }

            public void UpdateComponent()
            {
                components.Clear();
                if (transformIndex != -1 && transformIndex < transforms.Count)
                {
                    var copies = transforms[transformIndex].GetComponents<Component>();
                    foreach (var component in copies)
                    {
                        components.Add(component);
                    }
                }

                componentIndex = -1;
                cachedInput = false;
            }

            private void ShowTransform()
            {
                GUILayout.BeginHorizontal();
                transformName = GUILayout.TextField(transformName, GUILayout.Height(25));
                GUILayout.EndHorizontal();

                ScreenView = GUILayout.BeginScrollView(ScreenView);
                for (var i = 0; i < transforms.Count; i++)
                {
                    var obj = transforms[i].gameObject;
                    if (obj && obj.name.Contains(transformName))
                    {
                        GUILayout.BeginHorizontal();
                        GUI.contentColor = obj.activeInHierarchy ? Color.white : Color.gray;
                        var selected = transformIndex == i;
                        if (GUILayout.Toggle(selected, " " + obj.name) != selected)
                        {
                            transformIndex = transformIndex != i ? i : -1;
                            UpdateComponent();
                        }

                        GUILayout.EndHorizontal();

                        if (transformIndex == i)
                        {
                            GUILayout.BeginVertical("Box");

                            GUILayout.BeginHorizontal();
                            GUILayout.Label("Tag: " + obj.tag, GUILayout.Width(160));
                            GUILayout.Label("Layer: " + LayerMask.LayerToName(obj.layer));
                            GUILayout.EndHorizontal();

                            GUILayout.EndVertical();
                        }
                    }
                }

                GUILayout.EndScrollView();
            }

            private void ShowComponent()
            {
                if (transformIndex != -1)
                {
                    GUILayout.BeginHorizontal();
                    if (cachedInput)
                    {
                        componentName = GUILayout.TextField(componentName, GUILayout.Height(25));
                    }
                    else
                    {
                        if (componentIndex != -1 && componentIndex < components.Count && components[componentIndex])
                        {
                            if (GUILayout.Button("移除组件", GUILayout.Height(25)))
                            {
                                var component = components[componentIndex];
                                if (component is NetworkDebugger or Transform)
                                {
                                    Astraia.Log.Warn("无法销毁组件: " + component.GetType().Name);
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
                            if (GUILayout.Button("增加组件", GUILayout.Height(25)))
                            {
                                cachedInput = !cachedInput;
                            }
                        }
                    }

                    GUILayout.EndHorizontal();
                }

                SecondView = GUILayout.BeginScrollView(SecondView);

                if (transformIndex != -1)
                {
                    if (cachedInput)
                    {
                        foreach (var cachedType in cachedTypes)
                        {
                            if (cachedType.FullName == null)
                            {
                                continue;
                            }

                            if (!cachedType.FullName.Contains(componentName))
                            {
                                continue;
                            }

                            if (GUILayout.Button(cachedType.FullName, GUILayout.Height(25)))
                            {
                                transforms[transformIndex].gameObject.AddComponent(cachedType);
                                cachedInput = false;
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
                            if (component)
                            {
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
                }

                GUILayout.EndScrollView();
            }
        }

        [Serializable]
        private class 内存 : IWindow
        {
            private readonly Dictionary<int, long> minMemory = new Dictionary<int, long>();
            private readonly Dictionary<int, long> maxMemory = new Dictionary<int, long>();

            public void Execute(bool modified)
            {
                ScreenView = GUILayout.BeginScrollView(ScreenView, "Box");
                GUILayout.BeginVertical();
                DrawLabel(00, "程序分配的内存", Profiler.GetTotalReservedMemoryLong());
                DrawLabel(01, "正在使用的内存", Profiler.GetTotalAllocatedMemoryLong());
                DrawLabel(02, "空闲保留的内存", Profiler.GetTotalUnusedReservedMemoryLong());
                DrawLabel(03, "显卡占用的内存", Profiler.GetAllocatedMemoryForGraphicsDriver());
                DrawLabel(04, "Mono分配的内存", Profiler.GetMonoHeapSizeLong());
                DrawLabel(05, "Mono使用的内存", Profiler.GetMonoUsedSizeLong());

                GUILayout.EndVertical();
                GUILayout.EndScrollView();

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("垃圾回收", GUILayout.Height(30)))
                {
                    GC.Collect();
                }

                GUILayout.EndHorizontal();
            }

            private void DrawLabel(int key, string reason, long memory)
            {
                if (!minMemory.TryGetValue(key, out var minValue))
                {
                    minValue = long.MaxValue;
                    minMemory[key] = minValue;
                }

                if (!maxMemory.TryGetValue(key, out var maxValue))
                {
                    maxValue = 0;
                    maxMemory.Add(key, maxValue);
                }

                if (memory > maxValue)
                {
                    maxMemory[key] = memory;
                }

                if (memory < minValue)
                {
                    minMemory[key] = memory;
                }

                var result = string.Empty;
                result += "{0}: {1}".Format(reason, Debugger.PrettyBytes(memory)).Align(30);
                result += "Min: {0}".Format(Debugger.PrettyBytes(minMemory[key])).Align(20);
                result += "Max: {0}".Format(Debugger.PrettyBytes(maxMemory[key]));
                GUILayout.Label(result);
            }
        }

        [Serializable]
        private class 时间 : IWindow
        {
            public void Execute(bool modified)
            {
                ScreenView = GUILayout.BeginScrollView(ScreenView, "Box");
                GUILayout.BeginVertical();

                GUILayout.Label("当前日期:".Align(20) + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));
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
        }

        [Serializable]
        private class 系统 : IWindow
        {
            public void Execute(bool modified)
            {
                ScreenView = GUILayout.BeginScrollView(ScreenView, "Box");
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
                    Size = new Vector2(3200, 1800);
                }

                if (GUILayout.Button("1.0x", GUILayout.Height(30)))
                {
                    Size = new Vector2(2560, 1440);
                }

                if (GUILayout.Button("1.5x", GUILayout.Height(30)))
                {
                    Size = new Vector2(1920, 1080);
                }

                if (GUILayout.Button("2.0x", GUILayout.Height(30)))
                {
                    Size = new Vector2(1280, 720);
                }

                GUILayout.EndHorizontal();
            }
        }

        [Serializable]
        private class 程序 : IWindow
        {
            public void Execute(bool modified)
            {
                ScreenView = GUILayout.BeginScrollView(ScreenView, "Box");
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
                if (GUILayout.Button("重置位置", GUILayout.Height(30)))
                {
                    Rect.position = Vector2.zero;
                }

                if (GUILayout.Button("退出游戏", GUILayout.Height(30)))
                {
                    Application.Quit();
                }

                GUILayout.EndHorizontal();
            }
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

        private enum Pools
        {
            对象池,
            引用池,
            事件池,
            发送队列,
            接收队列,
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
    }

    internal sealed class DebugPool
    {
        public readonly Dictionary<Type, IPool> messages = new Dictionary<Type, IPool>();
        public readonly Dictionary<ushort, IPool> function = new Dictionary<ushort, IPool>();

        public void Record<T>(T message, int bytes) where T : struct, IMessage
        {
            switch (message)
            {
                case ServerRpcMessage server:
                    Record<T>(server.methodHash, bytes);
                    return;
                case ClientRpcMessage client:
                    Record<T>(client.methodHash, bytes);
                    return;
            }

            if (!messages.TryGetValue(typeof(T), out var item))
            {
                item = new Pool(typeof(T));
                messages[typeof(T)] = item;
            }

            ((Pool)item).Add(bytes);
        }

        private void Record<T>(ushort method, int bytes)
        {
            if (!function.TryGetValue(method, out var item))
            {
                item = new Pool(typeof(T));
                function[method] = item;

                var result = NetworkAttribute.GetInvoke(method);
                if (result != null)
                {
                    var name = result.Method.Name;
                    if (name.EndsWith("V2"))
                    {
                        name = name.Substring(0, name.Length - 2);
                    }

                    ((Pool)item).Path = "{0}.{1}".Format(result.Method.DeclaringType!.Name, name);
                }
            }

            ((Pool)item).Add(bytes);
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
            Reset();
            messages.Clear();
            function.Clear();
        }

        public class Pool : IPool
        {
            public Type Type { get; set; }
            public string Path { get; set; }
            public int Acquire { get; set; }
            public int Release { get; set; }
            public int Dequeue { get; set; }
            public int Enqueue { get; set; }

            public Pool()
            {
            }

            public Pool(Type type)
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
        }
    }

    internal static class Debugger
    {
        private static bool isActive;
        public static readonly DebugPool itemSend = new DebugPool();
        public static readonly DebugPool itemData = new DebugPool();
        public static readonly DebugPool.Pool clientSend = new DebugPool.Pool();
        public static readonly DebugPool.Pool clientData = new DebugPool.Pool();
        public static readonly DebugPool.Pool serverSend = new DebugPool.Pool();
        public static readonly DebugPool.Pool serverData = new DebugPool.Pool();

        public static void Enable()
        {
            if (NetworkManager.Transport)
            {
                NetworkManager.Transport.client.Send -= OnClientSend;
                NetworkManager.Transport.server.Send -= OnServerSend;
                NetworkManager.Transport.client.Receive -= OnClientReceive;
                NetworkManager.Transport.server.Receive -= OnServerReceive;
                NetworkManager.Transport.client.Send += OnClientSend;
                NetworkManager.Transport.server.Send += OnServerSend;
                NetworkManager.Transport.client.Receive += OnClientReceive;
                NetworkManager.Transport.server.Receive += OnServerReceive;
            }

            isActive = true;
        }

        private static void OnClientSend(ArraySegment<byte> segment)
        {
            clientSend.Add(segment.Count);
        }

        private static void OnClientReceive(ArraySegment<byte> segment, int channel)
        {
            clientData.Add(segment.Count);
        }

        private static void OnServerSend(int clientId, ArraySegment<byte> segment)
        {
            serverSend.Add(segment.Count);
        }

        private static void OnServerReceive(int clientId, ArraySegment<byte> segment, int channel)
        {
            serverData.Add(segment.Count);
        }

        public static void OnSend<T>(T message, int bytes) where T : struct, IMessage
        {
            if (isActive)
            {
                itemSend.Record(message, Compress.Invoke((uint)bytes) + bytes);
            }
        }

        public static void OnData<T>(T message, int bytes) where T : struct, IMessage
        {
            if (isActive)
            {
                itemData.Record(message, Compress.Invoke((uint)bytes) + bytes + 2);
            }
        }

        public static void Reset()
        {
            itemSend.Reset();
            itemData.Reset();
            clientSend.Dispose();
            clientData.Dispose();
            serverSend.Dispose();
            serverData.Dispose();
        }

        public static void Clear()
        {
            isActive = false;
            itemSend.Clear();
            itemData.Clear();
        }

        public static string PrettyBytes(long bytes)
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

        internal readonly struct Pool : IPool
        {
            public Type Type { get; }
            public string Path { get; }
            public int Acquire { get; }
            public int Release { get; }
            public int Dequeue { get; }
            public int Enqueue { get; }

            public Pool(IPool pool)
            {
                Type = pool.Type;
                Path = pool.Path;
                Acquire = pool.Acquire;
                Release = pool.Release;
                Dequeue = pool.Dequeue;
                Enqueue = pool.Enqueue;
            }

            public void Dispose()
            {
            }

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
    }
}