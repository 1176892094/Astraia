// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2024-12-21 23:12:50
// # Recently: 2024-12-22 20:12:21
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using UnityEngine;

namespace Astraia.Net
{
    public sealed class LobbyTransport : Transport
    {
        public Transport connection;
        public string roomName;
        public string roomData;
        public RoomMode roomMode;

        private void Awake()
        {
            NetworkManager.Lobby.connection = connection;
        }

        public override int GetLength(int channel)
        {
            return connection.GetLength(channel);
        }

        public override void SendToClient(int clientId, ArraySegment<byte> segment, int channel = Channel.Reliable)
        {
            if (NetworkManager.Lobby.players.TryGetValue(clientId, out var playerId))
            {
                using var writer = MemoryWriter.Pop();
                writer.WriteByte((byte)Lobby.同步网络数据);
                writer.WriteArraySegment(segment);
                writer.WriteInt(playerId);
                connection.SendToServer(writer);
            }
        }

        public override void SendToServer(ArraySegment<byte> segment, int channel = Channel.Reliable)
        {
            using var writer = MemoryWriter.Pop();
            writer.WriteByte((byte)Lobby.同步网络数据);
            writer.WriteArraySegment(segment);
            writer.WriteInt(0);
            connection.SendToServer(writer);
        }

        public override void StartServer()
        {
            if (!NetworkManager.Lobby.isConnected)
            {
                Debug.Log(Log.E208);
                return;
            }

            if (NetworkManager.Lobby.isClient || NetworkManager.Lobby.isServer)
            {
                Debug.Log(Log.E205);
                return;
            }

            NetworkManager.Lobby.isServer = true;
            using var writer = MemoryWriter.Pop();
            writer.WriteByte((byte)Lobby.请求创建房间);
            writer.WriteString(roomName);
            writer.WriteString(roomData);
            writer.WriteInt(NetworkManager.Instance.connection);
            writer.WriteByte((byte)roomMode);
            connection.SendToServer(writer);
        }

        public override void StopServer()
        {
            if (NetworkManager.Lobby.isServer)
            {
                NetworkManager.Lobby.isServer = false;
                using var writer = MemoryWriter.Pop();
                writer.WriteByte((byte)Lobby.请求离开房间);
                connection.SendToServer(writer);
            }
        }

        public override void Disconnect(int clientId)
        {
            if (NetworkManager.Lobby.players.TryGetValue(clientId, out var playerId))
            {
                using var writer = MemoryWriter.Pop();
                writer.WriteByte((byte)Lobby.请求移除玩家);
                writer.WriteInt(playerId);
                connection.SendToServer(writer);
            }
        }

        public override void StartClient()
        {
            if (!NetworkManager.Lobby.isConnected)
            {
                Debug.Log(Log.E208);
                return;
            }

            if (NetworkManager.Lobby.isClient || NetworkManager.Lobby.isServer)
            {
                Debug.Log(Log.E205);
                return;
            }

            NetworkManager.Lobby.isClient = true;
            using var writer = MemoryWriter.Pop();
            writer.WriteByte((byte)Lobby.请求加入房间);
            writer.WriteString(connection.address);
            connection.SendToServer(writer);
        }

        public override void StartClient(Uri uri)
        {
            if (uri != null)
            {
                connection.address = uri.Host;
            }

            StartClient();
        }

        public override void Disconnect()
        {
            if (NetworkManager.Lobby.isActive)
            {
                NetworkManager.Lobby.isClient = false;
                using var writer = MemoryWriter.Pop();
                writer.WriteByte((byte)Lobby.请求离开房间);
                connection.SendToServer(writer);
            }
        }

        public override void ClientEarlyUpdate()
        {
            connection.ClientEarlyUpdate();
        }

        public override void ClientAfterUpdate()
        {
            connection.ClientAfterUpdate();
        }

        public override void ServerEarlyUpdate()
        {
        }

        public override void ServerAfterUpdate()
        {
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