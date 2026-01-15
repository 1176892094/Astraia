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
using UnityEngine;

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
    public class UIMaskAttribute : Attribute
    {
        public readonly int layerMask;
        public readonly int groupMask;

        public UIMaskAttribute(int layerMask, int groupMask = 0)
        {
            this.layerMask = layerMask;
            this.groupMask = groupMask;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class UIPathAttribute : Attribute
    {
        public readonly string assetPath;
        public UIPathAttribute(string assetPath) => this.assetPath = assetPath;
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class UIRectAttribute : Attribute
    {
        public readonly bool direction;
        public readonly bool selection;
        public readonly Rect assetRect;

        public UIRectAttribute(int x, int y, float w, float h, bool direction = true, bool selection = false)
        {
            this.direction = direction;
            this.selection = selection;
            assetRect = new Rect(x, y, w, h);
        }
    }
}