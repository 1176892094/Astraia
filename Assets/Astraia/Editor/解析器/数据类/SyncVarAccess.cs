// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-09-06 23:09:25
// # Recently: 2026-09-06 23:20:25
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System.Collections.Generic;
using Mono.Cecil;

namespace Astraia
{
    internal class SyncVarAccess
    {
        public readonly IDictionary<FieldDefinition, MethodDefinition> getter = new Dictionary<FieldDefinition, MethodDefinition>();
        public readonly IDictionary<FieldDefinition, MethodDefinition> setter = new Dictionary<FieldDefinition, MethodDefinition>();
        private readonly IDictionary<string, int> syncVars = new Dictionary<string, int>();

        public int GetSyncVar(string className)
        {
            return syncVars.TryGetValue(className, out var value) ? value : 0;
        }

        public void SetSyncVar(string className, int index)
        {
            syncVars[className] = index;
        }
    }

    internal class SyncVarList<T>
    {
        private readonly Dictionary<T, T> syncMaps = new Dictionary<T, T>();
        private readonly List<T> syncVars = new List<T>();
        public ICollection<T> Keys => syncVars;
        public ICollection<T> Values => syncMaps.Values;
        public int Count => syncVars.Count;

        public T this[T key]
        {
            get => syncMaps[key];
            set => syncMaps[key] = value;
        }

        public void Add(T key)
        {
            syncVars.Add(key);
        }

        public void Clear()
        {
            syncVars.Clear();
            syncMaps.Clear();
        }
    }
}