// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2024-11-29 13:11:20
// # Recently: 2024-12-22 20:12:18
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;

namespace Astraia.Net
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ClientRpcAttribute : Attribute
    {
        private int channel;
        public ClientRpcAttribute(int channel = Channel.Reliable) => this.channel = channel;
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class ServerRpcAttribute : Attribute
    {
        private int channel;
        public ServerRpcAttribute(int channel = Channel.Reliable) => this.channel = channel;
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class TargetRpcAttribute : Attribute
    {
        private int channel;
        public TargetRpcAttribute(int channel = Channel.Reliable) => this.channel = channel;
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class SyncVarAttribute : Attribute
    {
        private string func;
        public SyncVarAttribute(string func = null) => this.func = func;
    }
}