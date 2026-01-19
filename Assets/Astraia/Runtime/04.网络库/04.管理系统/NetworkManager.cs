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
using Astraia.Core;
using UnityEngine;

namespace Astraia.Net
{
    [Serializable]
    public sealed partial class NetworkManager : Entity, IEvent<OnSceneComplete>
    {
        public static NetworkManager Instance;
        private static Transport connection;
        private static Transport collection;
        private static bool isRemote;

        public int maxPlayer = 100;
        public string roomGuid;
        public string roomData;
        public string roomName;
        public RoomMode roomMode;
        public static bool isHost => isServer && isClient;
        public static bool isLobby => Lobby.state != State.Disconnect;
        public static bool isServer => Server.state != State.Disconnect;
        public static bool isClient => Client.state != State.Disconnect;
        internal static Transport Transport => isRemote ? collection : connection;


        protected override void Awake()
        {
            base.Awake();
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Application.runInBackground = true;
            collection = gameObject.GetComponent<LobbyTransport>();
            connection = gameObject.AddComponent<NetworkTransport>();
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

        public static void SetTransport(string address, ushort port)
        {
            Transport.address = address;
            Transport.port = port;
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

            Client.Disconnect();
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
            if (isRemote)
            {
                return;
            }

            if (isLobby)
            {
                Service.Log.Warn("大厅服务器已经连接!");
                return;
            }

            Lobby.Start();
        }

        public static void StopLobby()
        {
            if (!isRemote)
            {
                return;
            }

            if (!isLobby)
            {
                Service.Log.Warn("大厅服务器已经停止!");
                return;
            }

            Lobby.Disconnect();
        }

        public static void UpdateLobby()
        {
            if (!isRemote || !Lobby.isActive)
            {
                Service.Log.Warn("您必须连接到大厅以请求房间列表!");
                return;
            }

            Lobby.Update();
        }

        public static void UpdateRoom(RoomMode roomMode)
        {
            if (!isRemote)
            {
                return;
            }

            if (!Lobby.isServer)
            {
                Service.Log.Warn("您必须连接到大厅以更新房间信息!");
                return;
            }

            Lobby.Submit(roomMode);
        }

        public static void CreateRoom(int maxPlayer)
        {
            if (!isRemote || !Lobby.isActive)
            {
                Service.Log.Warn("没有连接到大厅!");
            }

            if (isServer || isClient || Lobby.isServer || Lobby.isClient)
            {
                Service.Log.Warn("客户端或服务器已经连接!");
                return;
            }

            Instance.maxPlayer = maxPlayer;
            Server.Start(true);
            Client.Start(0);
        }

        public static void JoinRoom(string address)
        {
            if (!isRemote || !Lobby.isActive)
            {
                Service.Log.Warn("没有连接到大厅!");
            }

            if (isServer || isClient || Lobby.isServer || Lobby.isClient)
            {
                Service.Log.Warn("客户端或服务器已经连接!");
                return;
            }

            LobbyTransport.Transport.address = address;
            Client.Start(1);
        }
    }
}