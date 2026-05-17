// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2025-01-10 21:01:21
// # Recently: 2025-01-11 14:01:57
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Collections.Generic;

namespace Astraia.Net
{
    public delegate void InvokeDelegate(NetworkModule component, MemoryReader reader, NetworkClient client);

    public static class NetworkAttribute
    {
        private static readonly Dictionary<ushort, InvokeData> messages = new Dictionary<ushort, InvokeData>();

        public static void RegisterServerRpc(Type component, int channel, string name, InvokeDelegate func)
        {
            AddInvoke(component, channel, name, InvokeMode.ServerRpc, func);
        }

        public static void RegisterClientRpc(Type component, int channel, string name, InvokeDelegate func)
        {
            AddInvoke(component, channel, name, InvokeMode.ClientRpc, func);
        }

        private static void AddInvoke(Type component, int channel, string name, InvokeMode mode, InvokeDelegate func)
        {
            var id = (ushort)(NetworkMessage.Id(name) & 0xFFFF);
            if (!messages.TryGetValue(id, out var message))
            {
                message = new InvokeData(mode, func, component, channel);
                messages[id] = message;
            }

            if (message.mode != mode || message.component != component || message.func != func)
            {
                Log.Error("远程调用 [{0} {1}] 与 [{2} {3}] 冲突。", component, func.Method.Name, message.component, message.func.Method.Name);
            }
        }

        internal static bool HasInvoke(ushort id)
        {
            if (messages.TryGetValue(id, out var message) && message != null)
            {
                return (message.channel & Channel.IgnoreOwner) == 0 && message.mode == InvokeMode.ServerRpc;
            }

            return false;
        }

        internal static InvokeDelegate GetInvoke(ushort id)
        {
            if (messages.TryGetValue(id, out var message) && message != null)
            {
                return message.func;
            }

            return null;
        }

        internal static bool Invoke(ushort id, InvokeMode mode, NetworkClient client, MemoryReader reader, NetworkModule component)
        {
            if (messages.TryGetValue(id, out var message) && message != null)
            {
                if (message.mode != mode)
                {
                    return false;
                }

                if (!message.component.IsInstanceOfType(component)) // 判断是否是 NetworkModule 的实例或派生类型的实例
                {
                    return false;
                }

                message.func.Invoke(component, reader, client);
                return true;
            }

            return false;
        }

        private record InvokeData(InvokeMode mode, InvokeDelegate func, Type component, int channel);
    }
}