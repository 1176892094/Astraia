// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2024-12-21 23:12:15
// # Recently: 2024-12-22 20:12:19
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Net;
using Astraia.Net;

namespace Astraia.Common
{
    public struct ServerConnect : IEvent
    {
        public NetworkClient client { get; private set; }

        public ServerConnect(NetworkClient client)
        {
            this.client = client;
        }
    }

    public struct ServerDisconnect : IEvent
    {
        public NetworkClient client { get; private set; }

        public ServerDisconnect(NetworkClient client)
        {
            this.client = client;
        }
    }

    public struct ServerReady : IEvent
    {
        public NetworkClient client { get; private set; }

        public ServerReady(NetworkClient client)
        {
            this.client = client;
        }
    }

    public struct ServerChangeScene : IEvent
    {
        public string sceneName { get; private set; }

        public ServerChangeScene(string sceneName)
        {
            this.sceneName = sceneName;
        }
    }

    public struct ServerSceneChanged : IEvent
    {
        public string sceneName { get; private set; }

        public ServerSceneChanged(string sceneName)
        {
            this.sceneName = sceneName;
        }
    }

    public struct ClientConnect : IEvent
    {
    }

    public struct ClientDisconnect : IEvent
    {
    }

    public struct ClientNotReady : IEvent
    {
    }

    public struct ClientChangeScene : IEvent
    {
        public string sceneName { get; private set; }

        public ClientChangeScene(string sceneName)
        {
            this.sceneName = sceneName;
        }
    }

    public struct ClientSceneChanged : IEvent
    {
        public string sceneName { get; private set; }

        public ClientSceneChanged(string sceneName)
        {
            this.sceneName = sceneName;
        }
    }

    public struct ServerResponse : IEvent
    {
        public Uri uri { get; private set; }
        public IPEndPoint endPoint { get; private set; }

        public ServerResponse(Uri uri, IPEndPoint endPoint)
        {
            this.uri = uri;
            this.endPoint = endPoint;
        }
    }

    public struct LobbyUpdate : IEvent
    {
        public RoomData[] rooms { get; private set; }

        public LobbyUpdate(RoomData[] rooms)
        {
            this.rooms = rooms;
        }
    }

    public struct LobbyDisconnect : IEvent
    {
    }

    public struct LobbyCreateRoom : IEvent
    {
        public string roomId { get; private set; }

        public LobbyCreateRoom(string roomId)
        {
            this.roomId = roomId;
        }
    }

    public struct PingUpdate : IEvent
    {
        public double pingTime { get; private set; }

        public PingUpdate(double pingTime)
        {
            this.pingTime = pingTime;
        }
    }


    [Serializable]
    public struct RoomData
    {
        /// <summary>
        /// 房间拥有者
        /// </summary>
        public int clientId;

        /// <summary>
        /// 是否显示
        /// </summary>
        public RoomMode roomMode;

        /// <summary>
        /// 房间最大人数
        /// </summary>
        public int maxCount;

        /// <summary>
        /// 额外房间数据
        /// </summary>
        public string roomData;

        /// <summary>
        /// 房间Id
        /// </summary>
        public string roomId;

        /// <summary>
        /// 房间名称
        /// </summary>
        public string roomName;

        /// <summary>
        /// 客户端数量
        /// </summary>
        public int[] clients;
    }
}