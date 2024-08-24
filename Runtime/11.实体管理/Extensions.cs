// *********************************************************************************
// # Project: Forest
// # Unity: 2022.3.5f1c1
// # Author: jinyijie
// # Version: 1.0.0
// # History: 2024-08-25  01:08
// # Copyright: 2024, jinyijie
// # Description: This is an automatically generated comment.
// *********************************************************************************

using JFramework.Core;
using JFramework.Interface;
using UnityEngine;

namespace JFramework
{
    public static partial class Extensions
    {
        public static T Search<T>(this IEntity entity) where T : ScriptableObject, IComponent
        {
            return (T)EntityManager.GetComponent(entity, typeof(T));
        }

        public static void Destroy(this IEntity entity)
        {
            EntityManager.Destroy(entity);
        }
    }
}