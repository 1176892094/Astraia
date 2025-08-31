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

        private static List<Entity, IAgent> GetEntities(Type agent)
        {
            if (!entityData.TryGetValue(agent, out var entities))
            {
                entities = new List<Entity, IAgent>();
                entityData.Add(agent, entities);
            }

            return entities;
        }

        public static IAgent AddAgent(Entity owner, IAgent agent)
        {
            var agents = GetAgents(owner);
            var keyType = agent.GetType();
            var entities = GetEntities(keyType);
            if (!agents.ContainsKey(keyType))
            {
                agents.Add(keyType, agent);
                entities.Add(owner, agent);
                
                agent.Create(owner);
                agent.Dequeue();
                owner.OnShow += agent.OnShow;
                owner.OnHide += agent.OnHide;
                owner.OnFade += Enqueue;

                void Enqueue()
                {
                    agent.Enqueue();
                    HeapManager.Enqueue(agent, keyType);
                    agents.Remove(keyType);
                    entities.Remove(owner);
                }
            }

            return agent;
        }

        public static IAgent AddAgent(Entity owner, IAgent agent, Type queryType)
        {
            var agents = GetAgents(owner);
            var keyType = agent.GetType();
            var entities = GetEntities(queryType);
            if (!agents.ContainsKey(keyType))
            {
                agents.Add(keyType, agent);
                entities.Add(owner, agent);
                
                agent.Create(owner);
                agent.Dequeue();
                owner.OnShow += agent.OnShow;
                owner.OnHide += agent.OnHide;
                owner.OnFade += Enqueue;

                void Enqueue()
                {
                    agent.Enqueue();
                    HeapManager.Enqueue(agent, keyType);
                    agents.Remove(keyType);
                    entities.Remove(owner);
                }
            }

            return agent;
        }

        public static IAgent AddAgent(Entity owner, Type keyType, Type realType)
        {
            var agents = GetAgents(owner);
            var entities = GetEntities(keyType);
            if (!agents.TryGetValue(keyType, out var agent))
            {
                agent = HeapManager.Dequeue<IAgent>(realType);
                agents.Add(keyType, agent);
                entities.Add(owner, agent);
                
                agent.Create(owner);
                agent.Dequeue();
                owner.OnShow += agent.OnShow;
                owner.OnHide += agent.OnHide;
                owner.OnFade += Enqueue;

                void Enqueue()
                {
                    agent.Enqueue();
                    HeapManager.Enqueue(agent, realType);
                    agents.Remove(keyType);
                    entities.Remove(owner);
                }
            }

            return agent;
        }

        public static IAgent AddAgent(Entity owner, Type keyType, Type realType, Type queryType)
        {
            var agents = GetAgents(owner);
            var entities = GetEntities(queryType);
            if (!agents.TryGetValue(keyType, out var agent))
            {
                agent = HeapManager.Dequeue<IAgent>(realType);
                agents.Add(keyType, agent);
                entities.Add(owner, agent);
                
                agent.Create(owner);
                agent.Dequeue();
                owner.OnShow += agent.OnShow;
                owner.OnHide += agent.OnHide;
                owner.OnFade += Enqueue;

                void Enqueue()
                {
                    agent.Enqueue();
                    HeapManager.Enqueue(agent, realType);
                    agents.Remove(keyType);
                    entities.Remove(owner);
                }
            }

            return agent;
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

            foreach (var queries in entityData.Values)
            {
                queries.Clear();
            }

            agentData.Clear();
            entityData.Clear();
        }
    }
}