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
using System.Runtime.CompilerServices;
using Astraia.Common;
using UnityEngine;

namespace Astraia.Net
{
    [RequireComponent(typeof(KcpTransport))]
    public sealed partial class NetworkManager : MonoBehaviour, IEvent<SceneComplete>
    {
        public static NetworkManager Instance;

        public Transport transport;

        [Range(30, 90)] public int sendRate = 30;

        public int connection = 100;

        public string authorization;

        private string sceneName;

        public static EntryMode Mode
        {
            get
            {
                if (!Application.isPlaying)
                {
                    return EntryMode.None;
                }

                if (Server.isActive)
                {
                    return Client.isActive ? EntryMode.Host : EntryMode.Server;
                }

                return Client.isActive ? EntryMode.Client : EntryMode.None;
            }
        }

        private void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Transport.Instance = transport;
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

        public void Execute(SceneComplete message)
        {
            switch (Mode)
            {
                case EntryMode.Host:
                    Server.LoadSceneComplete(sceneName);
                    Client.LoadSceneComplete(sceneName);
                    break;
                case EntryMode.Server:
                    Server.LoadSceneComplete(sceneName);
                    break;
                case EntryMode.Client:
                    Client.LoadSceneComplete(sceneName);
                    break;
            }
        }

        public static void StartServer()
        {
            if (Server.isActive)
            {
                Debug.LogWarning(Logs.E201);
                return;
            }

            Server.Start(EntryMode.Server);
        }

        public static void StopServer()
        {
            if (!Server.isActive)
            {
                Debug.LogWarning(Logs.E202);
                return;
            }

            Server.Stop();
        }

        public static void StartClient()
        {
            if (Client.isActive)
            {
                Debug.LogWarning(Logs.E203);
                return;
            }

            Client.Start(EntryMode.Client);
        }

        public static void StartClient(Uri uri)
        {
            if (Client.isActive)
            {
                Debug.LogWarning(Logs.E203);
                return;
            }

            Client.Start(uri);
        }

        public static void StopClient()
        {
            if (!Client.isActive)
            {
                Debug.LogWarning(Logs.E204);
                return;
            }

            if (Mode == EntryMode.Host)
            {
                Server.OnServerDisconnect(Server.hostId);
            }

            Client.Stop();
        }

        public static void StartHost(EntryMode mode = EntryMode.Host)
        {
            if (Server.isActive || Client.isActive)
            {
                Debug.LogWarning(Logs.E205);
                return;
            }

            Server.Start(mode);
            Client.Start(EntryMode.Host);
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
                Debug.LogWarning(Logs.E206);
                return;
            }

            Lobby.Start();
        }

        public static void StopLobby()
        {
            if (!Lobby.isActive)
            {
                Debug.LogWarning(Logs.E207);
                return;
            }

            Lobby.Stop();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NetworkObject GetNetworkObject(uint objectId)
        {
            if (Server.isActive)
            {
                if (Server.spawns.TryGetValue(objectId, out var @object))
                {
                    return @object;
                }
            }

            if (Client.isActive)
            {
                if (Client.spawns.TryGetValue(objectId, out var @object))
                {
                    return @object;
                }
            }

            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsSceneObject(NetworkObject @object)
        {
            if (@object.sceneId == 0)
            {
                return false;
            }

            if (@object.gameObject.hideFlags == HideFlags.NotEditable)
            {
                return false;
            }

            return @object.gameObject.hideFlags != HideFlags.HideAndDontSave;
        }
    }
}