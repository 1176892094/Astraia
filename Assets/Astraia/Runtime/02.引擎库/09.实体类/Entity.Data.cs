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

using System;
using Astraia.Common;
using UnityEngine;

namespace Astraia
{
    using static GlobalManager;

    internal static class EntityManager
    {
        public static void AddAgent(int id, IAgent agent)
        {
            if (!entityData.TryGetValue(id, out var agents))
            {
                agents = new List<Type, IAgent>();
                entityData.Add(id, agents);
            }

            var realType = agent.GetType();
            if (!agents.ContainsKey(realType))
            {
                agents.Add(realType, agent);
                AddEvent(id, agent, realType);
            }
        }

        public static void AddAgent(int id, Type baseType, Type realType)
        {
            if (!entityData.TryGetValue(id, out var agents))
            {
                agents = new List<Type, IAgent>();
                entityData.Add(id, agents);
            }

            if (!agents.TryGetValue(baseType, out var agent))
            {
                agent = HeapManager.Dequeue<IAgent>(realType);
                agents.Add(baseType, agent);
                AddEvent(id, agent, realType);
            }
        }

        private static void AddEvent(int id, IAgent agent, Type type)
        {
            var owner = (Entity)Resources.EntityIdToObject(id);
            try
            {
                agent.OnAwake(id);
                agent.Dequeue();
                owner.OnShow += agent.OnShow;
                owner.OnHide += agent.OnHide;
                owner.OnFade += Faded;

                void Faded()
                {
                    agent.Enqueue();
                    HeapManager.Enqueue(agent, type);
                    entityData.Remove(owner);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning(Service.Text.Format("无法添加代理组件: {0} 类型: {1}\n{2}", agent, type, e), owner);
            }
        }

        public static T GetAgent<T>(int id) where T : IAgent
        {
            if (entityData.TryGetValue(id, out var agents))
            {
                if (agents.TryGetValue(typeof(T), out var agent))
                {
                    return (T)agent;
                }
            }

            return default;
        }

        public static void Dispose()
        {
            foreach (var agents in entityData.Values)
            {
                agents.Clear();
            }

            entityData.Clear();
        }
    }
}