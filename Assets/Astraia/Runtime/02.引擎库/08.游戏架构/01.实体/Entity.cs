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
        internal readonly Dictionary<Type, IModule> moduleData = new Dictionary<Type, IModule>();
        public event Action OnShow;
        public event Action OnHide;
        public event Action OnFade;
      

        protected virtual void Awake()
        {
            foreach (var module in moduleList)
            {
                AddComponent(Service.Ref.GetType(module));
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
            return (T)EntityManager.AddComponent(this, module.GetType(), module);
        }

        public T AddComponent<T>(T module, Type queryType) where T : IModule
        {
            return (T)EntityManager.AddComponent(this, module.GetType(), module, queryType);
        }

        public T AddComponent<T>() where T : IModule
        {
            return (T)EntityManager.AddComponent(this, typeof(T), typeof(T));
        }

        public T AddComponent<T>(Type realType) where T : IModule
        {
            return (T)EntityManager.AddComponent(this, typeof(T), realType);
        }

        public T FindComponent<T>() where T : IModule
        {
            return moduleData.TryGetValue(typeof(T), out var module) ? (T)module : default;
        }

        public IModule AddComponent(Type realType)
        {
            return EntityManager.AddComponent(this, realType, realType);
        }

        public IModule AddComponent(Type keyType, Type realType)
        {
            return EntityManager.AddComponent(this, keyType, realType);
        }

        public IModule AddComponent(Type queryType, Type keyType, Type realType)
        {
            return EntityManager.AddComponent(this, keyType, realType, queryType);
        }

        public IModule FindComponent(Type keyType)
        {
            return moduleData.TryGetValue(keyType, out var module) ? module : null;
        }

        public IEnumerable<IModule> FindComponents()
        {
            return moduleData.Values;
        }
    }

    public static partial class Extensions
    {
        public static T GetValue<T>(this Entity owner, Enum id, T value = default)
        {
            return Variable<T>.Get(owner, id, value);
        }

        public static void SetValue<T>(this Entity owner, Enum id, T value)
        {
            Variable<T>.Set(owner, id, value);
        }
    }
}