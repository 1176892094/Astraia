// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-08-31 16:08:49
// // # Recently: 2025-08-31 16:08:49
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using UnityEngine;

namespace Astraia
{
    public static partial class Extensions
    {
        public static UIPanel<T> Set<T>(this UIPanel<T> self, Type value)
        {
            self.assetType = value;
            return self;
        }

        public static UIPanel<T> Set<T>(this UIPanel<T> self, int value)
        {
            self.direction = value != 0;
            return self;
        }

        public static UIPanel<T> Set<T>(this UIPanel<T> self, int x, int y, int w, int h)
        {
            self.assetRect = new Rect(x, y, w, h);
            return self;
        }
        
        public static UIPanel<T> Set<T>(this UIPanel<T> self, bool value)
        {
            self.selection = value;
            return self;
        }
    }
}