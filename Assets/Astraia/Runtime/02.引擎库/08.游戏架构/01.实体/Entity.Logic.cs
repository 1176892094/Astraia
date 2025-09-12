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

    public static class EntityManager
    {
        private static readonly List<Entity> Empty = new List<Entity>();

        public static List<Entity> Query<T>() where T : IModule
        {
            if (queryData.TryGetValue(typeof(T), out var agents))
            {
                return agents;
            }

            return Empty;
        }

        private static List<Entity> GetQueries(Type queryType)
        {
            if (!queryData.TryGetValue(queryType, out var queries))
            {
                queries = new List<Entity>();
                queryData.Add(queryType, queries);
            }

            return queries;
        }

        internal static IModule AddComponent(Entity owner, IModule module, Type keyType, Type queryType = null)
        {
            queryType ??= keyType;
            var agents = owner.moduleData;
            var queries = GetQueries(queryType);
            if (!agents.ContainsKey(keyType))
            {
                queries.Add(owner);
                agents.Add(keyType, module);
                AddEvent(owner, module);
                owner.OnFade += Enqueue;
            }

            return module;

            void Enqueue()
            {
                module.Enqueue();
                agents.Remove(keyType);
                queries.Remove(owner);
                if (queries.Count == 0) queryData.Remove(keyType);
                HeapManager.Enqueue(module, keyType);
            }
        }

        internal static IModule AddComponent(Entity owner, Type keyType, Type realType, Type queryType = null)
        {
            queryType ??= keyType;
            var agents = owner.moduleData;
            var queries = GetQueries(queryType);
            if (!agents.TryGetValue(keyType, out var agent))
            {
                agent = HeapManager.Dequeue<IModule>(realType);
                queries.Add(owner);
                agents.Add(keyType, agent);
                AddEvent(owner, agent);
                owner.OnFade += Enqueue;
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

        private static void AddEvent(Entity owner, IModule module)
        {
            owner.Inject(module);
            module.Create(owner);
            module.Dequeue();

            var events = module.GetType().GetInterfaces();
            foreach (var @event in events)
            {
                if (@event.IsGenericType && @event.GetGenericTypeDefinition() == typeof(IEvent<>))
                {
                    var eventType = typeof(IEvent<>).MakeGenericType(@event.GetGenericArguments());
                    var agentShow = (Action)Delegate.CreateDelegate(typeof(Action), module, eventType.GetMethod("Listen", Service.Ref.Instance)!);
                    var agentHide = (Action)Delegate.CreateDelegate(typeof(Action), module, eventType.GetMethod("Remove", Service.Ref.Instance)!);
                    owner.OnShow += agentShow;
                    owner.OnHide += agentHide;
                }
            }

            if (module is IActive active)
            {
                owner.OnShow += active.OnShow;
                owner.OnHide += active.OnHide;
            }
        }

        internal static void Dispose()
        {
            foreach (var queries in queryData.Values)
            {
                queries.Clear();
            }

            queryData.Clear();
        }
    }
}