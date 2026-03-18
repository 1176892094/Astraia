// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2025-07-12 17:07:42
// # Recently: 2025-07-12 17:07:42
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using Astraia.Core;
using UnityEngine;
#if UNITY_EDITOR && ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Astraia
{
    public class Entity : MonoBehaviour
    {
        public Logic Logic;

        protected virtual void Awake()
        {
            Logic = new Logic(this, moduleList);
        }

        protected virtual void OnEnable()
        {
            Logic.OnShow?.Invoke();
        }

        protected virtual void OnDisable()
        {
            Logic.OnHide?.Invoke();
        }

        protected virtual void OnDestroy()
        {
            Logic.Clear();
            Logic = null;
            moduleList.Clear();
        }

#if UNITY_EDITOR && ODIN_INSPECTOR
        private static List<string> Windows = GlobalSetting.windows;

        [HideInEditorMode, ShowInInspector]
        private IEnumerable<IModule> windows
        {
            get => Logic?.Modules.ToList();
            set => Service.Log.Error(value);
        }

        [HideInPlayMode, PropertyOrder(1), ValueDropdown("Windows")]
#endif
        [SerializeField]
        private List<string> moduleList = new List<string>();
    }

    public class Logic
    {
        private readonly Dictionary<Type, IModule> moduleData = new Dictionary<Type, IModule>();
        private object owner;
        public Action OnShow;
        public Action OnHide;
        public Action OnFade;
        public ICollection<IModule> Modules => moduleData.Values;

        public Logic(object owner, List<string> moduleList)
        {
            Debug.Log(owner);
            this.owner = owner;
            foreach (var module in moduleList)
            {
                var result = Service.Ref.GetType(module);
                if (result != null)
                {
                    LoadComponent(result, result);
                }
            }
        }

        public void Clear()
        {
            OnFade?.Invoke();
            OnFade = null;
            OnShow = null;
            OnHide = null;
            moduleData.Clear();
        }

        public T AddComponent<T>() where T : IModule
        {
            return (T)LoadComponent(typeof(T), typeof(T));
        }

        public T AddComponent<T, V>() where T : IModule where V : IModule
        {
            return (T)LoadComponent(typeof(T), typeof(V));
        }

        public T FindComponent<T>() where T : IModule
        {
            return moduleData.TryGetValue(typeof(T), out var module) ? (T)module : default;
        }

        internal IModule LoadComponent(Type keyType, Type realType)
        {
            if (!moduleData.TryGetValue(keyType, out var module))
            {
                module = HeapManager.Dequeue<IModule>(realType);
                AddModule(module);
                OnFade += () =>
                {
                    module.Enqueue();
                    moduleData.Remove(keyType);
                    HeapManager.Enqueue(module, realType);
                };
                moduleData.Add(keyType, module);
            }

            return module;
        }

        private void AddModule(IModule module)
        {
            var entity = (Entity)owner;
            entity.Inject(module);
            module.Acquire(entity);
            module.Dequeue();

            var events = module.GetType().GetInterfaces();
            foreach (var @event in events)
            {
                if (@event.IsGenericType && @event.GetGenericTypeDefinition() == typeof(IEvent<>))
                {
                    var result = typeof(IEvent<>).MakeGenericType(@event.GetGenericArguments());
                    OnShow += (Action)Delegate.CreateDelegate(typeof(Action), module, result.GetMethod("Listen", Service.Ref.Instance)!);
                    OnHide += (Action)Delegate.CreateDelegate(typeof(Action), module, result.GetMethod("Remove", Service.Ref.Instance)!);
                }
            }

            if (module is ISystem system)
            {
                OnShow += system.AddSystem;
                OnHide += system.SubSystem;
            }

            if (module is IActive active)
            {
                OnShow += active.OnShow;
                OnHide += active.OnHide;
            }
        }
    }
}