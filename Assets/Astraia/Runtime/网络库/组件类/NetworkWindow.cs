// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-09-03 12:09:24
// # Recently: 2026-09-03 14:21:03
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assemblies;
using UnityEngine.Profiling;

namespace Astraia.Net
{
    internal partial class NetworkDebugger
    {
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
                Rebuild(poolData, HeapManager.poolData.Values, "引用池");
                ScreenView = GUILayout.BeginScrollView(ScreenView, "Box");
                Repaint(poolData, "未使用\t使用中\t使用次数\t释放次数");
                GUILayout.EndScrollView();
            }
        }

        [Serializable]
        private class 对象池 : IWindow
        {
            private Dictionary<string, List<IPool>> poolData = new Dictionary<string, List<IPool>>();

            public void Execute(bool modified)
            {
                Rebuild(poolData, PoolManager.Instance.Values, "对象池");
                ScreenView = GUILayout.BeginScrollView(ScreenView, "Box");
                Repaint(poolData, "未激活\t激活中\t出队次数\t入队次数");
                GUILayout.EndScrollView();
            }
        }

        [Serializable]
        private class 事件 : IWindow
        {
            private Dictionary<string, List<IPool>> poolData = new Dictionary<string, List<IPool>>();

            public void Execute(bool modified)
            {
                Rebuild(poolData, EventManager.poolData.Values, "事件池");
                ScreenView = GUILayout.BeginScrollView(ScreenView, "Box");
                Repaint(poolData, "触发数\t事件数\t添加次数\t移除次数");
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
                var ping = (int)Math.Min(NetworkManager.Client.pingTime * 1000, 999);
                var peer = NetworkManager.current != null ? NetworkManager.current.address : "127.0.0.1";
                var port = NetworkManager.current != null ? NetworkManager.current.port : (ushort)20974;
                GUILayout.Label("{0} : {1}".Format(peer, port), "Button", GUILayout.Width((ScreenX - 20) / 2), GUILayout.Height(30));
                GUILayout.Label(NetworkManager.isClient ? "Ping: {0} ms".Format(ping) : "Client is not active!", "Button", GUILayout.Height(30));
                GUILayout.EndHorizontal();

                if (waitTime < Time.realtimeSinceStartup)
                {
                    waitTime = Time.realtimeSinceStartup + 1;
                    clientSend = new PoolData(Send.client);
                    serverSend = new PoolData(Send.server);
                    clientData = new PoolData(Data.client);
                    serverData = new PoolData(Data.server);
                    Rebuild(itemSend, Send.Values, "发送队列");
                    Rebuild(itemData, Data.Values, "接收队列");
                    Dispose();
                }

                ScreenView = GUILayout.BeginScrollView(ScreenView, "Box");
                GUILayout.BeginVertical("Box");
                GUILayout.Label("Astraia.Net - 网络信息".Align(50) + "每秒数量\t每秒大小\t累计数量\t累计大小", GUILayout.Height(20));
                GUILayout.Label("NetworkSend".Align(50) + (NetworkManager.isServer ? serverSend : clientSend), GUILayout.Height(20));
                GUILayout.Label("NetworkReceive".Align(50) + (NetworkManager.isServer ? serverData : clientData), GUILayout.Height(20));
                GUILayout.EndVertical();
                Repaint(itemSend, "每秒发送\t每秒发送\t累计发送\t累计发送");
                Repaint(itemData, "每秒接收\t每秒接收\t累计接收\t累计接收");
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
                foreach (var assembly in CurrentAssemblies.GetLoadedAssemblies())
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
                result += "{0}: {1}".Format(reason, PrettyBytes(memory)).Align(30);
                result += "Min: {0}".Format(PrettyBytes(minMemory[key])).Align(20);
                result += "Max: {0}".Format(PrettyBytes(maxMemory[key]));
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
    }
}