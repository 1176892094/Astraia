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

using System.Collections.Generic;
using Mono.Cecil;
using Unity.CompilationPipeline.Common.Diagnostics;

namespace Astraia.Editor
{
    internal interface Logger
    {
        void Warn(string message, MemberReference member = null);
        void Error(string message, MemberReference member = null);
    }

    /// <summary>
    /// 网络代码注入日志
    /// </summary>
    internal class LogPostProcessor : Logger
    {
        /// <summary>
        /// 日志列表
        /// </summary>
        public readonly List<DiagnosticMessage> logs = new List<DiagnosticMessage>();

        /// <summary>
        /// 警告日志
        /// </summary>
        /// <param name="message">日志信息</param>
        /// <param name="member">成员参数</param>
        public void Warn(string message, MemberReference member = null)
        {
            Info(message, member, DiagnosticType.Warning);
        }

        /// <summary>
        /// 错误日志
        /// </summary>
        /// <param name="message">日志信息</param>
        /// <param name="member">成员参数</param>
        public void Error(string message, MemberReference member = null)
        {
            Info(message, member, DiagnosticType.Error);
        }

        /// <summary>
        /// 添加日志信息
        /// </summary>
        /// <param name="message"></param>
        /// <param name="mode"></param>
        private void Add(string message, DiagnosticType mode)
        {
            logs.Add(new DiagnosticMessage
            {
                DiagnosticType = mode,
                File = string.Empty,
                Line = 0,
                Column = 0,
                MessageData = message
            });
        }

        /// <summary>
        /// 处理日志
        /// </summary>
        /// <param name="message">日志信息</param>
        /// <param name="member">成员参数</param>
        /// <param name="mode">日志类型</param>
        private void Info(string message, MemberReference member, DiagnosticType mode)
        {
            if (member != null)
            {
                message = "{0} (as {1})".Format(message, member);
            }

            var splits = message.Split('\n');

            if (splits.Length == 1)
            {
                Add(message, mode);
            }
            else
            {
                foreach (var split in splits)
                {
                    Add(split, mode);
                }
            }
        }
    }
}