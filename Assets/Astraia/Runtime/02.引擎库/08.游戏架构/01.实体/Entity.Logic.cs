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
            if (queryData.TryGetValue(typeof(T), out var modules))
            {
                return modules;
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
            var modules = owner.moduleData;
            var queries = GetQueries(queryType);
            if (!modules.ContainsKey(keyType))
            {
                queries.Add(owner);
                modules.Add(keyType, module);
                AddEvent(owner, module);
                owner.OnFade += Enqueue;
            }

            return module;

            void Enqueue()
            {
                module.Enqueue();
                modules.Remove(keyType);
                queries.Remove(owner);
                if (queries.Count == 0) queryData.Remove(keyType);
                HeapManager.Enqueue(module, keyType);
            }
        }

        internal static IModule AddComponent(Entity owner, Type keyType, Type realType, Type queryType = null)
        {
            queryType ??= keyType;
            var modules = owner.moduleData;
            var queries = GetQueries(queryType);
            if (!modules.TryGetValue(keyType, out var module))
            {
                module = HeapManager.Dequeue<IModule>(realType);
                queries.Add(owner);
                modules.Add(keyType, module);
                AddEvent(owner, module);
                owner.OnFade += Enqueue;
            }

            return module;

            void Enqueue()
            {
                module.Enqueue();
                modules.Remove(keyType);
                queries.Remove(owner);
                if (queries.Count == 0) queryData.Remove(keyType);
                HeapManager.Enqueue(module, realType);
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
                    var moduleType = typeof(IEvent<>).MakeGenericType(@event.GetGenericArguments());
                    var moduleShow = (Action)Delegate.CreateDelegate(typeof(Action), module, moduleType.GetMethod("Listen", Service.Ref.Instance)!);
                    var moduleHide = (Action)Delegate.CreateDelegate(typeof(Action), module, moduleType.GetMethod("Remove", Service.Ref.Instance)!);
                    owner.OnShow += moduleShow;
                    owner.OnHide += moduleHide;
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