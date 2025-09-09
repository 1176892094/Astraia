// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2024-12-19 03:12:36
// # Recently: 2024-12-22 20:12:33
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using Mono.Cecil;

namespace Astraia.Editor
{
    internal static class Const
    {
        /// <summary>
        /// 程序集名称
        /// </summary>
        public const string ASSEMBLY = "Astraia.Net";

        /// <summary>
        /// 命名空间
        /// </summary>
        public const string GEN_TYPE = "Astraia.Net";

        /// <summary>
        /// 生成脚本名称
        /// </summary>
        public const string GEN_NAME = "NetworkGenerator";

        /// <summary>
        /// 处理方法名称
        /// </summary>
        public const string GEN_FUNC = "NetworkProcessor";

        /// <summary>
        /// 调用Rpc取代方法的方法
        /// </summary>
        public const string INV_METHOD = "_0";

        /// <summary>
        /// Rpc的取代方法
        /// </summary>
        public const string RPC_METHOD = "_1";

        /// <summary>
        /// 序列化网络变量
        /// </summary>
        public const string SER_METHOD = "SerializeSyncVars";

        /// <summary>
        /// 反序列化网络变量
        /// </summary>
        public const string DES_METHOD = "DeserializeSyncVars";

        /// <summary>
        /// 构造函数
        /// </summary>
        public const string CTOR = ".ctor";

        /// <summary>
        /// 单个网络对象携带的网络变量极限
        /// </summary>
        public const int SYNC_LIMIT = 64;

        /// <summary>
        /// Rpc调用属性
        /// </summary>
        public const MethodAttributes RPC_ATTRS = MethodAttributes.Family | MethodAttributes.Static | MethodAttributes.HideBySig;

        /// <summary>
        /// Reader 和 Writer 属性
        /// </summary>
        public const MethodAttributes RAW_ATTRS = MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig;

        /// <summary>
        /// 序列化属性
        /// </summary>
        public const MethodAttributes SER_ATTRS = MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig;

        /// <summary>
        /// 网络变量属性
        /// </summary>
        public const MethodAttributes VAR_ATTRS = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;

        /// <summary>
        /// 静态构造属性
        /// </summary>
        public const MethodAttributes CTOR_ATTRS = MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName | MethodAttributes.HideBySig;

        /// <summary>
        /// 类型属性
        /// </summary>
        public const TypeAttributes GEN_ATTRS = TypeAttributes.Class | TypeAttributes.AnsiClass | TypeAttributes.Public | TypeAttributes.AutoClass | TypeAttributes.Abstract | TypeAttributes.Sealed | TypeAttributes.BeforeFieldInit;
    }
}