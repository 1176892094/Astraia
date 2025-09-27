// // *********************************************************************************
// // # Project: Astraia
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
    internal static partial class Log
    {
        private static Action<string> onInfo = Console.WriteLine;
        private static Action<string> onWarn = Console.WriteLine;
        private static Action<string> onError = Console.Error.WriteLine;

        public static void Setup(Action<string> onInfo, Action<string> onWarn, Action<string> onError)
        {
            Log.onInfo = onInfo;
            Log.onWarn = onWarn;
            Log.onError = onError;
        }

        public static void Info(object message)
        {
            onInfo.Invoke(message.ToString());
        }

        public static void Warn(object message)
        {
            onWarn.Invoke(message.ToString());
        }

        public static void Error(object message)
        {
            onError.Invoke(message.ToString());
        }
    }

    internal static partial class Log
    {
        public static void Info<T>(string format, T arg1)
        {
            Info(format.Format(arg1));
        }

        public static void Info<T1, T2>(string format, T1 arg1, T2 arg2)
        {
            Info(format.Format(arg1, arg2));
        }

        public static void Info<T1, T2, T3>(string format, T1 arg1, T2 arg2, T3 arg3)
        {
            Info(format.Format(arg1, arg2, arg3));
        }

        public static void Info<T1, T2, T3, T4>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            Info(format.Format(arg1, arg2, arg3, arg4));
        }

        public static void Warn<T>(string format, T arg1)
        {
            Warn(format.Format(arg1));
        }

        public static void Warn<T1, T2>(string format, T1 arg1, T2 arg2)
        {
            Warn(format.Format(arg1, arg2));
        }

        public static void Warn<T1, T2, T3>(string format, T1 arg1, T2 arg2, T3 arg3)
        {
            Warn(format.Format(arg1, arg2, arg3));
        }

        public static void Warn<T1, T2, T3, T4>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            Warn(format.Format(arg1, arg2, arg3, arg4));
        }

        public static void Error<T>(string format, T arg1)
        {
            Error(format.Format(arg1));
        }

        public static void Error<T1, T2>(string format, T1 arg1, T2 arg2)
        {
            Error(format.Format(arg1, arg2));
        }

        public static void Error<T1, T2, T3>(string format, T1 arg1, T2 arg2, T3 arg3)
        {
            Error(format.Format(arg1, arg2, arg3));
        }

        public static void Error<T1, T2, T3, T4>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            Error(format.Format(arg1, arg2, arg3, arg4));
        }
    }

    internal static partial class Log
    {
        public const string E101 = "发送缓冲: {0} => {1} : {2:F}";
        public const string E102 = "接收缓冲: {0} => {1} : {2:F}";

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
        public const string E151 = "{0}: 收到未通过验证的网络消息。消息类型: {1} 消息长度: {2}";
        public const string E152 = "{0}: 收到无效的网络消息。消息类型: {1}";
        public const string E153 = "{0}: 网络发生异常，断开连接。\n{1}";

        //NetworkManager
        public const string E201 = "服务器已经连接!";
        public const string E202 = "服务器已经停止!";
        public const string E203 = "客户端已经连接!";
        public const string E204 = "客户端已经停止!";
        public const string E205 = "客户端或服务器已经连接!";
        public const string E206 = "大厅服务器已经连接!";
        public const string E207 = "大厅服务器已经停止!";
        public const string E208 = "没有连接到大厅!";
        public const string E209 = "网络对象的网络标识无效。";
        public const string E210 = "错误代码: {0} => {1}";

        //NetworkManager.Client
        public const string E211 = "没有连接到有效的服务器！";
        public const string E212 = "客户端已经准备就绪！";
        public const string E213 = "客户端不能加载空场景！";
        public const string E214 = "客户端正在加载 {0} 场景";
        public const string E215 = "{0} 调用失败。传输通道: {1}\n{2}";
        public const string E216 = "无法同步网络对象: {0}";
        public const string E217 = "客户端没有通过验证，无法加载场景。";
        public const string E218 = "客户端场景对象重复。网络对象: {0} {1}";
        public const string E219 = "没有连接到有效的服务器！";
        public const string E220 = "无法处理来自服务器的消息。";
        public const string E221 = "无法处理来自服务器的消息。没有头部。";
        public const string E222 = "无法处理来自服务器的消息。未知的消息{0}";
        public const string E223 = "无法处理来自服务器的消息。残留消息: {0}";
        public const string E224 = "无法注册网络对象 {0}。没有网络对象组件。\",";
        public const string E225 = "无法注册网络对象 {0}。因为该预置体为场景对象。";
        public const string E227 = "无法注册网络对象 {0}。场景标识无效。";

        //NetworkManager.Server
        public const string E231 = "服务器不能加载空场景！";
        public const string E232 = "服务器正在加载 {0} 场景";
        public const string E233 = "{0} 调用失败。传输通道: {1}\n{2}";
        public const string E234 = "无法为客户端 {0} 同步网络对象: {1}";
        public const string E235 = "无法为客户端 {0} 反序列化网络对象: {1}";
        public const string E236 = "无法为客户端 {0} 进行远程调用，未准备就绪。";
        public const string E237 = "无法为客户端 {0} 进行远程调用，未找到对象 {1}。";
        public const string E238 = "无法为客户端 {0} 进行远程调用，未通过验证 {1}。";
        public const string E239 = "无法为客户端 {0} 建立通信连接。";
        public const string E240 = "无法为客户端 {0} 进行处理消息。未知客户端。";
        public const string E241 = "无法为客户端 {0} 进行处理消息。";
        public const string E242 = "无法为客户端 {0} 进行处理消息。没有头部。";
        public const string E243 = "无法为客户端 {0} 进行处理消息。未知的消息 {1}。";
        public const string E244 = "无法为客户端 {0} 进行处理消息。残留消息: {1}。";
        public const string E245 = "服务器不是活跃的。";
        public const string E246 = "网络对象 {0} 没有 NetworkEntity 组件";
        public const string E247 = "网络对象 {0} 已经生成。";
        public const string E248 = "在客户端 {0} 找到了空的网络对象。";

        //NetworkManager.Lobby
        public const string E251 = "您必须连接到大厅以请求房间列表!";
        public const string E253 = "无法获取服务器列表: {0}:{1}";
        public const string E255 = "房间信息: {0}";
        public const string E256 = "您必须连接到大厅以更新房间信息!";
        public const string E257 = "没有连接到有效的传输！";

        //NetworkModule
        public const string E260 = "序列化对象失败。对象名称: {0}[{1}][{2}]\n{3}";
        public const string E261 = "反序列化字节不匹配。读取字节: {0} 哈希对比:{1}/{2}";
        public const string E262 = "调用 {0} 但是客户端不是活跃的。";
        public const string E263 = "调用 {0} 但是客户端没有准备就绪的。对象名称：{1}";
        public const string E264 = "调用 {0} 但是客户端没有对象权限。对象名称：{1}";
        public const string E265 = "调用 {0} 但是客户端的连接为空。对象名称：{1}";
        public const string E266 = "调用 {0} 但是服务器不是活跃的。";
        public const string E267 = "调用 {0} 但是对象未初始化。对象名称：{1}";
        public const string E268 = "调用 {0} 但是对象的连接为空。对象名称：{1}";
        public const string E269 = "设置网络变量的对象未初始化。对象名称: {0}";

        //NetworkEntity
        public const string E274 = "网络对象 {0} 在构建前需要打开并重新保存。因为网络对象 {1} 没有场景Id";
        public const string E275 = "Assigned AssetId";
        public const string E276 = "调用了已经删除的网络对象。{0} [{1}] {2}";
        public const string E277 = "网络对象 {0} 没有找到网络行为组件 {1}";
        public const string E278 = "无法调用{0} [{1}] 网络对象: {2} 网络标识: {3}";

        // Other
        public const string E290 = "远程调用 [{0} {1}] 与 [{2} {3}] 冲突。";
        public const string E291 = "发送消息大小过大！消息大小: {0}";
        public const string E292 = "网络发现不支持WebGL";
        public const string E293 = "接收到的消息版本不同！";
    }
}