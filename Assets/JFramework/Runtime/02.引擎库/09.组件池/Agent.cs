// *********************************************************************************
// # Project: JFramework
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2024-12-23 18:12:21
// # Recently: 2025-01-08 17:01:24
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Collections.Generic;
using UnityEngine;

namespace JFramework.Common
{
    internal static class AgentManager
    {
        internal static void Register<T>(Component owner, Type agentType)
        {
            if (!GlobalManager.Instance) return;
            if (!GlobalManager.agentData.TryGetValue(owner, out var agents))
            {
                agents = new Dictionary<Type, IAgent>();
                GlobalManager.agentData.Add(owner, agents);
            }

            var agentData = PoolManager.Dequeue<IAgent>(agentType);
            agents[typeof(T)] = agentData;
            agentData.OnShow(owner);
        }

        internal static IAgent Find<T>(Component owner)
        {
            if (!GlobalManager.Instance) return null;
            if (!GlobalManager.agentData.TryGetValue(owner, out var agents))
            {
                return null;
            }

            if (!agents.TryGetValue(typeof(T), out var agent))
            {
                return null;
            }

            return agent;
        }

        internal static void UnRegister<T>(Component owner)
        {
            if (!GlobalManager.Instance) return;
            if (!GlobalManager.agentData.TryGetValue(owner, out var agents))
            {
                return;
            }

            if (agents.TryGetValue(typeof(T), out var agent))
            {
                agent.OnHide();
                agents.Remove(typeof(T));
                PoolManager.Enqueue(agent, agent.GetType());
            }

            if (agents.Count == 0)
            {
                GlobalManager.agentData.Remove(owner);
            }
        }

        internal static void Update()
        {
            if (!GlobalManager.Instance) return;
            foreach (var agents in GlobalManager.agentData.Values)
            {
                foreach (var agent in agents.Values)
                {
                    agent.OnUpdate();
                }
            }
        }

        internal static void Dispose()
        {
            var agentCaches = new List<Component>(GlobalManager.agentData.Keys);
            foreach (var cache in agentCaches)
            {
                if (GlobalManager.agentData.TryGetValue(cache, out var agents))
                {
                    foreach (var agent in agents.Values)
                    {
                        agent.OnHide();
                        PoolManager.Enqueue(agent, agent.GetType());
                    }

                    agents.Clear();
                    GlobalManager.agentData.Remove(cache);
                }
            }

            GlobalManager.agentData.Clear();
        }
    }
}