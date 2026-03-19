// *********************************************************************************
// # Project: Forest
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2025-01-12 15:01:52
// # Recently: 2025-01-12 15:01:52
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System.Collections.Generic;
using Mono.Cecil;

namespace Astraia.Editor
{
    internal class SyncVarAccess
    {
        public readonly Dictionary<FieldDefinition, MethodDefinition> getter = new Dictionary<FieldDefinition, MethodDefinition>();

        public readonly Dictionary<FieldDefinition, MethodDefinition> setter = new Dictionary<FieldDefinition, MethodDefinition>();

        private readonly Dictionary<string, int> syncVars = new Dictionary<string, int>();

        public int GetSyncVar(string className)
        {
            return syncVars.TryGetValue(className, out var value) ? value : 0;
        }

        public void SetSyncVar(string className, int index)
        {
            syncVars[className] = index;
        }

        public void Clear()
        {
            setter.Clear();
            getter.Clear();
            syncVars.Clear();
        }
    }
}