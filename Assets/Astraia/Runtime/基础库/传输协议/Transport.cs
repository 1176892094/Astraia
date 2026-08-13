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

namespace Astraia
{
    internal readonly struct Setting
    {
        public readonly uint MaxUnit;
        public readonly uint Timeout;
        public readonly uint Interval;
        public readonly uint DeadLink;
        public readonly uint FastResend;
        public readonly uint SendWindow;
        public readonly uint ReceiveWindow;
        public readonly bool NoDelay;
        public readonly bool DualMode;
        public readonly bool Congestion;

        public Setting(uint MaxUnit = Kcp.IKCP_MTU_DEF, uint Timeout = 10000, uint Interval = 10, uint DeadLink = Kcp.IKCP_DEADLINK, uint FastResend = 0, uint SendWindow = Kcp.IKCP_WND_SND, uint ReceiveWindow = Kcp.IKCP_WND_RCV, bool NoDelay = true, bool DualMode = true, bool Congestion = false)
        {
            this.MaxUnit = MaxUnit;
            this.Timeout = Timeout;
            this.Interval = Interval;
            this.DeadLink = DeadLink;
            this.FastResend = FastResend;
            this.SendWindow = SendWindow;
            this.ReceiveWindow = ReceiveWindow;
            this.NoDelay = NoDelay;
            this.DualMode = DualMode;
            this.Congestion = Congestion;
        }
    }

    [Serializable]
    internal abstract class Transport
    {
        public string address = "localhost";
        public ushort port = 20974;

        public KcpClient client;
        public KcpServer server;
        public abstract void Register(bool isRemote);
        public abstract uint GetLength(int pass);
        public abstract void SendToClient(int clientId, ArraySegment<byte> segment, int pass = Pass.KCP);
        public abstract void SendToServer(ArraySegment<byte> segment, int pass = Pass.KCP);
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

    [Serializable]
    internal sealed class NetworkTransport : Transport
    {
        private const uint MAX_MTU = 1200;
        private const uint TIME_OUT = 10000;
        private const uint INTERVAL = 10;
        private const uint DEAD_LINK = 40;
        private const uint FAST_RESEND = 2;
        private const uint SEND_WIN = 1024 * 4;
        private const uint RECEIVE_WIN = 1024 * 4;

        public override void Register(bool isRemote)
        {
            var setting = new Setting(MAX_MTU, TIME_OUT, INTERVAL, DEAD_LINK, FAST_RESEND, SEND_WIN, RECEIVE_WIN);
            client = new KcpClient(setting);
            server = new KcpServer(setting);
            if (isRemote)
            {
                server.onError = OnServerError;
            }
            else
            {
                client.onError = OnClientError;
            }
        }

        private static void OnServerError(int clientId, Error error, string message)
        {
            if (error != Error.解析失败 && error != Error.连接超时)
            {
                Log.Warn("客户端: {0}  错误代码: {1}\n{2}".Format(clientId, error, message));
            }
        }

        private static void OnClientError(Error error, string message)
        {
            Log.Warn("错误代码: {0}\n{1}", error, message);
        }

        public override uint GetLength(int pass)
        {
            return pass == Pass.KCP ? KcpPeer.KcpLength(MAX_MTU, RECEIVE_WIN) : KcpPeer.UdpLength(MAX_MTU);
        }

        public override void SendToClient(int clientId, ArraySegment<byte> segment, int pass = Pass.KCP)
        {
            server.Send(clientId, segment, pass);
        }

        public override void SendToServer(ArraySegment<byte> segment, int pass = Pass.KCP)
        {
            client.Send(segment, pass);
        }

        public override void StartServer()
        {
            server.Connect(port);
        }

        public override void StopServer()
        {
            server.StopServer();
        }

        public override void Disconnect(int clientId)
        {
            server.Disconnect(clientId);
        }

        public override void StartClient()
        {
            client.Connect(address, port);
        }

        public override void StopClient()
        {
            client.Disconnect();
        }

        public override void ClientEarlyUpdate()
        {
            client.EarlyUpdate();
        }

        public override void ClientAfterUpdate()
        {
            client.AfterUpdate();
        }

        public override void ServerEarlyUpdate()
        {
            server.EarlyUpdate();
        }

        public override void ServerAfterUpdate()
        {
            server.AfterUpdate();
        }
    }
}