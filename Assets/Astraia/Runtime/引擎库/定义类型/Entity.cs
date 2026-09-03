using UnityEngine;

namespace Astraia
{
    public class Entity : Export
    {
       

        protected override void Awake()
        {
            var modules = GetComponents<IDequeue>();
            foreach (var module in modules)
            {
                module.Dequeue();
            }
        }

        protected override void OnDestroy()
        {
            var modules = GetComponents<IEnqueue>();
            for (var i = modules.Length - 1; i >= 0; i--)
            {
                modules[i].Enqueue();
            }
        }
    }

    public abstract class Singleton<T> : Export where T : Singleton<T>
    {
        private static T instance;

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindAnyObjectByType<T>();
                }

                return instance;
            }
        }

        protected override void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = (T)this;

            if (instance is IDontDestroy)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        protected override void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
    }

    public abstract class Export : MonoBehaviour
    {
        protected virtual void Awake() { }

        protected virtual void OnEnable() { }

        protected virtual void OnDisable() { }

        protected virtual void OnDestroy() { }
    }
}