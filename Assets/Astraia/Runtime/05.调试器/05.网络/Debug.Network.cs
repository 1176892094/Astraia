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
using System.Collections.Generic;
using Astraia.Net;
using UnityEngine;

namespace Astraia.Common
{
    public partial class DebugManager
    {
        private IList<Pool> sendList = new List<Pool>();
        private IList<Pool> receiveList = new List<Pool>();
        private float waitTime;
        private string address;

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

            GUILayout.Label(Service.Text.Format("{0} : {1}", peer, port), "Button", GUILayout.Width((screenWidth - 20) / 2), GUILayout.Height(30));
            var ping = NetworkManager.Mode is EntryMode.Client ? Service.Text.Format("Ping: {0} ms", Math.Min((int)(framePing * 1000), 999)) : "Client is not active!";
            GUILayout.Label(ping, "Button", GUILayout.Height(30));

            GUILayout.EndHorizontal();

            screenView = GUILayout.BeginScrollView(screenView, "Box");
            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical("Box", GUILayout.Width((screenWidth - 28) / 2));
            NetworkSimulator.Instance?.OnGUIServer();
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Box");
            NetworkSimulator.Instance?.OnGUIClient();
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            if (NetworkSimulator.Instance)
            {
                if (waitTime < Time.unscaledTime)
                {
                    waitTime = Time.unscaledTime + 1;
                    sendList = NetworkSimulator.Instance.SendReference();
                    receiveList = NetworkSimulator.Instance.ReceiveReference();
                    NetworkSimulator.Instance.ItemReset();
                }

                NetworkMessage(sendList, "发送队列", "每秒发送\t每秒发送\t全局发送\t全局发送");
                NetworkMessage(receiveList, "接收队列", "每秒接收\t每秒接收\t全局接收\t全局接收");
            }


            GUILayout.EndScrollView();

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

        private void NetworkMessage(IList<Pool> references, string message, string module)
        {
            poolData.Clear();
            foreach (var reference in references)
            {
                var assemblyName = Service.Text.Format("{0} - {1}", reference.type.Assembly.GetName().Name, message);
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
                    var assetName = data.type.Name;
                    if (!string.IsNullOrEmpty(data.path))
                    {
                        assetName = Service.Text.Format("{0} - {1}", data.type.Name, data.path);
                    }

                    GUILayout.Label(assetName, GUILayout.Height(20));
                }

                GUILayout.EndVertical();

                GUILayout.BeginVertical("Box");
                GUILayout.Label(module, GUILayout.Height(20));
                foreach (var data in poolPair.Value)
                {
                    var result = Service.Text.Format("{0}\t\t{1}\t\t{2}\t\t{3}", data.release, PrettyBytes(data.acquire), data.dequeue, PrettyBytes(data.enqueue));
                    GUILayout.Label(result, GUILayout.Height(20));
                }

                GUILayout.EndVertical();

                GUILayout.EndHorizontal();
            }
        }

        private static string PrettyBytes(long bytes)
        {
            if (bytes < 1024)
            {
                return Service.Text.Format("{0} B", bytes);
            }

            if (bytes < 1024 * 1024)
            {
                return Service.Text.Format("{0:F2} KB", bytes / 1024F);
            }

            if (bytes < 1024 * 1024 * 1024)
            {
                return Service.Text.Format("{0:F2} MB", bytes / 1024F / 1024F);
            }

            return Service.Text.Format("{0:F2} GB", bytes / 1024F / 1024F / 1024F);
        }
    }
}