// // *********************************************************************************
// // # Project: JFramework
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-04-23 15:04:38
// // # Recently: 2025-04-23 15:04:38
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;

namespace Astraia
{
    internal static class Log
    {
        public static Action<string> Info = Console.WriteLine;
        public static Action<string> Warn = Console.WriteLine;
        public static Action<string> Error = Console.Error.WriteLine;
    }

    internal static class Logs
    {
        public const string E101 = "发送缓冲: {0} => {1} : {2:F}";
        public const string E102 = "接收缓冲: {0} => {1} : {2:F}";

        public const string E111 = "试图在未知的传输通道发送消息!";
        public const string E112 = "{0}: 发送可靠消息失败。消息大小: {1}";
        public const string E113 = "{0}: 发送不可靠消息失败。消息大小: {1}";
        public const string E114 = "{0}: 接收无效的网络消息。消息类型: {1}";

        public const string E121 = "服务器已经连接!";
        public const string E122 = "服务器不支持双连接模式!\n{0}";
        public const string E123 = "服务器发送消息失败!\n{0}";
        public const string E124 = "服务器接收消息失败!\n{0}";
        public const string E125 = "客户端 {0} 连接到服务器。";
        public const string E126 = "客户端 {0} 从服务器断开。";
        public const string E127 = "客户端 {0} 移除验证: {1} 预期: {2}";

        public const string E128 = "客户端已经连接!";
        public const string E129 = "客户端没有连接，发送消息失败！";
        public const string E130 = "客户端连接到: {0} : {1}";
        public const string E131 = "客户端发送消息失败!\n{0}";
        public const string E132 = "客户端接收消息失败!\n{0}";
        public const string E133 = "客户端重新验证。旧: {0} 新: {1}";
        public const string E134 = "客户端连接成功。";
        public const string E135 = "客户端断开连接。";

        public const string E141 = "无法解析主机地址: {0}\n{1}";
        public const string E142 = "{0}: 接收网络消息过大。消息大小: {1} < {2}。";
        public const string E143 = "{0}: 接收网络消息失败。";
        public const string E144 = "{0}: 未知的网络消息头部 {1}。";
        public const string E145 = "{0}: 发送网络消息过大。消息大小: {1}。";
        public const string E146 = "{0}: 发送网络消息失败。消息大小: {1}。";
        public const string E147 = "{0}: 尝试发送空消息。";
        public const string E148 = "{0}: 网络消息被重传了 {1} 次而没有得到确认！";
        public const string E149 = "{0}: 在 {1}ms 内没有收到任何消息后的连接超时！";
        public const string E150 = "{0}: 断开连接，因为它处理数据的速度不够快！";
        public const string E151 = "{0}: 收到未通过验证的网络消息。消息类型: {1}";
        public const string E152 = "{0}: 收到无效的网络消息。消息类型: {1}";
        public const string E153 = "{0}: 网络发生异常，断开连接。\n{1}";
    }
}