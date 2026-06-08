// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2024-11-29 13:11:20
// # Recently: 2024-12-22 20:12:19
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using Astraia.Net;

namespace Astraia.Core
{
    public interface IStartClient
    {
        void OnStartClient();
    }

    public interface IStopClient
    {
        void OnStopClient();
    }

    public interface IStartServer
    {
        void OnStartServer();
    }

    public interface IStopServer
    {
        void OnStopServer();
    }

    public interface IStartAuthority
    {
        void OnStartAuthority();
    }

    public interface IStopAuthority
    {
        void OnStopAuthority();
    }

    public readonly struct ServerConnect : IEvent
    {
        public readonly NetworkClient client;

        public ServerConnect(NetworkClient client)
        {
            this.client = client;
        }
    }

    public readonly struct ServerDisconnect : IEvent
    {
        public readonly NetworkClient client;

        public ServerDisconnect(NetworkClient client)
        {
            this.client = client;
        }
    }

    public readonly struct ServerReady : IEvent
    {
        public readonly NetworkClient client;

        public ServerReady(NetworkClient client)
        {
            this.client = client;
        }
    }

    public readonly struct ClientConnect : IEvent
    {
    }

    public readonly struct ClientDisconnect : IEvent
    {
    }

    public readonly struct ServerLoadScene : IEvent
    {
        public readonly string sceneName;

        public ServerLoadScene(string sceneName)
        {
            this.sceneName = sceneName;
        }
    }

    public readonly struct ServerSceneLoaded : IEvent
    {
        public ServerSceneLoaded(string sceneName)
        {
            this.sceneName = sceneName;
        }

        public readonly string sceneName;
    }

    public readonly struct ClientLoadScene : IEvent
    {
        public readonly string sceneName;

        public ClientLoadScene(string sceneName)
        {
            this.sceneName = sceneName;
        }
    }

    public readonly struct ClientSceneLoaded : IEvent
    {
        public readonly string sceneName;

        public ClientSceneLoaded(string sceneName)
        {
            this.sceneName = sceneName;
        }
    }

    public readonly struct ServerResponse : IEvent
    {
        public readonly string address;
        public readonly ushort port;

        public ServerResponse(string address, ushort port)
        {
            this.address = address;
            this.port = port;
        }
    }

    public readonly struct LobbyUpdate : IEvent
    {
        public readonly LobbyData[] rooms;

        public LobbyUpdate(LobbyData[] rooms)
        {
            this.rooms = rooms;
        }
    }

    public readonly struct LobbyDisconnect : IEvent
    {
    }

    public readonly struct LobbyCreateRoom : IEvent
    {
        public readonly int index;
        public readonly string room;

        public LobbyCreateRoom(int index, string room)
        {
            this.index = index;
            this.room = room;
        }
    }

    public readonly struct PingUpdate : IEvent
    {
        public readonly double pingTime;

        public PingUpdate(double pingTime)
        {
            this.pingTime = pingTime;
        }
    }
}