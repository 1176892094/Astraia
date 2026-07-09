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
using System.Net.Http;
using Astraia.Core;

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
                NetworkAuthority.Instance.cEvent.Connect -= Connect;
                NetworkAuthority.Instance.cEvent.Disconnect -= Disconnect;
                NetworkAuthority.Instance.cEvent.Receive -= Receive;
                NetworkAuthority.Instance.cEvent.Connect += Connect;
                NetworkAuthority.Instance.cEvent.Disconnect += Disconnect;
                NetworkAuthority.Instance.cEvent.Receive += Receive;
                NetworkAuthority.Instance.port = Kcp.port;
                NetworkAuthority.Instance.address = Kcp.address;
                NetworkAuthority.Instance.StartClient();
            }

            internal static async void Update()
            {
                var texts = await Host.Http.GetStringAsync("http://{0}:{1}/api/compressed/servers".Format(Kcp.address, Kcp.port));
                var rooms = Zip.Decompress(texts);
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
                NetworkAuthority.Instance.SendToServer(writer);
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
                    NetworkAuthority.Instance.StopClient();
                    EventManager.Invoke(new LobbyDisconnect());
                }

                isRemote = false;
            }

            private static void Receive(ArraySegment<byte> segment, int pass)
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
                        NetworkAuthority.Instance.SendToServer(writer);
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
                        NetworkAuthority.Instance.address = address;
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
                            Kcp.sEvent.Connect(objectId);
                        }

                        if (isClient)
                        {
                            Kcp.cEvent.Connect();
                        }
                    }
                    else if (opcode == Lobby.离开房间成功)
                    {
                        if (isClient)
                        {
                            isClient = false;
                            Kcp.cEvent.Disconnect();
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
                                Kcp.sEvent.Receive(playerId, message, pass);
                            }
                        }

                        if (isClient)
                        {
                            Kcp.cEvent.Receive(message, pass);
                        }
                    }
                    else if (opcode == Lobby.断开玩家连接)
                    {
                        if (isServer)
                        {
                            var clientId = reader.ReadInt32();
                            if (clients.TryGetValue(clientId, out var playerId))
                            {
                                Kcp.sEvent.Disconnect(playerId);
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