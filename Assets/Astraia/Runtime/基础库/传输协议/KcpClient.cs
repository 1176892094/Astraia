// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2025-08-03 02:08:22
// # Recently: 2025-08-03 02:08:22
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Net;
using System.Net.Sockets;

namespace Astraia
{
    [Serializable]
    internal sealed class KcpClient
    {
        private readonly byte[] buffer = new byte[Const.MTU_DEF];
        private State state = State.断开连接;

        private Socket socket;
        private KcpPeer kcpPeer;
        private EndPoint endPoint;

        public Action<int> onConnect;
        public Action onDisconnect;
        public Action<Error, string> onError;
        public Action<ArraySegment<byte>> onSend;
        public Action<ArraySegment<byte>, int> onReceive;

        public void Connect(string address, ushort port)
        {
            try
            {
                if (state != State.断开连接)
                {
                    Log.Warn("客户端已经连接!");
                    return;
                }

                var addresses = Dns.GetHostAddresses(address);
                if (addresses.Length >= 1)
                {
                    Register();
                    state = State.正在连接;
                    endPoint = new IPEndPoint(addresses[0], port);
                    socket = new Socket(endPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
                    socket.Blocked();
                    socket.Connect(endPoint);
                    Log.Info("客户端连接到: {0} : {1}", addresses[0], port);
                    kcpPeer.Handshake(0);
                }
            }
            catch (SocketException e)
            {
                onError(Error.解析失败, "无法解析主机地址: {0}\n{1}".Format(address, e));
                onDisconnect();
            }
        }

        public void Send(ArraySegment<byte> segment, int pass)
        {
            if (state != State.断开连接)
            {
                kcpPeer.SendData(segment, pass);
                onSend?.Invoke(segment);
            }
        }

        private bool TryReceive(out ArraySegment<byte> segment)
        {
            segment = default;
            try
            {
                if (socket != null && socket.Poll(0, SelectMode.SelectRead))
                {
                    var count = socket.Receive(buffer, 0, buffer.Length, SocketFlags.None);
                    segment = new ArraySegment<byte>(buffer, 0, count);
                    return true;
                }

                return false;
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode != SocketError.WouldBlock)
                {
                    Log.Info("客户端接收消息失败!\n{0}", e);
                    kcpPeer.Disconnect();
                }

                return false;
            }
        }

        public void Disconnect()
        {
            if (state != State.断开连接)
            {
                kcpPeer.Disconnect();
            }
        }

        private void Register()
        {
            if (kcpPeer == null)
            {
                kcpPeer = new KcpPeer(nameof(KcpClient));
                kcpPeer.onConnect = OnConnect;
                kcpPeer.onDisconnect = OnDisconnect;
                kcpPeer.onError = OnError;
                kcpPeer.onReceive = OnReceive;
                kcpPeer.onSend = OnSend;
            }

            kcpPeer.Rebuild();
        }

        private void OnConnect(int serverId)
        {
            Log.Info("客户端 {0} 连接到服务器。".Format(serverId));
            state = State.连接成功;
            onConnect(serverId);
        }

        private void OnDisconnect(int serverId)
        {
            Log.Info("客户端 {0} 从服务器断开。".Format(serverId));
            state = State.断开连接;
            socket.Close();
            socket = null;
            endPoint = null;
            onDisconnect();
        }

        private void OnError(Error error, string message)
        {
            onError?.Invoke(error, message);
        }

        private void OnReceive(ArraySegment<byte> segment, int pass)
        {
            onReceive(segment, pass);
        }

        private void OnSend(ArraySegment<byte> segment)
        {
            try
            {
                if (socket != null)
                {
                    if (socket.Poll(0, SelectMode.SelectWrite))
                    {
                        socket.Send(segment.Array!, segment.Offset, segment.Count, SocketFlags.None);
                    }
                }
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode != SocketError.WouldBlock)
                {
                    Log.Info("客户端发送消息失败!\n{0}", e);
                }
            }
        }

        public void EarlyUpdate()
        {
            if (state != State.断开连接)
            {
                while (TryReceive(out var segment))
                {
                    kcpPeer.Input(segment);
                }

                kcpPeer.EarlyUpdate();
            }
        }

        public void AfterUpdate()
        {
            if (state != State.断开连接)
            {
                kcpPeer.AfterUpdate();
            }
        }
    }
}