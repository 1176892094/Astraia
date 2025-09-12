// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2025-01-10 21:01:21
// # Recently: 2025-01-11 14:01:56
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;

namespace Astraia.Net
{
    [Serializable]
    public struct NetworkVariable : IEquatable<NetworkVariable>
    {
        public byte moduleId;
        public uint objectId;
      

        public NetworkVariable(uint objectId, int moduleId)
        {
            this.objectId = objectId;
            this.moduleId = (byte)moduleId;
        }

        public bool Equals(uint objectId, int moduleId)
        {
            return this.objectId == objectId && this.moduleId == moduleId;
        }

        public bool Equals(NetworkVariable other)
        {
            return objectId == other.objectId && moduleId == other.moduleId;
        }

        public override bool Equals(object obj)
        {
            return obj is NetworkVariable other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(objectId, moduleId);
        }
    }
}