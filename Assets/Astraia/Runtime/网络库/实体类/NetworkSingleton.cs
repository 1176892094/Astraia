// *********************************************************************************
// # Project: Astraia
// # Unity: 6000.3.5f1
// # Author: 云谷千羽
// # Version: 1.0.0
// # History: 2026-09-02 21:09:44
// # Recently: 2026-09-03 14:21:03
// # Copyright: 2024, 云谷千羽
// # Description: This is an automatically generated comment.
// *********************************************************************************

namespace Astraia.Net
{
    public class NetworkSingleton<T> : NetworkModule where T : NetworkSingleton<T>
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