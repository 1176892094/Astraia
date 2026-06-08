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
    [Serializable]
    internal sealed class NetworkAuthority : Transport
    {
        public static Transport Instance;

        public override uint GetLength(int pass)
        {
            return Instance.GetLength(pass);
        }

        public override void SendToClient(int clientId, ArraySegment<byte> segment, int pass = Pass.KCP)
        {
            if (NetworkManager.Saloon.players.TryGetValue(clientId, out var playerId))
            {
                using var writer = MemoryWriter.Pop();
                writer.WriteByte((byte)Lobby.同步网络数据);
                writer.WriteInt32(playerId);
                writer.WriteArraySegment(segment);
                Instance.SendToServer(writer);
            }
        }

        public override void SendToServer(ArraySegment<byte> segment, int pass = Pass.KCP)
        {
            using var writer = MemoryWriter.Pop();
            writer.WriteByte((byte)Lobby.同步网络数据);
            writer.WriteInt32(0);
            writer.WriteArraySegment(segment);
            Instance.SendToServer(writer);
        }

        public override void StartServer()
        {
            NetworkManager.Saloon.isServer = true;
            using var writer = MemoryWriter.Pop();
            writer.WriteByte((byte)Lobby.请求创建房间);
            writer.WriteString(NetworkManager.Instance.roomName);
            writer.WriteString(NetworkManager.Instance.roomData);
            writer.WriteInt32(NetworkManager.Instance.maxPlayer);
            writer.WriteInt32((byte)NetworkManager.Instance.roomMode);
            Instance.SendToServer(writer);
        }

        public override void StopServer()
        {
            if (NetworkManager.Saloon.isServer)
            {
                NetworkManager.Saloon.isServer = false;
                using var writer = MemoryWriter.Pop();
                writer.WriteByte((byte)Lobby.请求离开房间);
                Instance.SendToServer(writer);
            }
        }

        public override void Disconnect(int clientId)
        {
            if (NetworkManager.Saloon.players.TryGetValue(clientId, out var playerId))
            {
                using var writer = MemoryWriter.Pop();
                writer.WriteByte((byte)Lobby.请求移除玩家);
                writer.WriteInt32(playerId);
                Instance.SendToServer(writer);
            }
        }

        public override void StartClient()
        {
            NetworkManager.Saloon.isClient = true;
            using var writer = MemoryWriter.Pop();
            writer.WriteByte((byte)Lobby.请求加入房间);
            writer.WriteString(Instance.address);
            Instance.SendToServer(writer);
        }

        public override void StopClient()
        {
            if (NetworkManager.isSaloon)
            {
                NetworkManager.Saloon.isClient = false;
                using var writer = MemoryWriter.Pop();
                writer.WriteByte((byte)Lobby.请求离开房间);
                Instance.SendToServer(writer);
            }
        }

        public override void ClientEarlyUpdate()
        {
            Instance.ClientEarlyUpdate();
        }

        public override void ClientAfterUpdate()
        {
            Instance.ClientAfterUpdate();
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
        public int Host;
        public int Count;
        public int Index;
        public RoomMode State;
        public string Id;
        public string Name;
        public string Data;
        public int[] Members;
    }

    public enum RoomMode : byte
    {
        公开,
        私有,
        锁定,
    }
}