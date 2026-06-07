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
using System.Net;
using System.Net.Sockets;
using Astraia.Core;
using UnityEngine;

namespace Astraia.Net
{
    [Serializable]
    public sealed partial class NetworkManager : MonoBehaviour, IEvent<OnSceneComplete>
    {
        public static NetworkManager Instance;
        private static UdpClient udpClient;
        private static UdpClient udpServer;
        private static Transport connection;
        private static Transport collection;
        private static bool isRemote;

        public int maxPlayer = 100;
        public string roomGuid;
        public string roomData;
        public string roomName;
        public RoomMode roomMode;
        public static bool isHost => isServer && isClient;
        public static bool isSaloon => Saloon.state != State.Disconnect;
        public static bool isServer => Server.state != State.Disconnect;
        public static bool isClient => Client.state != State.Disconnect;
        internal static Transport Transport => isRemote ? collection : connection;

        private void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Application.runInBackground = true;
            connection = gameObject.AddComponent<GenericTransport>();
            collection = gameObject.GetComponent<NetworkTransport>();
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

            StopDiscovery();
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

            NetworkTransport.Instance.address = address;
            Client.Start(false);
        }

        public static void StartDiscovery()
        {
            StopDiscovery();
            if (isServer)
            {
                udpServer = new UdpClient(47777) { EnableBroadcast = true, MulticastLoopback = false };
#if UNITY_ANDROID
                MulticastLock(true);
#endif
                ServerReceive();
            }
            else
            {
                udpClient = new UdpClient(0) { EnableBroadcast = true, MulticastLoopback = false };
                ClientReceive();
                ClientSend();
            }
        }

        public static void StopDiscovery()
        {
#if UNITY_ANDROID
            MulticastLock(false);
#endif
            udpServer?.Close();
            udpClient?.Close();
            udpServer = null;
            udpClient = null;
        }

        private static void ClientSend()
        {
            try
            {
                var address = IPAddress.Broadcast.ToString();
                var endPoint = new IPEndPoint(IPAddress.Broadcast, 47777);
                if (!string.IsNullOrWhiteSpace(address))
                {
                    endPoint = new IPEndPoint(IPAddress.Parse(address), 47777);
                }

                using var writer = MemoryWriter.Pop();
                writer.Invoke(new RequestMessage());
                ArraySegment<byte> segment = writer;
                udpClient.Send(segment.Array!, segment.Count, endPoint);
            }
            catch (SocketException)
            {
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        private static async void ServerReceive()
        {
            while (udpServer != null)
            {
                try
                {
                    var result = await udpServer.ReceiveAsync();
                    using var reader = MemoryReader.Pop(new ArraySegment<byte>(result.Buffer));
                    reader.Invoke<RequestMessage>();
                    ServerSend(result.RemoteEndPoint);
                }
                catch (ObjectDisposedException)
                {
                    return;
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        private static void ServerSend(IPEndPoint endPoint)
        {
            try
            {
                using var writer = MemoryWriter.Pop();
                writer.Invoke(new ResponseMessage(Transport.port));
                ArraySegment<byte> segment = writer;
                udpServer.Send(segment.Array!, segment.Count, endPoint);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        private static async void ClientReceive()
        {
            while (udpClient != null)
            {
                try
                {
                    var result = await udpClient.ReceiveAsync();
                    using var reader = MemoryReader.Pop(new ArraySegment<byte>(result.Buffer));
                    var response = reader.Invoke<ResponseMessage>();
                    EventManager.Invoke(new ServerResponse(result.RemoteEndPoint.Address.ToString(), response.port));
                }
                catch (ObjectDisposedException)
                {
                    return;
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }
#if UNITY_ANDROID
        private static bool multicast;
        private static AndroidJavaObject multicastLock;

        private static void MulticastLock(bool enabled)
        {
            if (enabled)
            {
                if (multicast) return;
                using var activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
                using var wifiManager = activity.Call<AndroidJavaObject>("getSystemService", "wifi");
                multicastLock = wifiManager.Call<AndroidJavaObject>("createMulticastLock", "lock");
                multicastLock.Call("acquire");
                multicast = true;
            }
            else
            {
                if (!multicast) return;
                multicastLock?.Call("release");
                multicast = false;
            }
        }
#endif
    }
}