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

namespace Astraia.Net
{
    public sealed class LobbyTransport : Transport
    {
        public Transport transport;
        public string roomName;
        public string roomData;
        public RoomMode roomMode;

        private void Awake()
        {
            NetworkManager.Lobby.connection = transport;
        }

        public override int GetLength(int channel)
        {
            return transport.GetLength(channel);
        }

        public override void SendToClient(int clientId, ArraySegment<byte> segment, int channel = Channel.Reliable)
        {
            if (NetworkManager.Lobby.players.TryGetValue(clientId, out var playerId))
            {
                using var writer = MemoryWriter.Pop();
                writer.WriteByte((byte)Lobby.同步网络数据);
                writer.WriteArraySegment(segment);
                writer.WriteInt(playerId);
                transport.SendToServer(writer);
            }
        }

        public override void SendToServer(ArraySegment<byte> segment, int channel = Channel.Reliable)
        {
            using var writer = MemoryWriter.Pop();
            writer.WriteByte((byte)Lobby.同步网络数据);
            writer.WriteArraySegment(segment);
            writer.WriteInt(0);
            transport.SendToServer(writer);
        }

        public override void StartServer()
        {
            if (!NetworkManager.Lobby.isConnected)
            {
                Log.Warn("没有连接到大厅!");
                return;
            }

            if (NetworkManager.Lobby.isClient || NetworkManager.Lobby.isServer)
            {
                Log.Warn("客户端或服务器已经连接!");
                return;
            }

            NetworkManager.Lobby.isServer = true;
            using var writer = MemoryWriter.Pop();
            writer.WriteByte((byte)Lobby.请求创建房间);
            writer.WriteString(roomName);
            writer.WriteString(roomData);
            writer.WriteInt(NetworkManager.Instance.connection);
            writer.WriteByte((byte)roomMode);
            transport.SendToServer(writer);
        }

        public override void StopServer()
        {
            if (NetworkManager.Lobby.isServer)
            {
                NetworkManager.Lobby.isServer = false;
                using var writer = MemoryWriter.Pop();
                writer.WriteByte((byte)Lobby.请求离开房间);
                transport.SendToServer(writer);
            }
        }

        public override void Disconnect(int clientId)
        {
            if (NetworkManager.Lobby.players.TryGetValue(clientId, out var playerId))
            {
                using var writer = MemoryWriter.Pop();
                writer.WriteByte((byte)Lobby.请求移除玩家);
                writer.WriteInt(playerId);
                transport.SendToServer(writer);
            }
        }

        public override void StartClient()
        {
            if (!NetworkManager.Lobby.isConnected)
            {
                Log.Warn("没有连接到大厅!");
                return;
            }

            if (NetworkManager.Lobby.isClient || NetworkManager.Lobby.isServer)
            {
                Log.Warn("客户端或服务器已经连接!");
                return;
            }

            NetworkManager.Lobby.isClient = true;
            using var writer = MemoryWriter.Pop();
            writer.WriteByte((byte)Lobby.请求加入房间);
            writer.WriteString(transport.address);
            transport.SendToServer(writer);
        }

        public override void StartClient(Uri uri)
        {
            if (uri != null)
            {
                transport.address = uri.Host;
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
                transport.SendToServer(writer);
            }
        }

        public override void ClientEarlyUpdate()
        {
            transport.ClientEarlyUpdate();
        }

        public override void ClientAfterUpdate()
        {
            transport.ClientAfterUpdate();
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