// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-08-26 11:08:45
// // # Recently: 2025-08-26 11:08:45
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using System.Collections.Generic;

namespace Astraia
{
    public sealed partial class List<Key, Value>
    {
        [Serializable]
        public struct Node : IEquatable<Node>
        {
            public Key Key;
            public Value Value;

            public Node(Key key, Value value)
            {
                Key = key;
                Value = value;
            }

            public bool Equals(Node other)
            {
                return EqualityComparer<Key>.Default.Equals(Key, other.Key) && EqualityComparer<Value>.Default.Equals(Value, other.Value);
            }

            public override bool Equals(object obj)
            {
                return obj is Node other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Key, Value);
            }
        }
    }
}