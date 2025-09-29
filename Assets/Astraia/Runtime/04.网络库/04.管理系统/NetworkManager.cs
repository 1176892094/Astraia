// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2024-11-29 13:11:20
// # Recently: 2024-12-22 21:12:51
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using Astraia.Common;
using UnityEngine;

namespace Astraia.Net
{
    [Serializable]
    public sealed partial class NetworkManager : MonoBehaviour, IEvent<OnSceneComplete>
    {
        public static NetworkManager Instance;

        internal const int Host = 0;

        public int sendRate = 30;

        public int connection = 100;

        public string authorization;

        private string sceneName;

        public static bool isHost => Server.isActive && Client.isActive;
        
        public static bool isServer => Server.isActive;

        public static bool isClient => Client.isActive;

        private void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Transport.Instance = GetComponent<Transport>();
        }

        private void Start()
        {
            Application.runInBackground = true;
        }

        private void OnEnable()
        {
            EventManager.Listen(this);
        }

        private void OnDisable()
        {
            EventManager.Remove(this);
        }

        private void OnApplicationQuit()
        {
            if (Lobby.isActive)
            {
                StopLobby();
            }

            if (Client.isConnected)
            {
                StopClient();
            }

            if (Server.isActive)
            {
                StopServer();
            }
        }

        public void Execute(OnSceneComplete message)
        {
            if (isHost)
            {
                Server.LoadSceneComplete(sceneName);
                Client.LoadSceneComplete(sceneName);
            }
            else if (isServer)
            {
                Server.LoadSceneComplete(sceneName);
            }
            else if (isClient)
            {
                Client.LoadSceneComplete(sceneName);
            }
        }

        public static void StartServer()
        {
            if (Server.isActive)
            {
                Log.Warn("服务器已经连接!");
                return;
            }

            Server.Start();
        }

        public static void StopServer()
        {
            if (!Server.isActive)
            {
                Log.Warn("服务器已经停止!");
                return;
            }

            Server.Stop();
        }

        public static void StartClient()
        {
            if (Client.isActive)
            {
                Log.Warn("客户端已经连接!");
                return;
            }

            Client.Start();
        }

        public static void StartClient(Uri uri)
        {
            if (Client.isActive)
            {
                Log.Warn("客户端已经连接!");
                return;
            }

            Client.Start(uri);
        }

        public static void StopClient()
        {
            if (!Client.isActive)
            {
                Log.Warn("客户端已经停止!");
                return;
            }
            
            Client.Stop();
        }

        public static void StartHost(bool isServer = true)
        {
            if (Server.isActive || Client.isActive)
            {
                Log.Warn("客户端或服务器已经连接!");
                return;
            }

            Server.Start(isServer);
            Client.Start(false);
        }

        public static void StopHost()
        {
            StopClient();
            StopServer();
        }

        public static void StartLobby()
        {
            if (Lobby.isActive)
            {
                Log.Warn("大厅服务器已经连接!");
                return;
            }

            Lobby.Start();
        }

        public static void StopLobby()
        {
            if (!Lobby.isActive)
            {
                Log.Warn("大厅服务器已经停止!");
                return;
            }

            Lobby.Stop();
        }
    }
}