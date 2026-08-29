using UnityEngine;

namespace Astraia
{
    public class Entity : Export
    {
        internal const int CREATE = 1 << 0;
        internal const int OWNING = 1 << 1;
        internal const int CLIENT = 1 << 2;
        internal const int SERVER = 1 << 3;
        internal const int ENABLE = 1 << 4;
        internal const int NOTIFY = 1 << 5;
        internal const int VISIBLE = 1 << 6;
        internal const int DESTROY = 1 << 7;

        internal int state;

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
        public static T Instance { get; private set; }

        protected override void Awake()
        {
            Instance = (T)this;
        }

        protected override void OnDestroy()
        {
            Instance = null;
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