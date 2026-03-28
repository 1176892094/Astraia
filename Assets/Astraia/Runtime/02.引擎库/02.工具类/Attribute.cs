// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2025-04-09 22:04:37
// # Recently: 2025-04-09 22:04:37
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

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
    public class UIMaskAttribute : Attribute
    {
        public readonly int layer;
        public readonly int group;

        public UIMaskAttribute(int layer, int group = 0)
        {
            this.layer = layer;
            this.group = group;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class UIPathAttribute : Attribute
    {
        public readonly string asset;
        public UIPathAttribute(string asset) => this.asset = asset;
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class UIRectAttribute : Attribute
    {
        public readonly int row;
        public readonly int col;
        public readonly float width;
        public readonly float height;
        public readonly bool rotation;

        public UIRectAttribute(int col, int row, float width, float height, bool rotation = true)
        {
            this.width = width;
            this.height = height;
            this.rotation = rotation;
            this.col = rotation ? col : col + 1;
            this.row = rotation ? row + 1 : row;
        }
    }
}