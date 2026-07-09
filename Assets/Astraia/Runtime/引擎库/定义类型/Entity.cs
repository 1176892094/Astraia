using System;
using System.Collections.Generic;
using UnityEngine;

namespace Astraia
{
    [Serializable]
    public class Entity : Inject
    {
        [SerializeReference]
        public List<IModule> moduleList = new List<IModule>();

        private int state;

        private const int CREATE = 1 << 0;

        protected override void OnEnable()
        {
            if ((state & CREATE) == 0)
            {
                foreach (var module in moduleList)
                {
                    module.Acquire(this);
                }

                foreach (var module in moduleList)
                {
                    module.Dequeue();
                }

                state |= CREATE;
            }

            foreach (var module in moduleList)
            {
                module.OnShow();
            }
        }

        protected override void OnDisable()
        {
            foreach (var module in moduleList)
            {
                module.OnHide();
            }
        }

        protected override void OnDestroy()
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
            moduleList.Add(module);
            module.Acquire(this);
            return module;
        }

        public T AddComponent<T>(Type item) where T : IModule
        {
            var module = (T)Activator.CreateInstance(item);
            moduleList.Add(module);
            module.Acquire(this);
            return module;
        }

        public T GetComponent<T>(int index) where T : IModule
        {
            return index < moduleList.Count ? (T)moduleList[index] : default;
        }
    }

    public abstract class Inject : MonoBehaviour
    {
        protected virtual void Awake()
        {
        }

        protected virtual void OnEnable()
        {
        }

        protected virtual void OnDisable()
        {
        }

        protected virtual void OnDestroy()
        {
        }
    }

    [Serializable]
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