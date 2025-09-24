// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-04-09 22:04:28
// // # Recently: 2025-04-09 22:04:28
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using UnityEngine;

namespace Astraia
{
    public static partial class Extensions
    {
        public static T GetOrAddComponent<T>(this Transform transform) where T : Component
        {
            var component = transform.GetComponent<T>();
            if (component == null)
            {
                component = transform.gameObject.AddComponent<T>();
            }

            return component;
        }

        public static Component GetOrAddComponent(this Transform transform, Type type)
        {
            var component = transform.GetComponent(type);
            if (component == null)
            {
                component = transform.gameObject.AddComponent(type);
            }

            return component;
        }

        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            var component = gameObject.GetComponent<T>();
            if (component == null)
            {
                component = gameObject.AddComponent<T>();
            }

            return component;
        }

        public static Component GetOrAddComponent(this GameObject gameObject, Type type)
        {
            var component = gameObject.GetComponent(type);
            if (component == null)
            {
                component = gameObject.AddComponent(type);
            }

            return component;
        }
    }
}