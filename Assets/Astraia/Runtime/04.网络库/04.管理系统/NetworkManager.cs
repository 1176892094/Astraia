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
    public sealed partial class NetworkManager : Entity, IEvent<OnSceneComplete>
    {
        public static NetworkManager Instance;
        private static Transport connection;
        private static Transport collection;
        public int roomCount = 100;
        public string roomGuid;
        public string roomData;
        public string roomName;
        public RoomMode roomMode;

        public static bool isHost => isServer && isClient;
        public static bool isLobby => Lobby.state != State.Disconnect;
        public static bool isServer => Server.state != State.Disconnect;
        public static bool isClient => Client.state != State.Disconnect;
        public static Transport Transport => isRemote ? collection : connection;

        private static bool isRemote
        {
            get => LobbyTransport.Instance.enabled;
            set => LobbyTransport.Instance.enabled = value;
        }

        protected override void Awake()
        {
            base.Awake();
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Application.runInBackground = true;
            connection = transform.GetOrAddComponent<NetworkTransport>();
            collection = transform.GetOrAddComponent<LobbyTransport>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            EventManager.Listen(this);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            EventManager.Remove(this);
        }

        private void OnApplicationQuit()
        {
            if (isLobby)
            {
                StopLobby();
            }

            if (Client.isActive)
            {
                StopClient();
            }

            if (isServer)
            {
                StopServer();
            }
        }

        public void Execute(OnSceneComplete message)
        {
            if (isHost)
            {
                Server.LoadSceneComplete(message.sceneName);
                Client.LoadSceneComplete(message.sceneName);
            }
            else if (isServer)
            {
                Server.LoadSceneComplete(message.sceneName);
            }
            else if (isClient)
            {
                Client.LoadSceneComplete(message.sceneName);
            }
        }

        public static void StartServer()
        {
            if (isServer)
            {
                Service.Log.Warn("服务器已经连接!");
                return;
            }

            Server.Start(true);
        }

        public static void StopServer()
        {
            if (!isServer)
            {
                Service.Log.Warn("服务器已经停止!");
                return;
            }

            Server.Stop();
        }

        public static void StartClient()
        {
            if (isClient)
            {
                Service.Log.Warn("客户端已经连接!");
                return;
            }

            Client.Start(1);
        }

        public static void StartClient(Uri uri)
        {
            if (isClient)
            {
                Service.Log.Warn("客户端已经连接!");
                return;
            }

            Client.Start(uri);
        }

        public static void StopClient()
        {
            if (!isClient)
            {
                Service.Log.Warn("客户端已经停止!");
                return;
            }

            if (isServer)
            {
                Server.Disconnect(0);
            }

            Client.Stop();
        }

        public static void StartHost(bool isHost = true)
        {
            if (isServer || isClient)
            {
                Service.Log.Warn("客户端或服务器已经连接!");
                return;
            }

            Server.Start(isHost);
            Client.Start(0);
        }

        public static void StopHost()
        {
            StopClient();
            StopServer();
        }

        public static void StartLobby()
        {
            if (isLobby)
            {
                Service.Log.Warn("大厅服务器已经连接!");
                return;
            }

            Lobby.Start();
        }

        public static void StopLobby()
        {
            if (!isLobby)
            {
                Service.Log.Warn("大厅服务器已经停止!");
                return;
            }

            Lobby.Stop();
        }
    }
}