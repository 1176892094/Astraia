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
    internal sealed class NetworkTransport : Transport
    {
        public static Transport Instance;

        private void Awake()
        {
            Instance = gameObject.AddComponent<GenericTransport>();
        }

        public override uint GetLength(int channel)
        {
            return Instance.GetLength(channel);
        }

        public override void SendToClient(int clientId, ArraySegment<byte> segment, int channel = Channel.Reliable)
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

        public override void SendToServer(ArraySegment<byte> segment, int channel = Channel.Reliable)
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

    internal abstract class Transport : MonoBehaviour
    {
        public string address = "localhost";
        public ushort port = 20974;

        public readonly CEvent client = new CEvent();
        public readonly SEvent server = new SEvent();

        public abstract uint GetLength(int channel);
        public abstract void SendToClient(int clientId, ArraySegment<byte> segment, int channel = Channel.Reliable);
        public abstract void SendToServer(ArraySegment<byte> segment, int channel = Channel.Reliable);
        public abstract void StartServer();
        public abstract void StopServer();
        public abstract void Disconnect(int clientId);
        public abstract void StartClient();
        public abstract void StopClient();
        public abstract void ClientEarlyUpdate();
        public abstract void ClientAfterUpdate();
        public abstract void ServerEarlyUpdate();
        public abstract void ServerAfterUpdate();
    }

    internal sealed class GenericTransport : Transport
    {
        private const uint MAX_MTU = 1200;
        private const uint TIME_OUT = 10000;
        private const uint INTERVAL = 10;
        private const uint DEAD_LINK = 40;
        private const uint FAST_RESEND = 2;
        private const uint SEND_WIN = 1024 * 4;
        private const uint RECEIVE_WIN = 1024 * 4;

        private KcpClient kcpClient;
        private KcpServer kcpServer;

        private void Awake()
        {
            var setting = new Setting(MAX_MTU, TIME_OUT, INTERVAL, DEAD_LINK, FAST_RESEND, SEND_WIN, RECEIVE_WIN);
            kcpClient = new KcpClient(setting, client);
            kcpServer = new KcpServer(setting, server);
            client.Error = OnError;
        }

        private static void OnError(Error error, string message)
        {
            Log.Warn("{0}: {1}", error, message);
        }

        public override uint GetLength(int channel)
        {
            return channel == Channel.Reliable ? KcpPeer.KcpLength(MAX_MTU, RECEIVE_WIN) : KcpPeer.UdpLength(MAX_MTU);
        }

        public override void SendToClient(int clientId, ArraySegment<byte> segment, int channel = Channel.Reliable)
        {
            kcpServer.Send(clientId, segment, channel);
            server.Send?.Invoke(clientId, segment);
        }

        public override void SendToServer(ArraySegment<byte> segment, int channel = Channel.Reliable)
        {
            kcpClient.Send(segment, channel);
            client.Send?.Invoke(segment);
        }

        public override void StartServer()
        {
            kcpServer.Connect(port);
        }

        public override void StopServer()
        {
            kcpServer.StopServer();
        }

        public override void Disconnect(int clientId)
        {
            kcpServer.Disconnect(clientId);
        }

        public override void StartClient()
        {
            kcpClient.Connect(address, port);
        }

        public override void StopClient()
        {
            kcpClient.Disconnect();
        }

        public override void ClientEarlyUpdate()
        {
            kcpClient.EarlyUpdate();
        }

        public override void ClientAfterUpdate()
        {
            kcpClient.AfterUpdate();
        }

        public override void ServerEarlyUpdate()
        {
            kcpServer.EarlyUpdate();
        }

        public override void ServerAfterUpdate()
        {
            kcpServer.AfterUpdate();
        }
    }
}