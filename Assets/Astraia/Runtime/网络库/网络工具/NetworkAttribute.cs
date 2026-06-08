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
    public static class NetworkAttribute
    {
        private static readonly Dictionary<ushort, HookData> messages = new Dictionary<ushort, HookData>();

        public static void RegisterServerRpc(Type module, int pass, string name, HookFunc func)
        {
            AddHook(module, pass, name, HookMode.服务器, func);
        }

        public static void RegisterClientRpc(Type module, int pass, string name, HookFunc func)
        {
            AddHook(module, pass, name, HookMode.客户端, func);
        }

        private static void AddHook(Type module, int pass, string name, HookMode mode, HookFunc func)
        {
            var id = (ushort)(NetworkMessage.Id(name) & 0xFFFF);
            if (!messages.TryGetValue(id, out var message))
            {
                message = new HookData(pass, mode, func, module);
                messages[id] = message;
            }

            if (message.mode != mode || message.module != module || message.func != func)
            {
                Log.Error("远程调用 [{0} {1}] 与 [{2} {3}] 冲突。", module, func.Method.Name, message.module, message.func.Method.Name);
            }
        }

        internal static bool HasHook(ushort id)
        {
            if (messages.TryGetValue(id, out var message))
            {
                return (message.pass & Pass.ANY) == 0 && message.mode == HookMode.服务器;
            }

            return false;
        }

        internal static HookFunc GetHook(ushort id)
        {
            if (messages.TryGetValue(id, out var message))
            {
                return message.func;
            }

            return null;
        }

        internal static bool Invoke(ushort id, HookMode mode, NetworkClient client, MemoryReader reader, NetworkModule component)
        {
            if (messages.TryGetValue(id, out var message))
            {
                if (message.mode != mode)
                {
                    return false;
                }

                if (!message.module.IsInstanceOfType(component)) // 判断是否是 NetworkModule 的实例或派生类型的实例
                {
                    return false;
                }

                message.func.Invoke(component, reader, client);
                return true;
            }

            return false;
        }

        private struct HookData
        {
            public readonly int pass;
            public readonly Type module;
            public readonly HookMode mode;
            public readonly HookFunc func;

            public HookData(int pass, HookMode mode, HookFunc func, Type module)
            {
                this.pass = pass;
                this.mode = mode;
                this.func = func;
                this.module = module;
            }
        }
    }

    public delegate void HookFunc(NetworkModule module, MemoryReader reader, NetworkClient client);
}