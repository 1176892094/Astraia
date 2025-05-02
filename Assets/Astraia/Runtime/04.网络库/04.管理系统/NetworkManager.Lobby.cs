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
using Astraia.Common;
using UnityEngine;
using UnityEngine.Networking;

namespace Astraia.Net
{
    public partial class NetworkManager
    {
        public static partial class Lobby
        {
            internal static readonly BiDictionary<int, int> clients = new BiDictionary<int, int>();

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

                using var setter = MemorySetter.Pop();
                setter.SetByte((byte)OpCodes.UpdateRoom);
                setter.SetString(roomName);
                setter.SetString(roomData);
                setter.SetByte((byte)roomMode);
                setter.SetInt(Instance.connection);
                connection.SendToServer(setter);
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
                    using var getter = MemoryGetter.Pop(segment);
                    var opcode = (OpCodes)getter.GetByte();
                    if (opcode == OpCodes.Connect)
                    {
                        using var setter = MemorySetter.Pop();
                        setter.SetByte((byte)OpCodes.Connected);
                        setter.SetString(Instance.authorization);
                        connection.SendToServer(setter);
                    }
                    else if (opcode == OpCodes.Connected)
                    {
                        state = State.Connected;
                        UpdateLobby();
                    }
                    else if (opcode == OpCodes.CreateRoom)
                    {
                        connection.address = getter.GetString();
                    }
                    else if (opcode == OpCodes.JoinRoom)
                    {
                        if (isServer)
                        {
                            objectId++;
                            var clientId = getter.GetInt();
                            clients.Add(clientId, objectId);
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
                        var message = getter.GetArraySegment();
                        if (isServer)
                        {
                            var clientId = getter.GetInt();
                            if (clients.TryGetByKey(clientId, out var playerId))
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
                            var clientId = getter.GetInt();
                            if (clients.TryGetByKey(clientId, out var playerId))
                            {
                                Transport.Instance.OnServerDisconnect.Invoke(playerId);
                                clients.RemoveByKey(clientId);
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