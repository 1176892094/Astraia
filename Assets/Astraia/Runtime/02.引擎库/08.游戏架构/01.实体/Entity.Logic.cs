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
        private static List<Entity> GetQueries(Type queryType)
        {
            if (!queryData.TryGetValue(queryType, out var queries))
            {
                queries = new List<Entity>();
                queryData.Add(queryType, queries);
            }

            return queries;
        }

        public static IAgent AddComponent(Entity owner, IAgent agent, Type keyType, Type queryType = null)
        {
            queryType ??= keyType;
            var agents = owner.agentData;
            var queries = GetQueries(queryType);
            if (!agents.ContainsKey(keyType))
            {
                queries.Add(owner);
                agents.Add(keyType, agent);
                owner.Inject(agent);
                agent.Create(owner);
                agent.Dequeue();
                owner.OnFade += Enqueue;
                if (agent is IActive active)
                {
                    owner.OnShow += active.OnShow;
                    owner.OnHide += active.OnHide;
                }
            }

            return agent;

            void Enqueue()
            {
                agent.Enqueue();
                agents.Remove(keyType);
                queries.Remove(owner);
                if (queries.Count == 0) queryData.Remove(keyType);
                HeapManager.Enqueue(agent, keyType);
            }
        }

        public static IAgent AddComponent(Entity owner, Type keyType, Type realType, Type queryType = null)
        {
            queryType ??= keyType;
            var agents = owner.agentData;
            var queries = GetQueries(queryType);
            if (!agents.TryGetValue(keyType, out var agent))
            {
                agent = HeapManager.Dequeue<IAgent>(realType);
                queries.Add(owner);
                agents.Add(keyType, agent);
                owner.Inject(agent);
                agent.Create(owner);
                agent.Dequeue();
                owner.OnFade += Enqueue;
                if (agent is IActive active)
                {
                    owner.OnShow += active.OnShow;
                    owner.OnHide += active.OnHide;
                }
            }

            return agent;

            void Enqueue()
            {
                agent.Enqueue();
                agents.Remove(keyType);
                queries.Remove(owner);
                if (queries.Count == 0) queryData.Remove(keyType);
                HeapManager.Enqueue(agent, realType);
            }
        }
        
        public static void Dispose()
        {
            foreach (var queries in queryData.Values)
            {
                queries.Clear();
            }

            queryData.Clear();
        }
    }
}