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
        public static UIPage<T> Set<T>(this UIPage<T> self, Type value)
        {
            self.assetType = value;
            return self;
        }
        
        public static UIPage<T> Set<T>(this UIPage<T> self, int x, int y, int width, int height)
        {
            self.assetRect = new Rect(x, y, width, height);
            return self;
        }

        public static UIPage<T> Set<T>(this UIPage<T> self, UIPage value)
        {
            self.direction = value;
            return self;
        }

        public static UIPage<T> Set<T>(this UIPage<T> self, RectTransform value)
        {
            self.content = value;
            return self;
        }

        public static UIPage<T> Set<T>(this UIPage<T> self, bool value)
        {
            self.selection = value;
            return self;
        }
    }
}