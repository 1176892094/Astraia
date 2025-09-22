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

namespace Astraia.Common
{
    using static GlobalManager;

    public interface ISystem
    {
        int index => 0;
        void Update();
        void AddSystem() => SystemManager.AddSystem(index, this);
        void SubSystem() => SystemManager.SubSystem(index, this);
    }

    internal static class SystemManager
    {
        public static void AddSystem(int index, ISystem system)
        {
            if (!systemLoop.TryGetValue(index, out var systems))
            {
                systems = new List<ISystem>();
                systemLoop.Add(index, systems);
            }

            systems.Add(system);
        }

        public static void SubSystem(int index, ISystem system)
        {
            if (systemLoop.TryGetValue(index, out var systems))
            {
                systems.Remove(system);
            }
        }

        public static IModule AddComponent(Entity owner, Type keyType, IModule module)
        {
            if (!owner.moduleData.ContainsKey(keyType))
            {
                AddEvent(owner, module);
                owner.OnFade += Enqueue;
                owner.moduleData.Add(keyType, module);
            }

            return module;

            void Enqueue()
            {
                module.Enqueue();
                owner.moduleData.Remove(keyType);
                HeapManager.Enqueue(module, keyType);
            }
        }

        public static IModule AddComponent(Entity owner, Type keyType, Type realType)
        {
            if (!owner.moduleData.TryGetValue(keyType, out var module))
            {
                module = HeapManager.Dequeue<IModule>(realType);
                AddEvent(owner, module);
                owner.OnFade += Enqueue;
                owner.moduleData.Add(keyType, module);
            }

            return module;

            void Enqueue()
            {
                module.Enqueue();
                owner.moduleData.Remove(keyType);
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
                    var result = typeof(IEvent<>).MakeGenericType(@event.GetGenericArguments());
                    var onShow = (Action)Delegate.CreateDelegate(typeof(Action), module, result.GetMethod("Listen", Service.Ref.Instance)!);
                    var onHide = (Action)Delegate.CreateDelegate(typeof(Action), module, result.GetMethod("Remove", Service.Ref.Instance)!);
                    owner.OnShow += onShow;
                    owner.OnHide += onHide;
                }
            }

            if (module is ISystem system)
            {
                owner.OnShow += system.AddSystem;
                owner.OnHide += system.SubSystem;
            }

            if (module is IActive active)
            {
                owner.OnShow += active.OnShow;
                owner.OnHide += active.OnHide;
            }
        }

        public static void Destroy(Entity owner)
        {
            foreach (var variable in owner.variables)
            {
                typeof(Variable<>).MakeGenericType(variable).Invoke("Dispose", owner);
            }

            owner.variables.Clear();
            owner.variables = null;
        }

        public static void Update()
        {
            foreach (var systems in systemLoop.Values)
            {
                for (int i = systems.Count - 1; i >= 0; i--)
                {
                    systems[i].Update();
                }
            }

            AudioManager.Update();
        }

        public static void Dispose()
        {
            audioLoop.Clear();
            systemLoop.Clear();
        }
    }
}