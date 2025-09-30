// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-09-30 20:09:52
// // # Recently: 2025-09-30 20:09:52
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using System.Collections.Generic;
using Astraia.Common;

namespace Astraia.Net
{
    using MessageDelegate = Action<NetworkClient, MemoryReader, int>;

    public static class NetworkRegister
    {
        private static readonly Dictionary<ushort, MessageDelegate> clientMessages = new Dictionary<ushort, MessageDelegate>();
        private static readonly Dictionary<ushort, MessageDelegate> serverMessages = new Dictionary<ushort, MessageDelegate>();

        public static bool ServerMessage(ushort message, out MessageDelegate onExecute)
        {
            return serverMessages.TryGetValue(message, out onExecute);
        }

        public static bool ClientMessage(ushort message, out MessageDelegate onExecute)
        {
            return clientMessages.TryGetValue(message, out onExecute);
        }

        public static void AddMessage<T>(Action<T> onReceive) where T : struct, IMessage
        {
            clientMessages[NetworkMessage<T>.Id] = (client, reader, channel) =>
            {
                try
                {
                    var position = reader.position;
                    var message = reader.Invoke<T>();
                    NetworkDebugger.OnReceive(message, reader.position - position);
                    onReceive.Invoke(message);
                }
                catch (Exception e)
                {
                    Log.Error("{0} 调用失败。传输通道: {1}\n{2}", typeof(T).Name, channel, e);
                    client.Disconnect();
                }
            };
        }

        public static void AddMessage<T>(Action<NetworkClient, T> onReceive) where T : struct, IMessage
        {
            serverMessages[NetworkMessage<T>.Id] = (client, reader, channel) =>
            {
                try
                {
                    var position = reader.position;
                    var message = reader.Invoke<T>();
                    NetworkDebugger.OnReceive(message, reader.position - position);
                    onReceive.Invoke(client, message);
                }
                catch (Exception e)
                {
                    Log.Error("{0} 调用失败。传输通道: {1}\n{2}", typeof(T).Name, channel, e);
                    client.Disconnect();
                }
            };
        }

        public static void AddMessage<T>(Action<NetworkClient, T, int> onReceive) where T : struct, IMessage
        {
            serverMessages[NetworkMessage<T>.Id] = (client, reader, channel) =>
            {
                try
                {
                    var position = reader.position;
                    var message = reader.Invoke<T>();
                    NetworkDebugger.OnReceive(message, reader.position - position);
                    onReceive.Invoke(client, message, channel);
                }
                catch (Exception e)
                {
                    Log.Error("{0} 调用失败。传输通道: {1}\n{2}", typeof(T).Name, channel, e);
                    client.Disconnect();
                }
            };
        }
    }
}