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
using Object = UnityEngine.Object;

namespace Astraia.Net
{
    [Serializable]
    public sealed partial class NetworkManager : Singleton<NetworkManager, Entity>, IEvent<OnSceneComplete>
    {
        public static bool isHost => isServer && isClient;
        public static bool isLobby => Lobby.state != State.Disconnect;
        public static bool isServer => Server.state != State.Disconnect;
        public static bool isClient => Client.state != State.Disconnect;

        public override void Dequeue()
        {
            Application.runInBackground = true;
            Object.DontDestroyOnLoad(gameObject);
            Transport.Instance = owner.GetComponent<Transport>();
        }

        public override void Enqueue()
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