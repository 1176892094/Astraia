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
using Astraia.Net;

namespace Astraia.Common
{
    // public record ServerConnect(NetworkClient client) : IEvent;
    //
    // public record ServerDisconnect(NetworkClient client) : IEvent;
    //
    // public record ServerReady(NetworkClient client) : IEvent;

    public record ClientConnect : IEvent;

    public record ClientDisconnect : IEvent;

    public record ClientNotReady : IEvent;

    public record ServerLoadScene(string sceneName) : IEvent;

    public record ServerSceneLoaded(string sceneName) : IEvent;

    public record ClientLoadScene(string sceneName) : IEvent;

    public record ClientSceneLoaded(string sceneName) : IEvent;

    public record ServerResponse(Uri uri) : IEvent;
    
    // public record ServerObserver(NetworkEntity entity) : IEvent;
    //
    // public record LobbyUpdate(RoomData[] rooms) : IEvent;

    public record LobbyDisconnect : IEvent;

    public record LobbyCreateRoom(string room) : IEvent;

    public record PingUpdate : IEvent;
}