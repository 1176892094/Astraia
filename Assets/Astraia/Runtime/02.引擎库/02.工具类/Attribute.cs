// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-04-09 22:04:37
// // # Recently: 2025-04-09 22:04:37
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;

namespace Astraia
{
    [AttributeUsage(AttributeTargets.Field)]
    public class InjectAttribute : Attribute
    {
    }
    
    [AttributeUsage(AttributeTargets.Property)]
    public class PrimaryAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class UIGroupAttribute : Attribute
    {
        public readonly int groupMask;

        public UIGroupAttribute(int groupMask) => this.groupMask = groupMask;
    }
    [AttributeUsage(AttributeTargets.Class)]
    public class UILayerAttribute : Attribute
    {
        public readonly int layerMask;

        public UILayerAttribute(int layerMask) => this.layerMask = layerMask;
    }
}