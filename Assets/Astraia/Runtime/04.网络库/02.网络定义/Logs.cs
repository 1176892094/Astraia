// // *********************************************************************************
// // # Project: JFramework
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-04-23 16:04:59
// // # Recently: 2025-04-23 16:04:59
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

namespace Astraia
{
    internal static class Logs
    {
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
        public const string E226 = "无法注册网络对象 {0}。持有多个网络对象组件。";
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
        public const string E246 = "网络对象 {0} 没有 NetworkObject 组件";
        public const string E247 = "网络对象 {0} 已经生成。";
        public const string E248 = "在客户端 {0} 找到了空的网络对象。";

        //NetworkManager.Lobby
        public const string E251 = "您必须连接到大厅以请求房间列表!";
        public const string E252 = "http://{0}:{1}/api/compressed/servers";
        public const string E253 = "无法获取服务器列表: {0}:{1}";
        public const string E254 = "{\"value\":{0}}";
        public const string E255 = "房间信息: {0}";
        public const string E256 = "您必须连接到大厅以更新房间信息!";
        public const string E257 = "没有连接到有效的传输！";

        //NetworkBehaviour
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
        
        //NetworkObject
        public const string E271 = "网络对象持有的 网络行为组件 为空";
        public const string E272 = "网络对象持有的 网络行为组件 的数量不能超过 64";
        public const string E273 = "找不到场景对象的预制父物体。对象名称: {0}";
        public const string E274 = "请保存场景后，再进行构建。";
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