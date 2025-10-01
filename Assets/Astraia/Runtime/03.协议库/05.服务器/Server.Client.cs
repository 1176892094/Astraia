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
        private sealed class Client : Peer
        {
            public readonly EndPoint endPoint;
            private readonly Action onDisconnect;
            private readonly Action<Client> onConnect;
            private readonly Action<Error, string> onError;
            private readonly Action<ArraySegment<byte>> onSend;
            private readonly Action<ArraySegment<byte>, int> onReceive;

            public Client(Action<Client> onConnect, Action onDisconnect, Action<Error, string> onError, Action<ArraySegment<byte>, int> onReceive,
                Action<ArraySegment<byte>> onSend, Setting setting, uint userData, EndPoint endPoint) : base(setting, userData)
            {
                this.onSend = onSend;
                this.onError = onError;
                this.onConnect = onConnect;
                this.onReceive = onReceive;
                this.onDisconnect = onDisconnect;
                this.endPoint = endPoint;
                state = State.Connect;
            }


            protected override void OnConnected()
            {
                SendReliable(Reliable.Connect);
                onConnect.Invoke(this);
            }

            protected override void OnDisconnect() => onDisconnect.Invoke();

            protected override void Send(ArraySegment<byte> segment) => onSend.Invoke(segment);

            protected override void Data(ArraySegment<byte> segment, int channel) => onReceive.Invoke(segment, channel);

            protected override void OnError(Error error, string message) => onError.Invoke(error, message);

            public void Input(ArraySegment<byte> segment)
            {
                if (segment.Count <= 1 + 4)
                {
                    return;
                }

                var channel = segment.Array![segment.Offset];
                var result = Process.Decode(segment.Array, segment.Offset + 1);

                if (state == State.Connected)
                {
                    if (result != userData)
                    {
                        Log.Info("客户端 {0} 移除验证: {1} 预期: {2}", endPoint, result, userData);
                        return;
                    }
                }

                Input(new ArraySegment<byte>(segment.Array, segment.Offset + 1 + 4, segment.Count - 1 - 4), channel);
            }
        }
    }
}