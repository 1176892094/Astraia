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
            Logic = new Logic(new InjectAdaptor(this), modules);
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
        }

#if UNITY_EDITOR
        private static readonly List<string> Windows = new List<string>();

        internal static void LoadComponent(Type module)
        {
            if (!module.IsAbstract && !module.IsGenericType && typeof(IModule).IsAssignableFrom(module))
            {
                Windows.Add("{0}, {1}".Format(module.FullName, module.Assembly.GetName().Name));
            }
        }

        internal static void LoadComplete()
        {
            Windows.Sort(StringComparer.Ordinal);
        }
#if ODIN_INSPECTOR
        [HideInEditorMode, ShowInInspector]
        private IEnumerable<IModule> windows
        {
            get => Logic?.Modules.ToList();
            set => Log.Error(value);
        }

        [HideInPlayMode, PropertyOrder(1), ValueDropdown("Windows")]
#endif
#endif
        [SerializeField]
        private List<string> modules = new List<string>();

        private readonly struct InjectAdaptor : Logic.IInject
        {
            private readonly Entity owner;
            public InjectAdaptor(Entity owner) => this.owner = owner;
            public object Inject(object target) => owner.Inject(target);
        }
    }

    public abstract class Module<T> : Acquire<T>, IModule where T : Entity
    {
        public Transform transform => owner?.transform;
        public GameObject gameObject => owner?.gameObject;

        public virtual void Dequeue()
        {
        }

        public virtual void Enqueue()
        {
        }

        public static implicit operator bool(Module<T> module)
        {
            return module != null && module.owner && module.owner.isActiveAndEnabled;
        }
    }

    public abstract class Singleton<TKey, T> : Module<T>, IAcquire, IActive where TKey : Singleton<TKey, T> where T : Entity
    {
        public static TKey Instance;

        void IAcquire.Acquire(object item)
        {
            owner = (T)item;
            Instance = (TKey)this;
        }

        public virtual void OnShow()
        {
        }

        public virtual void OnHide()
        {
        }
    }
}