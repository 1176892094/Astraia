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

        private static bool isRemote;
        private static int connection;
        private static int collection;

        public int sendRate = 30;
        public int maxPlayer = 100;
        public string roomGuid;
        public string roomData;
        public string roomName;
        public RoomMode roomMode;

        public static bool isHost => isServer && isClient;
        public static bool isServer => Server.state != State.断开连接;
        public static bool isClient => Client.state != State.断开连接;
        public static bool isSaloon => Saloon.state != State.断开连接;
        internal static double syncRate => 1.0 / Instance.sendRate;
        internal static double syncTime => Time.unscaledTimeAsDouble;
        internal static Transport kcp => Instance.GetComponent<Transport>(isRemote ? collection : connection);

        protected override void Awake()
        {
            base.Awake();
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Application.runInBackground = true;

            var isLocal = true;
            for (var i = 0; i < moduleList.Count; i++)
            {
                if (moduleList[i] is NetworkTransport)
                {
                    connection = i;
                    isLocal = false;
                    break;
                }
            }

            if (isLocal)
            {
                connection = moduleList.IndexOf(AddComponent<NetworkTransport>());
            }

            var isLobby = true;
            for (var i = 0; i < moduleList.Count; i++)
            {
                if (moduleList[i] is NetworkAuthority)
                {
                    collection = i;
                    isLobby = false;
                    NetworkAuthority.Instance = AddComponent<NetworkTransport>();
                    break;
                }
            }

            if (isLobby)
            {
                collection = moduleList.IndexOf(AddComponent<NetworkAuthority>());
                NetworkAuthority.Instance = AddComponent<NetworkTransport>();
            }
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
            if (isSaloon)
            {
                StopSaloon();
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
            kcp.address = address;
            kcp.port = port;
        }

        public static void StartServer()
        {
            if (isServer)
            {
                Log.Warn("服务器已经连接!");
                return;
            }

            Server.Start(true);
        }

        public static void StopServer()
        {
            if (!isServer)
            {
                Log.Warn("服务器已经停止!");
                return;
            }

            Server.Stop();
        }

        public static void StartClient()
        {
            if (isClient)
            {
                Log.Warn("客户端已经连接!");
                return;
            }

            Client.Start(false);
        }

        public static void StopClient()
        {
            if (!isClient)
            {
                Log.Warn("客户端已经停止!");
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
                Log.Warn("客户端或服务器已经连接!");
                return;
            }

            Server.Start(isHost);
            Client.Start(true);
        }

        public static void StopHost()
        {
            StopClient();
            StopServer();
        }

        public static void StartSaloon()
        {
            if (isRemote)
            {
                return;
            }

            if (isSaloon)
            {
                Log.Warn("大厅服务器已经连接!");
                return;
            }

            Saloon.Start();
        }

        public static void StopSaloon()
        {
            if (!isRemote)
            {
                return;
            }

            if (!isSaloon)
            {
                Log.Warn("大厅服务器已经停止!");
                return;
            }

            Saloon.Disconnect();
        }

        public static void UpdateRoom()
        {
            if (!isRemote || !Saloon.isActive)
            {
                Log.Warn("您必须连接到大厅以请求房间列表!");
                return;
            }

            Saloon.Update();
        }

        public static void SubmitRoom(RoomMode roomMode)
        {
            if (!isRemote)
            {
                return;
            }

            if (!Saloon.isServer)
            {
                Log.Warn("您必须连接到大厅以更新房间信息!");
                return;
            }

            Saloon.Submit(roomMode);
        }

        public static void CreateRoom(int maxPlayer)
        {
            if (!isRemote || !Saloon.isActive)
            {
                Log.Warn("没有连接到大厅!");
            }

            if (isServer || isClient || Saloon.isServer || Saloon.isClient)
            {
                Log.Warn("客户端或服务器已经连接!");
                return;
            }

            Instance.maxPlayer = maxPlayer;
            Server.Start(true);
            Client.Start(true);
        }

        public static void JoinRoom(string address)
        {
            if (!isRemote || !Saloon.isActive)
            {
                Log.Warn("没有连接到大厅!");
            }

            if (isServer || isClient || Saloon.isServer || Saloon.isClient)
            {
                Log.Warn("客户端或服务器已经连接!");
                return;
            }

            NetworkAuthority.Instance.address = address;
            Client.Start(false);
        }
    }
}