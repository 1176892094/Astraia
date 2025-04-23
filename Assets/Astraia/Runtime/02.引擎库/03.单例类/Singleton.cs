// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2025-01-08 19:01:57
// # Recently: 2025-01-08 19:01:57
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

using Astraia.Common;
using UnityEngine;

namespace Astraia
{
    public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        private static readonly object locked = typeof(T);

        private static T instance;

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (locked)
                    {
                        instance ??= FindFirstObjectByType<T>();
                        if (typeof(IAutomatic).IsAssignableFrom(typeof(T)))
                        {
                            instance ??= new GameObject(typeof(T).Name).AddComponent<T>();
                        }

                        instance?.Awake();
                    }
                }

                return instance;
            }
        }

        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = (T)this;
            }
            else if (instance != this)
            {
                Debug.LogWarning(typeof(T) + "单例重复！");
                Destroy(this);
            }

            if (typeof(IPerpetual).IsAssignableFrom(typeof(T)))
            {
                if (Application.isPlaying)
                {
                    DontDestroyOnLoad(gameObject);
                }
            }
        }

        protected virtual void OnDestroy()
        {
            instance = null;
        }
    }
}