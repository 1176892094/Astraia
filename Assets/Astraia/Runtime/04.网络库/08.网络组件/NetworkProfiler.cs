// // *********************************************************************************
// // # Project: JFramework
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-06-01 19:06:54
// // # Recently: 2025-06-01 19:06:54
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using UnityEngine;

namespace Astraia.Net
{
    public class NetworkProfiler : Singleton<NetworkProfiler>
    {
        private double waitTime;

        public int clientIntervalReceivedPackets;
        public long clientIntervalReceivedBytes;
        public int clientIntervalSentPackets;
        public long clientIntervalSentBytes;

        public int clientReceivedPacketsPerSecond;
        public long clientReceivedBytesPerSecond;
        public int clientSentPacketsPerSecond;
        public long clientSentBytesPerSecond;

        public int serverIntervalReceivedPackets;
        public long serverIntervalReceivedBytes;
        public int serverIntervalSentPackets;
        public long serverIntervalSentBytes;

        public int serverReceivedPacketsPerSecond;
        public long serverReceivedBytesPerSecond;
        public int serverSentPacketsPerSecond;
        public long serverSentBytesPerSecond;

        private void Start()
        {
            if (Transport.Instance != null)
            {
                Transport.Instance.OnClientSend += OnClientSend;
                Transport.Instance.OnServerSend += OnServerSend;
                Transport.Instance.OnClientReceive += OnClientReceive;
                Transport.Instance.OnServerReceive += OnServerReceive;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (Transport.Instance != null)
            {
                Transport.Instance.OnClientSend -= OnClientSend;
                Transport.Instance.OnServerSend -= OnServerSend;
                Transport.Instance.OnClientReceive -= OnClientReceive;
                Transport.Instance.OnServerReceive -= OnServerReceive;
            }
        }

        private void OnClientReceive(ArraySegment<byte> data, int channelId)
        {
            clientIntervalReceivedPackets++;
            clientIntervalReceivedBytes += data.Count;
        }

        private void OnClientSend(ArraySegment<byte> data, int channelId)
        {
            clientIntervalSentPackets++;
            clientIntervalSentBytes += data.Count;
        }

        private void OnServerReceive(int connectionId, ArraySegment<byte> data, int channelId)
        {
            serverIntervalReceivedPackets++;
            serverIntervalReceivedBytes += data.Count;
        }

        private void OnServerSend(int connectionId, ArraySegment<byte> data, int channelId)
        {
            serverIntervalSentPackets++;
            serverIntervalSentBytes += data.Count;
        }

        private void Update()
        {
            if (waitTime < Time.unscaledTimeAsDouble)
            {
                if (NetworkManager.Client.isActive)
                {
                    UpdateClient();
                }

                if (NetworkManager.Server.isActive)
                {
                    UpdateServer();
                }

                waitTime = Time.unscaledTimeAsDouble + 1;
            }
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

        public void OnGUIWindow()
        {
            GUILayout.Space(20);
            GUILayout.Label("客户端: \t\t" + (NetworkManager.Client.isActive ? "开启" : "关闭"));
            GUILayout.Label(Service.Text.Format("发送数量:\t\t{0}", serverSentPacketsPerSecond));
            GUILayout.Label(Service.Text.Format("发送大小:\t\t{0}/s", PrettyBytes(serverSentBytesPerSecond)));
            GUILayout.Label(Service.Text.Format("接收数量:\t\t{0}", serverReceivedPacketsPerSecond));
            GUILayout.Label(Service.Text.Format("接收大小:\t\t{0}/s", PrettyBytes(serverReceivedBytesPerSecond)));
            GUILayout.Space(20);
            GUILayout.Label("服务器: \t\t" + (NetworkManager.Server.isActive ? "开启" : "关闭"));
            GUILayout.Label(Service.Text.Format("发送数量:\t\t{0}", clientSentPacketsPerSecond));
            GUILayout.Label(Service.Text.Format("发送大小:\t\t{0}/s", PrettyBytes(clientSentBytesPerSecond)));
            GUILayout.Label(Service.Text.Format("接收数量:\t\t{0}", clientReceivedPacketsPerSecond));
            GUILayout.Label(Service.Text.Format("接收大小:\t\t{0}/s", PrettyBytes(clientReceivedBytesPerSecond)));
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