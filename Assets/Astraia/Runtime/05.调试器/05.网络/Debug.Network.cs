// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-03-13 13:03:55
// // # Recently: 2025-03-13 13:03:56
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using Astraia.Net;
using UnityEngine;

namespace Astraia.Common
{
    public partial class DebugManager
    {
        private void NetworkWindow()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(" 网络信息", GUILayout.Height(25));
            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal();

            screenView = GUILayout.BeginScrollView(screenView, "Box", GUILayout.Width((screenWidth - 20) / 2));
            if (Transport.Instance)
            {
                GUILayout.Label("网络地址: \t\t" + Transport.Instance.address);
                GUILayout.Label("网络端口: \t\t" + Transport.Instance.port);
            }
            else
            {
                GUILayout.Label("网络地址: \t\t" + "127.0.0.1");
                GUILayout.Label("网络端口: \t\t" + 0);
            }

            GUILayout.Label("网络模式: \t\t" + NetworkManager.Mode);
            string message;
            if (!NetworkManager.Client.isConnected && !NetworkManager.Server.isActive)
            {
                message = !NetworkManager.Client.isActive ? "未连接" : "连接中";
            }
            else
            {
                message = "已连接";
            }

            GUILayout.Label("连接状态: \t\t" + message);
            GUILayout.Label("传输状态: \t\t" + (NetworkManager.Client.isReady ? "传输中" : "未传输"));
            GUILayout.Label("往返时间: \t\t" + Math.Min((int)(framePing * 1000), 999) + "ms");
            if (NetworkManager.Instance)
            {
                GUILayout.Label("连接数量: \t\t" + NetworkManager.Server.connections + "/" + NetworkManager.Instance.connection);
                GUILayout.Label("同步帧率: \t\t" + NetworkManager.Instance.sendRate);
            }
            else
            {
                GUILayout.Label("连接数量: \t\t" + 0 + "/" + 0);
                GUILayout.Label("同步帧率: \t\t" + 0);
            }

            GUILayout.EndScrollView();

            windowView = GUILayout.BeginScrollView(windowView, "Box");
            NetworkSimulator.Instance?.OnGUIWindow();
            GUILayout.EndScrollView();

            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal();
            if (!NetworkManager.Client.isConnected && !NetworkManager.Server.isActive)
            {
                if (!NetworkManager.Client.isActive)
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

            if (NetworkManager.Client.isConnected && !NetworkManager.Client.isReady)
            {
                if (GUILayout.Button("Ready", GUILayout.Height(30)))
                {
                    NetworkManager.Client.Ready();
                }
            }

            if (NetworkManager.Server.isActive && NetworkManager.Client.isConnected)
            {
                if (GUILayout.Button("Stop Host", GUILayout.Height(30)))
                {
                    NetworkManager.StopHost();
                }
            }
            else if (NetworkManager.Client.isConnected)
            {
                if (GUILayout.Button("Stop Client", GUILayout.Height(30)))
                {
                    NetworkManager.StopClient();
                }
            }
            else if (NetworkManager.Server.isActive)
            {
                if (GUILayout.Button("Stop Server", GUILayout.Height(30)))
                {
                    NetworkManager.StopServer();
                }
            }

            GUILayout.EndHorizontal();
        }
    }
}