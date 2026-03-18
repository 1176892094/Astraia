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
            Logic.Show();
        }

        protected virtual void OnDisable()
        {
            Logic.Hide();
        }

        protected virtual void OnDestroy()
        {
            Logic.Clear();
            Logic = null;
            moduleList.Clear();
        }

#if UNITY_EDITOR && ODIN_INSPECTOR
        private static readonly List<string> Windows = GlobalSetting.windows;

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
        public event Action OnShow;
        public event Action OnHide;
        public event Action OnFade;
        public ICollection<IModule> Modules => moduleData.Values;

        public Logic(object owner, List<string> moduleList)
        {
            this.owner = owner;
            foreach (var module in moduleList)
            {
                var result = Service.Ref.GetType(module);
                if (result != null)
                {
                    GetComponent(result, result);
                }
            }
        }

        public void Show()
        {
            OnShow?.Invoke();
        }

        public void Hide()
        {
            OnHide?.Invoke();
        }

        public void Clear()
        {
            OnFade?.Invoke();
            OnFade = null;
            OnShow = null;
            OnHide = null;
            owner = null;
            moduleData.Clear();
        }

        public T AddComponent<T>() where T : IModule
        {
            return (T)GetComponent(typeof(T), typeof(T));
        }

        public T AddComponent<T>(Type result) where T : IModule
        {
            return (T)GetComponent(typeof(T), result);
        }

        public T GetComponent<T>() where T : IModule
        {
            return (T)moduleData.GetValueOrDefault(typeof(T));
        }

        internal IModule GetComponent(Type source, Type result)
        {
            if (!moduleData.TryGetValue(source, out var module))
            {
                module = HeapManager.Dequeue<IModule>(result);
                moduleData.Add(source, module);
                var entity = (Entity)owner;
                entity.Inject(module);
                module.Acquire(entity);
                module.Dequeue();
                OnFade += () =>
                {
                    module.Enqueue();
                    moduleData.Remove(source);
                    HeapManager.Enqueue(module, result);
                };

                var events = module.GetType().GetInterfaces();
                foreach (var @event in events)
                {
                    if (@event.IsGenericType && @event.GetGenericTypeDefinition() == typeof(IEvent<>))
                    {
                        var reason = typeof(IEvent<>).MakeGenericType(@event.GetGenericArguments());
                        OnShow += (Action)Delegate.CreateDelegate(typeof(Action), module, reason.GetMethod("Listen", Service.Ref.Instance)!);
                        OnHide += (Action)Delegate.CreateDelegate(typeof(Action), module, reason.GetMethod("Remove", Service.Ref.Instance)!);
                    }
                }

                if (module is ISystem system)
                {
                    OnShow += system.AddEvent;
                    OnHide += system.SubEvent;
                }

                if (module is IActive active)
                {
                    OnShow += active.OnShow;
                    OnHide += active.OnHide;
                }
            }

            return module;
        }
    }
}