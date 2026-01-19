// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2025-01-10 21:01:21
// # Recently: 2025-01-11 18:01:34
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************


using System;
using System.Collections.Generic;
using Astraia.Core;

namespace Astraia.Net
{
    using MessageDelegate = Action<NetworkClient, MemoryReader, int>;

    internal static class NetworkMessage
    {
        public static readonly Dictionary<ushort, MessageDelegate> client = new Dictionary<ushort, MessageDelegate>();
        public static readonly Dictionary<ushort, MessageDelegate> server = new Dictionary<ushort, MessageDelegate>();

        public static uint Id(string name)
        {
            var result = 23U;
            unchecked
            {
                foreach (var c in name)
                {
                    result = result * 31 + c;
                }

                return result;
            }
        }
    }

    public static class NetworkMessage<T> where T : struct, IMessage
    {
        public static readonly ushort Id = (ushort)NetworkMessage.Id(typeof(T).FullName);

        public static void Add(Action<T> onReceive)
        {
            NetworkMessage.client[Id] = (client, reader, channel) =>
            {
                try
                {
                    var position = reader.position;
                    var message = reader.Invoke<T>();
                    Debugger.OnReceive(message, reader.position - position);
                    onReceive.Invoke(message);
                }
                catch (Exception e)
                {
                    Service.Log.Error("{0} 调用失败。传输通道: {1}\n{2}", typeof(T).Name, channel, e);
                    client.Disconnect();
                }
            };
        }

        public static void Add(Action<NetworkClient, T> onReceive)
        {
            NetworkMessage.server[Id] = (client, reader, channel) =>
            {
                try
                {
                    var position = reader.position;
                    var message = reader.Invoke<T>();
                    Debugger.OnReceive(message, reader.position - position);
                    onReceive.Invoke(client, message);
                }
                catch (Exception e)
                {
                    Service.Log.Error("{0} 调用失败。传输通道: {1}\n{2}", typeof(T).Name, channel, e);
                    client.Disconnect();
                }
            };
        }

        public static void Add(Action<NetworkClient, T, int> onReceive)
        {
            NetworkMessage.server[Id] = (client, reader, channel) =>
            {
                try
                {
                    var position = reader.position;
                    var message = reader.Invoke<T>();
                    Debugger.OnReceive(message, reader.position - position);
                    onReceive.Invoke(client, message, channel);
                }
                catch (Exception e)
                {
                    Service.Log.Error("{0} 调用失败。传输通道: {1}\n{2}", typeof(T).Name, channel, e);
                    client.Disconnect();
                }
            };
        }
    }
}