// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-04-09 22:04:06
// // # Recently: 2025-04-09 22:04:06
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;

namespace Astraia
{
    [Serializable]
    internal struct PacketData : IEquatable<PacketData>
    {
        public string code;
        public string name;
        public int size;

        public PacketData(string code, string name, int size)
        {
            this.code = code;
            this.name = name;
            this.size = size;
        }

        public static bool operator ==(PacketData a, PacketData b) => a.code == b.code;

        public static bool operator !=(PacketData a, PacketData b) => a.code != b.code;

        public bool Equals(PacketData other)
        {
            return size == other.size && code == other.code && name == other.name;
        }

        public override bool Equals(object obj)
        {
            return obj is PacketData other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(code, name, size);
        }
    }
}