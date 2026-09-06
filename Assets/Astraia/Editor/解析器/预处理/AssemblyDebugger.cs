// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-09-06 18:09:55
// # Recently: 2026-09-06 18:36:55
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System.Collections.Generic;
using Mono.Cecil;
using Unity.CompilationPipeline.Common.Diagnostics;

namespace Astraia
{
    internal sealed class AssemblyDebugger
    {
        private readonly List<DiagnosticMessage> messages = new List<DiagnosticMessage>();

        private void Add(object message, MemberReference member, DiagnosticType mode)
        {
            var reason = message.ToString();
            if (member != null)
            {
                reason = "{0} [{1}]".Format(reason, member.ToString().Color("G"));
            }

            foreach (var result in reason.Split('\n'))
            {
                var item = new DiagnosticMessage();
                item.File = string.Empty;
                item.MessageData = result;
                item.DiagnosticType = mode;
                messages.Add(item);
            }
        }

        public void Warn(object message, MemberReference member = null)
        {
            Add(message, member, DiagnosticType.Warning);
        }

        public void Error(object message, MemberReference member = null)
        {
            Add(message, member, DiagnosticType.Error);
        }

        public static implicit operator List<DiagnosticMessage>(AssemblyDebugger processor)
        {
            return processor.messages;
        }
    }
}