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
        private static List<Type, IAgent> GetAgents(Entity owner)
        {
            if (!agentData.TryGetValue(owner, out var agents))
            {
                agents = new List<Type, IAgent>();
                agentData.Add(owner, agents);
            }

            return agents;
        }

        private static List<Entity, IAgent> GetQueries(Type agent)
        {
            if (!entityData.TryGetValue(agent, out var queries))
            {
                queries = new List<Entity, IAgent>();
                entityData.Add(agent, queries);
            }

            return queries;
        }

        public static IAgent AddAgent(Entity owner, IAgent agent)
        {
            var keyType = agent.GetType();
            var agents = GetAgents(owner);
            var queries = GetQueries(keyType);
            if (!agents.ContainsKey(keyType))
            {
                queries.Add(owner, agent);
                agents.Add(keyType, agent);
                AddLogic(owner, agent, keyType);
            }

            return agent;
        }

        public static IAgent AddAgent(Entity owner, IAgent agent, Type queryType)
        {
            var keyType = agent.GetType();
            var agents = GetAgents(owner);
            var queries = GetQueries(queryType);
            if (!agents.ContainsKey(keyType))
            {
                queries.Add(owner, agent);
                agents.Add(keyType, agent);
                AddLogic(owner, agent, keyType);
            }

            return agent;
        }


        public static IAgent AddAgent(Entity owner, Type keyType, Type realType)
        {
            var agents = GetAgents(owner);
            var queries = GetQueries(keyType);
            if (!agents.TryGetValue(keyType, out var agent))
            {
                agent = HeapManager.Dequeue<IAgent>(realType);
                queries.Add(owner, agent);
                agents.Add(keyType, agent);
                AddLogic(owner, agent, realType);
            }

            return agent;
        }

        public static IAgent AddAgent(Entity owner, Type keyType, Type realType, Type queryType)
        {
            var agents = GetAgents(owner);
            var queries = GetQueries(queryType);
            if (!agents.TryGetValue(keyType, out var agent))
            {
                agent = HeapManager.Dequeue<IAgent>(realType);
                queries.Add(owner, agent);
                agents.Add(keyType, agent);
                AddLogic(owner, agent, realType);
            }

            return agent;
        }

        private static void AddLogic(Entity owner, IAgent agent, Type realType)
        {
            try
            {
                agent.SetData(owner);
                agent.Dequeue();
                owner.OnShow += agent.OnShow;
                owner.OnHide += agent.OnHide;
                owner.OnFade += Reset;

                void Reset()
                {
                    agent.Enqueue();
                    HeapManager.Enqueue(agent, realType);
                    foreach (var query in entityData.Values)
                    {
                        query.Remove(owner);
                    }

                    agentData.Remove(owner);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning(Service.Text.Format("无法添加代理组件: {0} 类型: {1}\n{2}", agent, realType, e), owner);
            }
        }

        public static IAgent GetAgent(Entity owner, Type keyType)
        {
            if (agentData.TryGetValue(owner, out var agents))
            {
                if (agents.TryGetValue(keyType, out var agent))
                {
                    return agent;
                }
            }

            return null;
        }

        public static void Dispose()
        {
            foreach (var agents in agentData.Values)
            {
                agents.Clear();
            }

            agentData.Clear();
        }
    }
}