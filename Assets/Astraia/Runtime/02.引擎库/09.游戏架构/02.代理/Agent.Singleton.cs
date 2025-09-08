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
    public abstract class Singleton<S, T> : Agent<T>, IAgent, IActive where S : Singleton<S, T> where T : Entity
    {
        private static S instance;

        public static S Instance => instance;

        void IAgent.Create(Entity owner)
        {
            instance = (S)this;
            this.owner = (T)owner;
        }

        public virtual void OnShow()
        {
        }

        public virtual void OnHide()
        {
        }
    }
}