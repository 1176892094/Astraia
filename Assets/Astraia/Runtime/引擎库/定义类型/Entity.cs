using System;
using System.Collections.Generic;
using UnityEngine;

namespace Astraia.Core
{
    public class Entity : MonoBehaviour
    {
        [SerializeReference] public List<IModule> moduleList = new List<IModule>();

        protected virtual void Awake()
        {
            foreach (var module in moduleList)
            {
                module.Acquire(this);
            }

            foreach (var module in moduleList)
            {
                module.Dequeue();
            }
        }

        protected virtual void OnEnable()
        {
            foreach (var module in moduleList)
            {
                if (module is not UIPanel)
                {
                    module.OnShow();
                }
            }
        }

        protected virtual void OnDisable()
        {
            foreach (var module in moduleList)
            {
                if (module is not UIPanel)
                {
                    module.OnHide();
                }
            }
        }

        protected virtual void OnDestroy()
        {
            foreach (var module in moduleList)
            {
                module.Enqueue();
            }

            moduleList.Clear();
        }

        public T AddComponent<T>() where T : IModule
        {
            var module = Activator.CreateInstance<T>();
            module.Acquire(this);
            moduleList.Add(module);
            return module;
        }

        public T AddComponent<T>(Type item) where T : IModule
        {
            var module = (T)Activator.CreateInstance(item);
            module.Acquire(this);
            moduleList.Add(module);
            return module;
        }

        public T GetComponent<T>(int index) where T : IModule
        {
            return (T)moduleList[index];
        }

        public bool TryGetComponent<T>(int index, out T module) where T : IModule
        {
            module = (T)moduleList[index];
            return module != null;
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

    public abstract class Module
    {
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

    public abstract class Module<T> : Module, IModule
    {
        [NonSerialized] public T owner;

        void IModule.Acquire(object owner)
        {
            this.owner = (T)owner;
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