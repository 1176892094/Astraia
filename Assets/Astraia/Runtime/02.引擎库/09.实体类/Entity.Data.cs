// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-07-12 20:07:36
// // # Recently: 2025-07-12 20:07:36
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System.Collections.Generic;
using Astraia.Common;

namespace Astraia
{
    internal static class EntityManager
    {
        public static void Show(Entity entity)
        {
            GlobalManager.entityData[entity] = entity;
        }

        public static Entity Find(int instanceID)
        {
            return GlobalManager.entityData.GetValueOrDefault(instanceID);
        }

        public static void Hide(int instanceID)
        {
            GlobalManager.entityData.Remove(instanceID);
        }

        public static void Dispose()
        {
            GlobalManager.entityData.Clear();
        }
    }
}