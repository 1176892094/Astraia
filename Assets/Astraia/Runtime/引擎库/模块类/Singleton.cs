// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-09-02 15:09:12
// # Recently: 2026-09-02 15:13:13
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

namespace Astraia
{
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
}