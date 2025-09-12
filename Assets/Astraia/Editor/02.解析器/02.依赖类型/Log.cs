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
    internal interface ILog
    {
        void Warn(string message, MemberReference member = null);
        void Error(string message, MemberReference member = null);
    }

    internal class Log : ILog
    {
        public readonly List<DiagnosticMessage> logs = new List<DiagnosticMessage>();

        public void Warn(string message, MemberReference member = null)
        {
            Add(message, member, DiagnosticType.Warning);
        }

        public void Error(string message, MemberReference member = null)
        {
            Add(message, member, DiagnosticType.Error);
        }

        private void Add(string message, MemberReference member, DiagnosticType mode)
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
    }
}