// // *********************************************************************************
// // # Project: JFramework
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

            private static State state = State.Disconnect;

            private static int objectId;

            internal static bool isClient;

            internal static bool isServer;

            internal static Transport connection;

            public static bool isActive => state != State.Disconnect;

            public static bool isConnected => state == State.Connected;

            private static ushort port => Transport.Instance.port;

            private static string address => Transport.Instance.address;


            internal static void Start()
            {
                Register();
                connection.port = port;
                connection.address = address;
                connection.StartClient();
            }

            internal static void Stop()
            {
                if (isActive)
                {
                    objectId = 0;
                    clients.Clear();
                    players.Clear();
                    isServer = false;
                    isClient = false;
                    state = State.Disconnect;
                    EventManager.Invoke(new LobbyDisconnect());
                    connection.StopClient();
                }
            }

            public static async void UpdateLobby()
            {
                if (!isConnected)
                {
                    Debug.Log(Log.E251);
                    return;
                }

                var uri = Service.Text.Format(Log.E252, address, port);
                using var request = UnityWebRequest.Get(uri);
                await request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning(Service.Text.Format(Log.E253, address, port));
                    return;
                }

                var rooms = Service.Zip.Decompress(request.downloadHandler.text);
                var jsons = JsonManager.FromJson<RoomData[]>(Service.Text.Format(Log.E254, rooms));
                EventManager.Invoke(new LobbyUpdate(jsons));
                Debug.Log(Service.Text.Format(Log.E255, rooms));
            }

            public static void UpdateRoom(string roomName, string roomData, RoomMode roomMode)
            {
                if (!isServer)
                {
                    Debug.Log(Log.E256);
                    return;
                }

                using var writer = MemoryWriter.Pop();
                writer.WriteByte((byte)OpCodes.UpdateRoom);
                writer.WriteString(roomName);
                writer.WriteString(roomData);
                writer.WriteByte((byte)roomMode);
                writer.WriteInt(Instance.connection);
                connection.SendToServer(writer);
            }
        }

        public static partial class Lobby
        {
            private static void Register()
            {
                connection.OnClientConnect -= OnClientConnect;
                connection.OnClientDisconnect -= OnClientDisconnect;
                connection.OnClientReceive -= OnClientReceive;
                connection.OnClientConnect += OnClientConnect;
                connection.OnClientDisconnect += OnClientDisconnect;
                connection.OnClientReceive += OnClientReceive;
            }

            private static void OnClientConnect()
            {
                if (connection == null)
                {
                    Debug.LogError(Log.E257);
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
                    var opcode = (OpCodes)reader.ReadByte();
                    if (opcode == OpCodes.Connect)
                    {
                        using var writer = MemoryWriter.Pop();
                        writer.WriteByte((byte)OpCodes.Connected);
                        writer.WriteString(Instance.authorization);
                        connection.SendToServer(writer);
                    }
                    else if (opcode == OpCodes.Connected)
                    {
                        state = State.Connected;
                        UpdateLobby();
                    }
                    else if (opcode == OpCodes.CreateRoom)
                    {
                        connection.address = reader.ReadString();
                    }
                    else if (opcode == OpCodes.JoinRoom)
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
                    else if (opcode == OpCodes.LeaveRoom)
                    {
                        if (isClient)
                        {
                            isClient = false;
                            Transport.Instance.OnClientDisconnect.Invoke();
                        }
                    }
                    else if (opcode == OpCodes.UpdateData)
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
                    else if (opcode == OpCodes.KickRoom)
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
                    Debug.LogWarning(e.ToString());
                }
            }
        }
    }
}