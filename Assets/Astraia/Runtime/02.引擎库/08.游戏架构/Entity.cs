// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-07-12 17:07:42
// // # Recently: 2025-07-12 17:07:42
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using Astraia.Common;
using UnityEngine;
#if UNITY_EDITOR && ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Astraia
{
    public partial class Entity : MonoBehaviour
    {
        private readonly Dictionary<Type, IModule> moduleData = new Dictionary<Type, IModule>();
        public event Action OnShow;
        public event Action OnHide;
        public event Action OnFade;

        public ICollection<Type> Keys => moduleData.Keys;
        public ICollection<IModule> Values => moduleData.Values;

        protected virtual void Awake()
        {
            foreach (var module in moduleList)
            {
                var result = Service.Ref.GetType(module);
                if (result != null)
                {
                    AddComponent(result);
                }
            }
        }

        protected virtual void OnEnable()
        {
            OnShow?.Invoke();
        }

        protected virtual void OnDisable()
        {
            OnHide?.Invoke();
        }

        protected virtual void OnDestroy()
        {
            OnFade?.Invoke();
            OnFade = null;
            OnShow = null;
            OnHide = null;
            moduleList.Clear();
            moduleData.Clear();
        }

#if UNITY_EDITOR && ODIN_INSPECTOR
        private static List<string> Modules = GlobalSetting.modules;

        [HideInEditorMode, ShowInInspector]
        private IEnumerable<IModule> modules
        {
            get => moduleData.Values.ToList();
            set => Debug.LogError(value);
        }

        [HideInPlayMode, ValueDropdown("Modules")]
#endif
        [SerializeField]
        private List<string> moduleList = new List<string>();
    }

    public partial class Entity
    {
        public T AddComponent<T>(T module) where T : IModule
        {
            return (T)LoadComponent(module.GetType(), module);
        }

        public T AddComponent<T>() where T : IModule
        {
            return (T)LoadComponent(typeof(T), typeof(T));
        }

        public T AddComponent<T>(Type realType) where T : IModule
        {
            return (T)LoadComponent(typeof(T), realType);
        }

        public T FindComponent<T>() where T : IModule
        {
            return moduleData.TryGetValue(typeof(T), out var module) ? (T)module : default;
        }

        public IModule AddComponent(Type keyType)
        {
            return LoadComponent(keyType, keyType);
        }

        public IModule AddComponent(Type keyType, Type realType)
        {
            return LoadComponent(keyType, realType);
        }

        public IModule FindComponent(Type keyType)
        {
            return moduleData.TryGetValue(keyType, out var module) ? module : null;
        }
    }

    public partial class Entity
    {
        private IModule LoadComponent(Type keyType, IModule module)
        {
            if (!moduleData.ContainsKey(keyType))
            {
                AddEvent(module);
                OnFade += Enqueue;
                moduleData.Add(keyType, module);
            }

            return module;

            void Enqueue()
            {
                module.Enqueue();
                moduleData.Remove(keyType);
                HeapManager.Enqueue(module, keyType);
            }
        }

        private IModule LoadComponent(Type keyType, Type realType)
        {
            if (!moduleData.TryGetValue(keyType, out var module))
            {
                module = HeapManager.Dequeue<IModule>(realType);
                AddEvent(module);
                OnFade += Enqueue;
                moduleData.Add(keyType, module);
            }

            return module;

            void Enqueue()
            {
                module.Enqueue();
                moduleData.Remove(keyType);
                HeapManager.Enqueue(module, realType);
            }
        }

        private void AddEvent(IModule module)
        {
            this.Inject(module);
            module.Acquire(this);
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