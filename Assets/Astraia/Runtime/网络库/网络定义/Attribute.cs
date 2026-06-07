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
    [AttributeUsage(AttributeTargets.Field)]
    public class SyncVarAttribute : Attribute
    {
        private string func;
        public SyncVarAttribute(string func = null) => this.func = func;
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class ClientRpcAttribute : Attribute
    {
        private int pass;
        public ClientRpcAttribute(int pass = Pass.KCP) => this.pass = pass;
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class ServerRpcAttribute : Attribute
    {
        private int pass;
        public ServerRpcAttribute(int pass = Pass.KCP) => this.pass = pass;
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class TargetRpcAttribute : Attribute
    {
        private int pass;
        public TargetRpcAttribute(int pass = Pass.KCP) => this.pass = pass;
    }
}