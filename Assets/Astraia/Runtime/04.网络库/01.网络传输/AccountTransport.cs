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
    internal sealed class AccountTransport : Transport
    {
        public static Transport Transport;

        private void Awake()
        {
            Transport = gameObject.AddComponent<NetworkTransport>();
        }

        public override uint GetLength(int channel)
        {
            return Transport.GetLength(channel);
        }

        public override void SendToClient(int clientId, ArraySegment<byte> segment, int channel = Channel.Reliable)
        {
            if (NetworkManager.Lobby.players.TryGetValue(clientId, out var playerId))
            {
                using var writer = MemoryWriter.Pop();
                writer.WriteByte((byte)Lobby.同步网络数据);
                writer.WriteArraySegment(segment);
                writer.WriteInt(playerId);
                Transport.SendToServer(writer);
            }
        }

        public override void SendToServer(ArraySegment<byte> segment, int channel = Channel.Reliable)
        {
            using var writer = MemoryWriter.Pop();
            writer.WriteByte((byte)Lobby.同步网络数据);
            writer.WriteArraySegment(segment);
            writer.WriteInt(0);
            Transport.SendToServer(writer);
        }

        public override void StartServer()
        {
            NetworkManager.Lobby.isServer = true;
            using var writer = MemoryWriter.Pop();
            writer.WriteByte((byte)Lobby.请求创建房间);
            writer.WriteString(NetworkManager.Instance.roomName);
            writer.WriteString(NetworkManager.Instance.roomData);
            writer.WriteInt(NetworkManager.Instance.maxPlayer);
            writer.WriteByte((byte)NetworkManager.Instance.roomMode);
            Transport.SendToServer(writer);
        }

        public override void StopServer()
        {
            if (NetworkManager.Lobby.isServer)
            {
                NetworkManager.Lobby.isServer = false;
                using var writer = MemoryWriter.Pop();
                writer.WriteByte((byte)Lobby.请求离开房间);
                Transport.SendToServer(writer);
            }
        }

        public override void Disconnect(int clientId)
        {
            if (NetworkManager.Lobby.players.TryGetValue(clientId, out var playerId))
            {
                using var writer = MemoryWriter.Pop();
                writer.WriteByte((byte)Lobby.请求移除玩家);
                writer.WriteInt(playerId);
                Transport.SendToServer(writer);
            }
        }

        public override void StartClient()
        {
            NetworkManager.Lobby.isClient = true;
            using var writer = MemoryWriter.Pop();
            writer.WriteByte((byte)Lobby.请求加入房间);
            writer.WriteString(Transport.address);
            Transport.SendToServer(writer);
        }

        public override void StartClient(Uri uri)
        {
            NetworkManager.Lobby.isClient = true;
            using var writer = MemoryWriter.Pop();
            writer.WriteByte((byte)Lobby.请求加入房间);
            writer.WriteString(uri != null ? uri.Host : Transport.address);
            Transport.SendToServer(writer);
        }

        public override void Disconnect()
        {
            if (NetworkManager.isLobby)
            {
                NetworkManager.Lobby.isClient = false;
                using var writer = MemoryWriter.Pop();
                writer.WriteByte((byte)Lobby.请求离开房间);
                Transport.SendToServer(writer);
            }
        }

        public override void ClientEarlyUpdate()
        {
            Transport.ClientEarlyUpdate();
        }

        public override void ClientAfterUpdate()
        {
            Transport.ClientAfterUpdate();
        }

        public override void ServerEarlyUpdate()
        {
        }

        public override void ServerAfterUpdate()
        {
        }
    }

    [Serializable]
    public struct LobbyData
    {
        public int clientId;
        public RoomMode roomMode;
        public int maxCount;
        public string roomData;
        public string roomId;
        public string roomName;
        public int[] clients;
    }
}