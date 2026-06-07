using System;
using System.Collections.Generic;
using UnityEngine;

namespace Astraia.Core
{
    public class Entity : MonoBehaviour
    {
        [SerializeReference] public List<IModule> modules = new List<IModule>();

        protected virtual void Awake()
        {
            foreach (var module in modules)
            {
                module.Acquire(this);
            }

            foreach (var module in modules)
            {
                module.Dequeue();
            }
        }

        protected virtual void OnEnable()
        {
            foreach (var module in modules)
            {
                module.OnShow();
            }
        }

        protected virtual void OnDisable()
        {
            foreach (var module in modules)
            {
                module.OnHide();
            }
        }

        protected virtual void OnDestroy()
        {
            foreach (var module in modules)
            {
                module.Enqueue();
            }

            modules.Clear();
        }

        public T AddComponent<T>() where T : IModule
        {
            var module = Activator.CreateInstance<T>();
            modules.Add(module);
            return module;
        }

        public T GetComponent<T>(int index) where T : IModule
        {
            return (T)modules[index];
        }
    }

    public interface IModule
    {
        void Acquire(object owner);
        void Dequeue();
        void Enqueue();
        void OnShow();
        void OnHide();
    }

    public abstract class Module<T> : IModule
    {
        [NonSerialized] public T owner;

        void IModule.Acquire(object owner)
        {
            this.owner = (T)owner;
        }

        public virtual void Dequeue()
        {
        }

        public virtual void OnShow()
        {
        }

        public virtual void OnHide()
        {
        }

        public virtual void Enqueue()
        {
        }
    }

    public abstract class Singleton<T> : Module<Entity>, IModule where T : Singleton<T>
    {
        public static T Instance;

        void IModule.Acquire(object owner)
        {
            Instance = (T)this;
            this.owner = (Entity)owner;
        }
    }
}