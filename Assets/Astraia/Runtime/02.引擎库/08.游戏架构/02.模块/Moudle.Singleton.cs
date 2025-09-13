// // *********************************************************************************
// // # Project: Astraia
// // # Unity: 6000.3.5f1
// // # Author: 云谷千羽
// // # Version: 1.0.0
// // # History: 2025-09-05 21:09:38
// // # Recently: 2025-09-05 21:09:38
// // # Copyright: 2024, 云谷千羽
// // # Description: This is an automatically generated comment.
// // *********************************************************************************

using Astraia.Common;

namespace Astraia
{
    public abstract class Singleton<T, E> : Module<E>, IModule, IActive where T : Singleton<T, E> where E : Entity
    {
        public static T Instance { get; private set; }

        void IModule.Create(Entity owner)
        {
            Instance = (T)this;
            this.owner = (E)owner;
        }

        public virtual void OnShow()
        {
        }

        public virtual void OnHide()
        {
        }
    }
}