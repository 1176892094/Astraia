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
        public static IAgent AddAgent(Entity owner, IAgent agent)
        {
            if (!entityData.TryGetValue(owner, out var agents))
            {
                agents = new List<Type, IAgent>();
                entityData.Add(owner, agents);
            }

            var baseType = agent.GetType();
            if (!agentData.TryGetValue(baseType, out var query))
            {
                query = new List<Entity, IAgent>();
                agentData.Add(baseType, query);
            }

            if (!agents.ContainsKey(agent.GetType()))
            {
                query.Add(owner, agent);
                agents.Add(baseType, agent);
                AddLogic(owner, agent, baseType);
            }

            return agent;
        }

        public static IAgent AddAgent(Entity owner, Type baseType, Type realType)
        {
            if (!entityData.TryGetValue(owner, out var agents))
            {
                agents = new List<Type, IAgent>();
                entityData.Add(owner, agents);
            }

            if (!agentData.TryGetValue(baseType, out var query))
            {
                query = new List<Entity, IAgent>();
                agentData.Add(baseType, query);
            }

            if (!agents.TryGetValue(baseType, out var agent))
            {
                agent = HeapManager.Dequeue<IAgent>(realType);
                query.Add(owner, agent);
                agents.Add(baseType, agent);
                AddLogic(owner, agent, realType);
            }

            return agent;
        }

        public static IAgent AddAgent(Entity owner, Type baseType, Type realType, Type agentType)
        {
            if (!entityData.TryGetValue(owner, out var agents))
            {
                agents = new List<Type, IAgent>();
                entityData.Add(owner, agents);
            }

            if (!agentData.TryGetValue(agentType, out var query))
            {
                query = new List<Entity, IAgent>();
                agentData.Add(agentType, query);
            }

            if (!agents.TryGetValue(baseType, out var agent))
            {
                agent = HeapManager.Dequeue<IAgent>(realType);
                query.Add(owner, agent);
                agents.Add(baseType, agent);
                AddLogic(owner, agent, realType);
            }

            return agent;
        }

        private static void AddLogic(Entity owner, IAgent agent, Type type)
        {
            try
            {
                agent.Setup(owner);
                agent.Dequeue();
                owner.OnShow += agent.OnShow;
                owner.OnHide += agent.OnHide;
                owner.OnFade += Reset;

                void Reset()
                {
                    agent.Enqueue();
                    HeapManager.Enqueue(agent, type);
                    foreach (var query in agentData.Values)
                    {
                        query.Remove(owner);
                    }

                    entityData.Remove(owner);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning(Service.Text.Format("无法添加代理组件: {0} 类型: {1}\n{2}", agent, type, e), owner);
            }
        }

        public static IAgent GetAgent(Entity owner, Type type)
        {
            if (entityData.TryGetValue(owner, out var agents))
            {
                if (agents.TryGetValue(type, out var agent))
                {
                    return agent;
                }
            }

            return null;
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