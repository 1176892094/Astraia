// *********************************************************************************
// # Project: Astraia
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

namespace Astraia.Common
{
    internal static class AgentManager
    {
        internal static void Register<T>(Component owner, Type type)
        {
            if (!GlobalManager.Instance) return;
            if (!GlobalManager.agentGroup.TryGetValue(owner, out var agents))
            {
                agents = new Dictionary<Type, IAgent>();
                GlobalManager.agentGroup.Add(owner, agents);
            }

            var item = HeapManager.Dequeue<IAgent>(type);
            agents.Add(typeof(T), item);
            item.OnShow(owner);
        }

        internal static IAgent Find<T>(Component owner)
        {
            if (!GlobalManager.Instance) return null;
            if (GlobalManager.agentGroup.TryGetValue(owner, out var agents))
            {
                if (agents.TryGetValue(typeof(T), out var agent))
                {
                    return agent;
                }
            }

            return null;
        }

        internal static void UnRegister<T>(Component owner)
        {
            if (!GlobalManager.Instance) return;
            if (GlobalManager.agentGroup.TryGetValue(owner, out var agents))
            {
                if (agents.TryGetValue(typeof(T), out var agent))
                {
                    agent.OnHide();
                    agents.Remove(typeof(T));
                    HeapManager.Enqueue(agent, agent.GetType());
                }
            }
        }
        
        internal static void Dispose()
        {
            foreach (var agents in GlobalManager.agentGroup.Values)
            {
                foreach (var agent in agents.Values)
                {
                    agent.OnHide();
                    HeapManager.Enqueue(agent, agent.GetType());
                }

                agents.Clear();
            }
            
            GlobalManager.agentGroup.Clear();
        }
    }
}