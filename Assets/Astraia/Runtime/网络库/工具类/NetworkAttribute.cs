// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-09-02 23:09:16
// # Recently: 2026-09-03 14:21:02
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Collections.Generic;

namespace Astraia.Net
{
    public static class NetworkAttribute
    {
        private static readonly Dictionary<ushort, SyncData> messages = new Dictionary<ushort, SyncData>();

        public static void RegisterServerRpc(Type module, int pass, string name, SyncFunc func)
        {
            AddHook(module, pass, name, SyncMode.服务器, func);
        }

        public static void RegisterClientRpc(Type module, int pass, string name, SyncFunc func)
        {
            AddHook(module, pass, name, SyncMode.客户端, func);
        }

        private static void AddHook(Type module, int pass, string name, SyncMode mode, SyncFunc func)
        {
            var id = (ushort)(NetworkMessage.Id(name) & 0xFFFF);
            if (messages.TryGetValue(id, out var message))
            {
                Log.Error($"远程调用 [{module} {func.Method.Name}] 与 [{message.module} {message.func.Method.Name}] 冲突。");
                return;
            }

            message = new SyncData(pass, mode, func, module);
            messages[id] = message;

            if (message.mode != mode || message.module != module || message.func != func)
            {
                Log.Error($"远程调用 [{module} {func.Method.Name}] 与 [{message.module} {message.func.Method.Name}] 冲突。");
            }
        }

        internal static bool HasHook(ushort id)
        {
            if (messages.TryGetValue(id, out var message))
            {
                return (message.pass & Pass.ANY) == 0 && message.mode == SyncMode.服务器;
            }

            return false;
        }

        internal static SyncFunc GetHook(ushort id)
        {
            if (messages.TryGetValue(id, out var message))
            {
                return message.func;
            }

            return null;
        }

        internal static bool Invoke(ushort id, SyncMode mode, NetworkClient client, MemoryReader reader, NetworkModule component)
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

        private struct SyncData
        {
            public readonly int pass;
            public readonly Type module;
            public readonly SyncMode mode;
            public readonly SyncFunc func;

            public SyncData(int pass, SyncMode mode, SyncFunc func, Type module)
            {
                this.pass = pass;
                this.mode = mode;
                this.func = func;
                this.module = module;
            }
        }
    }

    public delegate void SyncFunc(NetworkModule module, MemoryReader reader, NetworkClient client);
}