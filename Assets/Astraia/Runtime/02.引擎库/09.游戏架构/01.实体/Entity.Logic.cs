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
using System.Collections.Generic;
using Astraia.Common;

namespace Astraia
{
    using static GlobalManager;

    internal static class EntityManager
    {
        private static Dictionary<Type, IAgent> GetAgents(Entity owner)
        {
            if (!agentData.TryGetValue(owner, out var agents))
            {
                agents = new Dictionary<Type, IAgent>();
                agentData.Add(owner, agents);
            }

            return agents;
        }

        private static Dictionary<Entity, IAgent> GetQueries(Type queryType)
        {
            if (!queryData.TryGetValue(queryType, out var queries))
            {
                queries = new Dictionary<Entity, IAgent>();
                queryData.Add(queryType, queries);
            }

            return queries;
        }

        public static IAgent AddAgent(Entity owner, IAgent agent, Type keyType, Type queryType = null)
        {
            queryType ??= keyType;
            var agents = GetAgents(owner);
            var queries = GetQueries(queryType);
            if (!agents.ContainsKey(keyType))
            {
                agents.Add(keyType, agent);
                queries.Add(owner, agent);
                owner.Inject(agent);
                agent.Create(owner);
                agent.Dequeue();
                owner.OnShow += agent.OnShow;
                owner.OnHide += agent.OnHide;
                owner.OnFade += Enqueue;
            }

            return agent;

            void Enqueue()
            {
                agent.Enqueue();
                agents.Remove(keyType);
                queries.Remove(owner);
                if (agents.Count == 0) agentData.Remove(owner);
                if (queries.Count == 0) queryData.Remove(keyType);
                HeapManager.Enqueue(agent, keyType);
            }
        }

        public static IAgent AddAgent(Entity owner, Type keyType, Type realType, Type queryType = null)
        {
            queryType ??= keyType;
            var agents = GetAgents(owner);
            var queries = GetQueries(queryType);
            if (!agents.TryGetValue(keyType, out var agent))
            {
                agent = HeapManager.Dequeue<IAgent>(realType);
                agents.Add(keyType, agent);
                queries.Add(owner, agent);
                owner.Inject(agent);
                agent.Create(owner);
                agent.Dequeue();
                owner.OnShow += agent.OnShow;
                owner.OnHide += agent.OnHide;
                owner.OnFade += Enqueue;
            }

            return agent;

            void Enqueue()
            {
                agent.Enqueue();
                agents.Remove(keyType);
                queries.Remove(owner);
                if (agents.Count == 0) agentData.Remove(owner);
                if (queries.Count == 0) queryData.Remove(keyType);
                HeapManager.Enqueue(agent, realType);
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

            foreach (var queries in queryData.Values)
            {
                queries.Clear();
            }

            agentData.Clear();
            queryData.Clear();
        }
    }
}