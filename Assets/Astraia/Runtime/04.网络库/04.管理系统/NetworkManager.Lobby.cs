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
        internal static class Lobby
        {
            internal static readonly Dictionary<int, int> clients = new Dictionary<int, int>();

            internal static readonly Dictionary<int, int> players = new Dictionary<int, int>();

            internal static State state = State.Disconnect;

            private static int objectId;

            internal static bool isClient;

            internal static bool isServer;
            internal static bool isActive => state == State.Connected;
            internal static Transport Account => AccountTransport.Transport;

            internal static void Start()
            {
                isRemote = true;
                Account.OnClientConnect -= OnClientConnect;
                Account.OnClientDisconnect -= OnClientDisconnect;
                Account.OnClientReceive -= OnClientReceive;
                Account.OnClientConnect += OnClientConnect;
                Account.OnClientDisconnect += OnClientDisconnect;
                Account.OnClientReceive += OnClientReceive;
                Account.port = Transport.port;
                Account.address = Transport.address;
                Account.StartClient();
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
                    Account.Disconnect();
                    EventManager.Invoke(new LobbyDisconnect());
                }

                isRemote = false;
            }

            internal static async void Update()
            {
                using var request = UnityWebRequest.Get("http://{0}:{1}/api/compressed/servers".Format(Transport.address, Transport.port));
                await request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Service.Log.Warn("无法获取服务器列表: {0}:{1}", Transport.address, Transport.port);
                    return;
                }

                var rooms = Service.Zip.Decompress(request.downloadHandler.text);
                var jsons = JsonManager.FromJson<LobbyData[]>("{{\"value\":{0}}}".Format(rooms));
                EventManager.Invoke(new LobbyUpdate(jsons));
                Service.Log.Info("房间信息: {0}", rooms);
            }

            internal static void Submit(RoomMode roomMode)
            {
                using var writer = MemoryWriter.Pop();
                writer.WriteByte((byte)Astraia.Lobby.更新房间数据);
                writer.WriteString(Instance.roomName);
                writer.WriteString(Instance.roomData);
                writer.WriteByte((byte)roomMode);
                writer.WriteInt(Instance.maxPlayer);
                Account.SendToServer(writer);
            }

            private static void OnClientConnect()
            {
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
                        writer.WriteString(Instance.roomGuid);
                        Account.SendToServer(writer);
                    }
                    else if (opcode == Astraia.Lobby.进入大厅成功)
                    {
                        state = State.Connected;
                        Update();
                    }
                    else if (opcode == Astraia.Lobby.创建房间成功)
                    {
                        Account.address = reader.ReadString();
                        EventManager.Invoke(new LobbyCreateRoom(Account.address));
                    }
                    else if (opcode == Astraia.Lobby.加入房间成功)
                    {
                        if (isServer)
                        {
                            objectId++;
                            var clientId = reader.ReadInt();
                            clients.Add(clientId, objectId);
                            players.Add(objectId, clientId);
                            Transport.OnServerConnect.Invoke(objectId);
                        }

                        if (isClient)
                        {
                            Transport.OnClientConnect.Invoke();
                        }
                    }
                    else if (opcode == Astraia.Lobby.离开房间成功)
                    {
                        if (isClient)
                        {
                            isClient = false;
                            Transport.OnClientDisconnect.Invoke();
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
                                Transport.OnServerReceive.Invoke(playerId, message, channel);
                            }
                        }

                        if (isClient)
                        {
                            Transport.OnClientReceive.Invoke(message, channel);
                        }
                    }
                    else if (opcode == Astraia.Lobby.断开玩家连接)
                    {
                        if (isServer)
                        {
                            var clientId = reader.ReadInt();
                            if (clients.TryGetValue(clientId, out var playerId))
                            {
                                Transport.OnServerDisconnect.Invoke(playerId);
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