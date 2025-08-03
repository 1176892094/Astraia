// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2025-01-11 18:01:44
// # Recently: 2025-01-11 18:01:44
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Net;
using Astraia.Common;

namespace Astraia
{
    internal sealed partial class Server
    {
        private sealed class Client : Agent
        {
            public readonly EndPoint endPoint;

            public Client(Action<Client> OnConnect, Action OnDisconnect, Action<Error, string> OnError, Action<ArraySegment<byte>, int> OnReceive, Action<ArraySegment<byte>> OnSend, Setting setting, uint cookie, EndPoint endPoint) : base(setting, cookie)
            {
                this.OnSend = OnSend;
                this.OnError = OnError;
                this.OnConnect = OnConnect;
                this.OnReceive = OnReceive;
                this.OnDisconnect = OnDisconnect;
                this.endPoint = endPoint;
                state = State.Connect;
            }

            private event Action OnDisconnect;
            private event Action<Client> OnConnect;
            private event Action<Error, string> OnError;
            private event Action<ArraySegment<byte>> OnSend;
            private event Action<ArraySegment<byte>, int> OnReceive;

            protected override void Connected()
            {
                SendReliable(Reliable.Connect);
                OnConnect?.Invoke(this);
            }

            protected override void Disconnected() => OnDisconnect?.Invoke();

            protected override void Send(ArraySegment<byte> segment) => OnSend?.Invoke(segment);

            protected override void Receive(ArraySegment<byte> message, int channel) => OnReceive?.Invoke(message, channel);

            protected override void LogError(Error error, string message) => OnError?.Invoke(error, message);

            public void Input(ArraySegment<byte> segment)
            {
                if (segment.Count <= 1 + 4)
                {
                    return;
                }

                var channel = segment.Array[segment.Offset];
                var result = Utils.Decode32U(segment.Array, segment.Offset + 1);

                if (state == State.Connected)
                {
                    if (result != cookie)
                    {
                        Logs.Info(Service.Text.Format(Log.E127, endPoint, result, cookie));
                        return;
                    }
                }

                Input(channel, new ArraySegment<byte>(segment.Array, segment.Offset + 1 + 4, segment.Count - 1 - 4));
            }
        }
    }
}