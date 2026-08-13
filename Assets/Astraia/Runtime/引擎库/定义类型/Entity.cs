using System;
using System.Collections.Generic;
using UnityEngine;

namespace Astraia
{
    [Serializable]
    public class Entity : Export
    {
        [SerializeReference]
        internal List<IModule> moduleList = new List<IModule>();

        internal int state;

        internal const int CREATE = 1 << 0;
        internal const int OWNING = 1 << 1;
        internal const int CLIENT = 1 << 2;
        internal const int SERVER = 1 << 3;
        internal const int ENABLE = 1 << 4;
        internal const int NOTIFY = 1 << 5;
        internal const int VISIBLE = 1 << 6;
        internal const int DESTROY = 1 << 7;

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

            foreach (var module in moduleList)
            {
                module.Release();
            }

            moduleList.Clear();
        }

        public T AddComponent<T>()
        {
            var module = (IModule)Activator.CreateInstance<T>();
            moduleList.Add(module);
            module.Acquire(this);
            return (T)module;
        }

        public T AddComponent<T>(Type item)
        {
            var module = (IModule)Activator.CreateInstance(item);
            moduleList.Add(module);
            module.Acquire(this);
            return (T)module;
        }

        public T GetComponent<T>(int index)
        {
            return index < moduleList.Count ? (T)moduleList[index] : default;
        }
    }

    public abstract class Export : MonoBehaviour
    {
        protected virtual void Awake() { }

        protected virtual void OnEnable() { }

        protected virtual void OnDisable() { }

        protected virtual void OnDestroy() { }
    }

    [Serializable]
    public abstract class Singleton<T> : Module<Entity>, IModule where T : Singleton<T>
    {
        private static T instance;
        public static T Instance => instance;

        void IModule.Dequeue()
        {
            if (instance != this)
            {
                instance = (T)this;
                instance.Dequeue();
            }
        }

        void IModule.Enqueue()
        {
            if (instance == this)
            {
                instance.Enqueue();
                instance = null;
            }
        }
    }
}