// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2025-04-23 16:04:19
// # Recently: 2025-04-23 16:04:19
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Collections.Generic;
using Astraia.Core;
using UnityEngine;
using UnityEngine.Networking;

namespace Astraia.Net
{
    public partial class NetworkManager
    {
        [Serializable]
        internal static class Saloon
        {
            internal static readonly Dictionary<int, int> clients = new Dictionary<int, int>();

            internal static readonly Dictionary<int, int> players = new Dictionary<int, int>();

            internal static State state = State.断开连接;

            private static int objectId;

            internal static bool isClient;

            internal static bool isServer;
            internal static bool isActive => state == State.连接成功;

            internal static void Start()
            {
                isRemote = true;
                NetworkTransport.Instance.client.Connect -= Connect;
                NetworkTransport.Instance.client.Disconnect -= Disconnect;
                NetworkTransport.Instance.client.Receive -= Receive;
                NetworkTransport.Instance.client.Connect += Connect;
                NetworkTransport.Instance.client.Disconnect += Disconnect;
                NetworkTransport.Instance.client.Receive += Receive;
                NetworkTransport.Instance.port = Transport.port;
                NetworkTransport.Instance.address = Transport.address;
                NetworkTransport.Instance.StartClient();
            }

            internal static async void Update()
            {
                using var request = UnityWebRequest.Get("http://{0}:{1}/api/compressed/servers".Format(Transport.address, Transport.port));
                await request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Log.Warn("无法获取服务器列表: {0}:{1}\n{2}", Transport.address, Transport.port, request.result);
                    return;
                }

                var rooms = Zip.Decompress(request.downloadHandler.text);
                var jsons = JsonManager.FromJson<LobbyData[]>("{{\"value\":{0}}}".Format(rooms));
                EventManager.Invoke(new LobbyUpdate(jsons));
                Log.Info("房间信息: {0}", rooms);
            }

            internal static void Submit(RoomMode roomMode)
            {
                using var writer = MemoryWriter.Pop();
                writer.WriteByte((byte)Lobby.更新房间数据);
                writer.WriteString(Instance.roomName);
                writer.WriteString(Instance.roomData);
                writer.WriteInt32(Instance.maxPlayer);
                writer.WriteInt32((byte)roomMode);
                NetworkTransport.Instance.SendToServer(writer);
            }

            private static void Connect()
            {
                state = State.正在连接;
            }

            internal static void Disconnect()
            {
                if (isSaloon)
                {
                    objectId = 0;
                    clients.Clear();
                    players.Clear();
                    isServer = false;
                    isClient = false;
                    state = State.断开连接;
                    NetworkTransport.Instance.StopClient();
                    EventManager.Invoke(new LobbyDisconnect());
                }

                isRemote = false;
            }

            private static void Receive(ArraySegment<byte> segment, int channel)
            {
                try
                {
                    using var reader = MemoryReader.Pop(segment);
                    var opcode = (Lobby)reader.ReadByte();
                    if (opcode == Lobby.身份验证成功)
                    {
                        using var writer = MemoryWriter.Pop();
                        writer.WriteByte((byte)Lobby.请求进入大厅);
                        writer.WriteString(Instance.roomGuid);
                        NetworkTransport.Instance.SendToServer(writer);
                    }
                    else if (opcode == Lobby.进入大厅成功)
                    {
                        state = State.连接成功;
                        Update();
                    }
                    else if (opcode == Lobby.创建房间成功)
                    {
                        var index = reader.ReadInt32();
                        var address = reader.ReadString();
                        NetworkTransport.Instance.address = address;
                        EventManager.Invoke(new LobbyCreateRoom(index, address));
                    }
                    else if (opcode == Lobby.加入房间成功)
                    {
                        if (isServer)
                        {
                            objectId++;
                            var clientId = reader.ReadInt32();
                            clients.Add(clientId, objectId);
                            players.Add(objectId, clientId);
                            Transport.server.Connect(objectId);
                        }

                        if (isClient)
                        {
                            Transport.client.Connect();
                        }
                    }
                    else if (opcode == Lobby.离开房间成功)
                    {
                        if (isClient)
                        {
                            isClient = false;
                            Transport.client.Disconnect();
                        }
                    }
                    else if (opcode == Lobby.同步网络数据)
                    {
                        var message = reader.ReadArraySegment();
                        if (isServer)
                        {
                            var clientId = reader.ReadInt32();
                            if (clients.TryGetValue(clientId, out var playerId))
                            {
                                Transport.server.Receive(playerId, message, channel);
                            }
                        }

                        if (isClient)
                        {
                            Transport.client.Receive(message, channel);
                        }
                    }
                    else if (opcode == Lobby.断开玩家连接)
                    {
                        if (isServer)
                        {
                            var clientId = reader.ReadInt32();
                            if (clients.TryGetValue(clientId, out var playerId))
                            {
                                Transport.server.Disconnect(playerId);
                                clients.Remove(clientId);
                                players.Remove(playerId);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Warn(e.Message);
                }
            }
        }
    }
}