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
using UnityEngine;
using State = Astraia.Async.State;

namespace Astraia.Net
{
    [Serializable]
    public sealed partial class NetworkManager : Singleton<NetworkManager>, IDontDestroy
    {
        public int sendRate = 30;
        public int maxPlayer = 100;
        public string roomGuid;
        public string roomData;
        public string roomName;
        public Lobby.Room roomMode;
        [SerializeReference] private NetworkObserving observing;
        [SerializeReference] private NetworkDiscovery discovery;
        [SerializeReference] private Transport connection = new NetworkTransport();
        [SerializeReference] private Transport management = new NetworkAuthority();
        [SerializeReference] private Transport collection = new NetworkTransport();

        public static bool isHost => isServer && isClient;
        public static bool isRunner => isServer || isClient;
        public static bool isServer => Server.state != State.Failure;
        public static bool isClient => Client.state != State.Failure;
        internal static bool isSaloon => Saloon != null && Saloon.isSaloon;
        internal static bool isRemote => Saloon != null && Saloon.isRemote;
        internal static double syncRate => 1.0 / Instance.sendRate;
        internal static double syncTime => Time.unscaledTimeAsDouble;
        internal static Transport Kcp => isRemote ? Saloon : Instance?.connection;
        internal static NetworkAuthority Saloon => (NetworkAuthority)Instance?.management;

        protected override void Awake()
        {
            base.Awake();
            Application.runInBackground = true;
            NetworkDiscovery.Instance = discovery;
            NetworkAuthority.Instance = collection;
            connection.Start(false);
            collection.Start(false);
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

            observing?.Dispose();
            discovery?.StopDiscovery();
        }

        protected override void OnEnable()
        {
            AssetManager.OnSceneComplete += OnSceneComplete;
        }

        protected override void OnDisable()
        {
            AssetManager.OnSceneComplete -= OnSceneComplete;
        }

        public void OnSceneComplete(string sceneName)
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

        public static void SetTransport(string address, ushort port)
        {
            Kcp.address = address;
            Kcp.port = port;
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

            Client.Disconnect(0);
        }

        public static void StartHost(bool isHost = true)
        {
            if (isRunner)
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

            ApplySaloon();
            Saloon.Start(true);
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

            Saloon.Stop();
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

        public static void SubmitRoom()
        {
            if (!isRemote || !Saloon.isActive)
            {
                return;
            }

            if (isRunner && !Saloon.isRunner)
            {
                Log.Warn("您必须连接到大厅以更新房间信息!");
                return;
            }

            ApplySaloon();
            Saloon.Submit();
        }

        public static void CreateRoom()
        {
            if (!isRemote || !Saloon.isActive)
            {
                Log.Warn("没有连接到大厅!");
            }

            if (isRunner || Saloon.isRunner)
            {
                Log.Warn("客户端或服务器已经连接!");
                return;
            }

            ApplySaloon();
            Server.Start(true);
            Client.Start(true);
        }

        public static void JoinRoom(string address)
        {
            if (!isRemote || !Saloon.isActive)
            {
                Log.Warn("没有连接到大厅!");
            }

            if (isRunner || Saloon.isRunner)
            {
                Log.Warn("客户端或服务器已经连接!");
                return;
            }

            Instance.collection.address = address;
            Client.Start(false);
        }

        private static void ApplySaloon()
        {
            Saloon.roomName = Instance.roomName;
            Saloon.roomData = Instance.roomData;
            Saloon.roomGuid = Instance.roomGuid;
            Saloon.roomMode = Instance.roomMode;
            Saloon.maxPlayer = Instance.maxPlayer;
        }
    }
}