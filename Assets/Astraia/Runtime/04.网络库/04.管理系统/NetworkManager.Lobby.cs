// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-04-23 16:04:19
// // # Recently: 2025-04-23 16:04:19
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using System.Collections.Generic;
using Astraia.Common;
using UnityEngine;
using UnityEngine.Networking;

namespace Astraia.Net
{
    public partial class NetworkManager
    {
        [Serializable]
        public static partial class Lobby
        {
            internal static readonly Dictionary<int, int> clients = new Dictionary<int, int>();

            internal static readonly Dictionary<int, int> players = new Dictionary<int, int>();

            internal static State state = State.Disconnect;

            private static int objectId;

            internal static bool isClient;

            internal static bool isServer;

            internal static Transport transport;

            private static ushort port => Transport.Instance.port;

            private static string address => Transport.Instance.address;

            public static bool isActive => state == State.Connected;

            internal static void Start()
            {
                AddMessage();
                transport.port = port;
                transport.address = address;
                transport.StartClient();
            }

            internal static void Stop()
            {
                if (isLobby)
                {
                    objectId = 0;
                    clients.Clear();
                    players.Clear();
                    isServer = false;
                    isClient = false;
                    state = State.Disconnect;
                    EventManager.Invoke(new LobbyDisconnect());
                    transport.Disconnect();
                }
            }

            public static async void UpdateLobby()
            {
                if (!isActive)
                {
                    Service.Log.Warn("您必须连接到大厅以请求房间列表!");
                    return;
                }

                using var request = UnityWebRequest.Get("http://{0}:{1}/api/compressed/servers".Format(address, port));
                await request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Service.Log.Warn("无法获取服务器列表: {0}:{1}", address, port);
                    return;
                }

                var rooms = Service.Zip.Decompress(request.downloadHandler.text);
                var jsons = JsonManager.FromJson<RoomData[]>("{{\"value\":{0}}}".Format(rooms));
                EventManager.Invoke(new LobbyUpdate(jsons));
                Service.Log.Info("房间信息: {0}", rooms);
            }

            public static void UpdateRoom(string roomName, string roomData, RoomMode roomMode)
            {
                if (!isServer)
                {
                    Service.Log.Warn("您必须连接到大厅以更新房间信息!");
                    return;
                }

                using var writer = MemoryWriter.Pop();
                writer.WriteByte((byte)Astraia.Lobby.更新房间数据);
                writer.WriteString(roomName);
                writer.WriteString(roomData);
                writer.WriteByte((byte)roomMode);
                writer.WriteInt(Instance.connection);
                transport.SendToServer(writer);
            }
        }

        public static partial class Lobby
        {
            private static void AddMessage()
            {
                transport.OnClientConnect -= OnClientConnect;
                transport.OnClientDisconnect -= OnClientDisconnect;
                transport.OnClientReceive -= OnClientReceive;
                transport.OnClientConnect += OnClientConnect;
                transport.OnClientDisconnect += OnClientDisconnect;
                transport.OnClientReceive += OnClientReceive;
            }

            private static void OnClientConnect()
            {
                if (transport == null)
                {
                    Service.Log.Error("没有连接到有效的传输！");
                    return;
                }

                state = State.Connect;
            }

            private static void OnClientDisconnect()
            {
                Stop();
            }

            private static void OnClientReceive(ArraySegment<byte> segment, int channel)
            {
                try
                {
                    using var reader = MemoryReader.Pop(segment);
                    var opcode = (Astraia.Lobby)reader.ReadByte();
                    if (opcode == Astraia.Lobby.身份验证成功)
                    {
                        using var writer = MemoryWriter.Pop();
                        writer.WriteByte((byte)Astraia.Lobby.请求进入大厅);
                        writer.WriteString(Instance.authorization);
                        transport.SendToServer(writer);
                    }
                    else if (opcode == Astraia.Lobby.进入大厅成功)
                    {
                        state = State.Connected;
                        UpdateLobby();
                    }
                    else if (opcode == Astraia.Lobby.创建房间成功)
                    {
                        transport.address = reader.ReadString();
                        EventManager.Invoke(new LobbyCreateRoom(transport.address));
                    }
                    else if (opcode == Astraia.Lobby.加入房间成功)
                    {
                        if (isServer)
                        {
                            objectId++;
                            var clientId = reader.ReadInt();
                            clients.Add(clientId, objectId);
                            players.Add(objectId, clientId);
                            Transport.Instance.OnServerConnect.Invoke(objectId);
                        }

                        if (isClient)
                        {
                            Transport.Instance.OnClientConnect.Invoke();
                        }
                    }
                    else if (opcode == Astraia.Lobby.离开房间成功)
                    {
                        if (isClient)
                        {
                            isClient = false;
                            Transport.Instance.OnClientDisconnect.Invoke();
                        }
                    }
                    else if (opcode == Astraia.Lobby.同步网络数据)
                    {
                        var message = reader.ReadArraySegment();
                        if (isServer)
                        {
                            var clientId = reader.ReadInt();
                            if (clients.TryGetValue(clientId, out var playerId))
                            {
                                Transport.Instance.OnServerReceive.Invoke(playerId, message, channel);
                            }
                        }

                        if (isClient)
                        {
                            Transport.Instance.OnClientReceive.Invoke(message, channel);
                        }
                    }
                    else if (opcode == Astraia.Lobby.断开玩家连接)
                    {
                        if (isServer)
                        {
                            var clientId = reader.ReadInt();
                            if (clients.TryGetValue(clientId, out var playerId))
                            {
                                Transport.Instance.OnServerDisconnect.Invoke(playerId);
                                clients.Remove(clientId);
                                players.Remove(playerId);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Service.Log.Warn(e.Message);
                }
            }
        }
    }
}